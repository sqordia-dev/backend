using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;

namespace Sqordia.Infrastructure.Services.QualityAgents;

/// <summary>
/// Orchestrates all quality agents, running them in parallel and aggregating results.
/// </summary>
public class QualityAgentOrchestrator : IQualityAgentOrchestrator
{
    private readonly IEnumerable<IQualityAgent> _agents;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<QualityAgentOrchestrator> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public QualityAgentOrchestrator(
        IEnumerable<IQualityAgent> agents,
        IApplicationDbContext context,
        ILogger<QualityAgentOrchestrator> logger)
    {
        _agents = agents;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<QualityAgentReport>> RunQualityAgentsAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Running quality agents for plan {PlanId}", businessPlanId);

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<QualityAgentReport>(
                    Error.NotFound("QualityAgent.Error.NotFound", $"Business plan {businessPlanId} not found."));
            }

            // Build context for agents
            var planContext = new BusinessPlanContext
            {
                BusinessPlanId = businessPlanId,
                PlanType = businessPlan.PlanType.ToString(),
                BusinessBriefJson = businessPlan.BusinessBriefJson,
                Language = language,
                Sections = BuildSectionsMap(businessPlan)
            };

            // Run all agents in parallel
            var agentTasks = _agents.Select(agent =>
                RunAgentSafelyAsync(agent, planContext, cancellationToken));

            var results = await Task.WhenAll(agentTasks);

            // Aggregate results
            var report = AggregateResults(businessPlanId, results);

            // Store report on entity
            var reportJson = JsonSerializer.Serialize(report, JsonOptions);
            businessPlan.SetQualityReport(reportJson, report.BankReadinessScore, report.OverallScore);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Quality agents completed for plan {PlanId}. Overall: {Score}, Bank-Ready: {BankScore}",
                businessPlanId, report.OverallScore, report.BankReadinessScore);

            return Result.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running quality agents for plan {PlanId}", businessPlanId);
            return Result.Failure<QualityAgentReport>($"Quality analysis failed: {ex.Message}");
        }
    }

    private async Task<AgentAnalysisResult> RunAgentSafelyAsync(
        IQualityAgent agent, BusinessPlanContext context, CancellationToken ct)
    {
        try
        {
            _logger.LogDebug("Running quality agent: {AgentName}", agent.AgentName);
            return await agent.AnalyzeAsync(context, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quality agent {AgentName} failed", agent.AgentName);
            return new AgentAnalysisResult
            {
                AgentName = agent.AgentName,
                Score = 0,
                Findings = new List<AgentFinding>
                {
                    new()
                    {
                        AgentName = agent.AgentName,
                        Section = "General",
                        Severity = FindingSeverity.Low,
                        Finding = $"Agent analysis failed: {ex.Message}",
                        Suggestion = "Re-run quality analysis"
                    }
                }
            };
        }
    }

    private static QualityAgentReport AggregateResults(
        Guid businessPlanId, AgentAnalysisResult[] results)
    {
        var writingResult = results.FirstOrDefault(r => r.AgentName == "WritingQuality");
        var financialResult = results.FirstOrDefault(r => r.AgentName == "FinancialConsistency");
        var complianceResult = results.FirstOrDefault(r => r.AgentName == "Compliance");
        var bankResult = results.FirstOrDefault(r => r.AgentName == "BankReadiness");

        var writingScore = writingResult?.Score ?? 50;
        var financialScore = financialResult?.Score ?? 50;
        var complianceScore = complianceResult?.Score ?? 50;
        var bankReadinessScore = bankResult?.Score ?? 50;

        // Weighted average: Bank readiness 30%, Financial 25%, Writing 25%, Compliance 20%
        var overallScore = Math.Round(
            bankReadinessScore * 0.30m +
            financialScore * 0.25m +
            writingScore * 0.25m +
            complianceScore * 0.20m, 1);

        return new QualityAgentReport
        {
            BusinessPlanId = businessPlanId,
            OverallScore = overallScore,
            WritingScore = writingScore,
            FinancialScore = financialScore,
            ComplianceScore = complianceScore,
            BankReadinessScore = bankReadinessScore,
            Findings = results.SelectMany(r => r.Findings).ToList(),
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private static Dictionary<string, string?> BuildSectionsMap(
        Domain.Entities.BusinessPlan.BusinessPlan plan)
    {
        return new Dictionary<string, string?>
        {
            ["ExecutiveSummary"] = plan.ExecutiveSummary,
            ["ProblemStatement"] = plan.ProblemStatement,
            ["Solution"] = plan.Solution,
            ["MarketAnalysis"] = plan.MarketAnalysis,
            ["CompetitiveAnalysis"] = plan.CompetitiveAnalysis,
            ["SwotAnalysis"] = plan.SwotAnalysis,
            ["BusinessModel"] = plan.BusinessModel,
            ["MarketingStrategy"] = plan.MarketingStrategy,
            ["BrandingStrategy"] = plan.BrandingStrategy,
            ["OperationsPlan"] = plan.OperationsPlan,
            ["ManagementTeam"] = plan.ManagementTeam,
            ["FinancialProjections"] = plan.FinancialProjections,
            ["FundingRequirements"] = plan.FundingRequirements,
            ["RiskAnalysis"] = plan.RiskAnalysis,
            ["ExitStrategy"] = plan.ExitStrategy,
            ["MissionStatement"] = plan.MissionStatement,
            ["SocialImpact"] = plan.SocialImpact,
            ["BeneficiaryProfile"] = plan.BeneficiaryProfile,
            ["GrantStrategy"] = plan.GrantStrategy,
            ["SustainabilityPlan"] = plan.SustainabilityPlan,
        };
    }
}

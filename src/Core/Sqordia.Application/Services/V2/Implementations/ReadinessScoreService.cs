using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.V2.Readiness;
using Sqordia.Domain.Entities.BusinessPlan;
using System.Text.RegularExpressions;

namespace Sqordia.Application.Services.V2.Implementations;

/// <summary>
/// Bank-ready percentage calculation service
/// Weights: 50% Consistency, 30% Risk Mitigation, 20% Completeness
/// </summary>
public class ReadinessScoreService : IReadinessScoreService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ReadinessScoreService> _logger;

    private const decimal ConsistencyWeight = 0.50m;
    private const decimal RiskMitigationWeight = 0.30m;
    private const decimal CompletenessWeight = 0.20m;

    public ReadinessScoreService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<ReadinessScoreService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<ReadinessScoreResponse>> CalculateReadinessScoreAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calculating readiness score for business plan {PlanId}", businessPlanId);

            var businessPlan = await GetBusinessPlanWithAccessCheckAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<ReadinessScoreResponse>(
                    Error.NotFound("BusinessPlan.NotFound", "Business plan not found or access denied."));
            }

            var consistencyScore = CalculateConsistencyScore(businessPlan);
            var riskMitigationScore = CalculateRiskMitigationScore(businessPlan);
            var completenessScore = CalculateCompletenessScore(businessPlan);

            var overallScore = (consistencyScore * ConsistencyWeight) +
                              (riskMitigationScore * RiskMitigationWeight) +
                              (completenessScore * CompletenessWeight);

            var readinessLevel = DetermineReadinessLevel(overallScore);
            var recommendations = GenerateRecommendations(businessPlan, consistencyScore, riskMitigationScore, completenessScore);

            var response = new ReadinessScoreResponse
            {
                BusinessPlanId = businessPlanId,
                OverallScore = Math.Round(overallScore, 2),
                ConsistencyScore = Math.Round(consistencyScore, 2),
                RiskMitigationScore = Math.Round(riskMitigationScore, 2),
                CompletenessScore = Math.Round(completenessScore, 2),
                ReadinessLevel = readinessLevel,
                Recommendations = recommendations,
                CalculatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Readiness score calculated: {Score} ({Level})", overallScore, readinessLevel);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating readiness score for business plan {PlanId}", businessPlanId);
            return Result.Failure<ReadinessScoreResponse>(
                Error.InternalServerError("Readiness.CalculationError", "An error occurred calculating the readiness score."));
        }
    }

    public async Task<Result<ReadinessBreakdownResponse>> GetReadinessBreakdownAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting readiness breakdown for business plan {PlanId}", businessPlanId);

            var businessPlan = await GetBusinessPlanWithAccessCheckAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<ReadinessBreakdownResponse>(
                    Error.NotFound("BusinessPlan.NotFound", "Business plan not found or access denied."));
            }

            var sections = new List<SectionReadiness>();
            var missingElements = new List<string>();
            var inconsistentElements = new List<string>();
            var riskGaps = new List<string>();

            // Analyze each section
            AnalyzeSection(businessPlan.ExecutiveSummary, "Executive Summary", sections, missingElements);
            AnalyzeSection(businessPlan.BusinessModel, "Business Model", sections, missingElements);
            AnalyzeSection(businessPlan.MarketAnalysis, "Market Analysis", sections, missingElements);
            AnalyzeSection(businessPlan.FinancialProjections, "Financial Projections", sections, missingElements);
            AnalyzeSection(businessPlan.SwotAnalysis, "SWOT Analysis", sections, missingElements);
            AnalyzeSection(businessPlan.OperationsPlan, "Operations Plan", sections, missingElements);

            // Check for inconsistencies
            CheckForInconsistencies(businessPlan, inconsistentElements);

            // Identify risk gaps
            IdentifyRiskGaps(businessPlan, riskGaps);

            var response = new ReadinessBreakdownResponse
            {
                BusinessPlanId = businessPlanId,
                Sections = sections,
                MissingElements = missingElements,
                InconsistentElements = inconsistentElements,
                RiskGaps = riskGaps,
                CalculatedAt = DateTime.UtcNow
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting readiness breakdown for business plan {PlanId}", businessPlanId);
            return Result.Failure<ReadinessBreakdownResponse>(
                Error.InternalServerError("Readiness.BreakdownError", "An error occurred getting the readiness breakdown."));
        }
    }

    public async Task<Result<decimal>> RecalculateAndSaveAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var scoreResult = await CalculateReadinessScoreAsync(businessPlanId, cancellationToken);
            if (scoreResult.IsFailure)
            {
                return Result.Failure<decimal>(scoreResult.Error!);
            }

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan != null && scoreResult.Value != null)
            {
                businessPlan.UpdateReadinessScore(scoreResult.Value.OverallScore);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Readiness score saved for business plan {PlanId}: {Score}",
                    businessPlanId, scoreResult.Value.OverallScore);
            }

            return Result.Success(scoreResult.Value!.OverallScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving readiness score for business plan {PlanId}", businessPlanId);
            return Result.Failure<decimal>(
                Error.InternalServerError("Readiness.SaveError", "An error occurred saving the readiness score."));
        }
    }

    private async Task<BusinessPlan?> GetBusinessPlanWithAccessCheckAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            return null;
        }

        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.QuestionnaireResponses)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null)
        {
            return null;
        }

        var isMember = await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                           om.UserId == currentUserId &&
                           om.IsActive, cancellationToken);

        return isMember ? businessPlan : null;
    }

    private static decimal CalculateConsistencyScore(BusinessPlan plan)
    {
        decimal score = 100;
        var deductions = 0;

        // Check if financial projections align with market analysis
        if (!string.IsNullOrWhiteSpace(plan.FinancialProjections) && !string.IsNullOrWhiteSpace(plan.MarketAnalysis))
        {
            var hasRevenueNumbers = Regex.IsMatch(plan.FinancialProjections, @"\d+");
            var hasMarketData = Regex.IsMatch(plan.MarketAnalysis, @"\d+%?");
            if (hasRevenueNumbers && !hasMarketData)
            {
                deductions += 20; // Financial projections without market data support
            }
        }

        // Check if executive summary reflects all sections
        if (!string.IsNullOrWhiteSpace(plan.ExecutiveSummary))
        {
            var summaryLength = plan.ExecutiveSummary.Length;
            var hasMultipleSections = !string.IsNullOrWhiteSpace(plan.MarketAnalysis) &&
                                     !string.IsNullOrWhiteSpace(plan.FinancialProjections);
            if (hasMultipleSections && summaryLength < 500)
            {
                deductions += 15; // Summary too short for comprehensive plan
            }
        }

        // Check SWOT alignment with other sections
        if (!string.IsNullOrWhiteSpace(plan.SwotAnalysis))
        {
            var hasStrengths = plan.SwotAnalysis.Contains("strength", StringComparison.OrdinalIgnoreCase) ||
                              plan.SwotAnalysis.Contains("force", StringComparison.OrdinalIgnoreCase);
            var hasWeaknesses = plan.SwotAnalysis.Contains("weakness", StringComparison.OrdinalIgnoreCase) ||
                               plan.SwotAnalysis.Contains("faiblesse", StringComparison.OrdinalIgnoreCase);
            if (!hasStrengths || !hasWeaknesses)
            {
                deductions += 15;
            }
        }

        return Math.Max(0, score - deductions);
    }

    private static decimal CalculateRiskMitigationScore(BusinessPlan plan)
    {
        decimal score = 0;

        // Check for risk-related content
        var allContent = string.Join(" ",
            plan.SwotAnalysis ?? "",
            plan.FinancialProjections ?? "",
            plan.MarketAnalysis ?? "",
            plan.OperationsPlan ?? "");

        // Risk identification (+30)
        var riskKeywords = new[] { "risk", "risque", "threat", "menace", "challenge", "défi", "obstacle" };
        if (riskKeywords.Any(k => allContent.Contains(k, StringComparison.OrdinalIgnoreCase)))
        {
            score += 30;
        }

        // Mitigation strategies (+40)
        var mitigationKeywords = new[] { "mitigation", "atténuation", "contingency", "plan B", "backup", "alternative", "reduce", "réduire" };
        if (mitigationKeywords.Any(k => allContent.Contains(k, StringComparison.OrdinalIgnoreCase)))
        {
            score += 40;
        }

        // Financial safeguards (+30)
        var financialKeywords = new[] { "reserve", "réserve", "buffer", "cushion", "insurance", "assurance", "runway" };
        if (financialKeywords.Any(k => allContent.Contains(k, StringComparison.OrdinalIgnoreCase)))
        {
            score += 30;
        }

        return Math.Min(100, score);
    }

    private static decimal CalculateCompletenessScore(BusinessPlan plan)
    {
        var sections = new[]
        {
            plan.ExecutiveSummary,
            plan.BusinessModel,
            plan.MarketAnalysis,
            plan.FinancialProjections,
            plan.SwotAnalysis,
            plan.OperationsPlan
        };

        var completeSections = sections.Count(s => !string.IsNullOrWhiteSpace(s) && s.Length > 100);
        var totalSections = sections.Length;

        return (decimal)completeSections / totalSections * 100;
    }

    private static string DetermineReadinessLevel(decimal score)
    {
        return score switch
        {
            >= 85 => "BankReady",
            >= 70 => "Ready",
            >= 50 => "Developing",
            _ => "NotReady"
        };
    }

    private static List<string> GenerateRecommendations(BusinessPlan plan, decimal consistency, decimal riskMitigation, decimal completeness)
    {
        var recommendations = new List<string>();

        if (completeness < 80)
        {
            if (string.IsNullOrWhiteSpace(plan.ExecutiveSummary))
                recommendations.Add("Add an executive summary to provide a clear overview");
            if (string.IsNullOrWhiteSpace(plan.FinancialProjections))
                recommendations.Add("Include detailed financial projections");
            if (string.IsNullOrWhiteSpace(plan.MarketAnalysis))
                recommendations.Add("Add comprehensive market analysis");
            if (string.IsNullOrWhiteSpace(plan.BusinessModel))
                recommendations.Add("Describe your business model clearly");
        }

        if (consistency < 70)
        {
            recommendations.Add("Ensure financial projections are supported by market data");
            recommendations.Add("Update executive summary to reflect all plan sections");
        }

        if (riskMitigation < 60)
        {
            recommendations.Add("Identify key business risks and mitigation strategies");
            recommendations.Add("Include contingency plans for major threats");
            recommendations.Add("Add financial reserves or runway calculations");
        }

        return recommendations.Take(5).ToList();
    }

    private static void AnalyzeSection(string? content, string sectionName, List<SectionReadiness> sections, List<string> missingElements)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            sections.Add(new SectionReadiness
            {
                SectionName = sectionName,
                Score = 0,
                IsComplete = false,
                Issues = new List<string> { "Section is empty" },
                Strengths = new List<string>()
            });
            missingElements.Add(sectionName);
            return;
        }

        var issues = new List<string>();
        var strengths = new List<string>();
        decimal score = 50;

        // Length analysis
        if (content.Length < 200)
        {
            issues.Add("Content is too brief");
            score -= 15;
        }
        else if (content.Length > 500)
        {
            strengths.Add("Comprehensive content");
            score += 15;
        }

        // Structure analysis
        if (content.Contains("\n") && content.Split('\n').Length > 3)
        {
            strengths.Add("Well-structured with paragraphs");
            score += 10;
        }

        // Data analysis
        if (Regex.IsMatch(content, @"\d+"))
        {
            strengths.Add("Contains quantitative data");
            score += 10;
        }
        else
        {
            issues.Add("Lacks quantitative data");
        }

        sections.Add(new SectionReadiness
        {
            SectionName = sectionName,
            Score = Math.Min(100, Math.Max(0, score)),
            IsComplete = content.Length >= 100,
            Issues = issues,
            Strengths = strengths
        });
    }

    private static void CheckForInconsistencies(BusinessPlan plan, List<string> inconsistencies)
    {
        // Check revenue claims vs market size
        if (!string.IsNullOrWhiteSpace(plan.FinancialProjections) && !string.IsNullOrWhiteSpace(plan.MarketAnalysis))
        {
            var hasLargeRevenue = Regex.IsMatch(plan.FinancialProjections, @"\$\s*\d{1,3}(,\d{3})*\s*(million|M)");
            var hasSmallMarket = plan.MarketAnalysis.Contains("niche", StringComparison.OrdinalIgnoreCase);
            if (hasLargeRevenue && hasSmallMarket)
            {
                inconsistencies.Add("Revenue projections may be inconsistent with niche market positioning");
            }
        }
    }

    private static void IdentifyRiskGaps(BusinessPlan plan, List<string> riskGaps)
    {
        var allContent = string.Join(" ",
            plan.SwotAnalysis ?? "",
            plan.FinancialProjections ?? "",
            plan.MarketAnalysis ?? "");

        if (!allContent.Contains("competition", StringComparison.OrdinalIgnoreCase) &&
            !allContent.Contains("concurrent", StringComparison.OrdinalIgnoreCase))
        {
            riskGaps.Add("Competitive risk analysis is missing");
        }

        if (!allContent.Contains("regulatory", StringComparison.OrdinalIgnoreCase) &&
            !allContent.Contains("réglementaire", StringComparison.OrdinalIgnoreCase))
        {
            riskGaps.Add("Regulatory risk considerations not addressed");
        }

        if (!allContent.Contains("cash flow", StringComparison.OrdinalIgnoreCase) &&
            !allContent.Contains("flux de trésorerie", StringComparison.OrdinalIgnoreCase))
        {
            riskGaps.Add("Cash flow risk not explicitly addressed");
        }
    }
}

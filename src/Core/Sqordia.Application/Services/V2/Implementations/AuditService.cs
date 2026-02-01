using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.V2.Audit;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;
using System.Text;
using System.Text.Json;

namespace Sqordia.Application.Services.V2.Implementations;

/// <summary>
/// Socratic Coach audit service implementation
/// Generates Nudge + Triad for business plan sections
/// </summary>
public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IApplicationDbContext context,
        IAIService aiService,
        ICurrentUserService currentUserService,
        ILogger<AuditService> logger)
    {
        _context = context;
        _aiService = aiService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<AuditSectionResponse>> AuditSectionAsync(
        Guid businessPlanId,
        string sectionName,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting Socratic Coach audit for section {Section} of business plan {PlanId}",
                sectionName, businessPlanId);

            var businessPlan = await GetBusinessPlanWithAccessCheckAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<AuditSectionResponse>(
                    Error.NotFound("BusinessPlan.NotFound", "Business plan not found or access denied."));
            }

            var sectionContent = GetSectionContent(businessPlan, sectionName);
            if (string.IsNullOrWhiteSpace(sectionContent))
            {
                return Result.Failure<AuditSectionResponse>(
                    Error.Validation("Section.Empty", $"Section '{sectionName}' has no content to audit."));
            }

            var category = DetermineAuditCategory(sectionName);
            var context = BuildAuditContext(businessPlan, sectionName, sectionContent);

            var systemPrompt = BuildSocraticCoachPrompt(language, category);
            var userPrompt = $"Section: {sectionName}\n\nContent:\n{sectionContent}\n\nContext:\n{context}";

            var aiResponse = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt,
                userPrompt,
                maxTokens: 1500,
                temperature: 0.7f,
                cancellationToken: cancellationToken);

            var auditResult = ParseAuditResponse(aiResponse, businessPlanId, sectionName, category);

            _logger.LogInformation("Socratic Coach audit completed for section {Section}", sectionName);

            return Result.Success(auditResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing Socratic Coach audit for section {Section}", sectionName);
            return Result.Failure<AuditSectionResponse>(
                Error.InternalServerError("Audit.Error", "An error occurred during the audit."));
        }
    }

    public async Task<Result<AuditSummaryResponse>> GetAuditSummaryAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating audit summary for business plan {PlanId}", businessPlanId);

            var businessPlan = await GetBusinessPlanWithAccessCheckAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<AuditSummaryResponse>(
                    Error.NotFound("BusinessPlan.NotFound", "Business plan not found or access denied."));
            }

            var sections = new[] { "ExecutiveSummary", "MarketAnalysis", "FinancialProjections", "SwotAnalysis", "BusinessModel", "OperationsPlan" };
            var sectionSummaries = new List<SectionAuditSummary>();
            var criticalIssues = new List<string>();
            var recommendations = new List<string>();
            decimal totalScore = 0;
            int sectionCount = 0;

            foreach (var section in sections)
            {
                var content = GetSectionContent(businessPlan, section);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var score = CalculateSectionScore(content);
                    totalScore += score;
                    sectionCount++;

                    sectionSummaries.Add(new SectionAuditSummary
                    {
                        SectionName = section,
                        Score = score,
                        Status = score >= 70 ? "Good" : score >= 50 ? "Needs Improvement" : "Critical",
                        IssueCount = score < 70 ? 1 : 0
                    });

                    if (score < 50)
                    {
                        criticalIssues.Add(GetCriticalIssueMessage(section, language));
                    }
                    else if (score < 70)
                    {
                        recommendations.Add(GetRecommendationMessage(section, language));
                    }
                }
                else
                {
                    sectionSummaries.Add(new SectionAuditSummary
                    {
                        SectionName = section,
                        Score = 0,
                        Status = "Missing",
                        IssueCount = 1
                    });
                    criticalIssues.Add(language == "en"
                        ? $"Section '{section}' is missing or empty"
                        : $"La section '{section}' est manquante ou vide");
                }
            }

            var overallScore = sectionCount > 0 ? totalScore / sectionCount : 0;

            var response = new AuditSummaryResponse
            {
                BusinessPlanId = businessPlanId,
                OverallScore = Math.Round(overallScore, 2),
                Sections = sectionSummaries,
                CriticalIssues = criticalIssues,
                Recommendations = recommendations,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Audit summary generated with overall score {Score}", overallScore);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit summary for business plan {PlanId}", businessPlanId);
            return Result.Failure<AuditSummaryResponse>(
                Error.InternalServerError("Audit.SummaryError", "An error occurred generating the audit summary."));
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
                .ThenInclude(qr => qr.QuestionTemplate)
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

    private static string? GetSectionContent(BusinessPlan plan, string sectionName)
    {
        return sectionName.ToLower() switch
        {
            "executivesummary" => plan.ExecutiveSummary,
            "marketanalysis" => plan.MarketAnalysis,
            "financialprojections" => plan.FinancialProjections,
            "swotanalysis" => plan.SwotAnalysis,
            "businessmodel" => plan.BusinessModel,
            "operationsplan" => plan.OperationsPlan,
            _ => null
        };
    }

    private static AuditCategory DetermineAuditCategory(string sectionName)
    {
        return sectionName.ToLower() switch
        {
            "financialprojections" => AuditCategory.Financial,
            "executivesummary" or "marketanalysis" => AuditCategory.Strategic,
            "companydescription" or "operationalplan" => AuditCategory.Compliance,
            _ => AuditCategory.Strategic
        };
    }

    private static string BuildAuditContext(BusinessPlan plan, string sectionName, string content)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Plan Title: {plan.Title}");
        sb.AppendLine($"Plan Type: {plan.PlanType}");
        sb.AppendLine($"Current Section: {sectionName}");
        sb.AppendLine($"Content Length: {content.Length} characters");
        return sb.ToString();
    }

    private static string BuildSocraticCoachPrompt(string language, AuditCategory category)
    {
        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);

        return isFrench
            ? $@"Tu es un coach Socratique expert en plans d'affaires BDC. Tu analyses les sections de plans d'affaires et fournis:
1. UN 'Nudge' - une question Socratique incisive qui pousse l'entrepreneur à réfléchir plus profondément
2. Un 'Triad' - exactement 3 suggestions intelligentes et actionnables

Catégorie d'audit: {category}

Réponds UNIQUEMENT en JSON valide avec ce format exact:
{{
  ""nudge"": {{
    ""question"": ""La question Socratique"",
    ""context"": ""Pourquoi cette question est importante"",
    ""severity"": ""Info|Warning|Critical""
  }},
  ""triad"": [
    {{""label"": ""Option A"", ""text"": ""Suggestion détaillée 1"", ""action"": ""action_identifier_1""}},
    {{""label"": ""Option B"", ""text"": ""Suggestion détaillée 2"", ""action"": ""action_identifier_2""}},
    {{""label"": ""Option C"", ""text"": ""Suggestion détaillée 3"", ""action"": ""action_identifier_3""}}
  ]
}}"
            : $@"You are a Socratic Coach expert in BDC business plans. You analyze business plan sections and provide:
1. A 'Nudge' - an incisive Socratic question that pushes the entrepreneur to think deeper
2. A 'Triad' - exactly 3 smart, actionable suggestions

Audit category: {category}

Respond ONLY with valid JSON in this exact format:
{{
  ""nudge"": {{
    ""question"": ""The Socratic question"",
    ""context"": ""Why this question matters"",
    ""severity"": ""Info|Warning|Critical""
  }},
  ""triad"": [
    {{""label"": ""Option A"", ""text"": ""Detailed suggestion 1"", ""action"": ""action_identifier_1""}},
    {{""label"": ""Option B"", ""text"": ""Detailed suggestion 2"", ""action"": ""action_identifier_2""}},
    {{""label"": ""Option C"", ""text"": ""Detailed suggestion 3"", ""action"": ""action_identifier_3""}}
  ]
}}";
    }

    private static AuditSectionResponse ParseAuditResponse(string aiResponse, Guid businessPlanId, string sectionName, AuditCategory category)
    {
        try
        {
            // Try to extract JSON from the response
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                var nudge = parsed.GetProperty("nudge");
                var triad = parsed.GetProperty("triad");

                return new AuditSectionResponse
                {
                    BusinessPlanId = businessPlanId,
                    SectionName = sectionName,
                    CategoryBadge = category.ToString(),
                    Nudge = new NudgeResponse
                    {
                        Question = nudge.GetProperty("question").GetString() ?? "Review this section",
                        Context = nudge.TryGetProperty("context", out var ctx) ? ctx.GetString() : null,
                        Severity = nudge.TryGetProperty("severity", out var sev) ? sev.GetString() ?? "Info" : "Info"
                    },
                    Triad = triad.EnumerateArray().Select(t => new SmartSuggestion
                    {
                        Label = t.GetProperty("label").GetString() ?? "Suggestion",
                        Text = t.GetProperty("text").GetString() ?? "Consider improving this section",
                        Action = t.TryGetProperty("action", out var act) ? act.GetString() ?? "review" : "review"
                    }).ToList(),
                    GeneratedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception)
        {
            // Fallback if JSON parsing fails
        }

        // Return default response if parsing fails
        return new AuditSectionResponse
        {
            BusinessPlanId = businessPlanId,
            SectionName = sectionName,
            CategoryBadge = category.ToString(),
            Nudge = new NudgeResponse
            {
                Question = "What assumptions are you making about your target market?",
                Context = "Understanding your assumptions helps identify potential blind spots.",
                Severity = "Info"
            },
            Triad = new List<SmartSuggestion>
            {
                new() { Label = "Option A", Text = "Add more market data to support your claims", Action = "add_data" },
                new() { Label = "Option B", Text = "Include competitive analysis", Action = "add_competition" },
                new() { Label = "Option C", Text = "Validate with customer interviews", Action = "validate" }
            },
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static decimal CalculateSectionScore(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return 0;

        decimal score = 50; // Base score

        // Length bonus (up to 20 points)
        if (content.Length > 500) score += 10;
        if (content.Length > 1000) score += 10;

        // Structure bonus (up to 15 points)
        if (content.Contains("\n")) score += 5;
        if (content.Split('\n').Length > 5) score += 5;
        if (content.Contains("-") || content.Contains("•")) score += 5;

        // Content quality indicators (up to 15 points)
        var hasNumbers = System.Text.RegularExpressions.Regex.IsMatch(content, @"\d+");
        if (hasNumbers) score += 5;

        var hasPercentages = content.Contains("%");
        if (hasPercentages) score += 5;

        var hasCurrency = content.Contains("$") || content.Contains("€") || content.Contains("CAD");
        if (hasCurrency) score += 5;

        return Math.Min(100, score);
    }

    private static string GetCriticalIssueMessage(string section, string language)
    {
        return language == "en"
            ? $"Section '{section}' requires significant improvement for bank readiness"
            : $"La section '{section}' nécessite des améliorations significatives pour être prête pour la banque";
    }

    private static string GetRecommendationMessage(string section, string language)
    {
        return language == "en"
            ? $"Consider expanding section '{section}' with more details and supporting data"
            : $"Envisagez d'enrichir la section '{section}' avec plus de détails et de données à l'appui";
    }
}

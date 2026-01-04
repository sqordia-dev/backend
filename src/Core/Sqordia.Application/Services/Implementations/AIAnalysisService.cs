using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Responses.AI;
using Sqordia.Domain.Entities.BusinessPlan;
using System.Text;

namespace Sqordia.Application.Services.Implementations;

public class AIAnalysisService : IAIAnalysisService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AIAnalysisService> _logger;

    public AIAnalysisService(
        IApplicationDbContext context,
        IAIService aiService,
        ICurrentUserService currentUserService,
        ILogger<AIAnalysisService> logger)
    {
        _context = context;
        _aiService = aiService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<StrategySuggestionResponse> GenerateStrategySuggestionsAsync(
        StrategySuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating strategy suggestions for business plan {BusinessPlanId}", request.BusinessPlanId);

            // Get business plan with context
            var businessPlan = await GetBusinessPlanWithContextAsync(request.BusinessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                throw new ArgumentException($"Business plan {request.BusinessPlanId} not found or access denied");
            }

            // Build context string
            var context = BuildBusinessPlanContext(businessPlan);

            // Generate suggestions using AI
            var response = await _aiService.GenerateStrategySuggestionsAsync(request, context, cancellationToken);

            _logger.LogInformation("Strategy suggestions generated successfully for business plan {BusinessPlanId}", request.BusinessPlanId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating strategy suggestions for business plan {BusinessPlanId}", request.BusinessPlanId);
            throw;
        }
    }

    public async Task<RiskMitigationResponse> AnalyzeRisksAsync(
        RiskMitigationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing risks for business plan {BusinessPlanId}", request.BusinessPlanId);

            // Get business plan with context
            var businessPlan = await GetBusinessPlanWithContextAsync(request.BusinessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                throw new ArgumentException($"Business plan {request.BusinessPlanId} not found or access denied");
            }

            // Build context string
            var context = BuildBusinessPlanContext(businessPlan);

            // Analyze risks using AI
            var response = await _aiService.AnalyzeRisksAsync(request, context, cancellationToken);

            _logger.LogInformation("Risk analysis completed successfully for business plan {BusinessPlanId}", request.BusinessPlanId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing risks for business plan {BusinessPlanId}", request.BusinessPlanId);
            throw;
        }
    }

    public async Task<BusinessMentorResponse> PerformBusinessMentorAnalysisAsync(
        BusinessMentorRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing business mentor analysis for business plan {BusinessPlanId}", request.BusinessPlanId);

            // Get business plan with context
            var businessPlan = await GetBusinessPlanWithContextAsync(request.BusinessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                throw new ArgumentException($"Business plan {request.BusinessPlanId} not found or access denied");
            }

            // Build context string
            var context = BuildBusinessPlanContext(businessPlan);

            // Perform analysis using AI
            var response = await _aiService.PerformBusinessMentorAnalysisAsync(request, context, cancellationToken);

            _logger.LogInformation("Business mentor analysis completed successfully for business plan {BusinessPlanId}", request.BusinessPlanId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing business mentor analysis for business plan {BusinessPlanId}", request.BusinessPlanId);
            throw;
        }
    }

    private async Task<BusinessPlan?> GetBusinessPlanWithContextAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            return null;
        }

        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
            .Include(bp => bp.QuestionnaireResponses)
                .ThenInclude(qr => qr.QuestionTemplate)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null)
        {
            return null;
        }

        // Verify user has access
        var isMember = await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                           om.UserId == currentUserId &&
                           om.IsActive, cancellationToken);

        return isMember ? businessPlan : null;
    }

    private string BuildBusinessPlanContext(BusinessPlan businessPlan)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Plan Title: {businessPlan.Title}");
        sb.AppendLine($"Plan Type: {businessPlan.PlanType}");
        sb.AppendLine($"Status: {businessPlan.Status}");

        // Add questionnaire responses
        if (businessPlan.QuestionnaireResponses.Any())
        {
            sb.AppendLine("\nQuestionnaire Responses:");
            foreach (var response in businessPlan.QuestionnaireResponses.OrderBy(r => r.QuestionTemplate.Order))
            {
                sb.AppendLine($"Q: {response.QuestionTemplate.QuestionText}");
                sb.AppendLine($"A: {response.ResponseText}");
                if (!string.IsNullOrEmpty(response.AiInsights))
                {
                    sb.AppendLine($"AI Insights: {response.AiInsights}");
                }
                sb.AppendLine();
            }
        }

        // Add key sections
        if (!string.IsNullOrWhiteSpace(businessPlan.ExecutiveSummary))
        {
            sb.AppendLine($"Executive Summary: {businessPlan.ExecutiveSummary.Substring(0, Math.Min(500, businessPlan.ExecutiveSummary.Length))}");
        }

        if (!string.IsNullOrWhiteSpace(businessPlan.MarketAnalysis))
        {
            sb.AppendLine($"Market Analysis: {businessPlan.MarketAnalysis.Substring(0, Math.Min(500, businessPlan.MarketAnalysis.Length))}");
        }

        if (!string.IsNullOrWhiteSpace(businessPlan.FinancialProjections))
        {
            sb.AppendLine($"Financial Projections: {businessPlan.FinancialProjections.Substring(0, Math.Min(500, businessPlan.FinancialProjections.Length))}");
        }

        if (!string.IsNullOrWhiteSpace(businessPlan.SwotAnalysis))
        {
            sb.AppendLine($"SWOT Analysis: {businessPlan.SwotAnalysis.Substring(0, Math.Min(500, businessPlan.SwotAnalysis.Length))}");
        }

        return sb.ToString();
    }
}


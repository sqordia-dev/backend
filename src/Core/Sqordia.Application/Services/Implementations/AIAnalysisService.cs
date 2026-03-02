using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
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
    private readonly IAuthorizationHelper _authHelper;
    private readonly ILogger<AIAnalysisService> _logger;

    public AIAnalysisService(
        IApplicationDbContext context,
        IAIService aiService,
        IAuthorizationHelper authHelper,
        ILogger<AIAnalysisService> logger)
    {
        _context = context;
        _aiService = aiService;
        _authHelper = authHelper;
        _logger = logger;
    }

    public async Task<Result<StrategySuggestionResponse>> GenerateStrategySuggestionsAsync(
        StrategySuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating strategy suggestions for business plan {BusinessPlanId}", request.BusinessPlanId);

            // Verify access to business plan using authorization helper
            var authResult = await _authHelper.RequireBusinessPlanAccessAsync(request.BusinessPlanId, cancellationToken);
            if (!authResult.IsSuccess)
            {
                return Result.Failure<StrategySuggestionResponse>(authResult.Error);
            }

            // Get business plan with context
            var businessPlan = await GetBusinessPlanWithContextAsync(request.BusinessPlanId, authResult.Value.UserId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<StrategySuggestionResponse>(
                    Error.NotFound("BusinessPlan.NotFound", $"Business plan {request.BusinessPlanId} not found"));
            }

            // Build context string
            var context = BuildBusinessPlanContext(businessPlan);

            // Generate suggestions using AI
            var response = await _aiService.GenerateStrategySuggestionsAsync(request, context, cancellationToken);

            _logger.LogInformation("Strategy suggestions generated successfully for business plan {BusinessPlanId}", request.BusinessPlanId);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating strategy suggestions for business plan {BusinessPlanId}", request.BusinessPlanId);
            return Result.Failure<StrategySuggestionResponse>(
                Error.InternalServerError("AI.Analysis.Failed", "Failed to generate strategy suggestions"));
        }
    }

    public async Task<Result<RiskMitigationResponse>> AnalyzeRisksAsync(
        RiskMitigationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing risks for business plan {BusinessPlanId}", request.BusinessPlanId);

            // Verify access to business plan using authorization helper
            var authResult = await _authHelper.RequireBusinessPlanAccessAsync(request.BusinessPlanId, cancellationToken);
            if (!authResult.IsSuccess)
            {
                return Result.Failure<RiskMitigationResponse>(authResult.Error);
            }

            // Get business plan with context
            var businessPlan = await GetBusinessPlanWithContextAsync(request.BusinessPlanId, authResult.Value.UserId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<RiskMitigationResponse>(
                    Error.NotFound("BusinessPlan.NotFound", $"Business plan {request.BusinessPlanId} not found"));
            }

            // Build context string
            var context = BuildBusinessPlanContext(businessPlan);

            // Analyze risks using AI
            var response = await _aiService.AnalyzeRisksAsync(request, context, cancellationToken);

            _logger.LogInformation("Risk analysis completed successfully for business plan {BusinessPlanId}", request.BusinessPlanId);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing risks for business plan {BusinessPlanId}", request.BusinessPlanId);
            return Result.Failure<RiskMitigationResponse>(
                Error.InternalServerError("AI.Analysis.Failed", "Failed to analyze risks"));
        }
    }

    public async Task<Result<BusinessMentorResponse>> PerformBusinessMentorAnalysisAsync(
        BusinessMentorRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing business mentor analysis for business plan {BusinessPlanId}", request.BusinessPlanId);

            // Verify access to business plan using authorization helper
            var authResult = await _authHelper.RequireBusinessPlanAccessAsync(request.BusinessPlanId, cancellationToken);
            if (!authResult.IsSuccess)
            {
                return Result.Failure<BusinessMentorResponse>(authResult.Error);
            }

            // Get business plan with context
            var businessPlan = await GetBusinessPlanWithContextAsync(request.BusinessPlanId, authResult.Value.UserId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<BusinessMentorResponse>(
                    Error.NotFound("BusinessPlan.NotFound", $"Business plan {request.BusinessPlanId} not found"));
            }

            // Build context string
            var context = BuildBusinessPlanContext(businessPlan);

            // Perform analysis using AI
            var response = await _aiService.PerformBusinessMentorAnalysisAsync(request, context, cancellationToken);

            _logger.LogInformation("Business mentor analysis completed successfully for business plan {BusinessPlanId}", request.BusinessPlanId);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing business mentor analysis for business plan {BusinessPlanId}", request.BusinessPlanId);
            return Result.Failure<BusinessMentorResponse>(
                Error.InternalServerError("AI.Analysis.Failed", "Failed to perform business mentor analysis"));
        }
    }

    private async Task<BusinessPlan?> GetBusinessPlanWithContextAsync(Guid businessPlanId, Guid userId, CancellationToken cancellationToken)
    {
        // Authorization is already verified by the calling method via IAuthorizationHelper
        // This method just retrieves the business plan with its context data
        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
            .Include(bp => bp.QuestionnaireResponses)
                .ThenInclude(qr => qr.QuestionTemplate)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        return businessPlan;
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


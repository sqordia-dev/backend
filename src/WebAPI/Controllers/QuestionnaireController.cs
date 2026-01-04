using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Questionnaire;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/questionnaire")]
[Authorize]
public class QuestionnaireController : BaseApiController
{
    private readonly IQuestionnaireService _questionnaireService;
    private readonly IAIService _aiService;
    private readonly ILogger<QuestionnaireController> _logger;

    public QuestionnaireController(
        IQuestionnaireService questionnaireService, 
        IAIService aiService,
        ILogger<QuestionnaireController> logger)
    {
        _questionnaireService = questionnaireService;
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Get all questions for a business plan (based on plan type)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetQuestionnaire(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var result = await _questionnaireService.GetQuestionnaireAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Submit or update a response to a question
    /// </summary>
    [HttpPost("responses")]
    public async Task<IActionResult> SubmitResponse(Guid businessPlanId, [FromBody] SubmitQuestionnaireResponseRequest request, CancellationToken cancellationToken)
    {
        var result = await _questionnaireService.SubmitResponseAsync(businessPlanId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all responses for a business plan
    /// </summary>
    [HttpGet("responses")]
    public async Task<IActionResult> GetResponses(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var result = await _questionnaireService.GetResponsesAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Generate AI suggestions for a specific questionnaire question
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="questionId">The question ID</param>
    /// <param name="request">The question suggestion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated suggestions for the question</returns>
    /// <remarks>
    /// This endpoint uses AI to generate helpful suggestions for answering questionnaire questions.
    /// The suggestions are tailored to the business plan type and can include different approaches
    /// (detailed, concise, professional, creative) with confidence scores and reasoning.
    /// Supports both French and English questions and responses.
    /// 
    /// Prerequisites:
    /// - The business plan must exist
    /// - The user must have access to the business plan's organization
    /// - AI service must be configured and available
    /// 
    /// Sample request (English):
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/questionnaire/questions/1/suggest-answer
    ///     {
    ///         "questionText": "What is your target market?",
    ///         "planType": "BusinessPlan",
    ///         "existingResponse": "Small businesses",
    ///         "organizationContext": "Tech startup",
    ///         "suggestionCount": 3,
    ///         "language": "en"
    ///     }
    /// 
    /// Sample request (French):
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/questionnaire/questions/1/suggest-answer
    ///     {
    ///         "questionText": "Quel est votre marché cible?",
    ///         "planType": "BusinessPlan",
    ///         "existingResponse": "Petites entreprises",
    ///         "organizationContext": "Startup technologique",
    ///         "suggestionCount": 3,
    ///         "language": "fr"
    ///     }
    /// 
    /// Sample response (English):
    /// {
    ///     "questionText": "What is your target market?",
    ///     "planType": "BusinessPlan",
    ///     "suggestions": [
    ///         {
    ///             "answer": "Our target market consists of small to medium-sized businesses (SMBs) in the technology sector...",
    ///             "confidence": 0.85,
    ///             "reasoning": "This provides a detailed, professional response suitable for a business plan",
    ///             "suggestionType": "Detailed"
    ///         }
    ///     ],
    ///     "generatedAt": "2025-01-14T19:30:00Z",
    ///     "model": "gpt-4",
    ///     "language": "en"
    /// }
    /// 
    /// Sample response (French):
    /// {
    ///     "questionText": "Quel est votre marché cible?",
    ///     "planType": "BusinessPlan",
    ///     "suggestions": [
    ///         {
    ///             "answer": "Notre marché cible se compose de petites et moyennes entreprises (PME) du secteur technologique...",
    ///             "confidence": 0.85,
    ///             "reasoning": "Cette réponse fournit une réponse détaillée et professionnelle adaptée à un plan d'affaires",
    ///             "suggestionType": "Détaillé"
    ///         }
    ///     ],
    ///     "generatedAt": "2025-01-14T19:30:00Z",
    ///     "model": "gpt-4",
    ///     "language": "fr"
    /// }
    /// </remarks>
    /// <response code="200">Suggestions generated successfully</response>
    /// <response code="400">Invalid request or AI service unavailable</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("questions/{questionId}/suggest-answer")]
    [ProducesResponseType(typeof(QuestionSuggestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuggestAnswer(
        Guid businessPlanId,
        Guid questionId,
        [FromBody] QuestionSuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("SuggestAnswer called for plan {PlanId}, question {QuestionId}. Request: {@Request}", 
                businessPlanId, questionId, request);

            // Validate model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();
                _logger.LogWarning("Model validation failed for SuggestAnswer: {Errors}", string.Join(", ", errors));
                return BadRequest(new { error = "Invalid request", details = errors });
            }

            // Validate required fields manually as additional check
            if (string.IsNullOrWhiteSpace(request.QuestionText))
            {
                _logger.LogWarning("QuestionText is null or empty");
                return BadRequest(new { error = "QuestionText is required" });
            }

            if (request.QuestionText.Length < 10)
            {
                _logger.LogWarning("QuestionText is too short: {Length}", request.QuestionText.Length);
                return BadRequest(new { error = "QuestionText must be at least 10 characters long" });
            }

            if (string.IsNullOrWhiteSpace(request.PlanType))
            {
                _logger.LogWarning("PlanType is null or empty");
                return BadRequest(new { error = "PlanType is required" });
            }

            // Validate that the business plan exists and user has access
            var businessPlanResult = await _questionnaireService.GetQuestionnaireAsync(businessPlanId, cancellationToken);
            if (!businessPlanResult.IsSuccess)
            {
                return HandleResult(businessPlanResult);
            }

            // Get business plan to extract plan type if not provided
            if (string.IsNullOrWhiteSpace(request.PlanType))
            {
                // Try to get plan type from business plan
                // This is a fallback - frontend should provide it
                request.PlanType = "StrategicPlan"; // Default for OBNL
            }

            // Check if AI service is available
            if (_aiService == null)
            {
                _logger.LogWarning("AI service is not available");
                return BadRequest("AI service is not configured");
            }

            _logger.LogInformation("Checking AI service availability. Service type: {ServiceType}", _aiService.GetType().Name);
            var isAvailable = await _aiService.IsAvailableAsync(cancellationToken);
            _logger.LogInformation("AI service availability check result: {IsAvailable}", isAvailable);
            if (!isAvailable)
            {
                _logger.LogWarning("AI service unavailable when trying to generate suggestions for plan {PlanId}, question {QuestionId}. Service type: {ServiceType}", 
                    businessPlanId, questionId, _aiService?.GetType().Name ?? "NULL");
                return BadRequest(new { error = "AI service is currently unavailable. Please try again later." });
            }

            // Generate suggestions using AI
            var suggestions = await _aiService.GenerateQuestionSuggestionsAsync(request, cancellationToken);
            
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI suggestions for plan {PlanId}, question {QuestionId}", 
                businessPlanId, questionId);
            return BadRequest(new { error = ex.Message, details = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Get questionnaire progress for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Questionnaire progress information</returns>
    /// <remarks>
    /// Returns detailed progress information for a business plan questionnaire including:
    /// - Completion percentage and statistics
    /// - Answered and unanswered questions
    /// - Time estimates and milestones
    /// - Recent activity and next steps
    /// 
    /// Sample response:
    /// {
    ///   "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "businessPlanTitle": "My Startup Business Plan",
    ///   "totalQuestions": 20,
    ///   "completedQuestions": 12,
    ///   "remainingQuestions": 8,
    ///   "completionPercentage": 60.0,
    ///   "status": "In Progress",
    ///   "isComplete": false,
    ///   "startedAt": "2025-01-14T10:00:00Z",
    ///   "completedAt": null,
    ///   "estimatedTimeRemaining": 25,
    ///   "averageTimePerQuestion": 3.5,
    ///   "unansweredQuestionIds": ["q13", "q14", "q15", "q16", "q17", "q18", "q19", "q20"],
    ///   "recentAnswers": [
    ///     {
    ///       "questionId": "q12",
    ///       "questionText": "What is your competitive advantage?",
    ///       "answeredAt": "2025-01-14T11:30:00Z",
    ///       "answer": "Our proprietary technology and experienced team...",
    ///       "isAISuggested": false
    ///     }
    ///   ],
    ///   "milestonesAchieved": ["25% Complete", "50% Complete"],
    ///   "nextMilestone": "75% Complete",
    ///   "questionsToNextMilestone": 3
    /// }
    /// </remarks>
    /// <response code="200">Questionnaire progress information</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet("progress")]
    [ProducesResponseType(typeof(QuestionnaireProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgress(Guid businessPlanId, CancellationToken cancellationToken)
    {
        try
        {
            // Get the business plan to access progress information
            var businessPlanResult = await _questionnaireService.GetQuestionnaireAsync(businessPlanId, cancellationToken);
            if (!businessPlanResult.IsSuccess)
            {
                return HandleResult(businessPlanResult);
            }

            // For now, we'll create a mock response based on the business plan data
            // In a real implementation, this would come from the database
            var progress = new QuestionnaireProgressDto
            {
                BusinessPlanId = businessPlanId,
                BusinessPlanTitle = "Sample Business Plan", // This would come from the business plan
                TotalQuestions = 20,
                CompletedQuestions = 12,
                RemainingQuestions = 8,
                CompletionPercentage = 60.0m,
                Status = "In Progress",
                IsComplete = false,
                StartedAt = DateTime.UtcNow.AddHours(-2),
                CompletedAt = null,
                EstimatedTimeRemaining = 25,
                AverageTimePerQuestion = 3.5m,
                UnansweredQuestionIds = new List<string> { "q13", "q14", "q15", "q16", "q17", "q18", "q19", "q20" },
                RecentAnswers = new List<RecentAnswerDto>
                {
                    new RecentAnswerDto
                    {
                        QuestionId = "q12",
                        QuestionText = "What is your competitive advantage?",
                        AnsweredAt = DateTime.UtcNow.AddMinutes(-30),
                        Answer = "Our proprietary technology and experienced team...",
                        IsAISuggested = false
                    },
                    new RecentAnswerDto
                    {
                        QuestionId = "q11",
                        QuestionText = "What is your target market?",
                        AnsweredAt = DateTime.UtcNow.AddMinutes(-45),
                        Answer = "Small to medium-sized businesses in the technology sector...",
                        IsAISuggested = true
                    }
                },
                MilestonesAchieved = new List<string> { "25% Complete", "50% Complete" },
                NextMilestone = "75% Complete",
                QuestionsToNextMilestone = 3
            };

            return Ok(progress);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}


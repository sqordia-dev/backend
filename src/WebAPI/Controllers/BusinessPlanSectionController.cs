using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Requests.Sections;
using Sqordia.Contracts.Responses.BusinessPlan;
using Sqordia.Contracts.Responses.Sections;
using System.Diagnostics;
using WebAPI.Constants;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/sections")]
[Authorize]
public class BusinessPlanSectionController : BaseApiController
{
    private readonly ISectionService _sectionService;
    private readonly IAIService _aiService;
    private readonly ILogger<BusinessPlanSectionController> _logger;

    public BusinessPlanSectionController(
        ISectionService sectionService,
        IAIService aiService,
        ILogger<BusinessPlanSectionController> logger)
    {
        _sectionService = sectionService;
        _aiService = aiService;
        _logger = logger;
    }
    /// <summary>
    /// Get all sections for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all business plan sections</returns>
    /// <remarks>
    /// Returns all sections for a business plan with their content, metadata, and completion status.
    /// This endpoint provides a comprehensive view of all sections in the business plan.
    /// 
    /// Sample response:
    /// {
    ///   "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "businessPlanTitle": "My Startup Business Plan",
    ///   "planType": "BusinessPlan",
    ///   "sections": [
    ///     {
    ///       "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "sectionName": "executive-summary",
    ///       "title": "Executive Summary",
    ///       "content": "Our company provides innovative solutions...",
    ///       "hasContent": true,
    ///       "wordCount": 150,
    ///       "characterCount": 850,
    ///       "lastUpdated": "2025-01-14T10:30:00Z",
    ///       "lastUpdatedBy": "user@example.com",
    ///       "isRequired": true,
    ///       "order": 1,
    ///       "description": "Brief overview of the business",
    ///       "isAIGenerated": true,
    ///       "isManuallyEdited": false,
    ///       "status": "draft",
    ///       "tags": ["overview", "summary"]
    ///     }
    ///   ],
    ///   "totalSections": 15,
    ///   "sectionsWithContent": 8,
    ///   "completionPercentage": 53.3,
    ///   "lastUpdated": "2025-01-14T10:30:00Z"
    /// }
    /// </remarks>
    /// <response code="200">List of business plan sections</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(BusinessPlanSectionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSections(Guid businessPlanId, CancellationToken cancellationToken)
    {
        try
        {
            var sections = await _sectionService.GetSectionsAsync(businessPlanId, cancellationToken);
            return Ok(sections);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific section of a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionName">The section name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Specific business plan section</returns>
    /// <remarks>
    /// Returns a specific section of a business plan with its content and metadata.
    /// 
    /// Common section names:
    /// - executive-summary
    /// - market-analysis
    /// - competitive-analysis
    /// - business-model
    /// - marketing-strategy
    /// - operations-plan
    /// - management-team
    /// - financial-projections
    /// - funding-requirements
    /// - risk-analysis
    /// 
    /// Sample response:
    /// {
    ///   "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "sectionName": "executive-summary",
    ///   "title": "Executive Summary",
    ///   "content": "Our company provides innovative solutions...",
    ///   "hasContent": true,
    ///   "wordCount": 150,
    ///   "characterCount": 850,
    ///   "lastUpdated": "2025-01-14T10:30:00Z",
    ///   "lastUpdatedBy": "user@example.com",
    ///   "isRequired": true,
    ///   "order": 1,
    ///   "description": "Brief overview of the business",
    ///   "isAIGenerated": true,
    ///   "isManuallyEdited": false,
    ///   "status": "draft",
    ///   "tags": ["overview", "summary"]
    /// }
    /// </remarks>
    /// <response code="200">Business plan section</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan or section not found</response>
    [HttpGet("{sectionName}")]
    [ProducesResponseType(typeof(BusinessPlanSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSection(Guid businessPlanId, string sectionName, CancellationToken cancellationToken)
    {
        try
        {
            var section = await _sectionService.GetSectionAsync(businessPlanId, sectionName, cancellationToken);
            return Ok(section);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a specific section of a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionName">The section name</param>
    /// <param name="request">The section update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated business plan section</returns>
    /// <remarks>
    /// Updates a specific section of a business plan with new content.
    /// The section will be marked as manually edited if the user provides the content.
    /// 
    /// Sample request:
    /// {
    ///   "content": "Updated executive summary content...",
    ///   "isAIGenerated": false,
    ///   "isManualEdit": true,
    ///   "status": "review",
    ///   "tags": ["updated", "review"],
    ///   "notes": "Updated based on feedback"
    /// }
    /// </remarks>
    /// <response code="200">Updated business plan section</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan or section not found</response>
    [HttpPut("{sectionName}")]
    [ProducesResponseType(typeof(BusinessPlanSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSection(
        Guid businessPlanId, 
        string sectionName, 
        [FromBody] UpdateSectionRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedSection = await _sectionService.UpdateSectionAsync(businessPlanId, sectionName, request, cancellationToken);
            return Ok(updatedSection);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// AI-assisted content modification for a section
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionId">The section ID or name</param>
    /// <param name="request">The AI assist request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-modified content</returns>
    /// <remarks>
    /// Uses AI to improve, expand, or shorten section content.
    ///
    /// Available actions:
    /// - **improve**: Enhance clarity, professionalism, and persuasiveness
    /// - **expand**: Add more detail, examples, and depth
    /// - **shorten**: Condense while preserving key information
    ///
    /// Sample request:
    ///     POST /api/v1/business-plans/{planId}/sections/{sectionId}/ai-assist
    ///     {
    ///         "action": "improve",
    ///         "currentContent": "Our company makes software...",
    ///         "instructions": "Make it more compelling for investors",
    ///         "language": "en"
    ///     }
    ///
    /// Sample response:
    ///     {
    ///         "content": "Our innovative software company delivers cutting-edge solutions...",
    ///         "action": "improve",
    ///         "originalWordCount": 5,
    ///         "newWordCount": 12,
    ///         "processingTimeMs": 1250,
    ///         "model": "gpt-4"
    ///     }
    /// </remarks>
    /// <response code="200">AI-modified content returned successfully</response>
    /// <response code="400">Invalid request or AI service unavailable</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan or section not found</response>
    [HttpPost("{sectionId}/ai-assist")]
    [ProducesResponseType(typeof(AiAssistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AiAssist(
        Guid businessPlanId,
        string sectionId,
        [FromBody] AiAssistRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "AI assist requested for business plan {BusinessPlanId}, section {SectionId}, action {Action}",
            businessPlanId, sectionId, request.Action);

        // Check AI service availability
        if (!await _aiService.IsAvailableAsync(cancellationToken))
        {
            return BadRequest(new { error = ControllerConstants.ErrorAiServiceUnavailable });
        }

        // Validate action
        var validActions = new[] { "improve", "expand", "shorten" };
        if (!validActions.Contains(request.Action.ToLowerInvariant()))
        {
            return BadRequest(new { error = "Action must be 'improve', 'expand', or 'shorten'" });
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Build the AI prompt based on action
            var systemPrompt = BuildSystemPrompt(request.Action, request.Language);
            var userPrompt = BuildUserPrompt(request.Action, request.CurrentContent, request.Instructions);

            // Generate content using AI service
            var generatedContent = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt,
                userPrompt,
                maxTokens: request.Action == "shorten" ? 1000 : 2000,
                temperature: 0.7f,
                maxRetries: 2,
                cancellationToken: cancellationToken);

            stopwatch.Stop();

            var response = new AiAssistResponse
            {
                Content = generatedContent,
                Action = request.Action,
                OriginalWordCount = CountWords(request.CurrentContent),
                NewWordCount = CountWords(generatedContent),
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Model = "ai-service" // The actual model is determined by the AI service configuration
            };

            _logger.LogInformation(
                "AI assist completed for business plan {BusinessPlanId}, section {SectionId}. " +
                "Original words: {OriginalWords}, New words: {NewWords}, Time: {TimeMs}ms",
                businessPlanId, sectionId, response.OriginalWordCount, response.NewWordCount, response.ProcessingTimeMs);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI assist failed for business plan {BusinessPlanId}, section {SectionId}",
                businessPlanId, sectionId);
            return BadRequest(new { error = "AI content generation failed. Please try again." });
        }
    }

    private static string BuildSystemPrompt(string action, string language)
    {
        var languageInstructions = language == "fr"
            ? "Respond entirely in French. Use professional French business terminology."
            : "Respond entirely in English. Use professional business terminology.";

        return action.ToLowerInvariant() switch
        {
            "improve" => $@"You are an expert business plan writer. Your task is to improve the given content to make it more professional, clear, compelling, and persuasive.
Maintain the original meaning and key points while enhancing the writing quality.
{languageInstructions}
Do not add any explanations or meta-commentary. Return only the improved content.",

            "expand" => $@"You are an expert business plan writer. Your task is to expand the given content by adding more detail, examples, supporting data, and depth.
Maintain the original tone and style while making the content more comprehensive.
{languageInstructions}
Do not add any explanations or meta-commentary. Return only the expanded content.",

            "shorten" => $@"You are an expert business plan writer. Your task is to condense the given content while preserving all key information and main points.
Remove redundancy, simplify complex sentences, and make the content more concise.
{languageInstructions}
Do not add any explanations or meta-commentary. Return only the shortened content.",

            _ => $@"You are an expert business plan writer. Improve the given content.
{languageInstructions}"
        };
    }

    private static string BuildUserPrompt(string action, string currentContent, string? instructions)
    {
        var basePrompt = $"Please {action} the following content:\n\n{currentContent}";

        if (!string.IsNullOrWhiteSpace(instructions))
        {
            basePrompt += $"\n\nAdditional instructions: {instructions}";
        }

        return basePrompt;
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

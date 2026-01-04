using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Sections;
using Sqordia.Contracts.Responses.Sections;
using WebAPI.Constants;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/sections/{sectionName}")]
[Authorize]
public class SectionImprovementController : BaseApiController
{
    private readonly IAIService _aiService;

    public SectionImprovementController(IAIService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// Improve a business plan section using AI
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionName">The section name to improve</param>
    /// <param name="request">The section improvement request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-improved section content</returns>
    /// <remarks>
    /// This endpoint uses AI to improve business plan section content for better clarity, professionalism, and persuasiveness.
    /// The improved content is returned for review before being saved.
    /// Supports both French and English content improvement.
    /// 
    /// Prerequisites:
    /// - The business plan must exist
    /// - The user must have access to the business plan's organization
    /// - AI service must be configured and available
    /// 
    /// Sample request (English):
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/sections/executive-summary/improve
    ///     {
    ///         "currentContent": "Our company provides innovative solutions...",
    ///         "improvementType": "improve",
    ///         "language": "en",
    ///         "planType": "BusinessPlan",
    ///         "instructions": "Make it more compelling for investors",
    ///         "targetAudience": "investors",
    ///         "industryContext": "Technology startup",
    ///         "tone": "professional"
    ///     }
    /// 
    /// Sample request (French):
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/sections/executive-summary/improve
    ///     {
    ///         "currentContent": "Notre entreprise fournit des solutions innovantes...",
    ///         "improvementType": "improve",
    ///         "language": "fr",
    ///         "planType": "BusinessPlan",
    ///         "instructions": "Rendez-le plus convaincant pour les investisseurs",
    ///         "targetAudience": "investisseurs",
    ///         "industryContext": "Startup technologique",
    ///         "tone": "professionnel"
    ///     }
    /// </remarks>
    /// <response code="200">Section improvement generated successfully</response>
    /// <response code="400">Invalid request or AI service unavailable</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("improve")]
    [ProducesResponseType(typeof(SectionImprovementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImproveSection(
        Guid businessPlanId,
        string sectionName,
        [FromBody] SectionImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _aiService.IsAvailableAsync(cancellationToken))
        {
            return BadRequest(new { error = ControllerConstants.ErrorAiServiceUnavailable });
        }

        var improvement = await _aiService.ImproveSectionAsync(request, cancellationToken);
        return Ok(improvement);
    }

    /// <summary>
    /// Expand a business plan section using AI
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionName">The section name to expand</param>
    /// <param name="request">The section improvement request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-expanded section content</returns>
    /// <remarks>
    /// This endpoint uses AI to expand business plan section content by adding details, examples, and subsections.
    /// The expanded content is returned for review before being saved.
    /// Supports both French and English content expansion.
    /// 
    /// Sample request (English):
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/sections/market-analysis/expand
    ///     {
    ///         "currentContent": "Our target market consists of small businesses...",
    ///         "improvementType": "expand",
    ///         "language": "en",
    ///         "planType": "BusinessPlan",
    ///         "instructions": "Add market research data and competitive analysis",
    ///         "targetAudience": "investors",
    ///         "industryContext": "SaaS technology"
    ///     }
    /// </remarks>
    /// <response code="200">Section expansion generated successfully</response>
    /// <response code="400">Invalid request or AI service unavailable</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("expand")]
    [ProducesResponseType(typeof(SectionExpansionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExpandSection(
        Guid businessPlanId,
        string sectionName,
        [FromBody] SectionImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _aiService.IsAvailableAsync(cancellationToken))
        {
            return BadRequest(new { error = ControllerConstants.ErrorAiServiceUnavailable });
        }

        var expansion = await _aiService.ExpandSectionAsync(request, cancellationToken);
        return Ok(expansion);
    }

    /// <summary>
    /// Simplify a business plan section using AI
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionName">The section name to simplify</param>
    /// <param name="request">The section improvement request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-simplified section content</returns>
    /// <remarks>
    /// This endpoint uses AI to simplify business plan section content for better readability and accessibility.
    /// The simplified content is returned for review before being saved.
    /// Supports both French and English content simplification.
    /// 
    /// Sample request (English):
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/sections/technical-specifications/simplify
    ///     {
    ///         "currentContent": "Our proprietary algorithm utilizes advanced machine learning techniques...",
    ///         "improvementType": "simplify",
    ///         "language": "en",
    ///         "planType": "BusinessPlan",
    ///         "instructions": "Make it accessible to non-technical stakeholders",
    ///         "targetAudience": "general public"
    ///     }
    /// </remarks>
    /// <response code="200">Section simplification generated successfully</response>
    /// <response code="400">Invalid request or AI service unavailable</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("simplify")]
    [ProducesResponseType(typeof(SectionSimplificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SimplifySection(
        Guid businessPlanId,
        string sectionName,
        [FromBody] SectionImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _aiService.IsAvailableAsync(cancellationToken))
        {
            return BadRequest(new { error = ControllerConstants.ErrorAiServiceUnavailable });
        }

        var simplification = await _aiService.SimplifySectionAsync(request, cancellationToken);
        return Ok(simplification);
    }
}

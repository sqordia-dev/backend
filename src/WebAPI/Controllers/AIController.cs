using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V2;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Requests.V2.Questionnaire;

namespace WebAPI.Controllers;

/// <summary>
/// AI endpoints for text enhancement and analysis
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai")]
[Authorize]
public class AIController : BaseApiController
{
    private readonly IQuestionPolishService _polishService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AIController> _logger;

    public AIController(
        IQuestionPolishService polishService,
        IAuditService auditService,
        ILogger<AIController> logger)
    {
        _polishService = polishService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Polish/enhance text using AI (v1 compatibility endpoint)
    /// Transforms raw notes into professional, BDC-standard prose
    /// </summary>
    /// <param name="request">Text polish request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Polished text</returns>
    [HttpPost("polish-text")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PolishText(
        [FromBody] PolishTextRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Polishing text with {Length} characters", request.Text.Length);

        var result = await _polishService.PolishTextAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Analyze a business plan section for gaps (Socratic Coach) (v1 compatibility endpoint)
    /// Proxies to V2 audit service
    /// </summary>
    /// <param name="planId">The business plan ID (from query parameter)</param>
    /// <param name="request">Section analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audit issues with Options A/B/C</returns>
    [HttpPost("analyze-section")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalyzeSection(
        [FromQuery] Guid planId,
        [FromBody] AnalyzeSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (planId == Guid.Empty)
        {
            return BadRequest(new { error = "PlanId query parameter is required" });
        }

        _logger.LogInformation("Analyzing section {Section} for plan {PlanId}", request.SectionName, planId);

        var result = await _auditService.AuditSectionAsync(planId, request.SectionName, request.Language, cancellationToken);
        return HandleResult(result);
    }
}

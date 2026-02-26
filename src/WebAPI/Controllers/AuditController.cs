using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V2;

namespace WebAPI.Controllers;

/// <summary>
/// Socratic Coach audit endpoints
/// Provides AI-powered business plan auditing with Nudge + Triad responses
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/audit")]
[Authorize]
public class AuditController : BaseApiController
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditService auditService,
        ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Perform Socratic Coach audit on a business plan section
    /// Returns a Nudge (probing question) and Triad (3 smart suggestions)
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="section">Section name: ExecutiveSummary, MarketAnalysis, FinancialProjections, SwotAnalysis, BusinessModel, OperationsPlan</param>
    /// <param name="language">Language code: fr (default) or en</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AuditSection(
        Guid businessPlanId,
        [FromQuery] string section,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(section))
        {
            return BadRequest(new { error = "Section parameter is required" });
        }

        _logger.LogInformation("Performing Socratic Coach audit on section {Section} for plan {PlanId}",
            section, businessPlanId);

        var result = await _auditService.AuditSectionAsync(businessPlanId, section, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get audit summary for entire business plan
    /// Provides an overview of all sections with scores and recommendations
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="language">Language code: fr (default) or en</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditSummary(
        Guid businessPlanId,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting audit summary for plan {PlanId}", businessPlanId);

        var result = await _auditService.GetAuditSummaryAsync(businessPlanId, language, cancellationToken);
        return HandleResult(result);
    }
}

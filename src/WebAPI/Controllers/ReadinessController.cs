using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V2;

namespace WebAPI.Controllers;

/// <summary>
/// Bank-readiness score endpoints
/// Calculates weighted readiness: 50% Consistency, 30% Risk Mitigation, 20% Completeness
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/readiness")]
[Authorize]
public class ReadinessController : BaseApiController
{
    private readonly IReadinessScoreService _readinessService;
    private readonly ILogger<ReadinessController> _logger;

    public ReadinessController(
        IReadinessScoreService readinessService,
        ILogger<ReadinessController> logger)
    {
        _readinessService = readinessService;
        _logger = logger;
    }

    /// <summary>
    /// Calculate the bank-readiness score for a business plan
    /// Returns overall score and component scores (Consistency, Risk Mitigation, Completeness)
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReadinessScore(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating readiness score for plan {PlanId}", businessPlanId);

        var result = await _readinessService.CalculateReadinessScoreAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get detailed readiness breakdown by section
    /// Shows per-section scores, missing elements, and risk gaps
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("breakdown")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReadinessBreakdown(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting readiness breakdown for plan {PlanId}", businessPlanId);

        var result = await _readinessService.GetReadinessBreakdownAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Recalculate and persist the readiness score
    /// Use this after making changes to ensure the stored score is up-to-date
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("recalculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecalculateReadiness(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recalculating and saving readiness score for plan {PlanId}", businessPlanId);

        var result = await _readinessService.RecalculateAndSaveAsync(businessPlanId, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(new { readinessScore = result.Value });
        }

        return HandleResult(result);
    }
}

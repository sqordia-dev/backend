using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V2;
using Sqordia.Contracts.Requests.V2.StrategyMap;

namespace WebAPI.Controllers.V2;

/// <summary>
/// Strategy map management endpoints
/// Handles saving/retrieving strategy maps and financial health metrics
/// </summary>
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/strategy-map")]
[Authorize]
public class StrategyMapController : BaseApiController
{
    private readonly IStrategyMapService _strategyMapService;
    private readonly ILogger<StrategyMapController> _logger;

    public StrategyMapController(
        IStrategyMapService strategyMapService,
        ILogger<StrategyMapController> logger)
    {
        _strategyMapService = strategyMapService;
        _logger = logger;
    }

    /// <summary>
    /// Save strategy map for a business plan
    /// Optionally triggers recalculation of financial projections and readiness score
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="request">Strategy map data (React Flow JSON)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveStrategyMap(
        Guid businessPlanId,
        [FromBody] SaveStrategyMapRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Saving strategy map for plan {PlanId}, triggerRecalculation: {Trigger}",
            businessPlanId, request.TriggerRecalculation);

        var result = await _strategyMapService.SaveStrategyMapAsync(businessPlanId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the current strategy map for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStrategyMap(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting strategy map for plan {PlanId}", businessPlanId);

        var result = await _strategyMapService.GetStrategyMapAsync(businessPlanId, cancellationToken);
        return HandleResult(result);
    }
}

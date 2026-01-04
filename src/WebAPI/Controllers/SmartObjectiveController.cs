using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.SmartObjective;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/smart-objectives")]
[Authorize]
public class SmartObjectiveController : BaseApiController
{
    private readonly ISmartObjectiveService _smartObjectiveService;
    private readonly ILogger<SmartObjectiveController> _logger;

    public SmartObjectiveController(
        ISmartObjectiveService smartObjectiveService,
        ILogger<SmartObjectiveController> logger)
    {
        _smartObjectiveService = smartObjectiveService;
        _logger = logger;
    }

    /// <summary>
    /// Generate SMART objectives for a business plan
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateSmartObjectives(
        Guid businessPlanId,
        [FromBody] GenerateSmartObjectivesRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate SMART objectives request for business plan {BusinessPlanId}", businessPlanId);

        request.BusinessPlanId = businessPlanId;
        var result = await _smartObjectiveService.GenerateSmartObjectivesAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get all SMART objectives for a business plan
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetObjectives(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var result = await _smartObjectiveService.GetObjectivesAsync(businessPlanId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update objective progress
    /// </summary>
    [HttpPut("{objectiveId}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProgress(
        Guid businessPlanId,
        Guid objectiveId,
        [FromBody] decimal progressPercentage,
        CancellationToken cancellationToken = default)
    {
        var result = await _smartObjectiveService.UpdateProgressAsync(objectiveId, progressPercentage, cancellationToken);
        return Ok(result);
    }
}


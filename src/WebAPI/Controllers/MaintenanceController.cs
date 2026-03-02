using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Maintenance;
using Sqordia.Contracts.Responses.Maintenance;
using WebAPI.Attributes;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for managing maintenance mode status.
/// Provides endpoints for CI/CD pipelines and admin users to control maintenance windows.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class MaintenanceController : BaseApiController
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ILogger<MaintenanceController> _logger;

    public MaintenanceController(
        IMaintenanceService maintenanceService,
        ILogger<MaintenanceController> logger)
    {
        _maintenanceService = maintenanceService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current maintenance mode status.
    /// This endpoint is public and always accessible, even during maintenance.
    /// </summary>
    /// <returns>Current maintenance status including progress and ETA.</returns>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MaintenanceStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var result = await _maintenanceService.GetStatusAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Enable maintenance mode. Requires Admin role.
    /// </summary>
    /// <param name="request">Maintenance settings including reason, ETA, and admin bypass option.</param>
    /// <returns>Success or error result.</returns>
    [HttpPost("enable")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Enable([FromBody] UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogWarning("Maintenance mode enabled by admin user: {User}", User.Identity?.Name);

        request.IsEnabled = true; // Ensure it's enabled
        var result = await _maintenanceService.EnableMaintenanceAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Disable maintenance mode. Requires Admin role.
    /// </summary>
    /// <returns>Success or error result.</returns>
    [HttpPost("disable")]
    [Authorize(Roles = "Admin,Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Disable(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Maintenance mode disabled by admin user: {User}", User.Identity?.Name);

        var result = await _maintenanceService.DisableMaintenanceAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Enable maintenance mode via CI/CD pipeline. Requires deployment API key.
    /// </summary>
    /// <param name="request">Maintenance settings including deployment ID.</param>
    /// <returns>Success or error result.</returns>
    [HttpPost("ci/enable")]
    [DeploymentApiKey]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CiEnable([FromBody] UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogWarning("Maintenance mode enabled by CI/CD pipeline. DeploymentId: {DeploymentId}", request.DeploymentId);

        request.IsEnabled = true;
        var result = await _maintenanceService.EnableMaintenanceAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Disable maintenance mode via CI/CD pipeline. Requires deployment API key.
    /// </summary>
    /// <returns>Success or error result.</returns>
    [HttpPost("ci/disable")]
    [DeploymentApiKey]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CiDisable(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Maintenance mode disabled by CI/CD pipeline");

        var result = await _maintenanceService.DisableMaintenanceAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update deployment progress during CI/CD pipeline execution.
    /// Requires deployment API key.
    /// </summary>
    /// <param name="request">Progress update including percentage and current step.</param>
    /// <returns>Success or error result.</returns>
    [HttpPost("ci/progress")]
    [DeploymentApiKey]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateDeploymentProgressRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _maintenanceService.UpdateProgressAsync(request, cancellationToken);
        return HandleResult(result);
    }
}

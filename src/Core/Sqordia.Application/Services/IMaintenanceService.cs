using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Maintenance;
using Sqordia.Contracts.Responses.Maintenance;

namespace Sqordia.Application.Services;

/// <summary>
/// Service interface for managing maintenance mode status.
/// Used to control application availability during deployments and scheduled maintenance.
/// </summary>
public interface IMaintenanceService
{
    /// <summary>
    /// Get the current maintenance mode status.
    /// </summary>
    Task<Result<MaintenanceStatusResponse>> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable maintenance mode with the specified settings.
    /// </summary>
    Task<Result> EnableMaintenanceAsync(UpdateMaintenanceStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disable maintenance mode.
    /// </summary>
    Task<Result> DisableMaintenanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the deployment progress during CI/CD pipeline execution.
    /// </summary>
    Task<Result> UpdateProgressAsync(UpdateDeploymentProgressRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if maintenance mode is currently enabled.
    /// </summary>
    Task<Result<bool>> IsInMaintenanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check and auto-disable maintenance mode if the timeout has been reached.
    /// Called by background service.
    /// </summary>
    Task<Result> CheckAndAutoDisableAsync(CancellationToken cancellationToken = default);
}

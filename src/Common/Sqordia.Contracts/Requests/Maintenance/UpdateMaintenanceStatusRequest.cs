namespace Sqordia.Contracts.Requests.Maintenance;

/// <summary>
/// Request to enable or update maintenance mode status.
/// </summary>
public class UpdateMaintenanceStatusRequest
{
    /// <summary>
    /// Whether maintenance mode is enabled.
    /// </summary>
    public required bool IsEnabled { get; set; }

    /// <summary>
    /// Reason for the maintenance (e.g., "Deploying new version", "Database migration").
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Estimated end time for the maintenance window.
    /// </summary>
    public DateTime? EstimatedEnd { get; set; }

    /// <summary>
    /// Current deployment progress percentage (0-100).
    /// </summary>
    public int? ProgressPercent { get; set; }

    /// <summary>
    /// Current step in the deployment process (e.g., "Building application", "Running tests").
    /// </summary>
    public string? CurrentStep { get; set; }

    /// <summary>
    /// Unique identifier for the deployment (e.g., GitHub run ID).
    /// </summary>
    public string? DeploymentId { get; set; }

    /// <summary>
    /// Type of maintenance: Deployment, Scheduled, Emergency, DatabaseMigration.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Whether admins can bypass maintenance mode and access the application.
    /// </summary>
    public bool AllowAdminAccess { get; set; } = true;

    /// <summary>
    /// Number of minutes after which maintenance mode should auto-disable.
    /// Set to prevent stuck maintenance state if deployment fails.
    /// </summary>
    public int? AutoDisableMinutes { get; set; }
}

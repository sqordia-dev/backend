namespace Sqordia.Contracts.Responses.Maintenance;

/// <summary>
/// Response containing the current maintenance mode status.
/// </summary>
public class MaintenanceStatusResponse
{
    /// <summary>
    /// Whether maintenance mode is currently enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Reason for the maintenance.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// When the maintenance started.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Estimated end time for the maintenance window.
    /// </summary>
    public DateTime? EstimatedEnd { get; set; }

    /// <summary>
    /// Current deployment progress percentage (0-100).
    /// </summary>
    public int ProgressPercent { get; set; }

    /// <summary>
    /// Current step in the deployment process.
    /// </summary>
    public string? CurrentStep { get; set; }

    /// <summary>
    /// Unique identifier for the deployment.
    /// </summary>
    public string? DeploymentId { get; set; }

    /// <summary>
    /// Type of maintenance: Deployment, Scheduled, Emergency, DatabaseMigration.
    /// </summary>
    public string Type { get; set; } = "Deployment";

    /// <summary>
    /// Whether admins can bypass maintenance mode.
    /// </summary>
    public bool AllowAdminAccess { get; set; } = true;

    /// <summary>
    /// When maintenance mode will auto-disable if not manually disabled.
    /// </summary>
    public DateTime? AutoDisableAt { get; set; }

    /// <summary>
    /// Localized content for the maintenance page.
    /// </summary>
    public MaintenanceContentResponse Content { get; set; } = new();
}

/// <summary>
/// Localized content for the maintenance page.
/// </summary>
public class MaintenanceContentResponse
{
    public MaintenanceLocalizedContent En { get; set; } = new();
    public MaintenanceLocalizedContent Fr { get; set; } = new();
}

/// <summary>
/// Localized strings for a specific language.
/// </summary>
public class MaintenanceLocalizedContent
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

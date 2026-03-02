namespace Sqordia.Contracts.Requests.Maintenance;

/// <summary>
/// Request to update deployment progress during CI/CD pipeline execution.
/// </summary>
public class UpdateDeploymentProgressRequest
{
    /// <summary>
    /// Unique identifier for the deployment (e.g., GitHub run ID).
    /// </summary>
    public required string DeploymentId { get; set; }

    /// <summary>
    /// Current deployment progress percentage (0-100).
    /// </summary>
    public required int ProgressPercent { get; set; }

    /// <summary>
    /// Current step in the deployment process.
    /// </summary>
    public required string CurrentStep { get; set; }

    /// <summary>
    /// Optional updated estimated end time.
    /// </summary>
    public DateTime? EstimatedEnd { get; set; }
}

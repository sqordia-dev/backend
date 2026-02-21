namespace Sqordia.Contracts.Requests.Cms;

/// <summary>
/// Request to restore a version (creates a new draft based on the specified version)
/// </summary>
public class RestoreVersionRequest
{
    /// <summary>
    /// Optional notes for the restored version
    /// </summary>
    public string? Notes { get; set; }
}

namespace Sqordia.Contracts.Responses.Privacy;

/// <summary>
/// Response containing user's consent status (Quebec Bill 25 compliance)
/// </summary>
public class ConsentStatusResponse
{
    public List<ConsentItem> Consents { get; set; } = new();
}

/// <summary>
/// Status of a single consent type
/// </summary>
public class ConsentItem
{
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// True if a newer version is available that needs acceptance
    /// </summary>
    public bool RequiresUpdate { get; set; }

    /// <summary>
    /// The latest available version
    /// </summary>
    public string? LatestVersion { get; set; }
}

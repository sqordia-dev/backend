namespace Sqordia.Contracts.Responses.V2.Share;

/// <summary>
/// Response for a created vault share
/// </summary>
public class VaultShareResponse
{
    public Guid ShareId { get; set; }
    public Guid BusinessPlanId { get; set; }

    /// <summary>
    /// The full shareable URL
    /// </summary>
    public required string ShareUrl { get; set; }

    /// <summary>
    /// The unique share token
    /// </summary>
    public required string Token { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public bool EnableWatermark { get; set; }
    public string? WatermarkText { get; set; }
    public bool AllowDownload { get; set; }
    public bool TrackViews { get; set; }
    public bool RequireEmailVerification { get; set; }
    public bool HasPassword { get; set; }
    public int? MaxViews { get; set; }
    public int CurrentViews { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Analytics for a vault share
/// </summary>
public class VaultShareAnalyticsResponse
{
    public Guid ShareId { get; set; }
    public int TotalViews { get; set; }
    public int UniqueViewers { get; set; }
    public DateTime? LastViewedAt { get; set; }
    public required List<ViewerActivity> RecentActivity { get; set; }
}

/// <summary>
/// Individual viewer activity record
/// </summary>
public class ViewerActivity
{
    public string? ViewerEmail { get; set; }
    public string? ViewerName { get; set; }
    public DateTime ViewedAt { get; set; }
    public string? Location { get; set; }
    public string? Device { get; set; }
    public int DurationSeconds { get; set; }
}

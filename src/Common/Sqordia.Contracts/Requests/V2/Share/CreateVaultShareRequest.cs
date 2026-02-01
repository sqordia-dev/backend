namespace Sqordia.Contracts.Requests.V2.Share;

/// <summary>
/// Request to create a secure vault share link
/// </summary>
public class CreateVaultShareRequest
{
    /// <summary>
    /// When the share link expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Enable watermark on the shared document
    /// </summary>
    public bool EnableWatermark { get; set; } = true;

    /// <summary>
    /// Custom watermark text (defaults to viewer email/name)
    /// </summary>
    public string? WatermarkText { get; set; }

    /// <summary>
    /// Allow PDF/document download
    /// </summary>
    public bool AllowDownload { get; set; } = false;

    /// <summary>
    /// Track view analytics
    /// </summary>
    public bool TrackViews { get; set; } = true;

    /// <summary>
    /// Require email verification to view
    /// </summary>
    public bool RequireEmailVerification { get; set; } = false;

    /// <summary>
    /// Optional password protection
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Maximum number of views allowed (null = unlimited)
    /// </summary>
    public int? MaxViews { get; set; }
}

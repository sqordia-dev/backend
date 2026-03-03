using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Responses.Settings;

/// <summary>
/// Detailed response for a feature flag including metadata and audit information.
/// </summary>
public class FeatureFlagResponse
{
    /// <summary>
    /// Display name of the feature flag.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique key for the feature flag (e.g., "AIGenerationEnabled").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Whether the feature is currently enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Description of what this feature flag controls.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category for grouping related flags (e.g., "AI", "Export", "Premium").
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Tags for filtering and organization.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Type of flag: Temporary (short-lived) or Permanent (long-lived).
    /// </summary>
    public FeatureFlagType Type { get; set; } = FeatureFlagType.Permanent;

    /// <summary>
    /// Current lifecycle state of the flag.
    /// </summary>
    public FeatureFlagState State { get; set; } = FeatureFlagState.Active;

    /// <summary>
    /// When the flag was created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// User who created the flag.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When the flag was last modified.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// User who last modified the flag.
    /// </summary>
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// When the flag should expire (for Temporary flags).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Response containing a list of feature flags with summary statistics.
/// </summary>
public class FeatureFlagListResponse
{
    /// <summary>
    /// List of feature flags.
    /// </summary>
    public List<FeatureFlagResponse> Flags { get; set; } = new();

    /// <summary>
    /// Total number of flags.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of enabled flags.
    /// </summary>
    public int EnabledCount { get; set; }

    /// <summary>
    /// Number of disabled flags.
    /// </summary>
    public int DisabledCount { get; set; }

    /// <summary>
    /// Number of stale flags (needing cleanup).
    /// </summary>
    public int StaleCount { get; set; }
}

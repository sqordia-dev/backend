using Sqordia.Domain.Enums;

namespace Sqordia.Application.Models;

/// <summary>
/// Metadata stored alongside a feature flag in the Settings table.
/// This is serialized to JSON in the Settings.Value field.
/// </summary>
public class FeatureFlagMetadata
{
    /// <summary>
    /// Whether the feature is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Description of what this feature flag controls.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category for grouping related flags (e.g., "AI", "Export", "Premium").
    /// </summary>
    public string Category { get; set; } = "General";

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
    /// When the flag should expire (for Temporary flags).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Creates default metadata for a simple boolean feature flag.
    /// </summary>
    public static FeatureFlagMetadata CreateDefault(bool isEnabled = false)
    {
        return new FeatureFlagMetadata
        {
            IsEnabled = isEnabled,
            Category = "General",
            Type = FeatureFlagType.Permanent,
            State = FeatureFlagState.Active
        };
    }
}

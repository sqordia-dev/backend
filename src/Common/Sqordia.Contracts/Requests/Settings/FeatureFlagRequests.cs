using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Requests.Settings;

/// <summary>
/// Request to create a new feature flag.
/// </summary>
public class CreateFeatureFlagRequest
{
    /// <summary>
    /// Unique name/key for the feature flag (e.g., "AIGenerationEnabled").
    /// </summary>
    public required string Name { get; set; }

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
    /// When the flag should expire (required for Temporary flags).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Initial enabled state of the flag.
    /// </summary>
    public bool IsEnabled { get; set; } = false;
}

/// <summary>
/// Request to update an existing feature flag's metadata.
/// </summary>
public class UpdateFeatureFlagRequest
{
    /// <summary>
    /// Updated description of the feature flag.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Updated category for the flag.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Updated tags for filtering and organization.
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Updated type of flag.
    /// </summary>
    public FeatureFlagType? Type { get; set; }

    /// <summary>
    /// Updated lifecycle state.
    /// </summary>
    public FeatureFlagState? State { get; set; }

    /// <summary>
    /// Updated expiration date.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Request to toggle a feature flag's enabled state.
/// </summary>
public class ToggleFeatureFlagRequest
{
    /// <summary>
    /// Whether to enable or disable the flag.
    /// </summary>
    public required bool IsEnabled { get; set; }
}

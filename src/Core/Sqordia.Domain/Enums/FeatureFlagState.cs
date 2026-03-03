namespace Sqordia.Domain.Enums;

/// <summary>
/// Lifecycle state of a feature flag
/// </summary>
public enum FeatureFlagState
{
    /// <summary>
    /// Flag is active and within expected lifetime
    /// </summary>
    Active = 0,

    /// <summary>
    /// Temporary flag that has exceeded its expected lifetime (30+ days).
    /// Should be reviewed for cleanup.
    /// </summary>
    PotentiallyStale = 1,

    /// <summary>
    /// Flag marked for cleanup or temporary flag older than 60 days.
    /// Should be removed from codebase.
    /// </summary>
    Stale = 2,

    /// <summary>
    /// Flag has been archived. Code references should be removed.
    /// Kept for audit trail purposes.
    /// </summary>
    Archived = 3
}

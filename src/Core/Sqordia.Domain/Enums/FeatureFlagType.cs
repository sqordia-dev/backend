namespace Sqordia.Domain.Enums;

/// <summary>
/// Type of feature flag indicating its lifecycle expectation
/// </summary>
public enum FeatureFlagType
{
    /// <summary>
    /// Short-lived flag for rollouts, experiments, or migrations.
    /// Should have an expiration date and be cleaned up after use.
    /// </summary>
    Temporary = 0,

    /// <summary>
    /// Long-lived flag for premium features, kill switches, or ops toggles.
    /// Does not require an expiration date.
    /// </summary>
    Permanent = 1
}

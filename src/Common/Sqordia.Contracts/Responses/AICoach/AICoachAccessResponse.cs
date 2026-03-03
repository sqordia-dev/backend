namespace Sqordia.Contracts.Responses.AICoach;

/// <summary>
/// Response indicating whether the user has access to AI Coach
/// </summary>
public class AICoachAccessResponse
{
    /// <summary>
    /// Whether the user has access to AI Coach
    /// </summary>
    public bool HasAccess { get; set; }

    /// <summary>
    /// The user's subscription tier name
    /// </summary>
    public string? SubscriptionTier { get; set; }

    /// <summary>
    /// Reason for access denial (if no access)
    /// </summary>
    public string? DenialReason { get; set; }

    /// <summary>
    /// Upgrade URL (if the user needs to upgrade)
    /// </summary>
    public string? UpgradeUrl { get; set; }

    /// <summary>
    /// Whether the feature is enabled globally
    /// </summary>
    public bool FeatureEnabled { get; set; }
}

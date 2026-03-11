using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Central service for checking plan-based feature availability and usage limits.
/// All enforcement points call through here — no direct plan-type checks elsewhere.
/// </summary>
public interface IFeatureGateService
{
    /// <summary>
    /// Check if a boolean feature is enabled for the organization's plan.
    /// </summary>
    Task<bool> IsFeatureEnabledAsync(Guid organizationId, string featureKey, CancellationToken ct = default);

    /// <summary>
    /// Get the numeric limit for a feature (-1 = unlimited, 0 = disabled).
    /// </summary>
    Task<int> GetLimitAsync(Guid organizationId, string featureKey, CancellationToken ct = default);

    /// <summary>
    /// Get a string feature value (e.g., AI provider tier).
    /// </summary>
    Task<string?> GetFeatureValueAsync(Guid organizationId, string featureKey, CancellationToken ct = default);

    /// <summary>
    /// Check if the organization can perform an action (under their monthly usage limit).
    /// Returns success or a descriptive error with upgrade info.
    /// </summary>
    Task<Result<FeatureCheckResult>> CheckUsageLimitAsync(
        Guid organizationId, string featureKey, CancellationToken ct = default);

    /// <summary>
    /// Record a usage event (increment the counter for the current billing period).
    /// </summary>
    Task RecordUsageAsync(Guid organizationId, string featureKey, int amount = 1, CancellationToken ct = default);

    /// <summary>
    /// Get the full feature snapshot for an organization's plan (for frontend display).
    /// </summary>
    Task<Result<PlanFeaturesSnapshot>> GetPlanFeaturesAsync(Guid organizationId, CancellationToken ct = default);
}

public class FeatureCheckResult
{
    public bool Allowed { get; set; }
    public int CurrentUsage { get; set; }
    public int Limit { get; set; } // -1 = unlimited
    public double UsagePercent { get; set; }
    public bool IsNearLimit { get; set; }
    public string? DenialReason { get; set; }
    public string? UpgradePrompt { get; set; }
}

public class PlanFeaturesSnapshot
{
    public string PlanType { get; set; } = null!;
    public string PlanName { get; set; } = null!;
    public Dictionary<string, string> Features { get; set; } = new();
    public Dictionary<string, UsageInfo> Usage { get; set; } = new();
}

public class UsageInfo
{
    public int Current { get; set; }
    public int Limit { get; set; } // -1 = unlimited
    public double Percent { get; set; }
    public bool IsNearLimit { get; set; }
}

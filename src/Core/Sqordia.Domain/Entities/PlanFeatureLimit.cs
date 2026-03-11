using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Defines a feature limit for a subscription plan.
/// Data-driven: add rows to grant features, no code changes needed.
/// </summary>
public class PlanFeatureLimit : BaseAuditableEntity
{
    public Guid SubscriptionPlanId { get; private set; }
    public string FeatureKey { get; private set; } = null!;

    /// <summary>
    /// For boolean features: "true"/"false".
    /// For numeric limits: integer as string (-1 = unlimited).
    /// For string features: the value (e.g. "gemini", "blended", "claude").
    /// </summary>
    public string Value { get; private set; } = null!;

    // Navigation
    public SubscriptionPlan Plan { get; private set; } = null!;

    private PlanFeatureLimit() { } // EF Core

    public PlanFeatureLimit(Guid subscriptionPlanId, string featureKey, string value)
    {
        SubscriptionPlanId = subscriptionPlanId;
        FeatureKey = featureKey ?? throw new ArgumentNullException(nameof(featureKey));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void UpdateValue(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    // ── Convenience accessors ────────────────────────────

    public bool AsBool() => bool.TryParse(Value, out var b) && b;
    public int AsInt() => int.TryParse(Value, out var i) ? i : 0;
    public bool IsUnlimited() => Value == "-1";
}

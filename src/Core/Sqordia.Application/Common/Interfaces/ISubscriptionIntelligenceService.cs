using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// ML-driven subscription intelligence: engagement scoring, churn prediction,
/// upgrade propensity, and personalized promotion recommendations.
/// Calls the Python ai-service subscription-ml endpoints with aggregated signals.
/// </summary>
public interface ISubscriptionIntelligenceService
{
    /// <summary>
    /// Get full intelligence for an organization (engagement + churn + upgrade + promotion).
    /// Aggregates all signals from DB, calls Python ML models, returns combined result.
    /// </summary>
    Task<Result<SubscriptionIntelligence>> GetIntelligenceAsync(
        Guid organizationId, CancellationToken ct = default);

    /// <summary>
    /// Validate and apply a coupon code for an organization.
    /// </summary>
    Task<Result<CouponValidationResult>> ValidateCouponAsync(
        string code, Guid organizationId, CancellationToken ct = default);

    /// <summary>
    /// Generate a personalized coupon for an organization based on ML recommendations.
    /// </summary>
    Task<Result<GeneratedCoupon>> GeneratePersonalizedCouponAsync(
        Guid organizationId, CancellationToken ct = default);

    /// <summary>
    /// Get active promotions for an organization (personalized + global).
    /// </summary>
    Task<Result<List<ActivePromotion>>> GetActivePromotionsAsync(
        Guid organizationId, CancellationToken ct = default);
}

// ── Response DTOs ────────────────────────────────────────

public class SubscriptionIntelligence
{
    public EngagementScore Engagement { get; set; } = new();
    public ChurnPrediction Churn { get; set; } = new();
    public UpgradePropensity Upgrade { get; set; } = new();
    public PromotionRecommendation Promotion { get; set; } = new();
}

public class EngagementScore
{
    public double Score { get; set; }
    public string Level { get; set; } = "medium";
    public Dictionary<string, double> Signals { get; set; } = new();
}

public class ChurnPrediction
{
    public double ChurnProbability { get; set; }
    public string RiskLevel { get; set; } = "low";
    public List<string> RiskFactors { get; set; } = new();
    public string RecommendedAction { get; set; } = "none";
    public int? DaysToLikelyChurn { get; set; }
}

public class UpgradePropensity
{
    public double UpgradeProbability { get; set; }
    public string? RecommendedPlan { get; set; }
    public List<string> UpgradeSignals { get; set; } = new();
    public string? SuggestedPromotionType { get; set; }
}

public class PromotionRecommendation
{
    public bool ShouldOffer { get; set; }
    public string PromotionType { get; set; } = "none";
    public int DiscountPercent { get; set; }
    public string? TargetPlan { get; set; }
    public string? MessageKey { get; set; }
    public string Urgency { get; set; } = "low";
    public int ValidDays { get; set; } = 30;
    public string Reason { get; set; } = "";
}

public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string? Code { get; set; }
    public int DiscountPercent { get; set; }
    public string? TargetPlan { get; set; }
    public string? ErrorMessage { get; set; }
}

public class GeneratedCoupon
{
    public string Code { get; set; } = null!;
    public int DiscountPercent { get; set; }
    public string PromotionType { get; set; } = null!;
    public string? TargetPlan { get; set; }
    public DateTime ValidUntil { get; set; }
    public string Reason { get; set; } = "";
}

public class ActivePromotion
{
    public string PromotionType { get; set; } = null!;
    public string? CouponCode { get; set; }
    public int DiscountPercent { get; set; }
    public string? TargetPlan { get; set; }
    public string? MessageKey { get; set; }
    public string Urgency { get; set; } = "low";
    public DateTime? ExpiresAt { get; set; }
}

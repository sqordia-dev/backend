using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Aggregates behavioral signals from the database, calls the Python ML service
/// for predictions, and manages coupon generation + validation.
/// </summary>
public class SubscriptionIntelligenceService : ISubscriptionIntelligenceService
{
    private readonly IApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SubscriptionIntelligenceService> _logger;
    private readonly string _aiServiceUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public SubscriptionIntelligenceService(
        IApplicationDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SubscriptionIntelligenceService> logger)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient("AIService");
        _logger = logger;
        _aiServiceUrl = configuration["AI:PythonService:BaseUrl"] ?? "http://localhost:8100";
    }

    public async Task<Result<SubscriptionIntelligence>> GetIntelligenceAsync(
        Guid organizationId, CancellationToken ct = default)
    {
        try
        {
            // Step 1: Aggregate signals from database
            var features = await AggregateEngagementFeaturesAsync(organizationId, ct);

            // Step 2: Call Python ML service
            var response = await _httpClient.PostAsJsonAsync(
                $"{_aiServiceUrl}/subscription-ml/intelligence",
                new { features },
                JsonOptions, ct);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SubscriptionIntelligence>(JsonOptions, ct);
                if (result != null)
                    return Result.Success(result);
            }

            // Fallback: use local heuristic if Python service unavailable
            _logger.LogWarning("Python subscription-ml service unavailable, using local fallback");
            return Result.Success(BuildLocalFallback(features));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Subscription intelligence failed for org {OrgId}", organizationId);
            return Result.Failure<SubscriptionIntelligence>(
                Error.Failure("Intelligence.Failed", ex.Message));
        }
    }

    public async Task<Result<CouponValidationResult>> ValidateCouponAsync(
        string code, Guid organizationId, CancellationToken ct = default)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant() && !c.IsDeleted, ct);

        if (coupon == null)
        {
            return Result.Success(new CouponValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid coupon code"
            });
        }

        if (!coupon.CanBeRedeemedBy(organizationId))
        {
            var reason = !coupon.IsValid() ? "Coupon has expired or reached max redemptions"
                : "This coupon is not available for your organization";

            return Result.Success(new CouponValidationResult
            {
                IsValid = false,
                ErrorMessage = reason
            });
        }

        // Check per-org redemption limit
        if (coupon.MaxRedemptionsPerOrg > 0)
        {
            var orgRedemptions = await _context.CouponRedemptions
                .CountAsync(r => r.CouponId == coupon.Id && r.OrganizationId == organizationId, ct);

            if (orgRedemptions >= coupon.MaxRedemptionsPerOrg)
            {
                return Result.Success(new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "You have already used this coupon"
                });
            }
        }

        return Result.Success(new CouponValidationResult
        {
            IsValid = true,
            Code = coupon.Code,
            DiscountPercent = coupon.DiscountPercent,
            TargetPlan = coupon.TargetPlanType?.ToString()
        });
    }

    public async Task<Result<GeneratedCoupon>> GeneratePersonalizedCouponAsync(
        Guid organizationId, CancellationToken ct = default)
    {
        // Get intelligence to determine what promotion to offer
        var intelligenceResult = await GetIntelligenceAsync(organizationId, ct);
        if (!intelligenceResult.IsSuccess)
            return Result.Failure<GeneratedCoupon>(intelligenceResult.Error!);

        var intelligence = intelligenceResult.Value!;
        var promo = intelligence.Promotion;

        if (!promo.ShouldOffer || promo.DiscountPercent <= 0)
        {
            return Result.Failure<GeneratedCoupon>(
                Error.Failure("Coupon.NoPromotion", "No promotion is recommended for this organization at this time"));
        }

        // Generate unique coupon code
        var code = $"{promo.PromotionType.ToUpperInvariant().Replace("_", "")}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        SubscriptionPlanType? targetPlan = promo.TargetPlan switch
        {
            "Starter" => SubscriptionPlanType.Starter,
            "Professional" => SubscriptionPlanType.Professional,
            "Enterprise" => SubscriptionPlanType.Enterprise,
            _ => null
        };

        var coupon = new Coupon(
            code: code,
            description: promo.Reason,
            discountPercent: promo.DiscountPercent,
            validFrom: DateTime.UtcNow,
            validUntil: DateTime.UtcNow.AddDays(promo.ValidDays),
            maxRedemptions: 1,
            targetPlanType: targetPlan,
            promotionType: promo.PromotionType,
            organizationId: organizationId);

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Generated personalized coupon {Code} ({Discount}% off) for org {OrgId} — type: {Type}",
            code, promo.DiscountPercent, organizationId, promo.PromotionType);

        return Result.Success(new GeneratedCoupon
        {
            Code = code,
            DiscountPercent = promo.DiscountPercent,
            PromotionType = promo.PromotionType,
            TargetPlan = promo.TargetPlan,
            ValidUntil = DateTime.UtcNow.AddDays(promo.ValidDays),
            Reason = promo.Reason
        });
    }

    public async Task<Result<List<ActivePromotion>>> GetActivePromotionsAsync(
        Guid organizationId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Get personalized coupons for this org
        var personalCoupons = await _context.Coupons
            .Where(c => c.OrganizationId == organizationId
                     && c.IsActive
                     && c.ValidFrom <= now
                     && c.ValidUntil >= now
                     && !c.IsDeleted)
            .ToListAsync(ct);

        // Get global (non-org-specific) active coupons
        var globalCoupons = await _context.Coupons
            .Where(c => c.OrganizationId == null
                     && c.IsActive
                     && c.ValidFrom <= now
                     && c.ValidUntil >= now
                     && (c.MaxRedemptions == 0 || c.TimesRedeemed < c.MaxRedemptions)
                     && !c.IsDeleted)
            .ToListAsync(ct);

        var promotions = personalCoupons.Concat(globalCoupons)
            .Select(c => new ActivePromotion
            {
                PromotionType = c.PromotionType ?? "manual",
                CouponCode = c.Code,
                DiscountPercent = c.DiscountPercent,
                TargetPlan = c.TargetPlanType?.ToString(),
                MessageKey = $"promo.{c.PromotionType ?? "manual"}.banner",
                Urgency = c.ValidUntil <= now.AddDays(3) ? "high" : "low",
                ExpiresAt = c.ValidUntil
            })
            .ToList();

        return Result.Success(promotions);
    }

    // ── Signal aggregation ───────────────────────────────

    private async Task<Dictionary<string, object>> AggregateEngagementFeaturesAsync(
        Guid organizationId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var sevenDaysAgo = now.AddDays(-7);
        var ninetyDaysAgo = now.AddDays(-90);
        var currentPeriod = int.Parse(now.ToString("yyyyMM"));

        // Get organization info
        var org = await _context.Organizations
            .Where(o => o.Id == organizationId && !o.IsDeleted)
            .Select(o => new { o.Created })
            .FirstOrDefaultAsync(ct);

        // Get subscription info
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.OrganizationId == organizationId
                     && s.Status == SubscriptionStatus.Active
                     && !s.IsDeleted)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync(ct);

        var planName = subscription?.Plan.PlanType.ToString() ?? "Free";

        // Get usage for current period
        var usage = await _context.OrganizationUsages
            .Where(u => u.OrganizationId == organizationId && u.Period == currentPeriod)
            .FirstOrDefaultAsync(ct);

        // Get team member count
        var teamMembers = await _context.OrganizationMembers
            .CountAsync(m => m.OrganizationId == organizationId && !m.IsDeleted, ct);

        // Get business plan stats
        var bpStats = await _context.BusinessPlans
            .Where(bp => bp.OrganizationId == organizationId && !bp.IsDeleted)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Completed = g.Count(bp => bp.GenerationCompletedAt != null)
            })
            .FirstOrDefaultAsync(ct);

        // Get login activity from users in this org
        var userIds = await _context.OrganizationMembers
            .Where(m => m.OrganizationId == organizationId && !m.IsDeleted)
            .Select(m => m.UserId)
            .ToListAsync(ct);

        var lastLogin = await _context.LoginHistories
            .Where(l => userIds.Contains(l.UserId) && l.IsSuccessful)
            .OrderByDescending(l => l.LoginAttemptAt)
            .Select(l => l.LoginAttemptAt)
            .FirstOrDefaultAsync(ct);

        var logins30d = await _context.LoginHistories
            .CountAsync(l => userIds.Contains(l.UserId) && l.IsSuccessful && l.LoginAttemptAt >= thirtyDaysAgo, ct);

        var logins7d = await _context.LoginHistories
            .CountAsync(l => userIds.Contains(l.UserId) && l.IsSuccessful && l.LoginAttemptAt >= sevenDaysAgo, ct);

        // Get business plan IDs for this org (needed for telemetry/edit joins)
        var bpIds = await _context.BusinessPlans
            .Where(bp => bp.OrganizationId == organizationId && !bp.IsDeleted)
            .Select(bp => bp.Id)
            .ToListAsync(ct);

        // Get AI telemetry stats (linked via BusinessPlanId)
        var aiTokens30d = await _context.AICallTelemetryRecords
            .Where(t => t.BusinessPlanId.HasValue && bpIds.Contains(t.BusinessPlanId.Value) && t.CreatedAt >= thirtyDaysAgo)
            .SumAsync(t => t.InputTokens + t.OutputTokens, ct);

        // Get section edits in last 30 days (linked via BusinessPlanId)
        var sectionEdits30d = await _context.SectionEditHistories
            .CountAsync(e => bpIds.Contains(e.BusinessPlanId) && e.CreatedAt >= thirtyDaysAgo, ct);

        // Average edit ratio
        var avgEditRatio = await _context.SectionEditHistories
            .Where(e => bpIds.Contains(e.BusinessPlanId) && e.CreatedAt >= thirtyDaysAgo)
            .AverageAsync(e => (double?)e.EditRatio, ct) ?? 0.0;

        // Calculate days since signup
        var daysSinceSignup = org != null ? (int)(now - org.Created).TotalDays : 0;
        var daysSinceLastLogin = lastLogin != default ? (int)(now - lastLogin).TotalDays : daysSinceSignup;

        // Months subscribed
        var monthsSubscribed = subscription != null ? (int)((now - subscription.StartDate).TotalDays / 30) : 0;

        return new Dictionary<string, object>
        {
            ["organization_id"] = organizationId.ToString(),
            ["current_plan"] = planName,
            ["days_since_signup"] = daysSinceSignup,
            ["days_since_last_login"] = daysSinceLastLogin,
            ["logins_last_30_days"] = logins30d,
            ["logins_last_7_days"] = logins7d,
            ["plans_generated_total"] = bpStats?.Total ?? 0,
            ["plans_generated_last_30_days"] = usage?.PlansGenerated ?? 0,
            ["ai_coach_messages_last_30_days"] = usage?.AiCoachMessages ?? 0,
            ["exports_last_30_days"] = usage?.ExportsGenerated ?? 0,
            ["team_members_count"] = teamMembers,
            ["business_plans_count"] = bpStats?.Total ?? 0,
            ["completed_plans_count"] = bpStats?.Completed ?? 0,
            ["ai_tokens_used_last_30_days"] = aiTokens30d,
            ["sections_edited_last_30_days"] = sectionEdits30d,
            ["avg_edit_ratio"] = avgEditRatio,
            ["feature_usage_breadth"] = CalculateFeatureBreadth(usage, bpStats?.Total ?? 0, teamMembers),
            ["financial_projections_used"] = false, // TODO: check FinancialPlansPrevisio
            ["payment_failures_last_90_days"] = 0, // TODO: track from Stripe webhooks
            ["is_yearly"] = subscription?.IsYearly ?? false,
            ["months_subscribed"] = monthsSubscribed,
            ["plan_limit_warnings_last_30_days"] = 0 // TODO: track from FeatureGateService
        };
    }

    private static double CalculateFeatureBreadth(Domain.Entities.OrganizationUsage? usage, int planCount, int teamCount)
    {
        if (usage == null && planCount == 0) return 0;

        var features = 0;
        var total = 5; // plan creation, AI coach, exports, team, financial

        if (planCount > 0) features++;
        if (usage?.AiCoachMessages > 0) features++;
        if (usage?.ExportsGenerated > 0) features++;
        if (teamCount > 1) features++;
        // financial projections would be another check

        return (double)features / total;
    }

    // ── Local fallback (when Python unavailable) ─────────

    private static SubscriptionIntelligence BuildLocalFallback(Dictionary<string, object> features)
    {
        var daysSinceLogin = Convert.ToInt32(features.GetValueOrDefault("days_since_last_login", 0));
        var logins30 = Convert.ToInt32(features.GetValueOrDefault("logins_last_30_days", 0));
        var plan = features.GetValueOrDefault("current_plan", "Free")?.ToString() ?? "Free";

        // Simple heuristic engagement
        var engScore = Math.Max(0, 100 - daysSinceLogin * 5);
        engScore = Math.Min(100, engScore + logins30 * 3);

        // Simple churn heuristic
        var churnProb = daysSinceLogin > 30 ? 0.8 : daysSinceLogin > 14 ? 0.4 : 0.1;

        return new SubscriptionIntelligence
        {
            Engagement = new EngagementScore { Score = engScore, Level = engScore > 50 ? "high" : "low" },
            Churn = new ChurnPrediction
            {
                ChurnProbability = churnProb,
                RiskLevel = churnProb > 0.5 ? "high" : "low"
            },
            Upgrade = new UpgradePropensity
            {
                UpgradeProbability = plan == "Free" ? 0.3 : 0.1
            },
            Promotion = new Application.Common.Interfaces.PromotionRecommendation()
        };
    }
}

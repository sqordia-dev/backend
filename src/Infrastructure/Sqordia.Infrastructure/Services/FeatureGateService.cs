using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Domain.Constants;
using Sqordia.Domain.Enums;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Central feature gate: all plan-based checks go through here.
/// Loads the org's active subscription + PlanFeatureLimits, checks usage.
/// </summary>
public class FeatureGateService : IFeatureGateService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FeatureGateService> _logger;

    private const double NearLimitThreshold = 0.80;

    public FeatureGateService(IApplicationDbContext context, ILogger<FeatureGateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsFeatureEnabledAsync(Guid organizationId, string featureKey, CancellationToken ct = default)
    {
        var value = await GetFeatureValueAsync(organizationId, featureKey, ct);
        return value != null && bool.TryParse(value, out var b) && b;
    }

    public async Task<int> GetLimitAsync(Guid organizationId, string featureKey, CancellationToken ct = default)
    {
        var value = await GetFeatureValueAsync(organizationId, featureKey, ct);
        if (value == null) return 0;
        return int.TryParse(value, out var i) ? i : 0;
    }

    public async Task<string?> GetFeatureValueAsync(Guid organizationId, string featureKey, CancellationToken ct = default)
    {
        var planId = await GetActivePlanIdAsync(organizationId, ct);
        if (planId == null) return GetFreeDefault(featureKey);

        var feature = await _context.PlanFeatureLimits
            .Where(f => f.SubscriptionPlanId == planId && f.FeatureKey == featureKey)
            .Select(f => f.Value)
            .FirstOrDefaultAsync(ct);

        return feature ?? GetFreeDefault(featureKey);
    }

    public async Task<Result<FeatureCheckResult>> CheckUsageLimitAsync(
        Guid organizationId, string featureKey, CancellationToken ct = default)
    {
        var limit = await GetLimitAsync(organizationId, featureKey, ct);

        // Unlimited
        if (limit < 0)
        {
            return Result.Success(new FeatureCheckResult
            {
                Allowed = true,
                CurrentUsage = 0,
                Limit = -1,
                UsagePercent = 0
            });
        }

        // Get current usage
        var period = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        var usage = await _context.OrganizationUsages
            .Where(u => u.OrganizationId == organizationId && u.Period == period)
            .FirstOrDefaultAsync(ct);

        var currentUsage = GetUsageCount(usage, featureKey);
        var usagePercent = limit > 0 ? (double)currentUsage / limit * 100 : 100;
        var isNearLimit = usagePercent >= NearLimitThreshold * 100;
        var exceeded = currentUsage >= limit;

        var result = new FeatureCheckResult
        {
            Allowed = !exceeded,
            CurrentUsage = currentUsage,
            Limit = limit,
            UsagePercent = Math.Min(usagePercent, 100),
            IsNearLimit = isNearLimit
        };

        if (exceeded)
        {
            result.DenialReason = featureKey switch
            {
                PlanFeatures.MaxAiGenerationsMonthly => "You have reached your monthly plan generation limit.",
                PlanFeatures.MaxAiCoachMessagesMonthly => "You have reached your monthly AI Coach message limit.",
                PlanFeatures.MaxBusinessPlans => "You have reached your business plan limit.",
                _ => $"You have reached the limit for {featureKey}."
            };
            result.UpgradePrompt = "Upgrade your plan to increase your limits.";
        }

        return Result.Success(result);
    }

    public async Task RecordUsageAsync(Guid organizationId, string featureKey, int amount = 1, CancellationToken ct = default)
    {
        var period = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));

        var usage = await _context.OrganizationUsages
            .Where(u => u.OrganizationId == organizationId && u.Period == period)
            .FirstOrDefaultAsync(ct);

        if (usage == null)
        {
            usage = new Domain.Entities.OrganizationUsage(organizationId, period);
            _context.OrganizationUsages.Add(usage);
        }

        switch (featureKey)
        {
            case PlanFeatures.MaxAiGenerationsMonthly:
                usage.IncrementPlansGenerated(amount);
                break;
            case PlanFeatures.MaxAiCoachMessagesMonthly:
                usage.IncrementAiCoachMessages(amount);
                break;
            case PlanFeatures.MaxStorageMb:
                usage.AddStorageUsed(amount); // amount in bytes
                break;
            default:
                if (featureKey.StartsWith("export_"))
                    usage.IncrementExports(amount);
                break;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<Result<PlanFeaturesSnapshot>> GetPlanFeaturesAsync(Guid organizationId, CancellationToken ct = default)
    {
        var planId = await GetActivePlanIdAsync(organizationId, ct);

        // Load plan info
        SubscriptionPlanType planType;
        string planName;

        if (planId != null)
        {
            var plan = await _context.SubscriptionPlans
                .Where(p => p.Id == planId)
                .Select(p => new { p.PlanType, p.Name })
                .FirstOrDefaultAsync(ct);

            planType = plan?.PlanType ?? SubscriptionPlanType.Free;
            planName = plan?.Name ?? "Decouverte";
        }
        else
        {
            planType = SubscriptionPlanType.Free;
            planName = "Decouverte";
        }

        // Load all features
        var features = planId != null
            ? await _context.PlanFeatureLimits
                .Where(f => f.SubscriptionPlanId == planId)
                .ToDictionaryAsync(f => f.FeatureKey, f => f.Value, ct)
            : GetAllFreeDefaults();

        // Load current month usage
        var period = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        var usage = await _context.OrganizationUsages
            .Where(u => u.OrganizationId == organizationId && u.Period == period)
            .FirstOrDefaultAsync(ct);

        var usageMap = new Dictionary<string, UsageInfo>();

        // Build usage info for tracked metrics
        foreach (var trackedKey in new[] {
            PlanFeatures.MaxAiGenerationsMonthly,
            PlanFeatures.MaxAiCoachMessagesMonthly,
            PlanFeatures.MaxBusinessPlans,
            PlanFeatures.MaxStorageMb })
        {
            var limit = features.TryGetValue(trackedKey, out var v) && int.TryParse(v, out var l) ? l : 0;
            var current = GetUsageCount(usage, trackedKey);

            // For max_business_plans, count active plans instead of monthly usage
            if (trackedKey == PlanFeatures.MaxBusinessPlans)
            {
                current = await _context.BusinessPlans
                    .CountAsync(bp => bp.OrganizationId == organizationId && !bp.IsDeleted, ct);
            }

            var percent = limit > 0 ? (double)current / limit * 100 : (limit < 0 ? 0 : 100);
            usageMap[trackedKey] = new UsageInfo
            {
                Current = current,
                Limit = limit,
                Percent = Math.Min(percent, 100),
                IsNearLimit = percent >= NearLimitThreshold * 100
            };
        }

        return Result.Success(new PlanFeaturesSnapshot
        {
            PlanType = planType.ToString(),
            PlanName = planName,
            Features = features,
            Usage = usageMap
        });
    }

    // ── Private helpers ──────────────────────────────────

    private async Task<Guid?> GetActivePlanIdAsync(Guid organizationId, CancellationToken ct)
    {
        return await _context.Subscriptions
            .Where(s => s.OrganizationId == organizationId
                     && s.Status == SubscriptionStatus.Active
                     && !s.IsDeleted
                     && s.StartDate <= DateTime.UtcNow
                     && s.EndDate >= DateTime.UtcNow)
            .OrderByDescending(s => s.StartDate)
            .Select(s => (Guid?)s.SubscriptionPlanId)
            .FirstOrDefaultAsync(ct);
    }

    private static int GetUsageCount(Domain.Entities.OrganizationUsage? usage, string featureKey)
    {
        if (usage == null) return 0;
        return featureKey switch
        {
            PlanFeatures.MaxAiGenerationsMonthly => usage.PlansGenerated,
            PlanFeatures.MaxAiCoachMessagesMonthly => usage.AiCoachMessages,
            PlanFeatures.MaxStorageMb => (int)(usage.StorageUsedBytes / (1024 * 1024)), // bytes → MB
            _ => 0
        };
    }

    /// <summary>
    /// Default values for users with no subscription (Free tier).
    /// </summary>
    private static string? GetFreeDefault(string featureKey) => featureKey switch
    {
        PlanFeatures.MaxBusinessPlans => "1",
        PlanFeatures.MaxOrganizations => "1",
        PlanFeatures.MaxTeamMembers => "1",
        PlanFeatures.MaxAiGenerationsMonthly => "1",
        PlanFeatures.MaxAiCoachMessagesMonthly => "10",
        PlanFeatures.MaxStorageMb => "100",
        PlanFeatures.ExportHtml => "true",
        PlanFeatures.ExportPdf => "false",
        PlanFeatures.ExportWord => "false",
        PlanFeatures.ExportPowerpoint => "false",
        PlanFeatures.ExportExcel => "false",
        PlanFeatures.ExportAgentBlueprints => "false",
        PlanFeatures.AiProviderTier => "gemini",
        PlanFeatures.PrioritySectionsClaude => "false",
        PlanFeatures.FinancialProjectionsBasic => "true",
        PlanFeatures.FinancialProjectionsAdvanced => "false",
        PlanFeatures.CustomBranding => "false",
        PlanFeatures.ApiAccess => "false",
        PlanFeatures.PrioritySupport => "false",
        PlanFeatures.DedicatedSupport => "false",
        PlanFeatures.WhiteLabel => "false",
        _ => null
    };

    private static Dictionary<string, string> GetAllFreeDefaults()
    {
        var keys = new[]
        {
            PlanFeatures.MaxBusinessPlans, PlanFeatures.MaxOrganizations,
            PlanFeatures.MaxTeamMembers, PlanFeatures.MaxAiGenerationsMonthly,
            PlanFeatures.MaxAiCoachMessagesMonthly, PlanFeatures.MaxStorageMb,
            PlanFeatures.ExportHtml, PlanFeatures.ExportPdf, PlanFeatures.ExportWord,
            PlanFeatures.ExportPowerpoint, PlanFeatures.ExportExcel, PlanFeatures.ExportAgentBlueprints,
            PlanFeatures.AiProviderTier, PlanFeatures.PrioritySectionsClaude,
            PlanFeatures.FinancialProjectionsBasic, PlanFeatures.FinancialProjectionsAdvanced,
            PlanFeatures.CustomBranding, PlanFeatures.ApiAccess,
            PlanFeatures.PrioritySupport, PlanFeatures.DedicatedSupport, PlanFeatures.WhiteLabel
        };

        return keys.ToDictionary(k => k, k => GetFreeDefault(k) ?? "false");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Domain.Constants;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Persistence.Seeds;

/// <summary>
/// Seeds the 4 subscription plans (Free, Starter, Professional, Enterprise)
/// and their corresponding PlanFeatureLimit rows (21 features × 4 tiers).
/// Idempotent: skips if plans already exist, upserts feature limits.
/// </summary>
public class SubscriptionPlanSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SubscriptionPlanSeeder> _logger;

    public SubscriptionPlanSeeder(ApplicationDbContext context, ILogger<SubscriptionPlanSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting subscription plan seeding...");

        // Ensure the 4 plans exist
        var plans = await EnsurePlansExistAsync(ct);

        // Seed feature limits for each plan
        await SeedFeatureLimitsAsync(plans, ct);

        _logger.LogInformation("Subscription plan seeding completed.");
    }

    private async Task<Dictionary<SubscriptionPlanType, SubscriptionPlan>> EnsurePlansExistAsync(CancellationToken ct)
    {
        var existing = await _context.SubscriptionPlans
            .Where(p => !p.IsDeleted)
            .ToListAsync(ct);

        var plans = existing.ToDictionary(p => p.PlanType);

        if (!plans.ContainsKey(SubscriptionPlanType.Free))
        {
            var plan = new SubscriptionPlan(
                SubscriptionPlanType.Free, "Découverte",
                "Explorez Sqordia gratuitement — 1 plan d'affaires, exports HTML",
                0m, BillingCycle.Monthly, maxUsers: 1, maxBusinessPlans: 1, maxStorageGB: 0);
            _context.SubscriptionPlans.Add(plan);
            plans[SubscriptionPlanType.Free] = plan;
        }

        if (!plans.ContainsKey(SubscriptionPlanType.Starter))
        {
            var plan = new SubscriptionPlan(
                SubscriptionPlanType.Starter, "Essentiel",
                "Pour les entrepreneurs solo — 3 plans, exports PDF/Word, coach IA",
                29m, BillingCycle.Monthly, maxUsers: 2, maxBusinessPlans: 3, maxStorageGB: 1);
            _context.SubscriptionPlans.Add(plan);
            plans[SubscriptionPlanType.Starter] = plan;
        }

        if (!plans.ContainsKey(SubscriptionPlanType.Professional))
        {
            var plan = new SubscriptionPlan(
                SubscriptionPlanType.Professional, "Professionnel",
                "Pour les PME en croissance — plans illimités, tous les exports, projections financières avancées",
                59m, BillingCycle.Monthly, maxUsers: 10, maxBusinessPlans: 999, maxStorageGB: 5);
            _context.SubscriptionPlans.Add(plan);
            plans[SubscriptionPlanType.Professional] = plan;
        }

        if (!plans.ContainsKey(SubscriptionPlanType.Enterprise))
        {
            var plan = new SubscriptionPlan(
                SubscriptionPlanType.Enterprise, "Entreprise",
                "Pour les agences et grandes organisations — tout illimité, marque blanche, support dédié",
                149m, BillingCycle.Monthly, maxUsers: 999, maxBusinessPlans: 999, maxStorageGB: 50);
            _context.SubscriptionPlans.Add(plan);
            plans[SubscriptionPlanType.Enterprise] = plan;
        }

        await _context.SaveChangesAsync(ct);
        return plans;
    }

    private async Task SeedFeatureLimitsAsync(
        Dictionary<SubscriptionPlanType, SubscriptionPlan> plans, CancellationToken ct)
    {
        // Feature definitions per tier: key → [Free, Starter, Professional, Enterprise]
        var featureMatrix = new Dictionary<string, string[]>
        {
            // ── Numeric limits ─────────────────────────────────
            [PlanFeatures.MaxBusinessPlans]           = ["1",     "3",     "-1",    "-1"],
            [PlanFeatures.MaxOrganizations]           = ["1",     "1",     "3",     "-1"],
            [PlanFeatures.MaxTeamMembers]             = ["1",     "2",     "10",    "-1"],
            [PlanFeatures.MaxAiGenerationsMonthly]    = ["1",     "5",     "30",    "-1"],
            [PlanFeatures.MaxAiCoachMessagesMonthly]  = ["10",    "50",    "300",   "-1"],
            [PlanFeatures.MaxStorageMb]               = ["100",   "1024",  "5120",  "51200"],

            // ── Export capabilities ────────────────────────────
            [PlanFeatures.ExportHtml]                 = ["true",  "true",  "true",  "true"],
            [PlanFeatures.ExportPdf]                  = ["false", "true",  "true",  "true"],
            [PlanFeatures.ExportWord]                 = ["false", "true",  "true",  "true"],
            [PlanFeatures.ExportPowerpoint]           = ["false", "false", "true",  "true"],
            [PlanFeatures.ExportExcel]                = ["false", "false", "true",  "true"],
            [PlanFeatures.ExportAgentBlueprints]      = ["false", "false", "true",  "true"],

            // ── AI capabilities ────────────────────────────────
            [PlanFeatures.AiProviderTier]             = ["gemini", "blended", "blended", "claude"],
            [PlanFeatures.PrioritySectionsClaude]     = ["false", "false", "true",  "true"],

            // ── Financial ──────────────────────────────────────
            [PlanFeatures.FinancialProjectionsBasic]  = ["true",  "true",  "true",  "true"],
            [PlanFeatures.FinancialProjectionsAdvanced] = ["false", "false", "true",  "true"],

            // ── Premium features ───────────────────────────────
            [PlanFeatures.CustomBranding]             = ["false", "false", "false", "true"],
            [PlanFeatures.ApiAccess]                  = ["false", "false", "false", "true"],
            [PlanFeatures.PrioritySupport]            = ["false", "false", "true",  "true"],
            [PlanFeatures.DedicatedSupport]           = ["false", "false", "false", "true"],
            [PlanFeatures.WhiteLabel]                 = ["false", "false", "false", "true"],
        };

        var tierOrder = new[] {
            SubscriptionPlanType.Free,
            SubscriptionPlanType.Starter,
            SubscriptionPlanType.Professional,
            SubscriptionPlanType.Enterprise
        };

        // Load existing limits for all plans
        var planIds = plans.Values.Select(p => p.Id).ToList();
        var existingLimits = await _context.PlanFeatureLimits
            .Where(f => planIds.Contains(f.SubscriptionPlanId))
            .ToListAsync(ct);

        var existingLookup = existingLimits
            .ToDictionary(f => (f.SubscriptionPlanId, f.FeatureKey));

        var added = 0;
        var updated = 0;

        foreach (var (featureKey, values) in featureMatrix)
        {
            for (var i = 0; i < tierOrder.Length; i++)
            {
                var tier = tierOrder[i];
                if (!plans.TryGetValue(tier, out var plan)) continue;

                var value = values[i];
                var key = (plan.Id, featureKey);

                if (existingLookup.TryGetValue(key, out var existing))
                {
                    if (existing.Value != value)
                    {
                        existing.UpdateValue(value);
                        updated++;
                    }
                }
                else
                {
                    _context.PlanFeatureLimits.Add(new PlanFeatureLimit(plan.Id, featureKey, value));
                    added++;
                }
            }
        }

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation(
            "PlanFeatureLimits: {Added} added, {Updated} updated across {Plans} plans",
            added, updated, plans.Count);
    }
}

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.Constants;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using Sqordia.Infrastructure.Services;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Application.UnitTests.Services.Subscription;

public class FeatureGateServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly FeatureGateService _sut;
    private readonly Guid _orgId = Guid.NewGuid();
    private readonly Guid _planId = Guid.NewGuid();

    public FeatureGateServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"FeatureGate_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        var logger = new Mock<ILogger<FeatureGateService>>();
        _sut = new FeatureGateService(_context, logger.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ── Helpers ──────────────────────────────────────────

    private async Task SeedPlanWithFeaturesAsync(
        SubscriptionPlanType planType,
        Dictionary<string, string>? features = null)
    {
        var plan = new SubscriptionPlan(
            planType, planType.ToString(), "Test plan",
            29m, BillingCycle.Monthly, 2, 3, 1);

        // Use reflection to set Id for testing
        typeof(SubscriptionPlan).BaseType!.BaseType!
            .GetProperty("Id")!.SetValue(plan, _planId);

        _context.SubscriptionPlans.Add(plan);

        var org = new Organization("Test Org", OrganizationType.Startup, null, null);
        typeof(Organization).BaseType!.BaseType!
            .GetProperty("Id")!.SetValue(org, _orgId);
        _context.Organizations.Add(org);

        var sub = new Domain.Entities.Subscription(
            Guid.NewGuid(), _orgId, _planId,
            false, 29m,
            DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1));
        _context.Subscriptions.Add(sub);

        if (features != null)
        {
            foreach (var (key, value) in features)
            {
                _context.PlanFeatureLimits.Add(new PlanFeatureLimit(_planId, key, value));
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedUsageAsync(int plansGenerated = 0, int aiCoachMessages = 0)
    {
        var period = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        var usage = new OrganizationUsage(_orgId, period);
        for (var i = 0; i < plansGenerated; i++) usage.IncrementPlansGenerated();
        for (var i = 0; i < aiCoachMessages; i++) usage.IncrementAiCoachMessages();
        _context.OrganizationUsages.Add(usage);
        await _context.SaveChangesAsync();
    }

    // ── IsFeatureEnabledAsync ────────────────────────────

    [Fact]
    public async Task IsFeatureEnabled_WithTrueValue_ReturnsTrue()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Professional,
            new() { [PlanFeatures.ExportPdf] = "true" });

        var result = await _sut.IsFeatureEnabledAsync(_orgId, PlanFeatures.ExportPdf);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsFeatureEnabled_WithFalseValue_ReturnsFalse()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Free,
            new() { [PlanFeatures.ExportPdf] = "false" });

        var result = await _sut.IsFeatureEnabledAsync(_orgId, PlanFeatures.ExportPdf);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsFeatureEnabled_NoSubscription_ReturnsFreeDefault()
    {
        // No subscription seeded — should fall back to Free defaults
        var orgId = Guid.NewGuid();

        // ExportHtml is true for Free, ExportPdf is false for Free
        var htmlResult = await _sut.IsFeatureEnabledAsync(orgId, PlanFeatures.ExportHtml);
        var pdfResult = await _sut.IsFeatureEnabledAsync(orgId, PlanFeatures.ExportPdf);

        htmlResult.Should().BeTrue();
        pdfResult.Should().BeFalse();
    }

    // ── GetLimitAsync ────────────────────────────────────

    [Fact]
    public async Task GetLimit_WithNumericValue_ReturnsInt()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter,
            new() { [PlanFeatures.MaxBusinessPlans] = "3" });

        var result = await _sut.GetLimitAsync(_orgId, PlanFeatures.MaxBusinessPlans);

        result.Should().Be(3);
    }

    [Fact]
    public async Task GetLimit_WithUnlimited_ReturnsNegativeOne()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Professional,
            new() { [PlanFeatures.MaxBusinessPlans] = "-1" });

        var result = await _sut.GetLimitAsync(_orgId, PlanFeatures.MaxBusinessPlans);

        result.Should().Be(-1);
    }

    [Fact]
    public async Task GetLimit_NoSubscription_ReturnsFreeDefault()
    {
        var orgId = Guid.NewGuid();

        var result = await _sut.GetLimitAsync(orgId, PlanFeatures.MaxBusinessPlans);

        result.Should().Be(1); // Free tier: 1 business plan
    }

    // ── GetFeatureValueAsync ─────────────────────────────

    [Fact]
    public async Task GetFeatureValue_WithStringValue_ReturnsValue()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Enterprise,
            new() { [PlanFeatures.AiProviderTier] = "claude" });

        var result = await _sut.GetFeatureValueAsync(_orgId, PlanFeatures.AiProviderTier);

        result.Should().Be("claude");
    }

    // ── CheckUsageLimitAsync ─────────────────────────────

    [Fact]
    public async Task CheckUsageLimit_UnderLimit_ReturnsAllowed()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter,
            new() { [PlanFeatures.MaxAiGenerationsMonthly] = "5" });
        await SeedUsageAsync(plansGenerated: 2);

        var result = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
        result.Value.CurrentUsage.Should().Be(2);
        result.Value.Limit.Should().Be(5);
        result.Value.UsagePercent.Should().BeApproximately(40, 0.1);
    }

    [Fact]
    public async Task CheckUsageLimit_AtLimit_ReturnsDenied()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter,
            new() { [PlanFeatures.MaxAiGenerationsMonthly] = "5" });
        await SeedUsageAsync(plansGenerated: 5);

        var result = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.CurrentUsage.Should().Be(5);
        result.Value.DenialReason.Should().NotBeNullOrEmpty();
        result.Value.UpgradePrompt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckUsageLimit_OverLimit_ReturnsDenied()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter,
            new() { [PlanFeatures.MaxAiCoachMessagesMonthly] = "50" });
        await SeedUsageAsync(aiCoachMessages: 55);

        var result = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiCoachMessagesMonthly);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.UsagePercent.Should().Be(100);
    }

    [Fact]
    public async Task CheckUsageLimit_Unlimited_ReturnsAllowed()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Enterprise,
            new() { [PlanFeatures.MaxAiGenerationsMonthly] = "-1" });

        var result = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
        result.Value.Limit.Should().Be(-1);
    }

    [Fact]
    public async Task CheckUsageLimit_NearLimit_SetsIsNearLimit()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter,
            new() { [PlanFeatures.MaxAiGenerationsMonthly] = "10" });
        await SeedUsageAsync(plansGenerated: 9); // 90%

        var result = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
        result.Value.IsNearLimit.Should().BeTrue();
    }

    [Fact]
    public async Task CheckUsageLimit_ZeroUsage_ReturnsAllowed()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter,
            new() { [PlanFeatures.MaxAiGenerationsMonthly] = "5" });
        // No usage record at all

        var result = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
        result.Value.CurrentUsage.Should().Be(0);
    }

    // ── RecordUsageAsync ─────────────────────────────────

    [Fact]
    public async Task RecordUsage_CreatesNewUsageRecord_WhenNoneExists()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter, new());

        await _sut.RecordUsageAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        var period = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        var usage = await _context.OrganizationUsages
            .FirstOrDefaultAsync(u => u.OrganizationId == _orgId && u.Period == period);

        usage.Should().NotBeNull();
        usage!.PlansGenerated.Should().Be(1);
    }

    [Fact]
    public async Task RecordUsage_IncrementsExistingRecord()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter, new());
        await SeedUsageAsync(plansGenerated: 3);

        await _sut.RecordUsageAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        var period = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        var usage = await _context.OrganizationUsages
            .FirstOrDefaultAsync(u => u.OrganizationId == _orgId && u.Period == period);

        usage!.PlansGenerated.Should().Be(4);
    }

    [Fact]
    public async Task RecordUsage_CoachMessages_IncrementsCorrectCounter()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter, new());

        await _sut.RecordUsageAsync(_orgId, PlanFeatures.MaxAiCoachMessagesMonthly, 3);

        var period = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        var usage = await _context.OrganizationUsages
            .FirstOrDefaultAsync(u => u.OrganizationId == _orgId && u.Period == period);

        usage!.AiCoachMessages.Should().Be(3);
        usage.PlansGenerated.Should().Be(0);
    }

    [Fact]
    public async Task RecordUsage_Export_IncrementsExportsCounter()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter, new());

        await _sut.RecordUsageAsync(_orgId, PlanFeatures.ExportPdf);

        var period = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        var usage = await _context.OrganizationUsages
            .FirstOrDefaultAsync(u => u.OrganizationId == _orgId && u.Period == period);

        usage!.ExportsGenerated.Should().Be(1);
    }

    // ── GetPlanFeaturesAsync ─────────────────────────────

    [Fact]
    public async Task GetPlanFeatures_WithSubscription_ReturnsFullSnapshot()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Professional,
            new()
            {
                [PlanFeatures.MaxBusinessPlans] = "-1",
                [PlanFeatures.MaxAiGenerationsMonthly] = "30",
                [PlanFeatures.ExportPdf] = "true",
            });
        await SeedUsageAsync(plansGenerated: 10);

        var result = await _sut.GetPlanFeaturesAsync(_orgId);

        result.IsSuccess.Should().BeTrue();
        var snapshot = result.Value!;
        snapshot.PlanType.Should().Be("Professional");
        snapshot.Features[PlanFeatures.MaxBusinessPlans].Should().Be("-1");
        snapshot.Features[PlanFeatures.ExportPdf].Should().Be("true");
        snapshot.Usage[PlanFeatures.MaxAiGenerationsMonthly].Current.Should().Be(10);
        snapshot.Usage[PlanFeatures.MaxAiGenerationsMonthly].Limit.Should().Be(30);
    }

    [Fact]
    public async Task GetPlanFeatures_NoSubscription_ReturnsFreeDefaults()
    {
        var orgId = Guid.NewGuid();

        var result = await _sut.GetPlanFeaturesAsync(orgId);

        result.IsSuccess.Should().BeTrue();
        var snapshot = result.Value!;
        snapshot.PlanType.Should().Be("Free");
        snapshot.Features[PlanFeatures.MaxBusinessPlans].Should().Be("1");
        snapshot.Features[PlanFeatures.ExportPdf].Should().Be("false");
        snapshot.Features[PlanFeatures.ExportHtml].Should().Be("true");
    }

    // ── Integration: Full workflow ───────────────────────

    [Fact]
    public async Task FullWorkflow_CheckThenRecord_TracksProperly()
    {
        await SeedPlanWithFeaturesAsync(SubscriptionPlanType.Starter,
            new() { [PlanFeatures.MaxAiGenerationsMonthly] = "3" });

        // First generation: allowed
        var check1 = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);
        check1.Value!.Allowed.Should().BeTrue();
        await _sut.RecordUsageAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        // Second generation: allowed
        var check2 = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);
        check2.Value!.Allowed.Should().BeTrue();
        await _sut.RecordUsageAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        // Third generation: allowed (at limit)
        var check3 = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);
        check3.Value!.Allowed.Should().BeTrue();
        await _sut.RecordUsageAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);

        // Fourth generation: blocked
        var check4 = await _sut.CheckUsageLimitAsync(_orgId, PlanFeatures.MaxAiGenerationsMonthly);
        check4.Value!.Allowed.Should().BeFalse();
        check4.Value.CurrentUsage.Should().Be(3);
        check4.Value.DenialReason.Should().Contain("limit");
    }
}

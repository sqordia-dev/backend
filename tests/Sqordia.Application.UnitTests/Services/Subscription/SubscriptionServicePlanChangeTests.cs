using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Contracts.Requests;
using Sqordia.Application.Services;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using Sqordia.Infrastructure.Services;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Application.UnitTests.Services.Subscription;

/// <summary>
/// Unit tests for proration and plan-change logic in SubscriptionService.
///
/// Time anchoring strategy: dates are expressed relative to DateTime.UtcNow.Date so
/// that (int)(endDate - DateTime.UtcNow).TotalDays == the intended remainingDays value
/// throughout the entire test run (the cast truncates fractional seconds).
/// </summary>
public class SubscriptionServicePlanChangeTests : IDisposable
{
    // ── Infrastructure ────────────────────────────────────────────────────────

    private readonly ApplicationDbContext _context;
    private readonly Mock<IStripeService> _stripeMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<SubscriptionService>> _loggerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly SubscriptionService _sut;

    // Stable plan IDs – set via reflection to match what FeatureGateServiceTests does
    private readonly Guid _freePlanId       = Guid.NewGuid();
    private readonly Guid _starterPlanId    = Guid.NewGuid();
    private readonly Guid _proPlanId        = Guid.NewGuid();
    private readonly Guid _enterprisePlanId = Guid.NewGuid();

    public SubscriptionServicePlanChangeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"PlanChange_{Guid.NewGuid()}")
            .Options;

        _context           = new ApplicationDbContext(options);
        _stripeMock        = new Mock<IStripeService>();
        _currentUserMock   = new Mock<ICurrentUserService>();
        _loggerMock        = new Mock<ILogger<SubscriptionService>>();
        _configMock        = new Mock<IConfiguration>();

        _sut = new SubscriptionService(
            _context,
            _loggerMock.Object,
            _currentUserMock.Object,
            _stripeMock.Object,
            _configMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ── Seed helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Seeds all four plans into the in-memory database.
    /// </summary>
    private async Task SeedPlansAsync()
    {
        var plans = new[]
        {
            CreatePlan(_freePlanId,       SubscriptionPlanType.Free,         "Decouverte",    0m,   BillingCycle.Monthly, 1,  1,   1),
            CreatePlan(_starterPlanId,    SubscriptionPlanType.Starter,      "Essentiel",    29m,   BillingCycle.Monthly, 3,  5,   5),
            CreatePlan(_proPlanId,        SubscriptionPlanType.Professional, "Professionnel",59m,   BillingCycle.Monthly, 15, 30,  20),
            CreatePlan(_enterprisePlanId, SubscriptionPlanType.Enterprise,   "Entreprise",  149m,   BillingCycle.Monthly, -1, -1, 100),
        };

        _context.SubscriptionPlans.AddRange(plans);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds an active subscription for <paramref name="userId"/> on the given plan,
    /// with a billing period that gives the specified remaining days when the service
    /// calls <see cref="DateTime.UtcNow"/> inside <c>CalculateProration</c>.
    ///
    /// The period length is always 30 days (monthly billing).
    /// </summary>
    private async Task<Domain.Entities.Subscription> SeedActiveSubscriptionAsync(
        Guid userId,
        Guid planId,
        decimal amount,
        int remainingDaysDesired,
        int totalDays = 30)
    {
        // endDate is set to midnight of (today + remainingDaysDesired + 1) so that
        // when UtcNow is evaluated inside the service the fractional difference
        // truncates to exactly remainingDaysDesired.
        var endDate   = DateTime.UtcNow.Date.AddDays(remainingDaysDesired + 1);
        var startDate = endDate.AddDays(-totalDays);

        var sub = new Domain.Entities.Subscription(
            userId,
            Guid.NewGuid(),   // orgId – not relevant for preview/change-plan tests
            planId,
            isYearly: false,
            amount: amount,
            startDate: startDate,
            endDate: endDate);

        SetId(sub, Guid.NewGuid());
        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync();
        return sub;
    }

    // ── Reflection helpers ────────────────────────────────────────────────────

    private static SubscriptionPlan CreatePlan(
        Guid id,
        SubscriptionPlanType planType,
        string name,
        decimal price,
        BillingCycle billingCycle,
        int maxUsers,
        int maxBusinessPlans,
        int maxStorageGB)
    {
        var plan = new SubscriptionPlan(
            planType, name, $"{name} plan",
            price, billingCycle,
            maxUsers, maxBusinessPlans, maxStorageGB);

        SetId(plan, id);
        return plan;
    }

    private static void SetId(object entity, Guid id)
    {
        // BaseEntity < BaseAuditableEntity – Id lives two levels up
        typeof(SubscriptionPlan).BaseType!.BaseType!
            .GetProperty("Id")!
            .SetValue(entity, id);
    }

    // ── PreviewPlanChangeAsync ────────────────────────────────────────────────

    [Fact]
    public async Task PreviewPlanChange_Upgrade_StarterToPro_CalculatesCorrectProration()
    {
        // Arrange
        await SeedPlansAsync();
        var userId = Guid.NewGuid();

        // 15 remaining days out of a 30-day Starter period ($29/mo)
        await SeedActiveSubscriptionAsync(userId, _starterPlanId, 29m, remainingDaysDesired: 15);

        var request = new ChangePlanRequest { NewPlanId = _proPlanId, IsYearly = false };

        // Act
        var result = await _sut.PreviewPlanChangeAsync(userId, request);

        // Assert – result shape
        result.IsSuccess.Should().BeTrue();
        var preview = result.Value!;

        // Upgrade flag
        preview.IsUpgrade.Should().BeTrue();

        // Days
        preview.RemainingDays.Should().Be(15);
        preview.TotalDays.Should().Be(30);

        // Credit  = 29 / 30 * 15 = 14.50
        preview.CreditAmount.Should().Be(14.50m);

        // Charge  = 59 / 30 * 15 = 29.50
        preview.ChargeAmount.Should().Be(29.50m);

        // Net     = 29.50 - 14.50 = 15.00
        preview.NetAmount.Should().Be(15.00m);

        // New recurring amount for a monthly Pro subscription
        preview.NewRecurringAmount.Should().Be(59m);

        // Plan names
        preview.CurrentPlanType.Should().Be("Starter");
        preview.NewPlanType.Should().Be("Professional");

        // Tax: 15.00 * 0.13 = 1.95
        preview.TaxAmount.Should().Be(1.95m);
        preview.TotalWithTax.Should().Be(16.95m);

        // Currency defaults
        preview.Currency.Should().Be("CAD");
    }

    [Fact]
    public async Task PreviewPlanChange_Downgrade_ProToStarter_CalculatesCredit()
    {
        // Arrange
        await SeedPlansAsync();
        var userId = Guid.NewGuid();

        // 20 remaining days of a 30-day Professional period ($59/mo)
        await SeedActiveSubscriptionAsync(userId, _proPlanId, 59m, remainingDaysDesired: 20);

        var request = new ChangePlanRequest { NewPlanId = _starterPlanId, IsYearly = false };

        // Act
        var result = await _sut.PreviewPlanChangeAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var preview = result.Value!;

        // Downgrade
        preview.IsUpgrade.Should().BeFalse();
        preview.RemainingDays.Should().Be(20);

        // Credit  = 59 / 30 * 20 = 39.33 (rounded)
        var expectedCredit = Math.Round(59m / 30m * 20m, 2);

        // Charge  = 29 / 30 * 20 = 19.33 (rounded)
        var expectedCharge = Math.Round(29m / 30m * 20m, 2);

        // Net is negative (refund/credit scenario)
        var expectedNet = Math.Round(expectedCharge - expectedCredit, 2);

        preview.CreditAmount.Should().Be(expectedCredit);
        preview.ChargeAmount.Should().Be(expectedCharge);
        preview.NetAmount.Should().Be(expectedNet);
        preview.NetAmount.Should().BeNegative();

        // Tax is 0 when net <= 0
        preview.TaxAmount.Should().Be(0m);
        preview.TotalWithTax.Should().Be(expectedNet);
    }

    [Fact]
    public async Task PreviewPlanChange_SamePlan_ReturnsError()
    {
        // Arrange
        await SeedPlansAsync();
        var userId = Guid.NewGuid();
        await SeedActiveSubscriptionAsync(userId, _starterPlanId, 29m, remainingDaysDesired: 10);

        var request = new ChangePlanRequest { NewPlanId = _starterPlanId, IsYearly = false };

        // Act
        var result = await _sut.PreviewPlanChangeAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("already on this plan");
    }

    [Fact]
    public async Task PreviewPlanChange_NoSubscription_ReturnsError()
    {
        // Arrange
        await SeedPlansAsync();
        var userId = Guid.NewGuid(); // user has no subscription

        var request = new ChangePlanRequest { NewPlanId = _proPlanId, IsYearly = false };

        // Act
        var result = await _sut.PreviewPlanChangeAsync(userId, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("No active subscription");
    }

    // ── ChangePlanAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePlan_ImmediateSwitch_UpdatesSubscription()
    {
        // Arrange
        await SeedPlansAsync();
        var userId = Guid.NewGuid();
        var original = await SeedActiveSubscriptionAsync(userId, _starterPlanId, 29m, remainingDaysDesired: 15);

        var request = new ChangePlanRequest { NewPlanId = _proPlanId, IsYearly = false };

        // Act
        var result = await _sut.ChangePlanAsync(userId, request);

        // Assert – service call succeeded
        result.IsSuccess.Should().BeTrue();

        // Verify the persisted subscription now references the Pro plan
        var updated = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == original.Id);

        updated.Should().NotBeNull();
        updated!.SubscriptionPlanId.Should().Be(_proPlanId);

        // The amount should equal the new recurring amount (59 for monthly Pro)
        updated.Amount.Should().Be(59m);
    }

    [Fact]
    public async Task ChangePlan_ToFreePlan_NoTax()
    {
        // Arrange
        await SeedPlansAsync();
        var userId = Guid.NewGuid();

        // Start on Starter, switch to Free
        await SeedActiveSubscriptionAsync(userId, _starterPlanId, 29m, remainingDaysDesired: 10);

        var request = new ChangePlanRequest { NewPlanId = _freePlanId, IsYearly = false };

        // Act – preview to inspect tax, then execute the change
        var previewResult = await _sut.PreviewPlanChangeAsync(userId, request);
        var changeResult  = await _sut.ChangePlanAsync(userId, request);

        // Assert – preview reports zero tax for free plan
        previewResult.IsSuccess.Should().BeTrue();
        previewResult.Value!.TaxAmount.Should().Be(0m);

        // Change itself should succeed and subscription updates to free plan
        changeResult.IsSuccess.Should().BeTrue();
        changeResult.Value!.Plan.PlanType.Should().Be("Free");
    }
}

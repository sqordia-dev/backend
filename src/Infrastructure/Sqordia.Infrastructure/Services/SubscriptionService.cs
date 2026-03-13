using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Contracts.Requests;
using Sqordia.Application.Contracts.Responses;
using Sqordia.Application.Services;
using Sqordia.Domain.Constants;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using System.Text.Json;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Subscription service implementation
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStripeService _stripeService;
    private readonly IConfiguration _configuration;

    public SubscriptionService(
        IApplicationDbContext context,
        ILogger<SubscriptionService> logger,
        ICurrentUserService currentUserService,
        IStripeService stripeService,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
        _stripeService = stripeService;
        _configuration = configuration;
    }

    public async Task<Result<List<SubscriptionPlanDto>>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var plans = await _context.SubscriptionPlans
                .Include(p => p.FeatureLimits.Where(f => !f.IsDeleted))
                .Where(p => p.IsActive && !p.IsDeleted)
                .OrderBy(p => p.PlanType)
                .ToListAsync(cancellationToken);

            var planDtos = plans.Select(p => MapPlanToDto(p)).ToList();

            return Result<List<SubscriptionPlanDto>>.Success(planDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription plans");
            return Result.Failure<List<SubscriptionPlanDto>>("Failed to retrieve subscription plans");
        }
    }

    public async Task<Result<SubscriptionDto>> GetCurrentSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription == null)
            {
                return Result.Failure<SubscriptionDto>("No subscription found");
            }

            var dto = MapToDto(subscription);
            return Result<SubscriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current subscription for user {UserId}", userId);
            return Result.Failure<SubscriptionDto>("Failed to retrieve subscription");
        }
    }

    public async Task<Result<SubscriptionDto>> GetOrganizationSubscriptionAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.OrganizationId == organizationId && !s.IsDeleted)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription == null)
            {
                return Result.Failure<SubscriptionDto>("No subscription found for organization");
            }

            var dto = MapToDto(subscription);
            return Result<SubscriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization subscription for {OrganizationId}", organizationId);
            return Result.Failure<SubscriptionDto>("Failed to retrieve subscription");
        }
    }

    public async Task<Result<SubscriptionDto>> SubscribeAsync(Guid userId, SubscribeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if organization already has an active subscription
            var existingSubscription = await _context.Subscriptions
                .Where(s => s.OrganizationId == request.OrganizationId && 
                           s.Status == SubscriptionStatus.Active && 
                           !s.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingSubscription != null)
            {
                return Result.Failure<SubscriptionDto>("Organization already has an active subscription. Please change plan instead.");
            }

            // Verify user belongs to organization
            var isMember = await _context.OrganizationMembers
                .AnyAsync(om => om.UserId == userId && 
                               om.OrganizationId == request.OrganizationId && 
                               !om.IsDeleted, 
                           cancellationToken);

            if (!isMember)
            {
                return Result.Failure<SubscriptionDto>("User is not a member of the specified organization");
            }

            // Get the plan
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive && !p.IsDeleted, cancellationToken);

            if (plan == null)
            {
                return Result.Failure<SubscriptionDto>("Subscription plan not found");
            }

            // Calculate dates
            var startDate = DateTime.UtcNow;
            var endDate = request.IsYearly 
                ? startDate.AddYears(1) 
                : startDate.AddMonths(1);

            // Calculate price
            var price = request.IsYearly && plan.BillingCycle == BillingCycle.Monthly
                ? plan.Price * 12
                : request.IsYearly && plan.BillingCycle == BillingCycle.Yearly
                    ? plan.Price
                    : plan.BillingCycle == BillingCycle.Monthly
                        ? plan.Price
                        : plan.Price / 12;

            // Create subscription
            var subscription = new Subscription(
                userId,
                request.OrganizationId,
                request.PlanId,
                request.IsYearly,
                price,
                startDate,
                endDate,
                isTrial: plan.PlanType == SubscriptionPlanType.Free,
                currency: plan.Currency);

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync(cancellationToken);

            // Reload subscription with plan for DTO mapping
            var subscriptionWithPlan = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == subscription.Id, cancellationToken);

            var dto = MapToDto(subscriptionWithPlan!);
            return Result<SubscriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing user {UserId} to plan {PlanId}", userId, request.PlanId);
            return Result.Failure<SubscriptionDto>("Failed to create subscription");
        }
    }

    public async Task<Result<PlanChangePreviewDto>> PreviewPlanChangeAsync(Guid userId, ChangePlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (subscription, newPlan, error) = await GetChangeContext(userId, request.NewPlanId, cancellationToken);
            if (error != null)
                return Result.Failure<PlanChangePreviewDto>(error);

            var preview = CalculateProration(subscription!, newPlan!, request.IsYearly);
            return Result<PlanChangePreviewDto>.Success(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing plan change for user {UserId}", userId);
            return Result.Failure<PlanChangePreviewDto>("Failed to preview plan change");
        }
    }

    public async Task<Result<SubscriptionDto>> ChangePlanAsync(Guid userId, ChangePlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (subscription, newPlan, error) = await GetChangeContext(userId, request.NewPlanId, cancellationToken);
            if (error != null)
                return Result.Failure<SubscriptionDto>(error);

            var currentSubscription = subscription!;
            var preview = CalculateProration(currentSubscription, newPlan!, request.IsYearly);

            // --- Stripe-managed subscription: update via Stripe API ---
            if (!string.IsNullOrEmpty(currentSubscription.StripeSubscriptionId))
            {
                var planType = newPlan!.PlanType.ToString();
                var billingCycle = request.IsYearly ? "Yearly" : "Monthly";
                var priceId = _configuration[$"Stripe:PriceIds:{planType}:{billingCycle}"];

                if (string.IsNullOrEmpty(priceId))
                    return Result.Failure<SubscriptionDto>($"Stripe price not configured for {planType} {billingCycle}");

                var stripeResult = await _stripeService.UpdateSubscriptionAsync(
                    currentSubscription.StripeSubscriptionId, priceId, cancellationToken);

                if (!stripeResult.IsSuccess)
                    return Result.Failure<SubscriptionDto>(stripeResult.Error?.Message ?? "Failed to update Stripe subscription");

                // Sync local record with Stripe
                var stripeInfo = await _stripeService.GetSubscriptionAsync(
                    currentSubscription.StripeSubscriptionId, cancellationToken);

                if (stripeInfo.IsSuccess)
                {
                    currentSubscription.SetStripeIds(
                        stripeInfo.Value!.CustomerId,
                        stripeInfo.Value.SubscriptionId,
                        priceId);
                }
            }

            // --- Update local subscription immediately ---
            currentSubscription.ChangePlan(request.NewPlanId, preview.NewRecurringAmount, request.IsYearly);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} changed from {OldPlan} to {NewPlan}. Net proration: {Net} {Currency}",
                userId, preview.CurrentPlanType, preview.NewPlanType, preview.NetAmount, preview.Currency);

            // Reload with navigation
            var updated = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == currentSubscription.Id, cancellationToken);

            return Result<SubscriptionDto>.Success(MapToDto(updated!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing plan for user {UserId}", userId);
            return Result.Failure<SubscriptionDto>("Failed to change subscription plan");
        }
    }

    // ── Plan change helpers ──────────────────────────────

    private async Task<(Subscription?, SubscriptionPlan?, string?)> GetChangeContext(
        Guid userId, Guid newPlanId, CancellationToken ct)
    {
        var currentSubscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId &&
                       s.Status == SubscriptionStatus.Active &&
                       !s.IsDeleted)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync(ct);

        if (currentSubscription == null)
            return (null, null, "No active subscription found");

        if (currentSubscription.SubscriptionPlanId == newPlanId)
            return (null, null, "You are already on this plan");

        var newPlan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == newPlanId && p.IsActive && !p.IsDeleted, ct);

        if (newPlan == null)
            return (null, null, "Subscription plan not found");

        return (currentSubscription, newPlan, null);
    }

    private static PlanChangePreviewDto CalculateProration(
        Subscription current, SubscriptionPlan newPlan, bool isYearly)
    {
        var now = DateTime.UtcNow;
        var totalDays = (int)Math.Max((current.EndDate - current.StartDate).TotalDays, 1);
        var remainingDays = (int)Math.Max((current.EndDate - now).TotalDays, 0);

        // Current plan daily rate
        var currentDailyRate = current.Amount / totalDays;

        // New plan full-period price
        var newFullPrice = isYearly && newPlan.BillingCycle == BillingCycle.Monthly
            ? newPlan.Price * 12
            : isYearly && newPlan.BillingCycle == BillingCycle.Yearly
                ? newPlan.Price
                : newPlan.BillingCycle == BillingCycle.Monthly
                    ? newPlan.Price
                    : newPlan.Price / 12;

        // New plan total days for the upcoming period
        var newTotalDays = isYearly ? 365 : 30;
        var newDailyRate = newFullPrice / newTotalDays;

        // Proration
        var credit = Math.Round(currentDailyRate * remainingDays, 2);
        var charge = Math.Round(newDailyRate * remainingDays, 2);
        var netAmount = Math.Round(charge - credit, 2);

        var isUpgrade = newPlan.PlanType > current.Plan.PlanType;

        // Tax (13% HST, 0% for free plans)
        var taxRate = newPlan.PlanType == SubscriptionPlanType.Free ? 0m : 0.13m;
        var taxOnNet = netAmount > 0 ? Math.Round(netAmount * taxRate, 2) : 0m;

        return new PlanChangePreviewDto
        {
            CurrentPlanName = current.Plan.Name,
            NewPlanName = newPlan.Name,
            CurrentPlanType = current.Plan.PlanType.ToString(),
            NewPlanType = newPlan.PlanType.ToString(),
            IsUpgrade = isUpgrade,
            RemainingDays = remainingDays,
            TotalDays = totalDays,
            CurrentPeriodEnd = current.EndDate,
            CreditAmount = credit,
            ChargeAmount = charge,
            NetAmount = netAmount,
            NewRecurringAmount = newFullPrice,
            Currency = current.Currency,
            IsYearly = isYearly,
            EffectiveDate = now,
            NewPeriodEnd = current.EndDate, // Keeps same end date for current period
            TaxAmount = taxOnNet,
            TotalWithTax = netAmount + taxOnNet
        };
    }

    public async Task<Result<bool>> CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Where(s => s.UserId == userId && 
                           s.Status == SubscriptionStatus.Active && 
                           !s.IsDeleted)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription == null)
            {
                return Result.Failure<bool>("No active subscription found");
            }

            subscription.Cancel(subscription.EndDate);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription for user {UserId}", userId);
            return Result.Failure<bool>("Failed to cancel subscription");
        }
    }

    public async Task<Result<List<InvoiceDto>>> GetInvoicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync(cancellationToken);

            var invoices = subscriptions.Select((subscription, index) => MapSubscriptionToInvoice(subscription, index)).ToList();

            // Enrich Stripe-backed subscriptions with hosted invoice URLs
            foreach (var subscription in subscriptions.Where(s => !string.IsNullOrEmpty(s.StripeSubscriptionId)))
            {
                try
                {
                    var stripeInvoices = await _stripeService.GetInvoicesForSubscriptionAsync(
                        subscription.StripeSubscriptionId!, cancellationToken);

                    if (stripeInvoices.IsSuccess && stripeInvoices.Value?.Count > 0)
                    {
                        var invoice = invoices.FirstOrDefault(i => i.SubscriptionId == subscription.Id);
                        if (invoice != null)
                        {
                            // Use the latest Stripe invoice's hosted URL
                            var latestStripeInvoice = stripeInvoices.Value.First();
                            invoice.PdfUrl = latestStripeInvoice.HostedUrl;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch Stripe invoices for subscription {SubscriptionId}, falling back to generated PDF",
                        subscription.Id);
                }
            }

            return Result<List<InvoiceDto>>.Success(invoices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoices for user {UserId}", userId);
            return Result.Failure<List<InvoiceDto>>("Failed to retrieve invoices");
        }
    }

    private InvoiceDto MapSubscriptionToInvoice(Subscription subscription, int index)
    {
        // Generate invoice number: INV-{YYYYMMDD}-{SubscriptionId first 8 chars}
        var invoiceNumber = $"INV-{subscription.StartDate:yyyyMMdd}-{subscription.Id.ToString("N")[..8].ToUpperInvariant()}";
        
        // Calculate tax (13% HST for Canada, 0% for free plans)
        var taxRate = subscription.Plan.PlanType == SubscriptionPlanType.Free ? 0m : 0.13m;
        var subtotal = subscription.Amount;
        var tax = subtotal * taxRate;
        var total = subtotal + tax;
        
        // Determine invoice status
        string status;
        DateTime? paidDate = null;
        
        if (subscription.IsTrial)
        {
            status = "paid"; // Free trials are considered paid
            paidDate = subscription.StartDate;
        }
        else if (subscription.Status == SubscriptionStatus.Active)
        {
            // If subscription is active and we're past the start date, consider it paid
            if (DateTime.UtcNow >= subscription.StartDate)
            {
                status = "paid";
                paidDate = subscription.StartDate;
            }
            else
            {
                status = "pending";
            }
        }
        else if (subscription.Status == SubscriptionStatus.Cancelled)
        {
            status = subscription.CancelledAt.HasValue && subscription.CancelledAt.Value <= subscription.EndDate 
                ? "paid" 
                : "pending";
            if (status == "paid")
            {
                paidDate = subscription.StartDate;
            }
        }
        else
        {
            status = "paid"; // Default to paid for historical subscriptions
            paidDate = subscription.StartDate;
        }
        
        // Generate description
        var billingPeriod = subscription.IsYearly ? "Yearly" : "Monthly";
        var description = $"{subscription.Plan.Name} - {billingPeriod} Subscription ({subscription.StartDate:MMM yyyy} - {subscription.EndDate:MMM yyyy})";
        
        return new InvoiceDto
        {
            Id = subscription.Id, // Use subscription ID as invoice ID for consistency
            SubscriptionId = subscription.Id,
            InvoiceNumber = invoiceNumber,
            IssueDate = subscription.StartDate,
            DueDate = subscription.StartDate.AddDays(30), // 30 days payment terms
            PaidDate = paidDate,
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            Currency = subscription.Currency,
            Status = status,
            PeriodStart = subscription.StartDate,
            PeriodEnd = subscription.EndDate,
            Description = description,
            PdfUrl = null // PDF generation can be added later
        };
    }

    private SubscriptionDto MapToDto(Subscription subscription)
    {
        return new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            OrganizationId = subscription.OrganizationId,
            SubscriptionPlanId = subscription.SubscriptionPlanId,
            Plan = MapPlanToDto(subscription.Plan),
            Status = subscription.Status.ToString(),
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            CancelledAt = subscription.CancelledAt,
            CancelledEffectiveDate = subscription.CancelledEffectiveDate,
            IsYearly = subscription.IsYearly,
            Amount = subscription.Amount,
            Currency = subscription.Currency,
            IsTrial = subscription.IsTrial,
            TrialEndDate = subscription.TrialEndDate,
            IsActive = subscription.IsActive()
        };
    }

    private SubscriptionPlanDto MapPlanToDto(SubscriptionPlan plan)
    {
        var features = ParseFeatures(plan.Features);
        var planType = plan.PlanType;

        // Load feature limits from PlanFeatureLimits (if eager-loaded) or use tier defaults
        var featureLimits = plan.FeatureLimits?.ToDictionary(f => f.FeatureKey, f => f.Value)
                            ?? new Dictionary<string, string>();

        int GetNumericFeature(string key, int fallback) =>
            featureLimits.TryGetValue(key, out var v) && int.TryParse(v, out var i) ? i : fallback;

        bool GetBoolFeature(string key, bool fallback) =>
            featureLimits.TryGetValue(key, out var v) ? bool.TryParse(v, out var b) && b : fallback;

        string GetStringFeature(string key, string fallback) =>
            featureLimits.TryGetValue(key, out var v) ? v : fallback;

        // Tier-based defaults when PlanFeatureLimits not yet seeded
        var (defMaxOrg, defMaxMembers, defMaxPlans, defMaxGenMo, defMaxCoachMo, defStorageMb) = planType switch
        {
            SubscriptionPlanType.Free => (1, 1, 1, 1, 10, 100),
            SubscriptionPlanType.Starter => (2, 3, 5, 5, 50, 1024),
            SubscriptionPlanType.Professional => (5, 15, 30, 15, 200, 5120),
            SubscriptionPlanType.Enterprise => (-1, -1, -1, -1, -1, 51200),
            _ => (1, 1, 1, 1, 10, 100)
        };

        var maxOrganizations = GetNumericFeature(PlanFeatures.MaxOrganizations, defMaxOrg);
        var maxTeamMembers = GetNumericFeature(PlanFeatures.MaxTeamMembers, defMaxMembers);
        var maxBusinessPlans = GetNumericFeature(PlanFeatures.MaxBusinessPlans, defMaxPlans);
        var maxAiGenMonthly = GetNumericFeature(PlanFeatures.MaxAiGenerationsMonthly, defMaxGenMo);
        var maxCoachMonthly = GetNumericFeature(PlanFeatures.MaxAiCoachMessagesMonthly, defMaxCoachMo);

        var isPaid = planType != SubscriptionPlanType.Free;
        var isPro = planType >= SubscriptionPlanType.Professional;
        var isEnterprise = planType == SubscriptionPlanType.Enterprise;

        int? displayOrder = planType switch
        {
            SubscriptionPlanType.Free => 0,
            SubscriptionPlanType.Starter => 1,
            SubscriptionPlanType.Professional => 2,
            SubscriptionPlanType.Enterprise => 3,
            _ => null
        };

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            PlanType = planType.ToString(),
            Name = plan.Name,
            Description = plan.Description,
            MonthlyPrice = plan.BillingCycle == BillingCycle.Monthly ? plan.Price : plan.Price / 12,
            YearlyPrice = plan.BillingCycle == BillingCycle.Yearly ? plan.Price : plan.Price * 12,
            Currency = plan.Currency,
            MaxUsers = plan.MaxUsers,
            MaxBusinessPlans = maxBusinessPlans,
            MaxStorageGB = plan.MaxStorageGB,
            IsActive = plan.IsActive,
            DisplayOrder = displayOrder,

            // Numeric limits
            MaxOrganizations = maxOrganizations,
            MaxTeamMembers = maxTeamMembers,
            MaxAiGenerationsMonthly = maxAiGenMonthly,
            MaxAiCoachMessagesMonthly = maxCoachMonthly,

            // Exports
            HasExportHtml = true,
            HasExportPDF = GetBoolFeature(PlanFeatures.ExportPdf, isPaid),
            HasExportWord = GetBoolFeature(PlanFeatures.ExportWord, isPaid),
            HasExportPowerpoint = GetBoolFeature(PlanFeatures.ExportPowerpoint, isPro),
            HasExportExcel = GetBoolFeature(PlanFeatures.ExportExcel, isPro),
            HasExportAgentBlueprints = GetBoolFeature(PlanFeatures.ExportAgentBlueprints, isPaid),

            // AI
            AiProviderTier = GetStringFeature(PlanFeatures.AiProviderTier, isEnterprise ? "claude" : isPro ? "blended" : "gemini"),
            HasAdvancedAI = isPaid,
            HasPrioritySectionsClaude = GetBoolFeature(PlanFeatures.PrioritySectionsClaude, isPro),

            // Financial
            HasFinancialProjectionsBasic = true,
            HasFinancialProjectionsAdvanced = GetBoolFeature(PlanFeatures.FinancialProjectionsAdvanced, isPro),

            // Premium
            HasCustomBranding = GetBoolFeature(PlanFeatures.CustomBranding, isEnterprise),
            HasAPIAccess = GetBoolFeature(PlanFeatures.ApiAccess, isEnterprise),
            HasPrioritySupport = GetBoolFeature(PlanFeatures.PrioritySupport, isPro),
            HasDedicatedSupport = GetBoolFeature(PlanFeatures.DedicatedSupport, isEnterprise),
            HasWhiteLabel = GetBoolFeature(PlanFeatures.WhiteLabel, isEnterprise),

            Features = features
        };
    }

    private List<string> ParseFeatures(string featuresJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(featuresJson))
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(featuresJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}


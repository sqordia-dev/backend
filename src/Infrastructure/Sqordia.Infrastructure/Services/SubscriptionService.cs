using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Contracts.Requests;
using Sqordia.Application.Contracts.Responses;
using Sqordia.Application.Services;
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

    public async Task<Result<SubscriptionDto>> ChangePlanAsync(Guid userId, ChangePlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current subscription
            var currentSubscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId && 
                           s.Status == SubscriptionStatus.Active && 
                           !s.IsDeleted)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentSubscription == null)
            {
                return Result.Failure<SubscriptionDto>("No active subscription found");
            }

            // Get new plan
            var newPlan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == request.NewPlanId && p.IsActive && !p.IsDeleted, cancellationToken);

            if (newPlan == null)
            {
                return Result.Failure<SubscriptionDto>("Subscription plan not found");
            }

            // Cancel current subscription at end of billing period
            currentSubscription.Cancel(currentSubscription.EndDate);

            // Calculate new subscription dates
            var startDate = currentSubscription.EndDate;
            var endDate = request.IsYearly 
                ? startDate.AddYears(1) 
                : startDate.AddMonths(1);

            // Calculate price
            var price = request.IsYearly && newPlan.BillingCycle == BillingCycle.Monthly
                ? newPlan.Price * 12
                : request.IsYearly && newPlan.BillingCycle == BillingCycle.Yearly
                    ? newPlan.Price
                    : newPlan.BillingCycle == BillingCycle.Monthly
                        ? newPlan.Price
                        : newPlan.Price / 12;

            // Create new subscription
            var newSubscription = new Subscription(
                userId,
                currentSubscription.OrganizationId,
                request.NewPlanId,
                request.IsYearly,
                price,
                startDate,
                endDate,
                isTrial: newPlan.PlanType == SubscriptionPlanType.Free,
                currency: newPlan.Currency);

            _context.Subscriptions.Add(newSubscription);
            await _context.SaveChangesAsync(cancellationToken);

            // Reload subscription with plan for DTO mapping
            var newSubscriptionWithPlan = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == newSubscription.Id, cancellationToken);

            var dto = MapToDto(newSubscriptionWithPlan!);
            return Result<SubscriptionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing plan for user {UserId}", userId);
            return Result.Failure<SubscriptionDto>("Failed to change subscription plan");
        }
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
            Id = Guid.NewGuid(), // Generate a unique ID for the invoice view
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
        
        // Map features to boolean flags based on plan type and features
        // Pro and Enterprise plans have export options
        var hasExportPDF = planType != SubscriptionPlanType.Free;
        var hasExportWord = planType != SubscriptionPlanType.Free;
        var hasExportExcel = planType != SubscriptionPlanType.Free;
        
        // Advanced AI is available for Pro and Enterprise
        var hasAdvancedAI = planType != SubscriptionPlanType.Free;
        
        // Priority support for Pro and Enterprise (Dedicated for Enterprise)
        var hasPrioritySupport = features.Any(f => f.Contains("Priority", StringComparison.OrdinalIgnoreCase) || 
                                                    f.Contains("Dedicated", StringComparison.OrdinalIgnoreCase)) ||
                                 planType != SubscriptionPlanType.Free;
        
        // Custom branding only for Enterprise
        var hasCustomBranding = features.Any(f => f.Contains("Branding", StringComparison.OrdinalIgnoreCase) || 
                                                   f.Contains("Custom", StringComparison.OrdinalIgnoreCase)) ||
                                planType == SubscriptionPlanType.Enterprise;
        
        // API Access only for Enterprise
        var hasAPIAccess = features.Any(f => f.Contains("API", StringComparison.OrdinalIgnoreCase)) ||
                          planType == SubscriptionPlanType.Enterprise;
        
        // Determine max organizations and team members based on plan type
        int maxOrganizations;
        int maxTeamMembers;
        
        switch (planType)
        {
            case SubscriptionPlanType.Free:
                maxOrganizations = 1;
                maxTeamMembers = 1;
                break;
            case SubscriptionPlanType.Pro:
                maxOrganizations = 5;
                maxTeamMembers = plan.MaxUsers;
                break;
            case SubscriptionPlanType.Enterprise:
                maxOrganizations = 999999; // Unlimited
                maxTeamMembers = plan.MaxUsers;
                break;
            default:
                maxOrganizations = 1;
                maxTeamMembers = plan.MaxUsers;
                break;
        }
        
        // Determine display order
        int? displayOrder = planType switch
        {
            SubscriptionPlanType.Free => 0,
            SubscriptionPlanType.Pro => 1,
            SubscriptionPlanType.Enterprise => 2,
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
            MaxBusinessPlans = plan.MaxBusinessPlans,
            MaxStorageGB = plan.MaxStorageGB,
            Features = features,
            IsActive = plan.IsActive,
            MaxOrganizations = maxOrganizations,
            MaxTeamMembers = maxTeamMembers,
            HasAdvancedAI = hasAdvancedAI,
            HasExportPDF = hasExportPDF,
            HasExportWord = hasExportWord,
            HasExportExcel = hasExportExcel,
            HasPrioritySupport = hasPrioritySupport,
            HasCustomBranding = hasCustomBranding,
            HasAPIAccess = hasAPIAccess,
            DisplayOrder = displayOrder
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


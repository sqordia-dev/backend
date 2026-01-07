using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Domain.Entities;

/// <summary>
/// User/Organization subscription to a plan
/// </summary>
public class Subscription : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid SubscriptionPlanId { get; private set; }
    
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? CancelledEffectiveDate { get; private set; }
    
    public bool IsYearly { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "CAD";
    
    public bool IsTrial { get; private set; }
    public DateTime? TrialEndDate { get; private set; }
    
    // Stripe integration
    public string? StripeCustomerId { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public string? StripePriceId { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public Organization Organization { get; private set; } = null!;
    public SubscriptionPlan Plan { get; private set; } = null!;
    
    private Subscription() { } // EF Core constructor
    
    public Subscription(
        Guid userId,
        Guid organizationId,
        Guid subscriptionPlanId,
        bool isYearly,
        decimal amount,
        DateTime startDate,
        DateTime endDate,
        bool isTrial = false,
        string currency = "CAD")
    {
        UserId = userId;
        OrganizationId = organizationId;
        SubscriptionPlanId = subscriptionPlanId;
        IsYearly = isYearly;
        Amount = amount;
        Currency = currency;
        StartDate = startDate;
        EndDate = endDate;
        Status = SubscriptionStatus.Active;
        IsTrial = isTrial;
        
        if (isTrial)
        {
            TrialEndDate = endDate;
        }
    }
    
    public void Cancel(DateTime effectiveDate)
    {
        if (Status == SubscriptionStatus.Active)
        {
            Status = SubscriptionStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancelledEffectiveDate = effectiveDate;
        }
    }
    
    public void Expire()
    {
        if (Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Cancelled)
        {
            Status = SubscriptionStatus.Expired;
        }
    }
    
    public void Suspend()
    {
        if (Status == SubscriptionStatus.Active)
        {
            Status = SubscriptionStatus.Suspended;
        }
    }
    
    public void Reactivate()
    {
        if (Status == SubscriptionStatus.Suspended)
        {
            Status = SubscriptionStatus.Active;
        }
    }
    
    public void Renew(DateTime newEndDate)
    {
        if (Status == SubscriptionStatus.Active)
        {
            EndDate = newEndDate;
        }
    }
    
    public void SetStripeIds(string customerId, string subscriptionId, string? priceId = null)
    {
        StripeCustomerId = customerId;
        StripeSubscriptionId = subscriptionId;
        if (!string.IsNullOrEmpty(priceId))
        {
            StripePriceId = priceId;
        }
    }
    
    public bool IsActive()
    {
        return Status == SubscriptionStatus.Active && 
               DateTime.UtcNow >= StartDate && 
               DateTime.UtcNow <= EndDate;
    }
}


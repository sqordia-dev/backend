using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Subscription plan definition
/// </summary>
public class SubscriptionPlan : BaseAuditableEntity
{
    public SubscriptionPlanType PlanType { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "CAD";
    public BillingCycle BillingCycle { get; private set; }
    
    // Limits
    public int MaxUsers { get; private set; }
    public int MaxBusinessPlans { get; private set; }
    public int MaxStorageGB { get; private set; }
    
    // Features (stored as JSON)
    public string Features { get; private set; } = "[]";
    
    public bool IsActive { get; private set; }
    
    // Navigation properties
    public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();
    
    private SubscriptionPlan() { } // EF Core constructor
    
    public SubscriptionPlan(
        SubscriptionPlanType planType,
        string name,
        string description,
        decimal price,
        BillingCycle billingCycle,
        int maxUsers,
        int maxBusinessPlans,
        int maxStorageGB,
        string features = "[]",
        string currency = "CAD")
    {
        PlanType = planType;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Price = price;
        Currency = currency;
        BillingCycle = billingCycle;
        MaxUsers = maxUsers;
        MaxBusinessPlans = maxBusinessPlans;
        MaxStorageGB = maxStorageGB;
        Features = features;
        IsActive = true;
    }
    
    public void UpdatePrice(decimal price)
    {
        Price = price;
    }
    
    public void UpdateLimits(int maxUsers, int maxBusinessPlans, int maxStorageGB)
    {
        MaxUsers = maxUsers;
        MaxBusinessPlans = maxBusinessPlans;
        MaxStorageGB = maxStorageGB;
    }
    
    public void UpdateFeatures(string features)
    {
        Features = features ?? "[]";
    }
    
    public void Activate()
    {
        IsActive = true;
    }
    
    public void Deactivate()
    {
        IsActive = false;
    }
}


namespace Sqordia.Domain.Enums;

/// <summary>
/// Subscription status
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// Active subscription
    /// </summary>
    Active = 0,
    
    /// <summary>
    /// Cancelled subscription (still active until end date)
    /// </summary>
    Cancelled = 1,
    
    /// <summary>
    /// Expired subscription
    /// </summary>
    Expired = 2,
    
    /// <summary>
    /// Suspended subscription
    /// </summary>
    Suspended = 3
}


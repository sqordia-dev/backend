using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Services;

/// <summary>
/// Stripe payment service interface
/// </summary>
public interface IStripeService
{
    /// <summary>
    /// Create a Stripe customer
    /// </summary>
    Task<Result<string>> CreateCustomerAsync(string email, string name, Guid userId, Guid organizationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a checkout session for subscription
    /// </summary>
    Task<Result<string>> CreateCheckoutSessionAsync(
        string customerId,
        string priceId,
        Guid subscriptionPlanId,
        Guid organizationId,
        bool isYearly,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a billing portal session
    /// </summary>
    Task<Result<string>> CreateBillingPortalSessionAsync(
        string customerId,
        string returnUrl,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get customer by email
    /// </summary>
    Task<Result<string?>> GetCustomerIdByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancel a Stripe subscription
    /// </summary>
    Task<Result<bool>> CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update subscription plan
    /// </summary>
    Task<Result<bool>> UpdateSubscriptionAsync(
        string subscriptionId,
        string newPriceId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get subscription by ID
    /// </summary>
    Task<Result<StripeSubscriptionInfo>> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Stripe subscription information
/// </summary>
public class StripeSubscriptionInfo
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PriceId { get; set; } = string.Empty;
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
}


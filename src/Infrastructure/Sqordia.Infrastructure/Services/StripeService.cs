using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Stripe;
using Stripe.Checkout;
using StripeSubscriptionService = Stripe.SubscriptionService;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Stripe payment service implementation
/// </summary>
public class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;

    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Initialize Stripe API key
        var secretKey = _configuration["Stripe:SecretKey"];
        if (!string.IsNullOrEmpty(secretKey))
        {
            StripeConfiguration.ApiKey = secretKey;
        }
    }

    public async Task<Result<string>> CreateCustomerAsync(
        string email,
        string name,
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customerService = new CustomerService();
            
            var customerOptions = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() },
                    { "organizationId", organizationId.ToString() }
                }
            };

            var customer = await customerService.CreateAsync(customerOptions, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Created Stripe customer {CustomerId} for user {UserId}", customer.Id, userId);
            
            return Result<string>.Success(customer.Id);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating customer for user {UserId}: {Message}", userId, ex.Message);
            return Result.Failure<string>($"Failed to create Stripe customer: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Stripe customer for user {UserId}", userId);
            return Result.Failure<string>("Failed to create Stripe customer");
        }
    }

    public async Task<Result<string>> CreateCheckoutSessionAsync(
        string customerId,
        string priceId,
        Guid subscriptionPlanId,
        Guid organizationId,
        bool isYearly,
        string successUrl,
        string cancelUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionService = new SessionService();
            
            var sessionOptions = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "subscriptionPlanId", subscriptionPlanId.ToString() },
                    { "organizationId", organizationId.ToString() },
                    { "isYearly", isYearly.ToString() }
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "subscriptionPlanId", subscriptionPlanId.ToString() },
                        { "organizationId", organizationId.ToString() },
                        { "isYearly", isYearly.ToString() }
                    }
                }
            };

            var session = await sessionService.CreateAsync(sessionOptions, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Created Stripe checkout session {SessionId} for organization {OrganizationId}", 
                session.Id, organizationId);
            
            return Result<string>.Success(session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating checkout session: {Message}", ex.Message);
            return Result.Failure<string>($"Failed to create checkout session: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
            return Result.Failure<string>("Failed to create checkout session");
        }
    }

    public async Task<Result<string>> CreateBillingPortalSessionAsync(
        string customerId,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionService = new Stripe.BillingPortal.SessionService();
            
            var sessionOptions = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = returnUrl
            };

            var session = await sessionService.CreateAsync(sessionOptions, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Created Stripe billing portal session {SessionId} for customer {CustomerId}", 
                session.Id, customerId);
            
            return Result<string>.Success(session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating billing portal session: {Message}", ex.Message);
            return Result.Failure<string>($"Failed to create billing portal session: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating billing portal session");
            return Result.Failure<string>("Failed to create billing portal session");
        }
    }

    public async Task<Result<string?>> GetCustomerIdByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customerService = new CustomerService();
            var customers = await customerService.ListAsync(
                new CustomerListOptions { Email = email },
                cancellationToken: cancellationToken);

            var customer = customers.Data.FirstOrDefault();
            return Result<string?>.Success(customer?.Id);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting customer by email: {Message}", ex.Message);
            return Result.Failure<string?>("Failed to get customer");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by email");
            return Result.Failure<string?>("Failed to get customer");
        }
    }

    public async Task<Result<bool>> CancelSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionService = new StripeSubscriptionService();
            
            var subscription = await subscriptionService.CancelAsync(subscriptionId, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Cancelled Stripe subscription {SubscriptionId}", subscriptionId);
            
            return Result<bool>.Success(true);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error cancelling subscription: {Message}", ex.Message);
            return Result.Failure<bool>($"Failed to cancel subscription: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription");
            return Result.Failure<bool>("Failed to cancel subscription");
        }
    }

    public async Task<Result<bool>> UpdateSubscriptionAsync(
        string subscriptionId,
        string newPriceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionService = new StripeSubscriptionService();
            
            var subscription = await subscriptionService.GetAsync(subscriptionId, cancellationToken: cancellationToken);
            
            var updateOptions = new Stripe.SubscriptionUpdateOptions
            {
                Items = new List<Stripe.SubscriptionItemOptions>
                {
                    new Stripe.SubscriptionItemOptions
                    {
                        Id = subscription.Items.Data[0].Id,
                        Price = newPriceId
                    }
                },
                ProrationBehavior = "create_prorations"
            };

            await subscriptionService.UpdateAsync(subscriptionId, updateOptions, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Updated Stripe subscription {SubscriptionId} to price {PriceId}", 
                subscriptionId, newPriceId);
            
            return Result<bool>.Success(true);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error updating subscription: {Message}", ex.Message);
            return Result.Failure<bool>($"Failed to update subscription: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription");
            return Result.Failure<bool>("Failed to update subscription");
        }
    }

    public async Task<Result<StripeSubscriptionInfo>> GetSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionService = new StripeSubscriptionService();
            var subscription = await subscriptionService.GetAsync(subscriptionId, cancellationToken: cancellationToken);
            
            var info = new StripeSubscriptionInfo
            {
                SubscriptionId = subscription.Id,
                CustomerId = subscription.CustomerId,
                Status = subscription.Status,
                PriceId = subscription.Items.Data[0].Price.Id,
                CurrentPeriodStart = subscription.CurrentPeriodStart,
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                CancelAtPeriodEnd = subscription.CancelAtPeriodEnd
            };
            
            return Result<StripeSubscriptionInfo>.Success(info);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting subscription: {Message}", ex.Message);
            return Result.Failure<StripeSubscriptionInfo>($"Failed to get subscription: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription");
            return Result.Failure<StripeSubscriptionInfo>("Failed to get subscription");
        }
    }
}


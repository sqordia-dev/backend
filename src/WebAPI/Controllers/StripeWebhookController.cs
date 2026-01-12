using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using Stripe;
using System.Text;

namespace WebAPI.Controllers;

/// <summary>
/// Stripe webhook controller for handling Stripe events
/// </summary>
[ApiController]
[Route("api/v1/stripe/webhook")]
public class StripeWebhookController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<StripeWebhookController> _logger;
    private readonly IConfiguration _configuration;

    public StripeWebhookController(
        IApplicationDbContext context,
        ILogger<StripeWebhookController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Handle Stripe webhook events
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret))
        {
            _logger.LogWarning("Stripe webhook secret is not configured");
            return BadRequest("Webhook secret not configured");
        }

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret);

            _logger.LogInformation("Received Stripe webhook event: {EventType} (ID: {EventId})", 
                stripeEvent.Type, stripeEvent.Id);

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted(stripeEvent, cancellationToken);
                    break;

                case "customer.subscription.created":
                case "customer.subscription.updated":
                    await HandleSubscriptionUpdated(stripeEvent, cancellationToken);
                    break;

                case "customer.subscription.deleted":
                    await HandleSubscriptionDeleted(stripeEvent, cancellationToken);
                    break;

                case "invoice.payment_succeeded":
                    await HandleInvoicePaymentSucceeded(stripeEvent, cancellationToken);
                    break;

                case "invoice.payment_failed":
                    await HandleInvoicePaymentFailed(stripeEvent, cancellationToken);
                    break;

                default:
                    _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error: {Message}", ex.Message);
            return BadRequest($"Webhook error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent, CancellationToken cancellationToken)
    {
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        if (session == null) return;

        _logger.LogInformation("Checkout session completed: {SessionId}", session.Id);

        // Extract metadata
        var subscriptionPlanIdStr = session.Metadata?.GetValueOrDefault("subscriptionPlanId");
        var organizationIdStr = session.Metadata?.GetValueOrDefault("organizationId");
        var isYearlyStr = session.Metadata?.GetValueOrDefault("isYearly");

        if (string.IsNullOrEmpty(subscriptionPlanIdStr) || string.IsNullOrEmpty(organizationIdStr))
        {
            _logger.LogWarning("Missing metadata in checkout session {SessionId}", session.Id);
            return;
        }

        if (!Guid.TryParse(subscriptionPlanIdStr, out var subscriptionPlanId) ||
            !Guid.TryParse(organizationIdStr, out var organizationId) ||
            !bool.TryParse(isYearlyStr, out var isYearly))
        {
            _logger.LogWarning("Invalid metadata in checkout session {SessionId}", session.Id);
            return;
        }

        // Get the subscription from Stripe
        var stripeSubscriptionService = new Stripe.SubscriptionService();
        var stripeSubscription = await stripeSubscriptionService.GetAsync(session.SubscriptionId, cancellationToken: cancellationToken);

        // Get organization and find the owner/admin user
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);

        if (organization == null)
        {
            _logger.LogWarning("Organization {OrganizationId} not found for checkout session {SessionId}", organizationId, session.Id);
            return;
        }

        // Get the first admin/owner user from the organization
        var organizationMember = await _context.OrganizationMembers
            .Include(om => om.User)
            .Where(om => om.OrganizationId == organizationId && 
                        (om.Role == OrganizationRole.Owner || om.Role == OrganizationRole.Admin) &&
                        !om.IsDeleted)
            .OrderBy(om => om.Role == OrganizationRole.Owner ? 0 : 1)
            .FirstOrDefaultAsync(cancellationToken);

        if (organizationMember == null)
        {
            _logger.LogWarning("No owner/admin found for organization {OrganizationId}", organizationId);
            return;
        }

        var userId = organizationMember.UserId;

        // Get the subscription plan
        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == subscriptionPlanId && p.IsActive && !p.IsDeleted, cancellationToken);

        if (plan == null)
        {
            _logger.LogWarning("Subscription plan {PlanId} not found", subscriptionPlanId);
            return;
        }

        // Find existing subscription (check by Stripe subscription ID first, then by organization)
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id, cancellationToken);

        if (subscription == null)
        {
            // Check for existing active subscription for this organization
            subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.OrganizationId == organizationId && 
                                         s.Status == SubscriptionStatus.Active && 
                                         !s.IsDeleted, 
                                     cancellationToken);
        }

        if (subscription != null)
        {
            // Update existing subscription with Stripe IDs and new plan
            subscription.SetStripeIds(
                stripeSubscription.CustomerId,
                stripeSubscription.Id,
                stripeSubscription.Items.Data[0].Price.Id);
            
            // Update plan if changed
            if (subscription.SubscriptionPlanId != subscriptionPlanId)
            {
                // Cancel old subscription
                subscription.Cancel(subscription.EndDate);
                
                // Create new subscription with new plan
                var newSubscription = new Sqordia.Domain.Entities.Subscription(
                    userId,
                    organizationId,
                    subscriptionPlanId,
                    isYearly,
                    CalculatePrice(plan, isYearly),
                    stripeSubscription.CurrentPeriodStart,
                    stripeSubscription.CurrentPeriodEnd,
                    isTrial: plan.PlanType == SubscriptionPlanType.Free,
                    currency: plan.Currency);
                
                newSubscription.SetStripeIds(
                    stripeSubscription.CustomerId,
                    stripeSubscription.Id,
                    stripeSubscription.Items.Data[0].Price.Id);
                
                _context.Subscriptions.Add(newSubscription);
            }
            else
            {
                subscription.Renew(stripeSubscription.CurrentPeriodEnd);
            }
        }
        else
        {
            // Create new subscription
            subscription = new Sqordia.Domain.Entities.Subscription(
                userId,
                organizationId,
                subscriptionPlanId,
                isYearly,
                CalculatePrice(plan, isYearly),
                stripeSubscription.CurrentPeriodStart,
                stripeSubscription.CurrentPeriodEnd,
                isTrial: plan.PlanType == SubscriptionPlanType.Free,
                currency: plan.Currency);
            
            subscription.SetStripeIds(
                stripeSubscription.CustomerId,
                stripeSubscription.Id,
                stripeSubscription.Items.Data[0].Price.Id);
            
            _context.Subscriptions.Add(subscription);
            _logger.LogInformation("Created new subscription {SubscriptionId} for organization {OrganizationId}", 
                subscription.Id, organizationId);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private decimal CalculatePrice(Sqordia.Domain.Entities.SubscriptionPlan plan, bool isYearly)
    {
        if (isYearly && plan.BillingCycle == BillingCycle.Monthly)
        {
            return plan.Price * 12;
        }
        else if (isYearly && plan.BillingCycle == BillingCycle.Yearly)
        {
            return plan.Price;
        }
        else if (!isYearly && plan.BillingCycle == BillingCycle.Monthly)
        {
            return plan.Price;
        }
        else
        {
            return plan.Price / 12;
        }
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent, CancellationToken cancellationToken)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (stripeSubscription == null) return;

        _logger.LogInformation("Subscription updated: {SubscriptionId}", stripeSubscription.Id);

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id, cancellationToken);

        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for Stripe subscription {StripeSubscriptionId}", stripeSubscription.Id);
            return;
        }

        // Update subscription dates
        subscription.Renew(stripeSubscription.CurrentPeriodEnd);

        // Update status based on Stripe status
        switch (stripeSubscription.Status)
        {
            case "active":
                if (subscription.Status != SubscriptionStatus.Active)
                {
                    subscription.Reactivate();
                }
                break;
            case "past_due":
            case "unpaid":
                subscription.Suspend();
                break;
            case "canceled":
                subscription.Cancel(stripeSubscription.CurrentPeriodEnd);
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent, CancellationToken cancellationToken)
    {
        var stripeSubscription = stripeEvent.Data.Object as Stripe.Subscription;
        if (stripeSubscription == null) return;

        _logger.LogInformation("Subscription deleted: {SubscriptionId}", stripeSubscription.Id);

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id, cancellationToken);

        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for Stripe subscription {StripeSubscriptionId}", stripeSubscription.Id);
            return;
        }

        subscription.Cancel(stripeSubscription.CurrentPeriodEnd);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleInvoicePaymentSucceeded(Event stripeEvent, CancellationToken cancellationToken)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        _logger.LogInformation("Invoice payment succeeded: {InvoiceId}", invoice.Id);

        if (string.IsNullOrEmpty(invoice.SubscriptionId)) return;

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for invoice {InvoiceId}", invoice.Id);
            return;
        }

        // Ensure subscription is active
        if (subscription.Status != SubscriptionStatus.Active)
        {
            subscription.Reactivate();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task HandleInvoicePaymentFailed(Event stripeEvent, CancellationToken cancellationToken)
    {
        var invoice = stripeEvent.Data.Object as Stripe.Invoice;
        if (invoice == null) return;

        _logger.LogWarning("Invoice payment failed: {InvoiceId}", invoice.Id);

        if (string.IsNullOrEmpty(invoice.SubscriptionId)) return;

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == invoice.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            _logger.LogWarning("Subscription not found for invoice {InvoiceId}", invoice.Id);
            return;
        }

        // Suspend subscription on payment failure
        subscription.Suspend();
        await _context.SaveChangesAsync(cancellationToken);
    }
}


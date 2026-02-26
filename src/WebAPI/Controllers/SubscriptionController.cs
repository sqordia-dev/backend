using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Contracts.Requests;
using Sqordia.Application.Services;
using Sqordia.Domain.Enums;
using System.Security.Claims;

namespace WebAPI.Controllers;

/// <summary>
/// Subscription management controller
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subscriptions")]
[Authorize]
public class SubscriptionController : BaseApiController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IStripeService _stripeService;
    private readonly IInvoicePdfService _invoicePdfService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SubscriptionController> _logger;
    private readonly IConfiguration _configuration;

    public SubscriptionController(
        ISubscriptionService subscriptionService,
        IStripeService stripeService,
        IInvoicePdfService invoicePdfService,
        IApplicationDbContext context,
        ILogger<SubscriptionController> logger,
        IConfiguration configuration)
    {
        _subscriptionService = subscriptionService;
        _stripeService = stripeService;
        _invoicePdfService = invoicePdfService;
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Get all available subscription plans
    /// </summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.GetPlansAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get current user's active subscription
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSubscription(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _subscriptionService.GetCurrentSubscriptionAsync(userId.Value, cancellationToken);

        // Return 200 with null if no subscription found (avoids browser 404 noise)
        if (!result.IsSuccess && result.Error != null &&
            (result.Error.Message?.Contains("No subscription found", StringComparison.OrdinalIgnoreCase) == true ||
             result.Error.Code?.Contains("NotFound", StringComparison.OrdinalIgnoreCase) == true))
        {
            return Ok(new { subscription = (object?)null, message = "No subscription found" });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Get organization's active subscription
    /// </summary>
    [HttpGet("organizations/{organizationId}/current")]
    public async Task<IActionResult> GetOrganizationSubscription(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.GetOrganizationSubscriptionAsync(organizationId, cancellationToken);
        
        // Return 404 if no subscription found (instead of error)
        // Check BEFORE HandleResult to avoid 400 BadRequest
        if (!result.IsSuccess && result.Error != null && 
            (result.Error.Message?.Contains("No subscription found", StringComparison.OrdinalIgnoreCase) == true ||
             result.Error.Code?.Contains("NotFound", StringComparison.OrdinalIgnoreCase) == true))
        {
            return NotFound(new { message = result.Error.Message ?? "No subscription found" });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Subscribe to a plan
    /// </summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe(
        [FromBody] SubscribeRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _subscriptionService.SubscribeAsync(userId.Value, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Change subscription plan
    /// </summary>
    [HttpPut("change-plan")]
    public async Task<IActionResult> ChangePlan(
        [FromBody] ChangePlanRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _subscriptionService.ChangePlanAsync(userId.Value, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelSubscription(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _subscriptionService.CancelSubscriptionAsync(userId.Value, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get user's invoices
    /// </summary>
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _subscriptionService.GetInvoicesAsync(userId.Value, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Download invoice as PDF
    /// </summary>
    [HttpGet("invoices/{invoiceId}/download")]
    public async Task<IActionResult> DownloadInvoicePdf(Guid invoiceId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            // Get user's invoices to verify access
            var invoicesResult = await _subscriptionService.GetInvoicesAsync(userId.Value, cancellationToken);
            if (!invoicesResult.IsSuccess || invoicesResult.Value == null)
            {
                return NotFound("Invoice not found");
            }

            var invoice = invoicesResult.Value.FirstOrDefault(i => i.Id == invoiceId);
            if (invoice == null)
            {
                return NotFound("Invoice not found or you don't have access to it");
            }

            // Get user and organization info for the invoice
            var user = await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Get subscription to find organization
            var subscription = await _context.Subscriptions
                .Include(s => s.Organization)
                .FirstOrDefaultAsync(s => s.Id == invoice.SubscriptionId, cancellationToken);

            var organizationName = subscription?.Organization?.Name ?? "";
            var customerName = $"{user.FirstName} {user.LastName}".Trim();
            var customerEmail = user.Email?.ToString() ?? "";

            // Generate PDF
            var pdfBytes = _invoicePdfService.GenerateInvoicePdf(invoice, organizationName, customerName, customerEmail);

            // Return PDF file
            return File(pdfBytes, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice PDF for invoice {InvoiceId}", invoiceId);
            return StatusCode(500, "Failed to generate invoice PDF");
        }
    }

    /// <summary>
    /// Create Stripe checkout session for subscription
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromBody] SubscribeRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            // Get user and organization
            var user = await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var organization = await _context.Organizations.FindAsync(new object[] { request.OrganizationId }, cancellationToken);
            if (organization == null)
            {
                return NotFound("Organization not found");
            }

            // Verify user belongs to organization
            var isMember = await _context.OrganizationMembers
                .AnyAsync(om => om.UserId == userId.Value && 
                               om.OrganizationId == request.OrganizationId && 
                               !om.IsDeleted, 
                           cancellationToken);

            if (!isMember)
            {
                return BadRequest("User is not a member of the specified organization");
            }

            // Get the plan
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive && !p.IsDeleted, cancellationToken);

            if (plan == null)
            {
                return NotFound("Subscription plan not found");
            }

            // For free plans, use direct subscription
            if (plan.PlanType == SubscriptionPlanType.Free)
            {
                var subscribeResult = await _subscriptionService.SubscribeAsync(userId.Value, request, cancellationToken);
                return HandleResult(subscribeResult);
            }

            // Get or create Stripe customer
            var userEmail = user.Email?.ToString() ?? throw new InvalidOperationException("User email is required");
            var customerIdResult = await _stripeService.GetCustomerIdByEmailAsync(userEmail, cancellationToken);
            string customerId;

            if (customerIdResult.IsSuccess && !string.IsNullOrEmpty(customerIdResult.Value))
            {
                customerId = customerIdResult.Value;
            }
            else
            {
                var createCustomerResult = await _stripeService.CreateCustomerAsync(
                    userEmail,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    userId.Value,
                    request.OrganizationId,
                    cancellationToken);

                if (!createCustomerResult.IsSuccess)
                {
                    return BadRequest(createCustomerResult.Error?.Message ?? "Failed to create customer");
                }

                customerId = createCustomerResult.Value!;
            }

            // Get price ID from configuration based on plan type and billing cycle
            var planType = plan.PlanType.ToString();
            var billingCycle = request.IsYearly ? "Yearly" : "Monthly";
            var priceId = _configuration[$"Stripe:PriceIds:{planType}:{billingCycle}"];

            if (string.IsNullOrEmpty(priceId))
            {
                _logger.LogWarning("Stripe price ID not configured for plan {PlanType} {BillingCycle}", planType, billingCycle);
                return BadRequest($"Stripe price ID not configured for {planType} {billingCycle} plan");
            }

            // Create checkout session
            // Get frontend URL from configuration (defaults to localhost:5173 for development)
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var successUrl = $"{frontendUrl}/checkout/success?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{frontendUrl}/checkout/cancel";

            var checkoutResult = await _stripeService.CreateCheckoutSessionAsync(
                customerId!,
                priceId,
                request.PlanId,
                request.OrganizationId,
                request.IsYearly,
                successUrl,
                cancelUrl,
                cancellationToken);

            if (!checkoutResult.IsSuccess)
            {
                return BadRequest(checkoutResult.Error?.Message ?? "Failed to create checkout session");
            }

            return Ok(new { checkoutUrl = checkoutResult.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create Stripe billing portal session
    /// </summary>
    [HttpPost("billing-portal")]
    public async Task<IActionResult> CreateBillingPortalSession(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            // Get user's subscription
            var subscriptionResult = await _subscriptionService.GetCurrentSubscriptionAsync(userId.Value, cancellationToken);
            if (!subscriptionResult.IsSuccess || subscriptionResult.Value == null)
            {
                return NotFound("No active subscription found");
            }

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionResult.Value.Id, cancellationToken);

            if (subscription == null || string.IsNullOrEmpty(subscription.StripeCustomerId))
            {
                return BadRequest("Subscription does not have a Stripe customer ID");
            }

            // Create billing portal session
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var returnUrl = $"{frontendUrl}/subscription";

            var portalResult = await _stripeService.CreateBillingPortalSessionAsync(
                subscription.StripeCustomerId,
                returnUrl,
                cancellationToken);

            if (!portalResult.IsSuccess)
            {
                return BadRequest(portalResult.Error?.Message ?? "Failed to create billing portal session");
            }

            return Ok(new { portalUrl = portalResult.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating billing portal session");
            return StatusCode(500, "Internal server error");
        }
    }
}


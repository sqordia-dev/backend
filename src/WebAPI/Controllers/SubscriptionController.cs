using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Contracts.Requests;
using Sqordia.Application.Services;
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
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        ISubscriptionService subscriptionService,
        ILogger<SubscriptionController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
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
}


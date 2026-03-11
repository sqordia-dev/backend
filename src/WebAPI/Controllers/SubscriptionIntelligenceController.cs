using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;

namespace WebAPI.Controllers;

/// <summary>
/// ML-driven subscription intelligence: engagement scoring, churn prediction,
/// personalized promotions, and coupon management.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subscription-intelligence")]
[Authorize]
public class SubscriptionIntelligenceController : BaseApiController
{
    private readonly ISubscriptionIntelligenceService _intelligenceService;

    public SubscriptionIntelligenceController(ISubscriptionIntelligenceService intelligenceService)
    {
        _intelligenceService = intelligenceService;
    }

    /// <summary>
    /// Get full ML intelligence for an organization (engagement, churn, upgrade, promotion).
    /// </summary>
    [HttpGet("organizations/{organizationId}")]
    public async Task<IActionResult> GetIntelligence(
        Guid organizationId, CancellationToken ct)
    {
        var result = await _intelligenceService.GetIntelligenceAsync(organizationId, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Validate a coupon code for an organization.
    /// </summary>
    [HttpPost("coupons/validate")]
    public async Task<IActionResult> ValidateCoupon(
        [FromBody] ValidateCouponRequest request, CancellationToken ct)
    {
        var result = await _intelligenceService.ValidateCouponAsync(
            request.Code, request.OrganizationId, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Generate a personalized coupon for an organization based on ML recommendations.
    /// </summary>
    [HttpPost("organizations/{organizationId}/generate-coupon")]
    public async Task<IActionResult> GeneratePersonalizedCoupon(
        Guid organizationId, CancellationToken ct)
    {
        var result = await _intelligenceService.GeneratePersonalizedCouponAsync(organizationId, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Get active promotions for an organization (personalized + global).
    /// </summary>
    [HttpGet("organizations/{organizationId}/promotions")]
    public async Task<IActionResult> GetActivePromotions(
        Guid organizationId, CancellationToken ct)
    {
        var result = await _intelligenceService.GetActivePromotionsAsync(organizationId, ct);
        return HandleResult(result);
    }
}

public class ValidateCouponRequest
{
    public string Code { get; set; } = null!;
    public Guid OrganizationId { get; set; }
}

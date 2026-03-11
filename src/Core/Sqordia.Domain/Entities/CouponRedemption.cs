using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Tracks when a coupon was redeemed by an organization.
/// </summary>
public class CouponRedemption : BaseAuditableEntity
{
    public Guid CouponId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public DateTime RedeemedAt { get; private set; }
    public decimal DiscountAmount { get; private set; }

    // Navigation
    public Coupon Coupon { get; private set; } = null!;
    public Organization Organization { get; private set; } = null!;
    public Subscription Subscription { get; private set; } = null!;

    private CouponRedemption() { } // EF Core

    public CouponRedemption(
        Guid couponId,
        Guid organizationId,
        Guid subscriptionId,
        decimal discountAmount)
    {
        CouponId = couponId;
        OrganizationId = organizationId;
        SubscriptionId = subscriptionId;
        DiscountAmount = discountAmount;
        RedeemedAt = DateTime.UtcNow;
    }
}

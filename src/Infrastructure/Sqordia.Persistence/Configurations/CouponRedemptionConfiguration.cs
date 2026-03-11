using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class CouponRedemptionConfiguration : IEntityTypeConfiguration<CouponRedemption>
{
    public void Configure(EntityTypeBuilder<CouponRedemption> builder)
    {
        builder.ToTable("CouponRedemptions");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.CouponId, e.OrganizationId })
            .HasDatabaseName("IX_CouponRedemptions_CouponId_OrgId");

        builder.HasIndex(e => e.OrganizationId)
            .HasDatabaseName("IX_CouponRedemptions_OrgId");

        builder.Property(e => e.DiscountAmount)
            .HasPrecision(18, 2);

        builder.HasOne(e => e.Coupon)
            .WithMany(c => c.Redemptions)
            .HasForeignKey(e => e.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Subscription)
            .WithMany()
            .HasForeignKey(e => e.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_Coupons_Code");

        builder.HasIndex(e => e.OrganizationId)
            .HasDatabaseName("IX_Coupons_OrganizationId");

        builder.HasIndex(e => new { e.IsActive, e.ValidFrom, e.ValidUntil })
            .HasDatabaseName("IX_Coupons_Active_Validity");

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.DiscountPercent)
            .IsRequired();

        builder.Property(e => e.TargetPlanType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.PromotionType)
            .HasMaxLength(100);

        builder.Property(e => e.StripeCouponId)
            .HasMaxLength(255);

        builder.HasMany(e => e.Redemptions)
            .WithOne(r => r.Coupon)
            .HasForeignKey(r => r.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class PlanFeatureLimitConfiguration : IEntityTypeConfiguration<PlanFeatureLimit>
{
    public void Configure(EntityTypeBuilder<PlanFeatureLimit> builder)
    {
        builder.ToTable("PlanFeatureLimits");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.SubscriptionPlanId, e.FeatureKey })
            .IsUnique()
            .HasDatabaseName("IX_PlanFeatureLimits_PlanId_FeatureKey");

        builder.Property(e => e.FeatureKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(e => e.Plan)
            .WithMany(p => p.FeatureLimits)
            .HasForeignKey(e => e.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.AICoach;

namespace Sqordia.Persistence.Configurations.AICoach;

public class AICoachUsageConfiguration : IEntityTypeConfiguration<AICoachUsage>
{
    public void Configure(EntityTypeBuilder<AICoachUsage> builder)
    {
        builder.ToTable("AICoachUsages");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId);

        builder.Property(u => u.OrganizationId);

        builder.Property(u => u.Month)
            .IsRequired();

        builder.Property(u => u.TotalTokensUsed)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.LastUpdated)
            .IsRequired();

        // Unique index: one usage record per user/org/month
        builder.HasIndex(u => new { u.UserId, u.OrganizationId, u.Month })
            .IsUnique()
            .HasDatabaseName("IX_AICoachUsages_UserId_OrganizationId_Month");

        // Performance indexes
        builder.HasIndex(u => u.UserId)
            .HasDatabaseName("IX_AICoachUsages_UserId");

        builder.HasIndex(u => u.OrganizationId)
            .HasDatabaseName("IX_AICoachUsages_OrganizationId");

        builder.HasIndex(u => u.Month)
            .HasDatabaseName("IX_AICoachUsages_Month");
    }
}

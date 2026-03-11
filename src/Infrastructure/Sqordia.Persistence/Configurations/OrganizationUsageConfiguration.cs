using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class OrganizationUsageConfiguration : IEntityTypeConfiguration<OrganizationUsage>
{
    public void Configure(EntityTypeBuilder<OrganizationUsage> builder)
    {
        builder.ToTable("OrganizationUsages");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.OrganizationId, e.Period })
            .IsUnique()
            .HasDatabaseName("IX_OrganizationUsages_OrgId_Period");

        builder.Property(e => e.Period).IsRequired();
        builder.Property(e => e.PlansGenerated).HasDefaultValue(0);
        builder.Property(e => e.AiCoachMessages).HasDefaultValue(0);
        builder.Property(e => e.ExportsGenerated).HasDefaultValue(0);
        builder.Property(e => e.AiTokensUsed).HasDefaultValue(0L);
        builder.Property(e => e.StorageUsedBytes).HasDefaultValue(0L);

        builder.HasOne(e => e.Organization)
            .WithMany()
            .HasForeignKey(e => e.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

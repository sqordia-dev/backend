using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Persistence.Configurations;

public class CmsVersionHistoryConfiguration : IEntityTypeConfiguration<CmsVersionHistory>
{
    public void Configure(EntityTypeBuilder<CmsVersionHistory> builder)
    {
        builder.ToTable("CmsVersionHistory");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.CmsVersionId)
            .IsRequired();

        builder.Property(h => h.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(h => h.PerformedByUserId)
            .IsRequired();

        builder.Property(h => h.PerformedAt)
            .IsRequired();

        builder.Property(h => h.Notes)
            .HasMaxLength(500);

        builder.Property(h => h.OldStatus)
            .HasConversion<int?>();

        builder.Property(h => h.NewStatus)
            .HasConversion<int?>();

        builder.Property(h => h.OldApprovalStatus)
            .HasConversion<int?>();

        builder.Property(h => h.NewApprovalStatus)
            .HasConversion<int?>();

        builder.Property(h => h.ChangeSummary)
            .HasMaxLength(1000);

        builder.Property(h => h.ScheduledPublishAt);

        // Indexes for common queries
        builder.HasIndex(h => h.CmsVersionId);
        builder.HasIndex(h => h.PerformedAt);
        builder.HasIndex(h => h.Action);
    }
}

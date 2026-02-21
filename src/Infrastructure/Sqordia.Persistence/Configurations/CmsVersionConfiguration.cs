using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Persistence.Configurations;

public class CmsVersionConfiguration : IEntityTypeConfiguration<CmsVersion>
{
    public void Configure(EntityTypeBuilder<CmsVersion> builder)
    {
        builder.ToTable("CmsVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VersionNumber)
            .IsRequired();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.CreatedByUserId)
            .IsRequired();

        builder.Property(v => v.PublishedAt);

        builder.Property(v => v.PublishedByUserId);

        builder.Property(v => v.Notes)
            .HasMaxLength(500);

        // Scheduling
        builder.Property(v => v.ScheduledPublishAt);

        // Approval workflow
        builder.Property(v => v.ApprovalStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.CmsApprovalStatus.None);

        builder.Property(v => v.ApprovedAt);

        builder.Property(v => v.ApprovedByUserId);

        builder.Property(v => v.RejectedAt);

        builder.Property(v => v.RejectedByUserId);

        builder.Property(v => v.RejectionReason)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.ApprovalStatus);
        builder.HasIndex(v => v.ScheduledPublishAt);

        // Relationships
        builder.HasMany(v => v.ContentBlocks)
            .WithOne(b => b.Version)
            .HasForeignKey(b => b.CmsVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.History)
            .WithOne(h => h.Version)
            .HasForeignKey(h => h.CmsVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class BugReportAttachmentConfiguration : IEntityTypeConfiguration<BugReportAttachment>
{
    public void Configure(EntityTypeBuilder<BugReportAttachment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.FileSizeBytes)
            .IsRequired();

        builder.Property(a => a.StorageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(a => a.BugReportId);
    }
}

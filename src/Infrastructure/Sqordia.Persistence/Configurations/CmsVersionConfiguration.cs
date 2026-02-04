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

        builder.HasIndex(v => v.Status);

        builder.HasMany(v => v.ContentBlocks)
            .WithOne(b => b.Version)
            .HasForeignKey(b => b.CmsVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

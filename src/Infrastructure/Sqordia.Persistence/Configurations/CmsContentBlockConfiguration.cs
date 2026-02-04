using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Persistence.Configurations;

public class CmsContentBlockConfiguration : IEntityTypeConfiguration<CmsContentBlock>
{
    public void Configure(EntityTypeBuilder<CmsContentBlock> builder)
    {
        builder.ToTable("CmsContentBlocks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CmsVersionId)
            .IsRequired();

        builder.Property(b => b.BlockKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.BlockType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.Content)
            .IsRequired();

        builder.Property(b => b.Language)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(b => b.SortOrder)
            .IsRequired();

        builder.Property(b => b.SectionKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Metadata)
            .HasMaxLength(8000);

        builder.HasIndex(b => new { b.CmsVersionId, b.BlockKey, b.Language })
            .IsUnique();

        builder.HasIndex(b => b.SectionKey);
    }
}

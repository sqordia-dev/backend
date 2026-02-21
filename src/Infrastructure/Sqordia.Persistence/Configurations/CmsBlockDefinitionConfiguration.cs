using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Persistence.Configurations;

public class CmsBlockDefinitionConfiguration : IEntityTypeConfiguration<CmsBlockDefinition>
{
    public void Configure(EntityTypeBuilder<CmsBlockDefinition> builder)
    {
        builder.ToTable("CmsBlockDefinitions");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CmsSectionId)
            .IsRequired();

        builder.Property(b => b.BlockKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.BlockType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.Label)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.Property(b => b.DefaultContent)
            .HasMaxLength(8000);

        builder.Property(b => b.SortOrder)
            .IsRequired();

        builder.Property(b => b.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.ValidationRules)
            .HasMaxLength(4000);

        builder.Property(b => b.MetadataSchema)
            .HasMaxLength(4000);

        builder.Property(b => b.Placeholder)
            .HasMaxLength(500);

        builder.Property(b => b.MaxLength)
            .IsRequired()
            .HasDefaultValue(0);

        // Composite unique index: section + blockKey
        builder.HasIndex(b => new { b.CmsSectionId, b.BlockKey })
            .IsUnique();

        // Composite index for section-ordered queries
        builder.HasIndex(b => new { b.CmsSectionId, b.SortOrder });

        // Index on IsActive for filtering
        builder.HasIndex(b => b.IsActive);

        // Index on BlockType for type-based queries
        builder.HasIndex(b => b.BlockType);
    }
}

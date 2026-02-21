using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Persistence.Configurations;

public class CmsSectionConfiguration : IEntityTypeConfiguration<CmsSection>
{
    public void Configure(EntityTypeBuilder<CmsSection> builder)
    {
        builder.ToTable("CmsSections");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CmsPageId)
            .IsRequired();

        builder.Property(s => s.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Label)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.SortOrder)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.IconName)
            .HasMaxLength(50);

        // Unique index on Key (section keys are globally unique)
        builder.HasIndex(s => s.Key)
            .IsUnique();

        // Composite index for page-ordered queries
        builder.HasIndex(s => new { s.CmsPageId, s.SortOrder });

        // Index on IsActive for filtering
        builder.HasIndex(s => s.IsActive);

        // Relationship: Section has many BlockDefinitions
        builder.HasMany(s => s.BlockDefinitions)
            .WithOne(b => b.Section)
            .HasForeignKey(b => b.CmsSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

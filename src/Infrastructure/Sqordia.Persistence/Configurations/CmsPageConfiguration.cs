using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Persistence.Configurations;

public class CmsPageConfiguration : IEntityTypeConfiguration<CmsPage>
{
    public void Configure(EntityTypeBuilder<CmsPage> builder)
    {
        builder.ToTable("CmsPages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Label)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.SortOrder)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.IconName)
            .HasMaxLength(50);

        builder.Property(p => p.SpecialRenderer)
            .HasMaxLength(100);

        // Unique index on Key
        builder.HasIndex(p => p.Key)
            .IsUnique();

        // Index on SortOrder for ordered queries
        builder.HasIndex(p => p.SortOrder);

        // Index on IsActive for filtering
        builder.HasIndex(p => p.IsActive);

        // Relationship: Page has many Sections
        builder.HasMany(p => p.Sections)
            .WithOne(s => s.Page)
            .HasForeignKey(s => s.CmsPageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class TableOfContentsSettingsConfiguration : IEntityTypeConfiguration<TableOfContentsSettings>
{
    public void Configure(EntityTypeBuilder<TableOfContentsSettings> builder)
    {
        builder.ToTable("TableOfContentsSettings");

        builder.HasKey(toc => toc.Id);

        builder.Property(toc => toc.Style)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("classic");

        builder.Property(toc => toc.ShowPageNumbers)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(toc => toc.ShowIcons)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(toc => toc.ShowCategoryHeaders)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(toc => toc.StyleSettingsJson)
            .HasColumnType("text");

        // Relationship - One-to-One with BusinessPlan
        builder.HasOne(toc => toc.BusinessPlan)
            .WithOne(bp => bp.TableOfContentsSettings)
            .HasForeignKey<TableOfContentsSettings>(toc => toc.BusinessPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(toc => toc.BusinessPlanId)
            .IsUnique(); // One TOC settings per business plan
    }
}

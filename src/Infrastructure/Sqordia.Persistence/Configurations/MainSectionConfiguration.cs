using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations;

/// <summary>
/// EF Core configuration for MainSection entity
/// </summary>
public class MainSectionConfiguration : IEntityTypeConfiguration<MainSection>
{
    public void Configure(EntityTypeBuilder<MainSection> builder)
    {
        builder.ToTable("MainSections");

        builder.HasKey(x => x.Id);

        // Section Number (0-7)
        builder.Property(x => x.Number)
            .IsRequired();

        // Unique code identifier
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(100);

        // Titles (bilingual)
        builder.Property(x => x.TitleFR)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.TitleEN)
            .IsRequired()
            .HasMaxLength(500);

        // Descriptions (bilingual)
        builder.Property(x => x.DescriptionFR)
            .HasMaxLength(2000);

        builder.Property(x => x.DescriptionEN)
            .HasMaxLength(2000);

        // Display order
        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        // IsActive
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Generated Last (for Executive Summary)
        builder.Property(x => x.GeneratedLast)
            .IsRequired()
            .HasDefaultValue(false);

        // Icon
        builder.Property(x => x.Icon)
            .HasMaxLength(100);

        // Audit fields
        builder.Property(x => x.Created)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(256);

        builder.Property(x => x.LastModified);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(256);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_MainSections_Code");

        builder.HasIndex(x => x.Number)
            .IsUnique()
            .HasDatabaseName("IX_MainSections_Number");

        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder })
            .HasDatabaseName("IX_MainSections_Active_Order");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Relationship to SubSections
        builder.HasMany(x => x.SubSections)
            .WithOne(x => x.MainSection)
            .HasForeignKey(x => x.MainSectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Prompts (master prompts)
        builder.HasMany(x => x.Prompts)
            .WithOne(x => x.MainSection)
            .HasForeignKey(x => x.MainSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

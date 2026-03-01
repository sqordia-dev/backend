using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations;

/// <summary>
/// EF Core configuration for SubSection entity
/// </summary>
public class SubSectionConfiguration : IEntityTypeConfiguration<SubSection>
{
    public void Configure(EntityTypeBuilder<SubSection> builder)
    {
        builder.ToTable("SubSections");

        builder.HasKey(x => x.Id);

        // Foreign key to MainSection
        builder.Property(x => x.MainSectionId)
            .IsRequired();

        // Sub-section code (e.g., "1.1", "3.7")
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

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

        // Educational notes (bilingual)
        builder.Property(x => x.NoteFR)
            .HasColumnType("text");

        builder.Property(x => x.NoteEN)
            .HasColumnType("text");

        // Display order
        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        // IsActive
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

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
            .HasDatabaseName("IX_SubSections_Code");

        builder.HasIndex(x => new { x.MainSectionId, x.DisplayOrder })
            .HasDatabaseName("IX_SubSections_MainSection_Order");

        builder.HasIndex(x => new { x.MainSectionId, x.IsActive })
            .HasDatabaseName("IX_SubSections_MainSection_Active");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Relationship to Prompts (override prompts)
        builder.HasMany(x => x.Prompts)
            .WithOne(x => x.SubSection)
            .HasForeignKey(x => x.SubSectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to QuestionMappings
        builder.HasMany(x => x.QuestionMappings)
            .WithOne(x => x.SubSection)
            .HasForeignKey(x => x.SubSectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

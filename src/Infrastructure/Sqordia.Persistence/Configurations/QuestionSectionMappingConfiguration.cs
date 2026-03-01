using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations;

/// <summary>
/// EF Core configuration for QuestionSectionMapping entity
/// </summary>
public class QuestionSectionMappingConfiguration : IEntityTypeConfiguration<QuestionSectionMapping>
{
    public void Configure(EntityTypeBuilder<QuestionSectionMapping> builder)
    {
        builder.ToTable("QuestionSectionMappings");

        builder.HasKey(x => x.Id);

        // Foreign Keys
        builder.Property(x => x.QuestionTemplateV3Id)
            .IsRequired();

        builder.Property(x => x.SubSectionId)
            .IsRequired();

        // Mapping Context
        builder.Property(x => x.MappingContext)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("primary");

        // Weight (0.0 - 1.0)
        builder.Property(x => x.Weight)
            .IsRequired()
            .HasPrecision(5, 4)
            .HasDefaultValue(1.0m);

        // Transformation Hint
        builder.Property(x => x.TransformationHint)
            .HasMaxLength(1000);

        // IsActive
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Display Order
        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes

        // Lookup by question
        builder.HasIndex(x => new { x.QuestionTemplateV3Id, x.IsActive })
            .HasDatabaseName("IX_QuestionSectionMappings_Question_Active");

        // Lookup by sub-section
        builder.HasIndex(x => new { x.SubSectionId, x.IsActive })
            .HasDatabaseName("IX_QuestionSectionMappings_SubSection_Active");

        // Lookup by context type
        builder.HasIndex(x => new { x.SubSectionId, x.MappingContext, x.IsActive })
            .HasDatabaseName("IX_QuestionSectionMappings_SubSection_Context_Active");

        // Unique constraint: one mapping per question-section pair
        builder.HasIndex(x => new { x.QuestionTemplateV3Id, x.SubSectionId })
            .IsUnique()
            .HasDatabaseName("IX_QuestionSectionMappings_Unique");

        // Order within section
        builder.HasIndex(x => new { x.SubSectionId, x.DisplayOrder })
            .HasDatabaseName("IX_QuestionSectionMappings_SubSection_Order");
    }
}

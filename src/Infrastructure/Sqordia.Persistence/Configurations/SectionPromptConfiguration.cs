using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Persistence.Configurations;

/// <summary>
/// EF Core configuration for SectionPrompt entity
/// </summary>
public class SectionPromptConfiguration : IEntityTypeConfiguration<SectionPrompt>
{
    public void Configure(EntityTypeBuilder<SectionPrompt> builder)
    {
        builder.ToTable("SectionPrompts");

        builder.HasKey(x => x.Id);

        // Foreign keys (nullable for hierarchy)
        builder.Property(x => x.MainSectionId);
        builder.Property(x => x.SubSectionId);

        // Prompt Level - stored as string
        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Plan Type - stored as string
        builder.Property(x => x.PlanType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Language
        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(10);

        // Industry Category (NAICS code)
        builder.Property(x => x.IndustryCategory)
            .HasMaxLength(50);

        // Name
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Description
        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        // System Prompt - can be large
        builder.Property(x => x.SystemPrompt)
            .IsRequired()
            .HasColumnType("text");

        // User Prompt Template - can be large
        builder.Property(x => x.UserPromptTemplate)
            .IsRequired()
            .HasColumnType("text");

        // Variables JSON
        builder.Property(x => x.VariablesJson)
            .HasColumnType("text");

        // Output Format - stored as string
        builder.Property(x => x.OutputFormat)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Visual Elements JSON
        builder.Property(x => x.VisualElementsJson)
            .HasColumnType("text");

        // Example Output
        builder.Property(x => x.ExampleOutput)
            .HasColumnType("text");

        // Version
        builder.Property(x => x.Version)
            .IsRequired()
            .HasDefaultValue(1);

        // IsActive
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

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

        // Master prompts lookup
        builder.HasIndex(x => new { x.MainSectionId, x.PlanType, x.Language, x.IsActive })
            .HasDatabaseName("IX_SectionPrompts_MainSection_Plan_Lang_Active");

        // Override prompts lookup
        builder.HasIndex(x => new { x.SubSectionId, x.PlanType, x.Language, x.IsActive })
            .HasDatabaseName("IX_SectionPrompts_SubSection_Plan_Lang_Active");

        // Level-based lookup
        builder.HasIndex(x => new { x.Level, x.IsActive })
            .HasDatabaseName("IX_SectionPrompts_Level_Active");

        // Industry-specific lookup
        builder.HasIndex(x => new { x.SubSectionId, x.IndustryCategory, x.IsActive })
            .HasDatabaseName("IX_SectionPrompts_SubSection_Industry_Active");

        // Unique constraint: only one active master prompt per main section/plan/language/industry
        builder.HasIndex(x => new { x.MainSectionId, x.PlanType, x.Language, x.IndustryCategory, x.IsActive })
            .HasFilter("\"IsActive\" = true AND \"MainSectionId\" IS NOT NULL")
            .IsUnique()
            .HasDatabaseName("IX_SectionPrompts_Unique_Active_Master");

        // Unique constraint: only one active override prompt per sub-section/plan/language/industry
        builder.HasIndex(x => new { x.SubSectionId, x.PlanType, x.Language, x.IndustryCategory, x.IsActive })
            .HasFilter("\"IsActive\" = true AND \"SubSectionId\" IS NOT NULL")
            .IsUnique()
            .HasDatabaseName("IX_SectionPrompts_Unique_Active_Override");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

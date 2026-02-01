using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PromptTemplate entity
/// </summary>
public class PromptTemplateConfiguration : IEntityTypeConfiguration<PromptTemplate>
{
    public void Configure(EntityTypeBuilder<PromptTemplate> builder)
    {
        builder.ToTable("PromptTemplates");

        builder.HasKey(x => x.Id);

        // Section Type - stored as string for readability
        builder.Property(x => x.SectionType)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        // Plan Type - stored as string for readability
        builder.Property(x => x.PlanType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Industry Category (NAICS code) - nullable for generic prompts
        builder.Property(x => x.IndustryCategory)
            .HasMaxLength(50);

        // Version
        builder.Property(x => x.Version)
            .IsRequired()
            .HasDefaultValue(1);

        // IsActive
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(false);

        // Alias - stored as string, nullable
        builder.Property(x => x.Alias)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Name
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Description
        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // System Prompt - can be large
        builder.Property(x => x.SystemPrompt)
            .IsRequired()
            .HasMaxLength(10000);

        // User Prompt Template - can be large
        builder.Property(x => x.UserPromptTemplate)
            .IsRequired()
            .HasMaxLength(10000);

        // Output Format - stored as string
        builder.Property(x => x.OutputFormat)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Visual Elements JSON - can be very large
        builder.Property(x => x.VisualElementsJson)
            .HasColumnType("nvarchar(max)");

        // Example Output - can be large
        builder.Property(x => x.ExampleOutput)
            .HasColumnType("nvarchar(max)");

        // Timestamps
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Created By
        builder.Property(x => x.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        // Indexes for fast lookup

        // Main query index: active prompts by section, plan type
        builder.HasIndex(x => new { x.SectionType, x.PlanType, x.IsActive })
            .HasDatabaseName("IX_PromptTemplates_Section_PlanType_Active");

        // Industry-specific query index
        builder.HasIndex(x => new { x.SectionType, x.IndustryCategory, x.IsActive })
            .HasDatabaseName("IX_PromptTemplates_Section_Industry_Active");

        // Alias query index
        builder.HasIndex(x => new { x.SectionType, x.PlanType, x.Alias })
            .HasDatabaseName("IX_PromptTemplates_Section_PlanType_Alias");

        // Version query index
        builder.HasIndex(x => new { x.SectionType, x.PlanType, x.Version })
            .HasDatabaseName("IX_PromptTemplates_Section_PlanType_Version");

        // Unique constraint: only one active prompt per section/plan/industry combo
        builder.HasIndex(x => new { x.SectionType, x.PlanType, x.IndustryCategory, x.IsActive })
            .HasFilter("[IsActive] = 1")
            .IsUnique()
            .HasDatabaseName("IX_PromptTemplates_Unique_Active");

        // Relationship to performance metrics
        builder.HasMany(x => x.PerformanceMetrics)
            .WithOne(x => x.PromptTemplate)
            .HasForeignKey(x => x.PromptTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

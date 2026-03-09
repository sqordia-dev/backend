using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Persistence.Configurations;

/// <summary>
/// EF Core configuration for QuestionTemplateV3 entity
/// </summary>
public class QuestionTemplateV3Configuration : IEntityTypeConfiguration<QuestionTemplateV3>
{
    public void Configure(EntityTypeBuilder<QuestionTemplateV3> builder)
    {
        builder.ToTable("QuestionTemplatesV3");

        builder.HasKey(x => x.Id);

        // Question Number (1-22 from STRUCTURE FINALE)
        builder.Property(x => x.QuestionNumber)
            .IsRequired();

        // Persona Type - stored as string, nullable
        builder.Property(x => x.PersonaType)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Step Number (1-7)
        builder.Property(x => x.StepNumber)
            .IsRequired();

        // Question Text (bilingual)
        builder.Property(x => x.QuestionTextFR)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.QuestionTextEN)
            .IsRequired()
            .HasMaxLength(2000);

        // Help Text (bilingual)
        builder.Property(x => x.HelpTextFR)
            .HasColumnType("text");

        builder.Property(x => x.HelpTextEN)
            .HasColumnType("text");

        // Question Type - stored as string
        builder.Property(x => x.QuestionType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Options JSON (bilingual)
        builder.Property(x => x.OptionsFR)
            .HasColumnType("text");

        builder.Property(x => x.OptionsEN)
            .HasColumnType("text");

        // Validation Rules JSON
        builder.Property(x => x.ValidationRules)
            .HasColumnType("text");

        // Conditional Logic JSON
        builder.Property(x => x.ConditionalLogic)
            .HasColumnType("text");

        // Coach Prompts (bilingual) - AI suggestion prompts
        builder.Property(x => x.CoachPromptFR)
            .HasColumnType("text");

        builder.Property(x => x.CoachPromptEN)
            .HasColumnType("text");

        // Expert Advice (bilingual)
        builder.Property(x => x.ExpertAdviceFR)
            .HasColumnType("text");

        builder.Property(x => x.ExpertAdviceEN)
            .HasColumnType("text");

        // Display Order
        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        // IsRequired
        builder.Property(x => x.IsRequired)
            .IsRequired()
            .HasDefaultValue(true);

        // IsActive
        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Icon
        builder.Property(x => x.Icon)
            .HasMaxLength(100);

        // Section Group
        builder.Property(x => x.SectionGroup)
            .HasMaxLength(200);

        // Profile Field Key (maps question to Organization profile field)
        builder.Property(x => x.ProfileFieldKey)
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

        // Question number lookup
        builder.HasIndex(x => x.QuestionNumber)
            .HasDatabaseName("IX_QuestionTemplatesV3_QuestionNumber");

        // Step-based lookup
        builder.HasIndex(x => new { x.StepNumber, x.DisplayOrder })
            .HasDatabaseName("IX_QuestionTemplatesV3_Step_Order");

        // Persona-based lookup
        builder.HasIndex(x => new { x.PersonaType, x.StepNumber, x.IsActive })
            .HasDatabaseName("IX_QuestionTemplatesV3_Persona_Step_Active");

        // Active questions lookup
        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder })
            .HasDatabaseName("IX_QuestionTemplatesV3_Active_Order");

        // Profile field key lookup
        builder.HasIndex(x => x.ProfileFieldKey)
            .HasDatabaseName("IX_QuestionTemplatesV3_ProfileFieldKey")
            .HasFilter("\"ProfileFieldKey\" IS NOT NULL");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Relationship to Section Mappings
        builder.HasMany(x => x.SectionMappings)
            .WithOne(x => x.QuestionTemplate)
            .HasForeignKey(x => x.QuestionTemplateV3Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

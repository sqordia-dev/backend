using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class QuestionTemplateV2Configuration : IEntityTypeConfiguration<QuestionTemplateV2>
{
    public void Configure(EntityTypeBuilder<QuestionTemplateV2> builder)
    {
        builder.ToTable("QuestionTemplatesV2");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.PersonaType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(q => q.StepNumber)
            .IsRequired();

        builder.Property(q => q.QuestionText)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(q => q.QuestionTextEN)
            .HasMaxLength(1000);

        builder.Property(q => q.HelpText)
            .HasMaxLength(2000);

        builder.Property(q => q.HelpTextEN)
            .HasMaxLength(2000);

        builder.Property(q => q.QuestionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(q => q.Section)
            .HasMaxLength(100);

        builder.Property(q => q.Options)
            .HasColumnType("jsonb");

        builder.Property(q => q.OptionsEN)
            .HasColumnType("jsonb");

        builder.Property(q => q.ValidationRules)
            .HasColumnType("jsonb");

        builder.Property(q => q.ConditionalLogic)
            .HasColumnType("jsonb");

        builder.Property(q => q.Icon)
            .HasMaxLength(50);

        // Indexes for efficient querying
        builder.HasIndex(q => q.PersonaType);
        builder.HasIndex(q => q.StepNumber);
        builder.HasIndex(q => q.IsActive);
        builder.HasIndex(q => new { q.PersonaType, q.StepNumber, q.Order });
        builder.HasIndex(q => new { q.StepNumber, q.Order });
    }
}

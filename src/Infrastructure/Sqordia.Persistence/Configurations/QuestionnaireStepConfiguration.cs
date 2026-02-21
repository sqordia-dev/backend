using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations;

public class QuestionnaireStepConfiguration : IEntityTypeConfiguration<QuestionnaireStep>
{
    public void Configure(EntityTypeBuilder<QuestionnaireStep> builder)
    {
        builder.ToTable("QuestionnaireSteps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StepNumber)
            .IsRequired();

        builder.Property(s => s.TitleFR)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.TitleEN)
            .HasMaxLength(200);

        builder.Property(s => s.DescriptionFR)
            .HasMaxLength(500);

        builder.Property(s => s.DescriptionEN)
            .HasMaxLength(500);

        builder.Property(s => s.Icon)
            .HasMaxLength(50);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique constraint on StepNumber
        builder.HasIndex(s => s.StepNumber)
            .IsUnique();
    }
}

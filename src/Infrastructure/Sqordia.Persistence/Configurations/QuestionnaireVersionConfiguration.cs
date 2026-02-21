using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations;

public class QuestionnaireVersionConfiguration : IEntityTypeConfiguration<QuestionnaireVersion>
{
    public void Configure(EntityTypeBuilder<QuestionnaireVersion> builder)
    {
        builder.ToTable("QuestionnaireVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VersionNumber)
            .IsRequired();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.CreatedByUserId)
            .IsRequired();

        builder.Property(v => v.PublishedAt);

        builder.Property(v => v.PublishedByUserId);

        builder.Property(v => v.Notes)
            .HasMaxLength(1000);

        builder.Property(v => v.QuestionsSnapshot)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(v => v.StepsSnapshot)
            .IsRequired()
            .HasColumnType("text");

        // Indexes
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.VersionNumber);
    }
}

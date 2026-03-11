using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.ML;

namespace Sqordia.Persistence.Configurations.ML;

public class LearnedPreferenceConfiguration : IEntityTypeConfiguration<LearnedPreference>
{
    public void Configure(EntityTypeBuilder<LearnedPreference> builder)
    {
        builder.ToTable("learned_preferences");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SectionType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Industry).HasMaxLength(200);
        builder.Property(e => e.PlanType).HasMaxLength(50);
        builder.Property(e => e.Language).HasMaxLength(10).IsRequired();
        builder.Property(e => e.PreferenceType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.PreferenceJson).IsRequired();
        builder.Property(e => e.Confidence).HasColumnType("double precision");

        // Indexes for lookup
        builder.HasIndex(e => new { e.SectionType, e.Language, e.IsActive });
        builder.HasIndex(e => new { e.SectionType, e.Industry, e.IsActive });
    }
}

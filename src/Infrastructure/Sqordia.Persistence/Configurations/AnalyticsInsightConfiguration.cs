using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class AnalyticsInsightConfiguration : IEntityTypeConfiguration<AnalyticsInsight>
{
    public void Configure(EntityTypeBuilder<AnalyticsInsight> builder)
    {
        builder.ToTable("analytics_insights");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.InsightType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.Period)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.MetadataJson)
            .HasColumnType("jsonb");

        builder.Property(e => e.ModelUsed)
            .HasMaxLength(100)
            .IsRequired();

        // Filtered index for latest insights
        builder.HasIndex(e => new { e.InsightType, e.IsLatest })
            .HasFilter("\"IsLatest\" = true");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

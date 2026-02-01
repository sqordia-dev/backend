using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PromptPerformance entity
/// </summary>
public class PromptPerformanceConfiguration : IEntityTypeConfiguration<PromptPerformance>
{
    public void Configure(EntityTypeBuilder<PromptPerformance> builder)
    {
        builder.ToTable("PromptPerformance");

        builder.HasKey(x => x.Id);

        // Foreign key to PromptTemplate
        builder.Property(x => x.PromptTemplateId)
            .IsRequired();

        // Usage metrics
        builder.Property(x => x.UsageCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.EditCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.RegenerateCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.AcceptCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Rating metrics
        builder.Property(x => x.TotalRating)
            .IsRequired()
            .HasDefaultValue(0.0)
            .HasPrecision(10, 2);

        builder.Property(x => x.RatingCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Period tracking
        builder.Property(x => x.PeriodStart)
            .IsRequired();

        builder.Property(x => x.PeriodEnd)
            .IsRequired();

        // Timestamps
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Calculated properties are ignored (not stored in DB)
        builder.Ignore(x => x.EditRate);
        builder.Ignore(x => x.RegenerateRate);
        builder.Ignore(x => x.AcceptanceRate);
        builder.Ignore(x => x.AverageRating);

        // Indexes

        // Query by prompt and date range
        builder.HasIndex(x => new { x.PromptTemplateId, x.PeriodStart })
            .HasDatabaseName("IX_PromptPerformance_PromptId_PeriodStart");

        // Query by date range
        builder.HasIndex(x => new { x.PeriodStart, x.PeriodEnd })
            .HasDatabaseName("IX_PromptPerformance_Period");

        // Relationship is configured in PromptTemplateConfiguration
    }
}

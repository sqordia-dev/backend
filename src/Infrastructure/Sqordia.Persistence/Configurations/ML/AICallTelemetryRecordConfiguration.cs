using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.ML;

namespace Sqordia.Persistence.Configurations.ML;

public class AICallTelemetryRecordConfiguration : IEntityTypeConfiguration<AICallTelemetryRecord>
{
    public void Configure(EntityTypeBuilder<AICallTelemetryRecord> builder)
    {
        builder.ToTable("ai_call_telemetry");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ModelUsed).HasMaxLength(100).IsRequired();
        builder.Property(e => e.SectionType).HasMaxLength(100);
        builder.Property(e => e.PipelinePass).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Language).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Temperature).HasColumnType("real");
        builder.Property(e => e.QualityScore).HasColumnType("decimal(5,2)");
        builder.Property(e => e.EditRatio).HasColumnType("double precision");

        // Indexes for ML queries
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.SectionType);
        builder.HasIndex(e => new { e.SectionType, e.CreatedAt });
        builder.HasIndex(e => e.BusinessPlanId);
        builder.HasIndex(e => e.PromptTemplateId);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.ML;

namespace Sqordia.Persistence.Configurations.ML;

public class SectionEditHistoryConfiguration : IEntityTypeConfiguration<SectionEditHistory>
{
    public void Configure(EntityTypeBuilder<SectionEditHistory> builder)
    {
        builder.ToTable("section_edit_history");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SectionType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AiGeneratedContent).IsRequired();
        builder.Property(e => e.UserEditedContent).IsRequired();
        builder.Property(e => e.EditRatio).HasColumnType("double precision");
        builder.Property(e => e.Industry).HasMaxLength(200);
        builder.Property(e => e.PlanType).HasMaxLength(50);
        builder.Property(e => e.Language).HasMaxLength(10).IsRequired();

        // Indexes for ML queries
        builder.HasIndex(e => e.BusinessPlanId);
        builder.HasIndex(e => e.SectionType);
        builder.HasIndex(e => new { e.SectionType, e.Industry });
        builder.HasIndex(e => e.CreatedAt);
    }
}

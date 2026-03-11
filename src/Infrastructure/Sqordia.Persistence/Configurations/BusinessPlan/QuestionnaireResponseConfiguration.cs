using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class QuestionnaireResponseConfiguration : IEntityTypeConfiguration<QuestionnaireResponse>
{
    public void Configure(EntityTypeBuilder<QuestionnaireResponse> builder)
    {
        builder.ToTable("QuestionnaireResponses");

        builder.HasKey(qr => qr.Id);

        builder.Property(qr => qr.ResponseText)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(qr => qr.NumericValue)
            .HasPrecision(18, 2);

        builder.Property(qr => qr.SelectedOptions)
            .HasColumnType("text"); // JSON array

        builder.Property(qr => qr.AiInsights)
            .HasColumnType("text");

        // Relationships
        builder.HasOne(qr => qr.BusinessPlan)
            .WithMany(bp => bp.QuestionnaireResponses)
            .HasForeignKey(qr => qr.BusinessPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // V3 question template (STRUCTURE FINALE)
        builder.HasOne(qr => qr.QuestionTemplate)
            .WithMany()
            .HasForeignKey(qr => qr.QuestionTemplateId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(qr => qr.BusinessPlanId);
        builder.HasIndex(qr => qr.QuestionTemplateId);

        // Unique constraint: one response per question per business plan
        builder.HasIndex(qr => new { qr.BusinessPlanId, qr.QuestionTemplateId })
            .IsUnique()
            .HasFilter("\"QuestionTemplateId\" IS NOT NULL");
    }
}

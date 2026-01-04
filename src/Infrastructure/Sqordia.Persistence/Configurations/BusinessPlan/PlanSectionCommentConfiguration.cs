using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class PlanSectionCommentConfiguration : IEntityTypeConfiguration<PlanSectionComment>
{
    public void Configure(EntityTypeBuilder<PlanSectionComment> builder)
    {
        builder.ToTable("PlanSectionComments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.SectionName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.CommentText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(c => c.BusinessPlan)
            .WithMany()
            .HasForeignKey(c => c.BusinessPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.BusinessPlanId);
        builder.HasIndex(c => new { c.BusinessPlanId, c.SectionName });
        builder.HasIndex(c => c.ParentCommentId);
    }
}


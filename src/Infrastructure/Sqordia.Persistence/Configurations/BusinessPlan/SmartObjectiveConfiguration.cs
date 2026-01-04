using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class SmartObjectiveConfiguration : IEntityTypeConfiguration<SmartObjective>
{
    public void Configure(EntityTypeBuilder<SmartObjective> builder)
    {
        builder.ToTable("SmartObjectives");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(o => o.Specific)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.Measurable)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.Achievable)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.Relevant)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.TimeBound)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ProgressPercentage)
            .HasPrecision(5, 2);

        builder.HasOne(o => o.BusinessPlan)
            .WithMany()
            .HasForeignKey(o => o.BusinessPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.BusinessPlanId);
        builder.HasIndex(o => new { o.BusinessPlanId, o.Category });
    }
}


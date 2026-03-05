using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class ProjectCostConfiguration : IEntityTypeConfiguration<ProjectCost>
{
    public void Configure(EntityTypeBuilder<ProjectCost> builder)
    {
        builder.ToTable("ProjectCosts");

        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.TotalStartupCosts)
            .HasPrecision(18, 2);

        builder.Property(pc => pc.TotalWorkingCapital)
            .HasPrecision(18, 2);

        builder.Property(pc => pc.TotalCapex)
            .HasPrecision(18, 2);

        builder.Property(pc => pc.TotalProjectCost)
            .HasPrecision(18, 2);

        // 1:1 with FinancialPlan
        builder.HasOne(pc => pc.FinancialPlan)
            .WithMany()
            .HasForeignKey(pc => pc.FinancialPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pc => pc.FinancialPlanId)
            .IsUnique()
            .HasDatabaseName("IX_ProjectCosts_FinancialPlanId");
    }
}

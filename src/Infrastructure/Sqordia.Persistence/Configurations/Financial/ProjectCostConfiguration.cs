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

        // Computed totals
        builder.Property(pc => pc.TotalStartupCosts).HasPrecision(18, 2);
        builder.Property(pc => pc.TotalWorkingCapital).HasPrecision(18, 2);
        builder.Property(pc => pc.TotalCapex).HasPrecision(18, 2);
        builder.Property(pc => pc.TotalProjectCost).HasPrecision(18, 2);

        // Salary breakdown
        builder.Property(pc => pc.SalaryAlreadyAcquired).HasPrecision(18, 2);
        builder.Property(pc => pc.SalaryAcquireBefore).HasPrecision(18, 2);
        builder.Property(pc => pc.SalaryAcquireAfter).HasPrecision(18, 2);

        // Sales expenses breakdown
        builder.Property(pc => pc.SalesExpAlreadyAcquired).HasPrecision(18, 2);
        builder.Property(pc => pc.SalesExpAcquireBefore).HasPrecision(18, 2);
        builder.Property(pc => pc.SalesExpAcquireAfter).HasPrecision(18, 2);

        // Admin expenses breakdown
        builder.Property(pc => pc.AdminExpAlreadyAcquired).HasPrecision(18, 2);
        builder.Property(pc => pc.AdminExpAcquireBefore).HasPrecision(18, 2);
        builder.Property(pc => pc.AdminExpAcquireAfter).HasPrecision(18, 2);

        // Inventory breakdown
        builder.Property(pc => pc.InventoryAlreadyAcquired).HasPrecision(18, 2);
        builder.Property(pc => pc.InventoryAcquireBefore).HasPrecision(18, 2);
        builder.Property(pc => pc.InventoryAcquireAfter).HasPrecision(18, 2);

        // Capex breakdown
        builder.Property(pc => pc.CapexAlreadyAcquired).HasPrecision(18, 2);
        builder.Property(pc => pc.CapexAcquireBefore).HasPrecision(18, 2);
        builder.Property(pc => pc.CapexAcquireAfter).HasPrecision(18, 2);

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

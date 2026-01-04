using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class FinancialProjectionConfiguration : IEntityTypeConfiguration<FinancialProjection>
{
    public void Configure(EntityTypeBuilder<FinancialProjection> builder)
    {
        builder.ToTable("FinancialProjections");
        
        builder.HasKey(fp => fp.Id);
        
        // All financial values with precision
        builder.Property(fp => fp.Revenue)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.RevenueGrowthRate)
            .HasPrecision(5, 2); // 0.00 to 999.99 %
            
        builder.Property(fp => fp.CostOfGoodsSold)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.OperatingExpenses)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.MarketingExpenses)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.RAndDExpenses)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.AdministrativeExpenses)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.OtherExpenses)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.GrossProfit)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.NetIncome)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.EBITDA)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.CashFlow)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.CashBalance)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.AverageRevenuePerCustomer)
            .HasPrecision(18, 2);
            
        builder.Property(fp => fp.Notes)
            .HasMaxLength(1000);
            
        builder.Property(fp => fp.Assumptions)
            .HasColumnType("text");
        
        // Relationships
        builder.HasOne(fp => fp.BusinessPlan)
            .WithMany(bp => bp.FinancialProjectionDetails)
            .HasForeignKey(fp => fp.BusinessPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(fp => fp.BusinessPlanId);
        builder.HasIndex(fp => new { fp.BusinessPlanId, fp.Year, fp.Month })
            .IsUnique()
            .HasFilter("\"Month\" IS NOT NULL"); // Unique constraint for monthly projections
            
        builder.HasIndex(fp => new { fp.BusinessPlanId, fp.Year, fp.Quarter })
            .IsUnique()
            .HasFilter("\"Quarter\" IS NOT NULL"); // Unique constraint for quarterly projections
            
        builder.HasIndex(fp => new { fp.BusinessPlanId, fp.Year })
            .HasFilter("\"Month\" IS NULL AND \"Quarter\" IS NULL"); // For yearly projections
    }
}


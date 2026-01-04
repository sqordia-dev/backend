using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class FinancialKPIConfiguration : IEntityTypeConfiguration<FinancialKPI>
{
    public void Configure(EntityTypeBuilder<FinancialKPI> builder)
    {
        builder.ToTable("FinancialKPIs");
        
        builder.HasKey(kpi => kpi.Id);
        
        builder.Property(kpi => kpi.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(kpi => kpi.Description)
            .HasMaxLength(1000);
            
        builder.Property(kpi => kpi.Category)
            .HasMaxLength(100);
            
        builder.Property(kpi => kpi.MetricType)
            .HasMaxLength(50);
            
        builder.Property(kpi => kpi.Value)
            .HasPrecision(18, 2);
            
        builder.Property(kpi => kpi.TargetValue)
            .HasPrecision(18, 2);
            
        builder.Property(kpi => kpi.PreviousValue)
            .HasPrecision(18, 2);
            
        builder.Property(kpi => kpi.ChangePercentage)
            .HasPrecision(5, 2); // Percentage: -999.99 to 999.99
            
        builder.Property(kpi => kpi.Unit)
            .HasMaxLength(20);
            
        builder.Property(kpi => kpi.CurrencyCode)
            .HasMaxLength(10);
            
        builder.Property(kpi => kpi.Trend)
            .HasMaxLength(20);
            
        builder.Property(kpi => kpi.Benchmark)
            .HasMaxLength(200);
            
        builder.Property(kpi => kpi.Status)
            .HasMaxLength(20);
    }
}


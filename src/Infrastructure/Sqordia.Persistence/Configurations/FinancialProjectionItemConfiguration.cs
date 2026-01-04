using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class FinancialProjectionItemConfiguration : IEntityTypeConfiguration<FinancialProjectionItem>
{
    public void Configure(EntityTypeBuilder<FinancialProjectionItem> builder)
    {
        builder.ToTable("FinancialProjectionItems");
        
        builder.HasKey(fpi => fpi.Id);
        
        builder.Property(fpi => fpi.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(fpi => fpi.Description)
            .HasMaxLength(1000);
            
        builder.Property(fpi => fpi.ProjectionType)
            .HasMaxLength(50);
            
        builder.Property(fpi => fpi.Scenario)
            .HasMaxLength(50);
            
        builder.Property(fpi => fpi.Amount)
            .HasPrecision(18, 2);
            
        builder.Property(fpi => fpi.ExchangeRate)
            .HasPrecision(18, 6);
            
        builder.Property(fpi => fpi.BaseAmount)
            .HasPrecision(18, 2);
            
        builder.Property(fpi => fpi.Category)
            .HasMaxLength(100);
            
        builder.Property(fpi => fpi.SubCategory)
            .HasMaxLength(100);
            
        builder.Property(fpi => fpi.Frequency)
            .HasMaxLength(50);
            
        builder.Property(fpi => fpi.GrowthRate)
            .HasPrecision(5, 2); // Percentage: 0.00 to 999.99
            
        builder.Property(fpi => fpi.CurrencyCode)
            .HasMaxLength(10);
    }
}


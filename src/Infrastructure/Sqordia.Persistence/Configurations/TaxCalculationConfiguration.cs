using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class TaxCalculationConfiguration : IEntityTypeConfiguration<TaxCalculation>
{
    public void Configure(EntityTypeBuilder<TaxCalculation> builder)
    {
        builder.ToTable("TaxCalculations");
        
        builder.HasKey(tc => tc.Id);
        
        builder.Property(tc => tc.TaxName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(tc => tc.TaxType)
            .HasMaxLength(50);
            
        builder.Property(tc => tc.TaxableAmount)
            .HasPrecision(18, 2);
            
        builder.Property(tc => tc.TaxRate)
            .HasPrecision(5, 2); // Tax rate percentage: 0.00 to 999.99
            
        builder.Property(tc => tc.TaxAmount)
            .HasPrecision(18, 2);
            
        builder.Property(tc => tc.CurrencyCode)
            .HasMaxLength(10);
            
        builder.Property(tc => tc.CalculationMethod)
            .HasMaxLength(50);
            
        builder.Property(tc => tc.Country)
            .HasMaxLength(100);
            
        builder.Property(tc => tc.Region)
            .HasMaxLength(100);
            
        builder.Property(tc => tc.PaymentReference)
            .HasMaxLength(200);
    }
}


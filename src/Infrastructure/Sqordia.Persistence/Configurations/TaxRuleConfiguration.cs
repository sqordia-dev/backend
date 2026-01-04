using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class TaxRuleConfiguration : IEntityTypeConfiguration<TaxRule>
{
    public void Configure(EntityTypeBuilder<TaxRule> builder)
    {
        builder.ToTable("TaxRules");
        
        builder.HasKey(tr => tr.Id);
        
        builder.Property(tr => tr.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(tr => tr.Description)
            .HasMaxLength(1000);
            
        builder.Property(tr => tr.Country)
            .HasMaxLength(100);
            
        builder.Property(tr => tr.Region)
            .HasMaxLength(100);
            
        builder.Property(tr => tr.TaxType)
            .HasMaxLength(50);
            
        builder.Property(tr => tr.Rate)
            .HasPrecision(5, 2); // Tax rate percentage: 0.00 to 999.99
            
        builder.Property(tr => tr.MinAmount)
            .HasPrecision(18, 2);
            
        builder.Property(tr => tr.MaxAmount)
            .HasPrecision(18, 2);
            
        builder.Property(tr => tr.CalculationMethod)
            .HasMaxLength(50);
            
        builder.Property(tr => tr.ApplicableTo)
            .HasMaxLength(200);
            
        builder.Property(tr => tr.CurrencyCode)
            .HasMaxLength(10);
            
        builder.Property(tr => tr.LegalReference)
            .HasMaxLength(200);
    }
}


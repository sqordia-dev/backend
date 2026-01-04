using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);
            
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(c => c.Symbol)
            .HasMaxLength(10);
            
        builder.Property(c => c.Country)
            .HasMaxLength(100);
            
        builder.Property(c => c.Region)
            .HasMaxLength(100);
            
        builder.Property(c => c.ExchangeRate)
            .HasPrecision(18, 6); // Exchange rates need high precision
            
        builder.Property(c => c.Source)
            .HasMaxLength(100);
            
        builder.HasIndex(c => c.Code)
            .IsUnique();
    }
}


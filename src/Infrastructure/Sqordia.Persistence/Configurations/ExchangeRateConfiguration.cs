using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");
        
        builder.HasKey(er => er.Id);
        
        builder.Property(er => er.Rate)
            .HasPrecision(18, 6); // Exchange rates need high precision
            
        builder.Property(er => er.InverseRate)
            .HasPrecision(18, 6);
            
        builder.Property(er => er.Spread)
            .HasPrecision(18, 6);
            
        builder.Property(er => er.Source)
            .HasMaxLength(100);
            
        builder.Property(er => er.Provider)
            .HasMaxLength(100);
    }
}


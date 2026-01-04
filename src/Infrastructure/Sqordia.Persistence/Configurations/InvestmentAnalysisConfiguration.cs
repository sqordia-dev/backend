using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class InvestmentAnalysisConfiguration : IEntityTypeConfiguration<InvestmentAnalysis>
{
    public void Configure(EntityTypeBuilder<InvestmentAnalysis> builder)
    {
        builder.ToTable("InvestmentAnalyses");
        
        builder.HasKey(ia => ia.Id);
        
        builder.Property(ia => ia.AnalysisType)
            .HasMaxLength(50);
            
        builder.Property(ia => ia.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(ia => ia.Description)
            .HasMaxLength(1000);
            
        builder.Property(ia => ia.InitialInvestment)
            .HasPrecision(18, 2);
            
        builder.Property(ia => ia.ExpectedReturn)
            .HasPrecision(18, 2);
            
        builder.Property(ia => ia.NetPresentValue)
            .HasPrecision(18, 2);
            
        builder.Property(ia => ia.InternalRateOfReturn)
            .HasPrecision(5, 2); // Percentage: 0.00 to 999.99
            
        builder.Property(ia => ia.PaybackPeriod)
            .HasPrecision(10, 2); // Months: can be large
            
        builder.Property(ia => ia.ReturnOnInvestment)
            .HasPrecision(5, 2); // Percentage: 0.00 to 999.99
            
        builder.Property(ia => ia.CurrencyCode)
            .HasMaxLength(10);
            
        builder.Property(ia => ia.DiscountRate)
            .HasPrecision(5, 2); // Percentage: 0.00 to 999.99
            
        builder.Property(ia => ia.Valuation)
            .HasPrecision(18, 2);
            
        builder.Property(ia => ia.EquityOffering)
            .HasPrecision(5, 2); // Percentage: 0.00 to 100.00
            
        builder.Property(ia => ia.FundingRequired)
            .HasPrecision(18, 2);
            
        builder.Property(ia => ia.RiskLevel)
            .HasMaxLength(20);
            
        builder.Property(ia => ia.InvestmentType)
            .HasMaxLength(50);
            
        builder.Property(ia => ia.InvestorType)
            .HasMaxLength(50);
            
        builder.Property(ia => ia.FundingStage)
            .HasMaxLength(50);
    }
}


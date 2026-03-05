using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class AmortizationEntryConfiguration : IEntityTypeConfiguration<AmortizationEntry>
{
    public void Configure(EntityTypeBuilder<AmortizationEntry> builder)
    {
        builder.ToTable("AmortizationEntries");

        builder.HasKey(ae => ae.Id);

        builder.Property(ae => ae.PaymentAmount)
            .HasPrecision(18, 2);

        builder.Property(ae => ae.PrincipalPortion)
            .HasPrecision(18, 2);

        builder.Property(ae => ae.InterestPortion)
            .HasPrecision(18, 2);

        builder.Property(ae => ae.RemainingBalance)
            .HasPrecision(18, 2);

        builder.HasOne(ae => ae.FinancingSource)
            .WithMany(fs => fs.AmortizationEntries)
            .HasForeignKey(ae => ae.FinancingSourceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique: one entry per payment number per financing source
        builder.HasIndex(ae => new { ae.FinancingSourceId, ae.PaymentNumber })
            .IsUnique()
            .HasDatabaseName("IX_AmortizationEntries_Source_PaymentNumber");
    }
}

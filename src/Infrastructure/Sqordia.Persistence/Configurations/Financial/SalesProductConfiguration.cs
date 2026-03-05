using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class SalesProductConfiguration : IEntityTypeConfiguration<SalesProduct>
{
    public void Configure(EntityTypeBuilder<SalesProduct> builder)
    {
        builder.ToTable("SalesProducts");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sp => sp.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(sp => sp.PaymentDelay)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(sp => sp.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(sp => sp.InputMode)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(SalesInputMode.Quantity);

        builder.Property(sp => sp.VolumeIndexationRate)
            .HasPrecision(5, 2);

        builder.Property(sp => sp.PriceIndexationRate)
            .HasPrecision(5, 2);

        builder.HasOne(sp => sp.FinancialPlan)
            .WithMany(fp => fp.SalesProducts)
            .HasForeignKey(sp => sp.FinancialPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sp => sp.FinancialPlanId)
            .HasDatabaseName("IX_SalesProducts_FinancialPlanId");
    }
}

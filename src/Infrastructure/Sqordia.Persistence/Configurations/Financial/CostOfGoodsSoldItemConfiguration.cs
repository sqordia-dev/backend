using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class CostOfGoodsSoldItemConfiguration : IEntityTypeConfiguration<CostOfGoodsSoldItem>
{
    public void Configure(EntityTypeBuilder<CostOfGoodsSoldItem> builder)
    {
        builder.ToTable("CostOfGoodsSoldItems");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CostMode)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.CostValue)
            .HasPrecision(18, 4);

        builder.Property(c => c.BeginningInventory)
            .HasPrecision(18, 2);

        builder.Property(c => c.CostIndexationRate)
            .HasPrecision(5, 2);

        builder.HasOne(c => c.FinancialPlan)
            .WithMany(fp => fp.CostOfGoodsSoldItems)
            .HasForeignKey(c => c.FinancialPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:1 with SalesProduct
        builder.HasOne(c => c.LinkedSalesProduct)
            .WithOne(sp => sp.CostOfGoodsSoldItem)
            .HasForeignKey<CostOfGoodsSoldItem>(c => c.LinkedSalesProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.FinancialPlanId)
            .HasDatabaseName("IX_CostOfGoodsSoldItems_FinancialPlanId");

        builder.HasIndex(c => c.LinkedSalesProductId)
            .IsUnique()
            .HasDatabaseName("IX_CostOfGoodsSoldItems_LinkedSalesProductId");
    }
}

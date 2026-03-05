using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class CapexAssetConfiguration : IEntityTypeConfiguration<CapexAsset>
{
    public void Configure(EntityTypeBuilder<CapexAsset> builder)
    {
        builder.ToTable("CapexAssets");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AssetType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.PurchaseValue)
            .HasPrecision(18, 2);

        builder.Property(a => a.DepreciationMethod)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.SalvageValue)
            .HasPrecision(18, 2);

        builder.HasOne(a => a.FinancialPlan)
            .WithMany(fp => fp.CapexAssets)
            .HasForeignKey(a => a.FinancialPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.FinancialPlanId)
            .HasDatabaseName("IX_CapexAssets_FinancialPlanId");
    }
}

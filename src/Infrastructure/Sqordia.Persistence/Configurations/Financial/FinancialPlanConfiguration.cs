using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class FinancialPlanConfiguration : IEntityTypeConfiguration<FinancialPlan>
{
    public void Configure(EntityTypeBuilder<FinancialPlan> builder)
    {
        builder.ToTable("FinancialPlans");

        builder.HasKey(fp => fp.Id);

        builder.Property(fp => fp.ProjectionYears)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(fp => fp.StartYear)
            .IsRequired();

        builder.Property(fp => fp.StartMonth)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(fp => fp.SalesTaxFrequency)
            .HasMaxLength(20);

        builder.Property(fp => fp.IsAlreadyOperating)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(fp => fp.DefaultVolumeGrowthRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(5.0m);

        builder.Property(fp => fp.DefaultPriceIndexationRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(2.0m);

        builder.Property(fp => fp.DefaultExpenseIndexationRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(2.0m);

        builder.Property(fp => fp.DefaultSocialChargeRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(15.0m);

        builder.Property(fp => fp.DefaultSalesTaxRate)
            .HasPrecision(5, 2)
            .HasDefaultValue(14.98m);

        // 1:1 with BusinessPlan
        builder.HasOne(fp => fp.BusinessPlan)
            .WithMany()
            .HasForeignKey(fp => fp.BusinessPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fp => fp.BusinessPlanId)
            .IsUnique()
            .HasDatabaseName("IX_FinancialPlans_BusinessPlanId");
    }
}

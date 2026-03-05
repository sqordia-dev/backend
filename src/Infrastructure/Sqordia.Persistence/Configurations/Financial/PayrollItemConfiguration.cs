using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class PayrollItemConfiguration : IEntityTypeConfiguration<PayrollItem>
{
    public void Configure(EntityTypeBuilder<PayrollItem> builder)
    {
        builder.ToTable("PayrollItems");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.JobTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.PayrollType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.EmploymentStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.SalaryFrequency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.SalaryAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.SocialChargeRate)
            .HasPrecision(5, 2);

        builder.Property(p => p.SalaryIndexationRate)
            .HasPrecision(5, 2);

        builder.HasOne(p => p.FinancialPlan)
            .WithMany(fp => fp.PayrollItems)
            .HasForeignKey(p => p.FinancialPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.FinancialPlanId)
            .HasDatabaseName("IX_PayrollItems_FinancialPlanId");
    }
}

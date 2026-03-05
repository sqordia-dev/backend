using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class AdminExpenseItemConfiguration : IEntityTypeConfiguration<AdminExpenseItem>
{
    public void Configure(EntityTypeBuilder<AdminExpenseItem> builder)
    {
        builder.ToTable("AdminExpenseItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.MonthlyAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Frequency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.IndexationRate)
            .HasPrecision(5, 2);

        builder.HasOne(e => e.FinancialPlan)
            .WithMany(fp => fp.AdminExpenseItems)
            .HasForeignKey(e => e.FinancialPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.FinancialPlanId)
            .HasDatabaseName("IX_AdminExpenseItems_FinancialPlanId");
    }
}

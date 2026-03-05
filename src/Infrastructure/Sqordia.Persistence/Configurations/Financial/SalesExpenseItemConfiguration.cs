using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class SalesExpenseItemConfiguration : IEntityTypeConfiguration<SalesExpenseItem>
{
    public void Configure(EntityTypeBuilder<SalesExpenseItem> builder)
    {
        builder.ToTable("SalesExpenseItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.ExpenseMode)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Frequency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.IndexationRate)
            .HasPrecision(5, 2);

        builder.HasOne(e => e.FinancialPlan)
            .WithMany(fp => fp.SalesExpenseItems)
            .HasForeignKey(e => e.FinancialPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.FinancialPlanId)
            .HasDatabaseName("IX_SalesExpenseItems_FinancialPlanId");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class FinancingSourceConfiguration : IEntityTypeConfiguration<FinancingSource>
{
    public void Configure(EntityTypeBuilder<FinancingSource> builder)
    {
        builder.ToTable("FinancingSources");

        builder.HasKey(fs => fs.Id);

        builder.Property(fs => fs.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(fs => fs.FinancingType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(fs => fs.Amount)
            .HasPrecision(18, 2);

        builder.Property(fs => fs.InterestRate)
            .HasPrecision(5, 2);

        builder.HasOne(fs => fs.FinancialPlan)
            .WithMany(fp => fp.FinancingSources)
            .HasForeignKey(fs => fs.FinancialPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fs => fs.FinancialPlanId)
            .HasDatabaseName("IX_FinancingSources_FinancialPlanId");
    }
}

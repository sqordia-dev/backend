using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Persistence.Configurations.Financial;

public class SalesVolumeConfiguration : IEntityTypeConfiguration<SalesVolume>
{
    public void Configure(EntityTypeBuilder<SalesVolume> builder)
    {
        builder.ToTable("SalesVolumes");

        builder.HasKey(sv => sv.Id);

        builder.Property(sv => sv.Year)
            .IsRequired();

        builder.Property(sv => sv.Month)
            .IsRequired();

        builder.Property(sv => sv.Quantity)
            .HasPrecision(18, 2);

        builder.HasOne(sv => sv.SalesProduct)
            .WithMany(sp => sp.SalesVolumes)
            .HasForeignKey(sv => sv.SalesProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique: one volume entry per product per year/month
        builder.HasIndex(sv => new { sv.SalesProductId, sv.Year, sv.Month })
            .IsUnique()
            .HasDatabaseName("IX_SalesVolumes_Product_Year_Month");
    }
}

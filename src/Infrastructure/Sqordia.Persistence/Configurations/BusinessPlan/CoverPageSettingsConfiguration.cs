using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class CoverPageSettingsConfiguration : IEntityTypeConfiguration<CoverPageSettings>
{
    public void Configure(EntityTypeBuilder<CoverPageSettings> builder)
    {
        builder.ToTable("CoverPageSettings");

        builder.HasKey(cp => cp.Id);

        // Branding
        builder.Property(cp => cp.LogoUrl)
            .HasColumnType("text");

        builder.Property(cp => cp.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cp => cp.DocumentTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cp => cp.PrimaryColor)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(cp => cp.LayoutStyle)
            .IsRequired()
            .HasMaxLength(20);

        // Contact Information
        builder.Property(cp => cp.ContactName)
            .HasMaxLength(100);

        builder.Property(cp => cp.ContactTitle)
            .HasMaxLength(100);

        builder.Property(cp => cp.ContactPhone)
            .HasMaxLength(30);

        builder.Property(cp => cp.ContactEmail)
            .HasMaxLength(200);

        builder.Property(cp => cp.Website)
            .HasMaxLength(200);

        // Business Address
        builder.Property(cp => cp.AddressLine1)
            .HasMaxLength(200);

        builder.Property(cp => cp.AddressLine2)
            .HasMaxLength(200);

        builder.Property(cp => cp.City)
            .HasMaxLength(100);

        builder.Property(cp => cp.StateProvince)
            .HasMaxLength(100);

        builder.Property(cp => cp.PostalCode)
            .HasMaxLength(20);

        builder.Property(cp => cp.Country)
            .HasMaxLength(100);

        // Relationship - One-to-One with BusinessPlan
        builder.HasOne(cp => cp.BusinessPlan)
            .WithOne(bp => bp.CoverPageSettings)
            .HasForeignKey<CoverPageSettings>(cp => cp.BusinessPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cp => cp.BusinessPlanId)
            .IsUnique(); // One cover page per business plan
    }
}

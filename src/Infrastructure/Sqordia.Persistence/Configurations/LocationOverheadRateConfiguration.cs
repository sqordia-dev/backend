using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class LocationOverheadRateConfiguration : IEntityTypeConfiguration<LocationOverheadRate>
{
    public void Configure(EntityTypeBuilder<LocationOverheadRate> builder)
    {
        builder.ToTable("LocationOverheadRates");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Province)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.ProvinceCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(r => r.OverheadRate)
            .HasPrecision(5, 2); // Percentage: 0.00 to 999.99

        builder.Property(r => r.InsuranceRate)
            .HasPrecision(18, 2); // Monthly amount in CAD

        builder.Property(r => r.TaxRate)
            .HasPrecision(5, 2); // Percentage: 0.00 to 999.99

        builder.Property(r => r.OfficeCost)
            .HasPrecision(18, 2); // Monthly amount in CAD

        builder.Property(r => r.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("CAD");

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(100);

        builder.Property(r => r.UpdatedBy)
            .HasMaxLength(100);

        // Index for efficient lookups by province
        builder.HasIndex(r => new { r.Province, r.IsActive });
        builder.HasIndex(r => new { r.ProvinceCode, r.IsActive });

        // Seed data for Canadian provinces/territories
        SeedData(builder);
    }

    private static void SeedData(EntityTypeBuilder<LocationOverheadRate> builder)
    {
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Use anonymous objects with explicit Guids for seed data since BaseEntity.Id has protected setter
        builder.HasData(
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000001"),
                Province = "Alberta",
                ProvinceCode = "AB",
                OverheadRate = 10.0m,
                InsuranceRate = 200m,
                TaxRate = 15.0m,
                OfficeCost = 600m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000002"),
                Province = "British Columbia",
                ProvinceCode = "BC",
                OverheadRate = 12.0m,
                InsuranceRate = 250m,
                TaxRate = 20.0m,
                OfficeCost = 800m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000003"),
                Province = "Manitoba",
                ProvinceCode = "MB",
                OverheadRate = 10.0m,
                InsuranceRate = 180m,
                TaxRate = 17.0m,
                OfficeCost = 450m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000004"),
                Province = "New Brunswick",
                ProvinceCode = "NB",
                OverheadRate = 9.0m,
                InsuranceRate = 170m,
                TaxRate = 20.0m,
                OfficeCost = 400m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000005"),
                Province = "Newfoundland and Labrador",
                ProvinceCode = "NL",
                OverheadRate = 9.0m,
                InsuranceRate = 175m,
                TaxRate = 20.0m,
                OfficeCost = 420m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000006"),
                Province = "Nova Scotia",
                ProvinceCode = "NS",
                OverheadRate = 9.5m,
                InsuranceRate = 175m,
                TaxRate = 21.0m,
                OfficeCost = 450m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000007"),
                Province = "Ontario",
                ProvinceCode = "ON",
                OverheadRate = 12.0m,
                InsuranceRate = 250m,
                TaxRate = 20.0m,
                OfficeCost = 750m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000008"),
                Province = "Prince Edward Island",
                ProvinceCode = "PE",
                OverheadRate = 8.5m,
                InsuranceRate = 160m,
                TaxRate = 20.0m,
                OfficeCost = 380m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000009"),
                Province = "Quebec",
                ProvinceCode = "QC",
                OverheadRate = 11.0m,
                InsuranceRate = 220m,
                TaxRate = 24.0m,
                OfficeCost = 600m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000010"),
                Province = "Saskatchewan",
                ProvinceCode = "SK",
                OverheadRate = 9.5m,
                InsuranceRate = 180m,
                TaxRate = 16.0m,
                OfficeCost = 450m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000011"),
                Province = "Northwest Territories",
                ProvinceCode = "NT",
                OverheadRate = 11.0m,
                InsuranceRate = 200m,
                TaxRate = 15.0m,
                OfficeCost = 700m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000012"),
                Province = "Nunavut",
                ProvinceCode = "NU",
                OverheadRate = 12.0m,
                InsuranceRate = 220m,
                TaxRate = 15.0m,
                OfficeCost = 900m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            },
            new
            {
                Id = new Guid("a0000000-0000-0000-0000-000000000013"),
                Province = "Yukon",
                ProvinceCode = "YT",
                OverheadRate = 10.5m,
                InsuranceRate = 190m,
                TaxRate = 15.0m,
                OfficeCost = 650m,
                Currency = "CAD",
                EffectiveDate = now,
                ExpiryDate = (DateTime?)null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "System",
                UpdatedBy = "System"
            }
        );
    }
}

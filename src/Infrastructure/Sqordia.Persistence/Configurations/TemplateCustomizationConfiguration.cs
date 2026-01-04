using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class TemplateCustomizationConfiguration : IEntityTypeConfiguration<TemplateCustomization>
{
    public void Configure(EntityTypeBuilder<TemplateCustomization> builder)
    {
        builder.ToTable("TemplateCustomizations");
        
        builder.HasKey(tc => tc.Id);
        
        builder.Property(tc => tc.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(tc => tc.Description)
            .HasMaxLength(1000);
            
        builder.Property(tc => tc.Rating)
            .HasPrecision(3, 2); // Rating: 0.00 to 5.00
            
        builder.Property(tc => tc.Version)
            .HasMaxLength(50);
    }
}


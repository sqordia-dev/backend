using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("Templates");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(t => t.Description)
            .HasMaxLength(1000);
            
        builder.Property(t => t.Industry)
            .HasMaxLength(100);
            
        builder.Property(t => t.TargetAudience)
            .HasMaxLength(200);
            
        builder.Property(t => t.Language)
            .HasMaxLength(10);
            
        builder.Property(t => t.Country)
            .HasMaxLength(100);
            
        builder.Property(t => t.Rating)
            .HasPrecision(3, 2); // Rating: 0.00 to 5.00
            
        builder.Property(t => t.Author)
            .HasMaxLength(200);
            
        builder.Property(t => t.AuthorEmail)
            .HasMaxLength(256);
            
        builder.Property(t => t.Version)
            .HasMaxLength(50);
    }
}


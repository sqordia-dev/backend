using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

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
            .HasMaxLength(2000);

        builder.Property(t => t.Content)
            .HasColumnType("text");

        builder.Property(t => t.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(TemplateStatus.Draft);

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

        builder.Property(t => t.Tags)
            .HasMaxLength(500);

        builder.Property(t => t.PreviewImage)
            .HasMaxLength(500);

        builder.Property(t => t.Author)
            .HasMaxLength(200);

        builder.Property(t => t.AuthorEmail)
            .HasMaxLength(256);

        builder.Property(t => t.Version)
            .HasMaxLength(50);

        builder.Property(t => t.Changelog)
            .HasColumnType("text");

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(200);

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(t => t.Name);
        builder.HasIndex(t => t.Category);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.IsPublic);
        builder.HasIndex(t => t.Author);
        builder.HasIndex(t => new { t.Category, t.Status });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class ContentPageConfiguration : IEntityTypeConfiguration<ContentPage>
{
    public void Configure(EntityTypeBuilder<ContentPage> builder)
    {
        builder.ToTable("ContentPages");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.PageKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Content)
            .IsRequired();

        builder.Property(c => c.Language)
            .IsRequired()
            .HasMaxLength(2);

        builder.HasIndex(c => new { c.PageKey, c.Language })
            .IsUnique();

        builder.HasIndex(c => c.IsPublished);
    }
}


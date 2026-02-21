using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Persistence.Configurations;

public class CmsContentTemplateConfiguration : IEntityTypeConfiguration<CmsContentTemplate>
{
    public void Configure(EntityTypeBuilder<CmsContentTemplate> builder)
    {
        builder.ToTable("CmsContentTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.PageKey)
            .HasMaxLength(100);

        builder.Property(t => t.SectionKey)
            .HasMaxLength(100);

        builder.Property(t => t.TemplateData)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(t => t.PreviewImageUrl)
            .HasMaxLength(500);

        builder.Property(t => t.IsPublic)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.CreatedByUserId)
            .IsRequired();

        // Indexes
        builder.HasIndex(t => t.CreatedByUserId);
        builder.HasIndex(t => t.IsPublic);
        builder.HasIndex(t => t.PageKey);
        builder.HasIndex(t => t.SectionKey);
        builder.HasIndex(t => new { t.PageKey, t.SectionKey });
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Persistence.Configurations;

public class CmsAssetConfiguration : IEntityTypeConfiguration<CmsAsset>
{
    public void Configure(EntityTypeBuilder<CmsAsset> builder)
    {
        builder.ToTable("CmsAssets");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Url)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.FileSize)
            .IsRequired();

        builder.Property(a => a.UploadedByUserId)
            .IsRequired();

        builder.Property(a => a.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(a => a.Category);

        builder.HasIndex(a => a.UploadedByUserId);
    }
}

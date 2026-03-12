using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Enums.NotificationPriority.Normal);

        builder.Property(n => n.TitleFr)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.TitleEn)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.MessageFr)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.MessageEn)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.Property(n => n.MetadataJson)
            .HasColumnType("jsonb");

        builder.Property(n => n.GroupKey)
            .HasMaxLength(200);

        // Indexes for common queries
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => new { n.UserId, n.Created });
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => new { n.UserId, n.GroupKey })
            .HasFilter("\"GroupKey\" IS NOT NULL");

        // Soft delete query filter
        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}

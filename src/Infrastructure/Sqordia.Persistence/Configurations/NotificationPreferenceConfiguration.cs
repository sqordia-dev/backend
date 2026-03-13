using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.NotificationType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.InAppEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.EmailEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.EmailFrequency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Enums.NotificationFrequency.Instant);

        builder.Property(p => p.SoundEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Unique constraint: one preference per user per type
        builder.HasIndex(p => new { p.UserId, p.NotificationType })
            .IsUnique();

        builder.HasIndex(p => p.UserId);

        // Soft delete query filter
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

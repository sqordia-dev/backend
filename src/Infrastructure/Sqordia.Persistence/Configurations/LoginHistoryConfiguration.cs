using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Persistence.Configurations;

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.HasKey(lh => lh.Id);

        builder.Property(lh => lh.LoginAttemptAt)
            .IsRequired();

        builder.Property(lh => lh.IsSuccessful)
            .IsRequired();

        builder.Property(lh => lh.FailureReason)
            .HasMaxLength(500);

        builder.Property(lh => lh.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length

        builder.Property(lh => lh.UserAgent)
            .HasMaxLength(500);

        builder.Property(lh => lh.DeviceType)
            .HasMaxLength(50);

        builder.Property(lh => lh.Browser)
            .HasMaxLength(100);

        builder.Property(lh => lh.OperatingSystem)
            .HasMaxLength(100);

        builder.Property(lh => lh.Country)
            .HasMaxLength(100);

        builder.Property(lh => lh.City)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(lh => lh.User)
            .WithMany()
            .HasForeignKey(lh => lh.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(lh => lh.UserId);
        builder.HasIndex(lh => lh.LoginAttemptAt);
        builder.HasIndex(lh => new { lh.UserId, lh.LoginAttemptAt });
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Persistence.Configurations;

public class TwoFactorAuthConfiguration : IEntityTypeConfiguration<TwoFactorAuth>
{
    public void Configure(EntityTypeBuilder<TwoFactorAuth> builder)
    {
        builder.ToTable("TwoFactorAuths");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.SecretKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.IsEnabled)
            .IsRequired();

        builder.Property(t => t.BackupCodes)
            .HasMaxLength(2000);

        builder.Property(t => t.FailedAttempts)
            .IsRequired();

        // Relationship with User
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on UserId for faster queries
        builder.HasIndex(t => t.UserId)
            .IsUnique();
    }
}


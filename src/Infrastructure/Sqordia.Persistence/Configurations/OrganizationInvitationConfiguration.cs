using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class OrganizationInvitationConfiguration : IEntityTypeConfiguration<OrganizationInvitation>
{
    public void Configure(EntityTypeBuilder<OrganizationInvitation> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Token)
            .IsRequired();

        builder.HasOne(i => i.Organization)
            .WithMany()
            .HasForeignKey(i => i.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.InvitedByUser)
            .WithMany()
            .HasForeignKey(i => i.InvitedByUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.AcceptedByUser)
            .WithMany()
            .HasForeignKey(i => i.AcceptedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique token for invitation links
        builder.HasIndex(i => i.Token)
            .IsUnique();

        // Prevent duplicate pending invitations for same email+org
        builder.HasIndex(i => new { i.OrganizationId, i.Email, i.Status })
            .HasFilter("\"Status\" = 'Pending'");

        builder.HasIndex(i => i.OrganizationId);
        builder.HasIndex(i => i.Email);
        builder.HasIndex(i => i.Status);
    }
}

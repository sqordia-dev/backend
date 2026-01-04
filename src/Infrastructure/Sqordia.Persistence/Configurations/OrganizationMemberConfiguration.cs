using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> builder)
    {
        builder.HasKey(om => om.Id);
        
        builder.Property(om => om.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(om => om.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.HasOne(om => om.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(om => om.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(om => om.User)
            .WithMany()
            .HasForeignKey(om => om.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        // Ensure a user can only be a member once per organization
        builder.HasIndex(om => new { om.OrganizationId, om.UserId })
            .IsUnique()
            .HasFilter("\"IsActive\" = true");
        
        builder.HasIndex(om => om.OrganizationId);
        builder.HasIndex(om => om.UserId);
        builder.HasIndex(om => om.Role);
        builder.HasIndex(om => om.IsActive);
    }
}


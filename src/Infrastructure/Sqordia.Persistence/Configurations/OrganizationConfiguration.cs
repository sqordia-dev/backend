using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(o => o.Description)
            .HasMaxLength(1000);
        
        builder.Property(o => o.Website)
            .HasMaxLength(500);
        
        builder.Property(o => o.LogoUrl)
            .HasMaxLength(500);
        
        builder.Property(o => o.OrganizationType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(o => o.MaxMembers)
            .IsRequired()
            .HasDefaultValue(10);
        
        builder.Property(o => o.AllowMemberInvites)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(o => o.RequireEmailVerification)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.HasMany(o => o.Members)
            .WithOne(m => m.Organization)
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(o => o.Name);
        builder.HasIndex(o => o.IsActive);
        builder.HasIndex(o => o.CreatedBy);
        builder.HasIndex(o => new { o.IsActive, o.CreatedBy });
    }
}


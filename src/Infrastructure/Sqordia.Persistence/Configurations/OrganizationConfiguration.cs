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

        // Business Context fields
        builder.Property(o => o.Industry).HasMaxLength(100);
        builder.Property(o => o.Sector).HasMaxLength(100);
        builder.Property(o => o.TeamSize).HasMaxLength(50);
        builder.Property(o => o.FundingStatus).HasMaxLength(50);
        builder.Property(o => o.TargetMarket).HasMaxLength(100);
        builder.Property(o => o.BusinessStage).HasMaxLength(50);
        builder.Property(o => o.GoalsJson).HasColumnType("text");
        builder.Property(o => o.City).HasMaxLength(100);
        builder.Property(o => o.Province).HasMaxLength(100);
        builder.Property(o => o.Country).HasMaxLength(100);
        builder.Property(o => o.ProfileCompletenessScore)
            .IsRequired()
            .HasDefaultValue(0);

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


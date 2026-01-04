using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class GrantApplicationConfiguration : IEntityTypeConfiguration<GrantApplication>
{
    public void Configure(EntityTypeBuilder<GrantApplication> builder)
    {
        builder.ToTable("GrantApplications");
        
        builder.HasKey(ga => ga.Id);
        
        builder.Property(ga => ga.GrantName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(ga => ga.GrantingOrganization)
            .HasMaxLength(200);
            
        builder.Property(ga => ga.GrantType)
            .HasMaxLength(100);
            
        builder.Property(ga => ga.RequestedAmount)
            .HasPrecision(18, 2);
            
        builder.Property(ga => ga.MatchingFunds)
            .HasPrecision(18, 2);
            
        builder.Property(ga => ga.Status)
            .HasMaxLength(50);
            
        builder.Property(ga => ga.Decision)
            .HasMaxLength(50);
    }
}


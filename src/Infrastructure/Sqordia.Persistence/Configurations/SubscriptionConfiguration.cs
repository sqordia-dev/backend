using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(s => s.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("CAD");
            
        builder.Property(s => s.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
        
        // Stripe fields
        builder.Property(s => s.StripeCustomerId)
            .HasMaxLength(255);
            
        builder.Property(s => s.StripeSubscriptionId)
            .HasMaxLength(255);
            
        builder.Property(s => s.StripePriceId)
            .HasMaxLength(255);
            
        // Indexes
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.OrganizationId);
        builder.HasIndex(s => s.SubscriptionPlanId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => new { s.OrganizationId, s.Status });
        builder.HasIndex(s => s.StripeCustomerId);
        builder.HasIndex(s => s.StripeSubscriptionId);
        
        // Relationships
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(s => s.Organization)
            .WithMany()
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(s => s.Plan)
            .WithMany(sp => sp.Subscriptions)
            .HasForeignKey(s => s.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


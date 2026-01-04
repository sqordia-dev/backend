using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        
        builder.HasKey(sp => sp.Id);
        
        builder.Property(sp => sp.PlanType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(sp => sp.Description)
            .HasMaxLength(500);
            
        builder.Property(sp => sp.Price)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(sp => sp.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("CAD");
            
        builder.Property(sp => sp.BillingCycle)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
            
        builder.Property(sp => sp.Features)
            .HasColumnType("text")
            .HasDefaultValue("[]");
            
        builder.Property(sp => sp.IsActive)
            .HasDefaultValue(true);
            
        // Indexes
        builder.HasIndex(sp => sp.PlanType);
        builder.HasIndex(sp => sp.IsActive);
        
        // Relationships
        builder.HasMany(sp => sp.Subscriptions)
            .WithOne(s => s.Plan)
            .HasForeignKey(s => s.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


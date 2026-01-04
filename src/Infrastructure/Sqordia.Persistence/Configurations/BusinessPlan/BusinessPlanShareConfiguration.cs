using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class BusinessPlanShareConfiguration : IEntityTypeConfiguration<BusinessPlanShare>
{
    public void Configure(EntityTypeBuilder<BusinessPlanShare> builder)
    {
        builder.ToTable("BusinessPlanShares");
        
        builder.HasKey(bs => bs.Id);
        
        builder.Property(bs => bs.SharedWithEmail)
            .HasMaxLength(256);
            
        builder.Property(bs => bs.Permission)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(bs => bs.PublicToken)
            .HasMaxLength(50);
        
        // Relationships
        builder.HasOne(bs => bs.BusinessPlan)
            .WithMany(bp => bp.Shares)
            .HasForeignKey(bs => bs.BusinessPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(bs => bs.SharedWithUser)
            .WithMany()
            .HasForeignKey(bs => bs.SharedWithUserId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascade cycles in SQL Server
        
        // Indexes
        builder.HasIndex(bs => bs.BusinessPlanId);
        builder.HasIndex(bs => bs.SharedWithUserId);
        builder.HasIndex(bs => bs.PublicToken)
            .IsUnique()
            .HasFilter("\"PublicToken\" IS NOT NULL");
        builder.HasIndex(bs => new { bs.BusinessPlanId, bs.IsActive });
    }
}


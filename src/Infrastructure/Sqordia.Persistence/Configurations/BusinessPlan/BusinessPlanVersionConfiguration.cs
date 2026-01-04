using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class BusinessPlanVersionConfiguration : IEntityTypeConfiguration<BusinessPlanVersion>
{
    public void Configure(EntityTypeBuilder<BusinessPlanVersion> builder)
    {
        builder.ToTable("BusinessPlanVersions");
        
        builder.HasKey(bv => bv.Id);
        
        builder.Property(bv => bv.VersionNumber)
            .IsRequired();
            
        builder.Property(bv => bv.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(bv => bv.Description)
            .HasMaxLength(1000);
            
        builder.Property(bv => bv.PlanType)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(bv => bv.Status)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(bv => bv.Comment)
            .HasMaxLength(1000);
        
        // Content sections (large text fields)
        builder.Property(bv => bv.ExecutiveSummary)
            .HasColumnType("text");
            
        builder.Property(bv => bv.ProblemStatement)
            .HasColumnType("text");
            
        builder.Property(bv => bv.Solution)
            .HasColumnType("text");
            
        builder.Property(bv => bv.MarketAnalysis)
            .HasColumnType("text");
            
        builder.Property(bv => bv.CompetitiveAnalysis)
            .HasColumnType("text");
            
        builder.Property(bv => bv.SwotAnalysis)
            .HasColumnType("text");
            
        builder.Property(bv => bv.BusinessModel)
            .HasColumnType("text");
            
        builder.Property(bv => bv.MarketingStrategy)
            .HasColumnType("text");
            
        builder.Property(bv => bv.BrandingStrategy)
            .HasColumnType("text");
            
        builder.Property(bv => bv.OperationsPlan)
            .HasColumnType("text");
            
        builder.Property(bv => bv.ManagementTeam)
            .HasColumnType("text");
            
        builder.Property(bv => bv.FinancialProjections)
            .HasColumnType("text");
            
        builder.Property(bv => bv.FundingRequirements)
            .HasColumnType("text");
            
        builder.Property(bv => bv.RiskAnalysis)
            .HasColumnType("text");
            
        builder.Property(bv => bv.ExitStrategy)
            .HasColumnType("text");
            
        builder.Property(bv => bv.AppendixData)
            .HasColumnType("text");
            
        // OBNL-specific sections
        builder.Property(bv => bv.MissionStatement)
            .HasColumnType("text");
            
        builder.Property(bv => bv.SocialImpact)
            .HasColumnType("text");
            
        builder.Property(bv => bv.BeneficiaryProfile)
            .HasColumnType("text");
            
        builder.Property(bv => bv.GrantStrategy)
            .HasColumnType("text");
            
        builder.Property(bv => bv.SustainabilityPlan)
            .HasColumnType("text");
        
        // Relationships
        builder.HasOne(bv => bv.BusinessPlan)
            .WithMany(bp => bp.Versions)
            .HasForeignKey(bv => bv.BusinessPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(bv => bv.BusinessPlanId);
        builder.HasIndex(bv => new { bv.BusinessPlanId, bv.VersionNumber })
            .IsUnique();
        builder.HasIndex(bv => bv.Created);
    }
}


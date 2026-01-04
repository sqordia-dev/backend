using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class BusinessPlanConfiguration : IEntityTypeConfiguration<Domain.Entities.BusinessPlan.BusinessPlan>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.BusinessPlan.BusinessPlan> builder)
    {
        builder.ToTable("BusinessPlans");
        
        builder.HasKey(bp => bp.Id);
        
        builder.Property(bp => bp.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(bp => bp.Description)
            .HasMaxLength(1000);
            
        builder.Property(bp => bp.PlanType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(bp => bp.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(bp => bp.CompletionPercentage)
            .HasPrecision(5, 2); // 0.00 to 100.00
            
        builder.Property(bp => bp.GenerationModel)
            .HasMaxLength(100);
            
        // Content sections (large text fields)
        builder.Property(bp => bp.ExecutiveSummary)
            .HasColumnType("text");
            
        builder.Property(bp => bp.ProblemStatement)
            .HasColumnType("text");
            
        builder.Property(bp => bp.Solution)
            .HasColumnType("text");
            
        builder.Property(bp => bp.MarketAnalysis)
            .HasColumnType("text");
            
        builder.Property(bp => bp.CompetitiveAnalysis)
            .HasColumnType("text");
            
        builder.Property(bp => bp.SwotAnalysis)
            .HasColumnType("text");
            
        builder.Property(bp => bp.BusinessModel)
            .HasColumnType("text");
            
        builder.Property(bp => bp.MarketingStrategy)
            .HasColumnType("text");
            
        builder.Property(bp => bp.BrandingStrategy)
            .HasColumnType("text");
            
        builder.Property(bp => bp.OperationsPlan)
            .HasColumnType("text");
            
        builder.Property(bp => bp.ManagementTeam)
            .HasColumnType("text");
            
        builder.Property(bp => bp.FinancialProjections)
            .HasColumnType("text");
            
        builder.Property(bp => bp.FundingRequirements)
            .HasColumnType("text");
            
        builder.Property(bp => bp.RiskAnalysis)
            .HasColumnType("text");
            
        builder.Property(bp => bp.ExitStrategy)
            .HasColumnType("text");
            
        builder.Property(bp => bp.AppendixData)
            .HasColumnType("text");
            
        // OBNL-specific sections
        builder.Property(bp => bp.MissionStatement)
            .HasColumnType("text");
            
        builder.Property(bp => bp.SocialImpact)
            .HasColumnType("text");
            
        builder.Property(bp => bp.BeneficiaryProfile)
            .HasColumnType("text");
            
        builder.Property(bp => bp.GrantStrategy)
            .HasColumnType("text");
            
        builder.Property(bp => bp.SustainabilityPlan)
            .HasColumnType("text");
        
        // Relationships
        builder.HasOne(bp => bp.Organization)
            .WithMany() // Organization doesn't have navigation property back
            .HasForeignKey(bp => bp.OrganizationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(bp => bp.QuestionnaireResponses)
            .WithOne(qr => qr.BusinessPlan)
            .HasForeignKey(qr => qr.BusinessPlanId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(bp => bp.FinancialProjectionDetails)
            .WithOne(fp => fp.BusinessPlan)
            .HasForeignKey(fp => fp.BusinessPlanId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(bp => bp.Shares)
            .WithOne(bs => bs.BusinessPlan)
            .HasForeignKey(bs => bs.BusinessPlanId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(bp => bp.Versions)
            .WithOne(bv => bv.BusinessPlan)
            .HasForeignKey(bv => bv.BusinessPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(bp => bp.OrganizationId);
        builder.HasIndex(bp => bp.Status);
        builder.HasIndex(bp => bp.PlanType);
        builder.HasIndex(bp => bp.CreatedBy);
        builder.HasIndex(bp => new { bp.OrganizationId, bp.Status });
    }
}


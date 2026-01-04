using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Represents a version snapshot of a business plan
/// Stores the complete state of a business plan at a specific point in time
/// </summary>
public class BusinessPlanVersion : BaseAuditableEntity
{
    public Guid BusinessPlanId { get; private set; }
    public int VersionNumber { get; private set; }
    public string? Comment { get; private set; }
    
    // Snapshot of all content sections
    public string? ExecutiveSummary { get; private set; }
    public string? ProblemStatement { get; private set; }
    public string? Solution { get; private set; }
    public string? MarketAnalysis { get; private set; }
    public string? CompetitiveAnalysis { get; private set; }
    public string? SwotAnalysis { get; private set; }
    public string? BusinessModel { get; private set; }
    public string? MarketingStrategy { get; private set; }
    public string? BrandingStrategy { get; private set; }
    public string? OperationsPlan { get; private set; }
    public string? ManagementTeam { get; private set; }
    public string? FinancialProjections { get; private set; }
    public string? FundingRequirements { get; private set; }
    public string? RiskAnalysis { get; private set; }
    public string? ExitStrategy { get; private set; }
    public string? AppendixData { get; private set; }
    
    // OBNL-specific sections
    public string? MissionStatement { get; private set; }
    public string? SocialImpact { get; private set; }
    public string? BeneficiaryProfile { get; private set; }
    public string? GrantStrategy { get; private set; }
    public string? SustainabilityPlan { get; private set; }
    
    // Metadata snapshot
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string PlanType { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    
    // Navigation properties
    public BusinessPlan BusinessPlan { get; private set; } = null!;
    
    private BusinessPlanVersion() { } // EF Core constructor
    
    public BusinessPlanVersion(
        Guid businessPlanId,
        int versionNumber,
        BusinessPlan businessPlan,
        string? comment = null)
    {
        BusinessPlanId = businessPlanId;
        VersionNumber = versionNumber;
        Comment = comment;
        
        // Snapshot all content
        Title = businessPlan.Title;
        Description = businessPlan.Description;
        PlanType = businessPlan.PlanType.ToString();
        Status = businessPlan.Status.ToString();
        
        ExecutiveSummary = businessPlan.ExecutiveSummary;
        ProblemStatement = businessPlan.ProblemStatement;
        Solution = businessPlan.Solution;
        MarketAnalysis = businessPlan.MarketAnalysis;
        CompetitiveAnalysis = businessPlan.CompetitiveAnalysis;
        SwotAnalysis = businessPlan.SwotAnalysis;
        BusinessModel = businessPlan.BusinessModel;
        MarketingStrategy = businessPlan.MarketingStrategy;
        BrandingStrategy = businessPlan.BrandingStrategy;
        OperationsPlan = businessPlan.OperationsPlan;
        ManagementTeam = businessPlan.ManagementTeam;
        FinancialProjections = businessPlan.FinancialProjections;
        FundingRequirements = businessPlan.FundingRequirements;
        RiskAnalysis = businessPlan.RiskAnalysis;
        ExitStrategy = businessPlan.ExitStrategy;
        AppendixData = businessPlan.AppendixData;
        
        MissionStatement = businessPlan.MissionStatement;
        SocialImpact = businessPlan.SocialImpact;
        BeneficiaryProfile = businessPlan.BeneficiaryProfile;
        GrantStrategy = businessPlan.GrantStrategy;
        SustainabilityPlan = businessPlan.SustainabilityPlan;
    }
    
    public void UpdateComment(string? comment)
    {
        Comment = comment;
    }
}


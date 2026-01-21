namespace Sqordia.Functions.ExportHandler.Models;

/// <summary>
/// Data model for business plan export containing all sections
/// </summary>
public class BusinessPlanExportData
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }

    // Organization info
    public string OrganizationName { get; set; } = string.Empty;

    // Content sections
    public string? ExecutiveSummary { get; set; }
    public string? ProblemStatement { get; set; }
    public string? Solution { get; set; }
    public string? MarketAnalysis { get; set; }
    public string? CompetitiveAnalysis { get; set; }
    public string? SwotAnalysis { get; set; }
    public string? BusinessModel { get; set; }
    public string? MarketingStrategy { get; set; }
    public string? BrandingStrategy { get; set; }
    public string? OperationsPlan { get; set; }
    public string? ManagementTeam { get; set; }
    public string? FinancialProjections { get; set; }
    public string? FundingRequirements { get; set; }
    public string? RiskAnalysis { get; set; }
    public string? ExitStrategy { get; set; }
    public string? AppendixData { get; set; }

    // OBNL-specific sections
    public string? MissionStatement { get; set; }
    public string? SocialImpact { get; set; }
    public string? BeneficiaryProfile { get; set; }
    public string? GrantStrategy { get; set; }
    public string? SustainabilityPlan { get; set; }

    /// <summary>
    /// Returns all non-empty content sections in display order
    /// </summary>
    public IEnumerable<(string Title, string Content)> GetSections()
    {
        var sections = new List<(string, string?)>
        {
            ("Executive Summary", ExecutiveSummary),
            ("Mission Statement", MissionStatement),
            ("Problem Statement", ProblemStatement),
            ("Solution", Solution),
            ("Market Analysis", MarketAnalysis),
            ("Competitive Analysis", CompetitiveAnalysis),
            ("SWOT Analysis", SwotAnalysis),
            ("Business Model", BusinessModel),
            ("Marketing Strategy", MarketingStrategy),
            ("Branding Strategy", BrandingStrategy),
            ("Operations Plan", OperationsPlan),
            ("Management Team", ManagementTeam),
            ("Financial Projections", FinancialProjections),
            ("Funding Requirements", FundingRequirements),
            ("Risk Analysis", RiskAnalysis),
            ("Social Impact", SocialImpact),
            ("Beneficiary Profile", BeneficiaryProfile),
            ("Grant Strategy", GrantStrategy),
            ("Sustainability Plan", SustainabilityPlan),
            ("Exit Strategy", ExitStrategy),
            ("Appendix", AppendixData)
        };

        return sections
            .Where(s => !string.IsNullOrWhiteSpace(s.Item2))
            .Select(s => (s.Item1, s.Item2!));
    }
}

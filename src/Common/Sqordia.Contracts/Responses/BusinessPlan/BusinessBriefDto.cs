namespace Sqordia.Contracts.Responses.BusinessPlan;

/// <summary>
/// Structured Business Brief that provides unified context for AI generation.
/// Contains a holistic understanding of the business derived from all questionnaire answers + onboarding data.
/// </summary>
public class BusinessBriefDto
{
    public Guid BusinessPlanId { get; set; }
    public CompanyProfileDto CompanyProfile { get; set; } = new();
    public BusinessConceptDto BusinessConcept { get; set; } = new();
    public MarketContextDto MarketContext { get; set; } = new();
    public OperationalContextDto OperationalContext { get; set; } = new();
    public FinancialContextDto FinancialContext { get; set; } = new();
    public StrategicContextDto StrategicContext { get; set; } = new();
    public MaturityAssessmentDto MaturityAssessment { get; set; } = new();
    public GenerationGuidanceDto GenerationGuidance { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class CompanyProfileDto
{
    public string? Name { get; set; }
    public string? LegalStructure { get; set; }
    public string? Industry { get; set; }
    public string? Sector { get; set; }
    public string? Stage { get; set; }
}

public class BusinessConceptDto
{
    public string? Problem { get; set; }
    public string? Solution { get; set; }
    public string? ValueProposition { get; set; }
    public List<string> Differentiators { get; set; } = new();
}

public class MarketContextDto
{
    public string? TargetCustomers { get; set; }
    public string? MarketSize { get; set; }
    public string? Competitors { get; set; }
    public string? Positioning { get; set; }
}

public class OperationalContextDto
{
    public string? Team { get; set; }
    public string? Resources { get; set; }
    public string? Timeline { get; set; }
    public string? Location { get; set; }
}

public class FinancialContextDto
{
    public string? PersonalInvestment { get; set; }
    public string? FundingNeeds { get; set; }
    public string? RevenueModel { get; set; }
    public string? Pricing { get; set; }
}

public class StrategicContextDto
{
    public string? Objectives { get; set; }
    public SwotSummaryDto? Swot { get; set; }
    public string? Risks { get; set; }
    public string? GrowthStrategy { get; set; }
}

public class SwotSummaryDto
{
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Opportunities { get; set; } = new();
    public List<string> Threats { get; set; } = new();
}

public class MaturityAssessmentDto
{
    public int Score { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Gaps { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class GenerationGuidanceDto
{
    public string? Tone { get; set; }
    public List<string> FocusAreas { get; set; } = new();
    public List<string> CautionAreas { get; set; } = new();
    public string? PersonaSpecificNotes { get; set; }
}

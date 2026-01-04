using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class GrantApplication : BaseEntity
{
    public Guid OBNLBusinessPlanId { get; set; }
    public string GrantName { get; set; } = string.Empty;
    public string GrantingOrganization { get; set; } = string.Empty;
    public string GrantType { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal MatchingFunds { get; set; }
    public string ProjectDescription { get; set; } = string.Empty;
    public string Objectives { get; set; } = string.Empty;
    public string ExpectedOutcomes { get; set; } = string.Empty;
    public string TargetPopulation { get; set; } = string.Empty;
    public string GeographicScope { get; set; } = string.Empty;
    public string Timeline { get; set; } = string.Empty;
    public string BudgetBreakdown { get; set; } = string.Empty;
    public string EvaluationPlan { get; set; } = string.Empty;
    public string SustainabilityPlan { get; set; } = string.Empty;
    public DateTime ApplicationDeadline { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public OBNLBusinessPlan OBNLBusinessPlan { get; set; } = null!;
}

namespace Sqordia.Application.OBNL.Queries;

public record GrantApplicationDto
{
    public Guid Id { get; init; }
    public Guid OBNLBusinessPlanId { get; init; }
    public string GrantName { get; init; } = string.Empty;
    public string GrantingOrganization { get; init; } = string.Empty;
    public string GrantType { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public decimal MatchingFunds { get; init; }
    public string ProjectDescription { get; init; } = string.Empty;
    public string Objectives { get; init; } = string.Empty;
    public string ExpectedOutcomes { get; init; } = string.Empty;
    public string TargetPopulation { get; init; } = string.Empty;
    public string GeographicScope { get; init; } = string.Empty;
    public string Timeline { get; init; } = string.Empty;
    public string BudgetBreakdown { get; init; } = string.Empty;
    public string EvaluationPlan { get; init; } = string.Empty;
    public string SustainabilityPlan { get; init; } = string.Empty;
    public DateTime ApplicationDeadline { get; init; }
    public DateTime SubmissionDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Decision { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

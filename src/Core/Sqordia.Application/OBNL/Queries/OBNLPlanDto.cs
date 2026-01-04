namespace Sqordia.Application.OBNL.Queries;

public record OBNLPlanDto
{
    public Guid Id { get; init; }
    public Guid OrganizationId { get; init; }
    public string OBNLType { get; init; } = string.Empty;
    public string Mission { get; init; } = string.Empty;
    public string Vision { get; init; } = string.Empty;
    public string Values { get; init; } = string.Empty;
    public decimal FundingRequirements { get; init; }
    public string FundingPurpose { get; init; } = string.Empty;
    public string ComplianceStatus { get; init; } = string.Empty;
    public string LegalStructure { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
    public DateTime RegistrationDate { get; init; }
    public string GoverningBody { get; init; } = string.Empty;
    public string BoardComposition { get; init; } = string.Empty;
    public string StakeholderEngagement { get; init; } = string.Empty;
    public string ImpactMeasurement { get; init; } = string.Empty;
    public string SustainabilityStrategy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string UpdatedBy { get; init; } = string.Empty;
}

using MediatR;

namespace Sqordia.Application.OBNL.Commands;

public record UpdateOBNLPlanCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public string OBNLType { get; init; } = string.Empty;
    public string Mission { get; init; } = string.Empty;
    public string Vision { get; init; } = string.Empty;
    public string Values { get; init; } = string.Empty;
    public decimal FundingRequirements { get; init; }
    public string FundingPurpose { get; init; } = string.Empty;
    public string LegalStructure { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
    public DateTime RegistrationDate { get; init; }
    public string GoverningBody { get; init; } = string.Empty;
    public string BoardComposition { get; init; } = string.Empty;
    public string StakeholderEngagement { get; init; } = string.Empty;
    public string ImpactMeasurement { get; init; } = string.Empty;
    public string SustainabilityStrategy { get; init; } = string.Empty;
    public string UpdatedBy { get; init; } = string.Empty;
}

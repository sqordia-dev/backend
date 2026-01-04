using MediatR;
using Sqordia.Domain.Entities;

namespace Sqordia.Application.OBNL.Commands;

public record CreateOBNLPlanCommand : IRequest<Guid>
{
    public Guid OrganizationId { get; init; }
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
    public string CreatedBy { get; init; } = string.Empty;
}

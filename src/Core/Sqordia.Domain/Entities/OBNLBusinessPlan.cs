using Sqordia.Domain.Common;
using Sqordia.Domain.ValueObjects;

namespace Sqordia.Domain.Entities;

public class OBNLBusinessPlan : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string OBNLType { get; set; } = string.Empty;
    public string Mission { get; set; } = string.Empty;
    public string Vision { get; set; } = string.Empty;
    public string Values { get; set; } = string.Empty;
    public decimal FundingRequirements { get; set; }
    public string FundingPurpose { get; set; } = string.Empty;
    public ComplianceStatus ComplianceStatus { get; set; } = ComplianceStatus.Pending;
    public string LegalStructure { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string GoverningBody { get; set; } = string.Empty;
    public string BoardComposition { get; set; } = string.Empty;
    public string StakeholderEngagement { get; set; } = string.Empty;
    public string ImpactMeasurement { get; set; } = string.Empty;
    public string SustainabilityStrategy { get; set; } = string.Empty;
    public string GrantApplications { get; set; } = string.Empty;
    public string ReportingRequirements { get; set; } = string.Empty;
    public string RiskManagement { get; set; } = string.Empty;
    public string SuccessMetrics { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public List<OBNLCompliance> ComplianceRequirements { get; set; } = new();
    public List<GrantApplication> GrantApplicationsList { get; set; } = new();
    public List<ImpactMeasurement> ImpactMeasurements { get; set; } = new();
}

using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class OBNLCompliance : BaseEntity
{
    public Guid OBNLBusinessPlanId { get; set; }
    public string RequirementType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Jurisdiction { get; set; } = string.Empty;
    public string RegulatoryBody { get; set; } = string.Empty;
    public string ComplianceLevel { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Documentation { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public OBNLBusinessPlan OBNLBusinessPlan { get; set; } = null!;
}

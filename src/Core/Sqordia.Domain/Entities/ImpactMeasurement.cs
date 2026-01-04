using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class ImpactMeasurement : BaseEntity
{
    public Guid OBNLBusinessPlanId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MeasurementType { get; set; } = string.Empty;
    public string UnitOfMeasurement { get; set; } = string.Empty;
    public decimal BaselineValue { get; set; }
    public decimal TargetValue { get; set; }
    public decimal CurrentValue { get; set; }
    public string DataSource { get; set; } = string.Empty;
    public string CollectionMethod { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string ResponsibleParty { get; set; } = string.Empty;
    public DateTime LastMeasurement { get; set; }
    public DateTime NextMeasurement { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public OBNLBusinessPlan OBNLBusinessPlan { get; set; } = null!;
}

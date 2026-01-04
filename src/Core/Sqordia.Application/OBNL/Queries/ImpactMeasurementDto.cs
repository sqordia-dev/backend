namespace Sqordia.Application.OBNL.Queries;

public record ImpactMeasurementDto
{
    public Guid Id { get; init; }
    public Guid OBNLBusinessPlanId { get; init; }
    public string MetricName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string MeasurementType { get; init; } = string.Empty;
    public string UnitOfMeasurement { get; init; } = string.Empty;
    public decimal BaselineValue { get; init; }
    public decimal TargetValue { get; init; }
    public decimal CurrentValue { get; init; }
    public string DataSource { get; init; } = string.Empty;
    public string CollectionMethod { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public string ResponsibleParty { get; init; } = string.Empty;
    public DateTime LastMeasurement { get; init; }
    public DateTime NextMeasurement { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class FinancialKPI : BaseEntity
{
    public Guid BusinessPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Revenue, Profitability, Liquidity, etc.
    public string MetricType { get; set; } = string.Empty; // Ratio, Amount, Percentage, etc.
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty; // $, %, ratio, etc.
    public string CurrencyCode { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TargetValue { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal ChangePercentage { get; set; }
    public string Trend { get; set; } = string.Empty; // Up, Down, Stable
    public string Benchmark { get; set; } = string.Empty; // Industry average, etc.
    public string Status { get; set; } = string.Empty; // Good, Warning, Critical
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Domain.Entities.BusinessPlan.BusinessPlan BusinessPlan { get; set; } = null!;
    public Currency? Currency { get; set; }
}

namespace Sqordia.Application.Financial.Queries;

public class FinancialProjectionDto
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectionType { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public bool IsRecurring { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public decimal GrowthRate { get; set; }
    public string Assumptions { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

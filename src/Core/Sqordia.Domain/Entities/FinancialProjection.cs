using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class FinancialProjectionItem : BaseEntity
{
    public Guid BusinessPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectionType { get; set; } = string.Empty; // Revenue, Expenses, Cash Flow, etc.
    public string Scenario { get; set; } = string.Empty; // Optimistic, Realistic, Pessimistic
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal BaseAmount { get; set; } // Amount in base currency
    public string Category { get; set; } = string.Empty; // Revenue, COGS, Operating, etc.
    public string SubCategory { get; set; } = string.Empty; // Product Sales, Services, etc.
    public bool IsRecurring { get; set; }
    public string Frequency { get; set; } = string.Empty; // Monthly, Quarterly, Annually
    public decimal GrowthRate { get; set; } // Year-over-year growth
    public string Assumptions { get; set; } = string.Empty; // JSON of assumptions
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Domain.Entities.BusinessPlan.BusinessPlan BusinessPlan { get; set; } = null!;
    public Currency? Currency { get; set; }
    public List<TaxCalculation> TaxCalculations { get; set; } = new();
}

namespace Sqordia.Contracts.Responses.Financial;

/// <summary>
/// DTO for business plan financial projection data
/// </summary>
public class FinancialProjectionResponse
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public int? Quarter { get; set; }

    // Revenue
    public decimal? Revenue { get; set; }
    public decimal? RevenueGrowthRate { get; set; }

    // Costs
    public decimal? CostOfGoodsSold { get; set; }
    public decimal? OperatingExpenses { get; set; }
    public decimal? MarketingExpenses { get; set; }
    public decimal? RAndDExpenses { get; set; }
    public decimal? AdministrativeExpenses { get; set; }
    public decimal? OtherExpenses { get; set; }

    // Calculated
    public decimal? GrossProfit { get; set; }
    public decimal? NetIncome { get; set; }
    public decimal? EBITDA { get; set; }

    // Cash flow
    public decimal? CashFlow { get; set; }
    public decimal? CashBalance { get; set; }

    // Metrics
    public int? Employees { get; set; }
    public int? Customers { get; set; }
    public int? UnitsSold { get; set; }
    public decimal? AverageRevenuePerCustomer { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? Assumptions { get; set; }
}

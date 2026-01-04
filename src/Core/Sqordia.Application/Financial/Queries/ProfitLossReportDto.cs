namespace Sqordia.Application.Financial.Queries;

public class ProfitLossReportDto
{
    public Guid BusinessPlanId { get; set; }
    public string Period { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal OperatingExpenses { get; set; }
    public decimal OperatingIncome { get; set; }
    public decimal InterestExpense { get; set; }
    public decimal TaxExpense { get; set; }
    public decimal NetIncome { get; set; }
    public List<ProfitLossItemDto> Items { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class ProfitLossItemDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // Revenue or Expense
    public int Month { get; set; }
    public int Year { get; set; }
}

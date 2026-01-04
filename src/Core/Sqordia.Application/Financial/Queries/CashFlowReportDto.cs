namespace Sqordia.Application.Financial.Queries;

public class CashFlowReportDto
{
    public Guid BusinessPlanId { get; set; }
    public string Period { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal CashInflows { get; set; }
    public decimal CashOutflows { get; set; }
    public decimal NetCashFlow { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<CashFlowItemDto> Items { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class CashFlowItemDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // Inflow or Outflow
    public int Month { get; set; }
    public int Year { get; set; }
}

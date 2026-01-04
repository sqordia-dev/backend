namespace Sqordia.Application.Financial.Queries;

public class BalanceSheetReportDto
{
    public Guid BusinessPlanId { get; set; }
    public string Period { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public List<BalanceSheetItemDto> Assets { get; set; } = new();
    public List<BalanceSheetItemDto> Liabilities { get; set; } = new();
    public List<BalanceSheetItemDto> Equity { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class BalanceSheetItemDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // Asset, Liability, or Equity
    public int Month { get; set; }
    public int Year { get; set; }
}

namespace Sqordia.Application.Financial.Queries;

public class BreakEvenAnalysisDto
{
    public Guid BusinessPlanId { get; set; }
    public decimal FixedCosts { get; set; }
    public decimal VariableCostsPerUnit { get; set; }
    public decimal SellingPricePerUnit { get; set; }
    public decimal BreakEvenUnits { get; set; }
    public decimal BreakEvenRevenue { get; set; }
    public decimal ContributionMargin { get; set; }
    public decimal ContributionMarginRatio { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
    public List<BreakEvenPointDto> Points { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}

public class BreakEvenPointDto
{
    public decimal Units { get; set; }
    public decimal Revenue { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal Profit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}

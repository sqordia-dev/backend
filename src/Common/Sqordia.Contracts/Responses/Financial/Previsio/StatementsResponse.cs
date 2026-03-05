namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class FinancialStatementsResponse
{
    public ProfitLossStatement ProfitLoss { get; set; } = new();
    public CashFlowStatement CashFlow { get; set; } = new();
    public BalanceSheetStatement BalanceSheet { get; set; } = new();
    public FinancialRatiosResponse Ratios { get; set; } = new();
}

public class ProfitLossStatement
{
    public List<StatementLineItem> Revenue { get; set; } = new();
    public List<StatementLineItem> CostOfGoodsSold { get; set; } = new();
    public StatementLineItem GrossProfit { get; set; } = new();
    public List<StatementLineItem> Payroll { get; set; } = new();
    public List<StatementLineItem> SalesExpenses { get; set; } = new();
    public List<StatementLineItem> AdminExpenses { get; set; } = new();
    public StatementLineItem Depreciation { get; set; } = new();
    public StatementLineItem TotalOperatingExpenses { get; set; } = new();
    public StatementLineItem EBIT { get; set; } = new();
    public StatementLineItem InterestExpense { get; set; } = new();
    public StatementLineItem NetIncome { get; set; } = new();
}

public class CashFlowStatement
{
    public List<StatementLineItem> CashInflows { get; set; } = new();
    public List<StatementLineItem> CashOutflows { get; set; } = new();
    public StatementLineItem NetCashFlow { get; set; } = new();
    public StatementLineItem CumulativeCashFlow { get; set; } = new();
}

public class BalanceSheetStatement
{
    public List<StatementLineItem> Assets { get; set; } = new();
    public StatementLineItem TotalAssets { get; set; } = new();
    public List<StatementLineItem> Liabilities { get; set; } = new();
    public StatementLineItem TotalLiabilities { get; set; } = new();
    public List<StatementLineItem> Equity { get; set; } = new();
    public StatementLineItem TotalEquity { get; set; } = new();
    public StatementLineItem TotalLiabilitiesAndEquity { get; set; } = new();
    public bool IsBalanced { get; set; }
}

public class StatementLineItem
{
    public string Label { get; set; } = null!;
    public decimal[] MonthlyValues { get; set; } = new decimal[12];
    public decimal AnnualTotal { get; set; }
    public bool IsHeader { get; set; }
    public bool IsBold { get; set; }
    public int IndentLevel { get; set; }
}

public class FinancialRatiosResponse
{
    public decimal DebtRatio { get; set; }
    public decimal LiquidityRatio { get; set; }
    public decimal GrossMargin { get; set; }
    public decimal NetMargin { get; set; }
    public int? BreakEvenMonth { get; set; }
    public decimal WorkingCapitalRatio { get; set; }
}

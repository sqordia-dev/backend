namespace Sqordia.Application.Financial.Engine;

/// <summary>
/// Internal monthly grid used during calculation. Index 0 = Month 1, Index 11 = Month 12.
/// </summary>
public sealed class MonthlyGrid
{
    public decimal[] Values { get; } = new decimal[12];
    public decimal Total => Values.Sum();

    public decimal this[int monthIndex]
    {
        get => Values[monthIndex];
        set => Values[monthIndex] = value;
    }

    public static MonthlyGrid operator +(MonthlyGrid a, MonthlyGrid b)
    {
        var result = new MonthlyGrid();
        for (int i = 0; i < 12; i++)
            result[i] = a[i] + b[i];
        return result;
    }

    public static MonthlyGrid operator -(MonthlyGrid a, MonthlyGrid b)
    {
        var result = new MonthlyGrid();
        for (int i = 0; i < 12; i++)
            result[i] = a[i] - b[i];
        return result;
    }

    public MonthlyGrid Negate()
    {
        var result = new MonthlyGrid();
        for (int i = 0; i < 12; i++)
            result[i] = -Values[i];
        return result;
    }

    public decimal[] ToArray() => (decimal[])Values.Clone();
}

/// <summary>
/// A named line item with monthly values, used to build statements.
/// </summary>
public sealed record LineResult(string Label, MonthlyGrid Grid, int IndentLevel = 0, bool IsBold = false, bool IsHeader = false);

/// <summary>
/// Full result of the calculation engine for one year.
/// </summary>
public sealed class YearCalculationResult
{
    public int Year { get; init; }

    // Revenue
    public List<LineResult> RevenueLines { get; init; } = [];
    public MonthlyGrid TotalRevenue { get; init; } = new();

    // COGS
    public List<LineResult> COGSLines { get; init; } = [];
    public MonthlyGrid TotalCOGS { get; init; } = new();
    public MonthlyGrid GrossProfit { get; init; } = new();

    // Payroll
    public List<LineResult> PayrollLines { get; init; } = [];
    public MonthlyGrid TotalPayroll { get; init; } = new();

    // Expenses
    public List<LineResult> SalesExpenseLines { get; init; } = [];
    public MonthlyGrid TotalSalesExpenses { get; init; } = new();
    public List<LineResult> AdminExpenseLines { get; init; } = [];
    public MonthlyGrid TotalAdminExpenses { get; init; } = new();

    // Depreciation
    public MonthlyGrid TotalDepreciation { get; init; } = new();

    // P&L
    public MonthlyGrid TotalOperatingExpenses { get; init; } = new();
    public MonthlyGrid EBIT { get; init; } = new();
    public MonthlyGrid InterestExpense { get; init; } = new();
    public MonthlyGrid NetIncome { get; init; } = new();

    // Cash Flow
    public MonthlyGrid CashFromSales { get; init; } = new();
    public MonthlyGrid CashOutCOGS { get; init; } = new();
    public MonthlyGrid CashOutPayroll { get; init; } = new();
    public MonthlyGrid CashOutSalesExpenses { get; init; } = new();
    public MonthlyGrid CashOutAdminExpenses { get; init; } = new();
    public MonthlyGrid CashOutCapex { get; init; } = new();
    public MonthlyGrid CashInFinancing { get; init; } = new();
    public MonthlyGrid CashOutLoanPayments { get; init; } = new();
    public MonthlyGrid CashOutInterest { get; init; } = new();
    public MonthlyGrid NetCashFlow { get; init; } = new();
    public MonthlyGrid CumulativeCash { get; init; } = new();

    // Balance Sheet (end-of-month snapshots)
    public MonthlyGrid Cash { get; init; } = new();
    public MonthlyGrid AccountsReceivable { get; init; } = new();
    public MonthlyGrid Inventory { get; init; } = new();
    public MonthlyGrid FixedAssetsNet { get; init; } = new();
    public MonthlyGrid TotalAssets { get; init; } = new();
    public MonthlyGrid LoansPayable { get; init; } = new();
    public MonthlyGrid TotalLiabilities { get; init; } = new();
    public MonthlyGrid InvestedCapital { get; init; } = new();
    public MonthlyGrid RetainedEarnings { get; init; } = new();
    public MonthlyGrid TotalEquity { get; init; } = new();
}

/// <summary>
/// Full multi-year calculation output.
/// </summary>
public sealed class CalculationOutput
{
    public Dictionary<int, YearCalculationResult> YearResults { get; init; } = new();
    public decimal DebtRatio { get; init; }
    public decimal LiquidityRatio { get; init; }
    public decimal GrossMargin { get; init; }
    public decimal NetMargin { get; init; }
    public int? BreakEvenMonth { get; init; }
    public decimal WorkingCapitalRatio { get; init; }
}

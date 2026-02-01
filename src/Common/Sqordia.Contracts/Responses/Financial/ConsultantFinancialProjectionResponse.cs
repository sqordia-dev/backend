namespace Sqordia.Contracts.Responses.Financial;

public class ConsultantFinancialProjectionResponse
{
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal YearlyExpenses { get; set; }
    public decimal NetIncome { get; set; }
    public ConsultantFinancialBreakdown Breakdown { get; set; } = null!;
}

public class ConsultantFinancialBreakdown
{
    public RevenueBreakdown Revenue { get; set; } = null!;
    public ExpenseBreakdown Expenses { get; set; } = null!;
}

public class RevenueBreakdown
{
    public decimal BillableHours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal UtilizationPercent { get; set; }
}

public class ExpenseBreakdown
{
    public decimal Overhead { get; set; }
    public decimal ClientAcquisition { get; set; }
    public decimal Insurance { get; set; }
    public decimal Software { get; set; }
    public decimal Taxes { get; set; }
    public decimal Office { get; set; }
}

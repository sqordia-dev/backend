namespace Sqordia.Application.Financial.Queries;

public class ConsultantFinancialProjectionDto
{
    public decimal MonthlyRevenue { get; set; }
    public decimal YearlyRevenue { get; set; }
    public decimal MonthlyExpenses { get; set; }
    public decimal YearlyExpenses { get; set; }
    public decimal NetIncome { get; set; }
    public ConsultantFinancialBreakdownDto Breakdown { get; set; } = null!;
}

public class ConsultantFinancialBreakdownDto
{
    public RevenueBreakdownDto Revenue { get; set; } = null!;
    public ExpenseBreakdownDto Expenses { get; set; } = null!;
}

public class RevenueBreakdownDto
{
    public decimal BillableHours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal UtilizationPercent { get; set; }
}

public class ExpenseBreakdownDto
{
    public decimal Overhead { get; set; }
    public decimal ClientAcquisition { get; set; }
    public decimal Insurance { get; set; }
    public decimal Software { get; set; }
    public decimal Taxes { get; set; }
    public decimal Office { get; set; }
}

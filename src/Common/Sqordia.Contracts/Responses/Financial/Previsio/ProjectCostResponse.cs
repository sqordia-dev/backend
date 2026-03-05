namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class ProjectCostResponse
{
    public Guid Id { get; set; }
    public int WorkingCapitalMonthsCOGS { get; set; }
    public int WorkingCapitalMonthsPayroll { get; set; }
    public int WorkingCapitalMonthsSalesExpenses { get; set; }
    public int WorkingCapitalMonthsAdminExpenses { get; set; }
    public int CapexInclusionMonths { get; set; }
    public decimal TotalStartupCosts { get; set; }
    public decimal TotalWorkingCapital { get; set; }
    public decimal TotalCapex { get; set; }
    public decimal TotalProjectCost { get; set; }
    public ProjectCostBreakdownResponse Breakdown { get; set; } = new();
}

public class ProjectCostBreakdownResponse
{
    public decimal WorkingCapitalCOGS { get; set; }
    public decimal WorkingCapitalPayroll { get; set; }
    public decimal WorkingCapitalSalesExpenses { get; set; }
    public decimal WorkingCapitalAdminExpenses { get; set; }
    public List<CapexBreakdownItem> CapexItems { get; set; } = new();
}

public class CapexBreakdownItem
{
    public string Name { get; set; } = null!;
    public decimal Amount { get; set; }
}

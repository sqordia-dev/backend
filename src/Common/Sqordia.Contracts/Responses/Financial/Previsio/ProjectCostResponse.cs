namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class ProjectCostResponse
{
    public Guid Id { get; set; }
    public int WorkingCapitalMonthsCOGS { get; set; }
    public int WorkingCapitalMonthsPayroll { get; set; }
    public int WorkingCapitalMonthsSalesExpenses { get; set; }
    public int WorkingCapitalMonthsAdminExpenses { get; set; }
    public int CapexInclusionMonths { get; set; }

    // Per-category breakdown
    public decimal SalaryAlreadyAcquired { get; set; }
    public decimal SalaryAcquireBefore { get; set; }
    public decimal SalaryAcquireAfter { get; set; }
    public int SalaryDurationMonths { get; set; }

    public decimal SalesExpAlreadyAcquired { get; set; }
    public decimal SalesExpAcquireBefore { get; set; }
    public decimal SalesExpAcquireAfter { get; set; }
    public int SalesExpDurationMonths { get; set; }

    public decimal AdminExpAlreadyAcquired { get; set; }
    public decimal AdminExpAcquireBefore { get; set; }
    public decimal AdminExpAcquireAfter { get; set; }
    public int AdminExpDurationMonths { get; set; }

    public decimal InventoryAlreadyAcquired { get; set; }
    public decimal InventoryAcquireBefore { get; set; }
    public decimal InventoryAcquireAfter { get; set; }
    public int InventoryDurationMonths { get; set; }

    public decimal CapexAlreadyAcquired { get; set; }
    public decimal CapexAcquireBefore { get; set; }
    public decimal CapexAcquireAfter { get; set; }
    public int CapexDurationMonths { get; set; }

    // Computed totals
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

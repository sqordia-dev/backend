namespace Sqordia.Contracts.Requests.Financial.Previsio;

public class UpdateProjectCostSettingsRequest
{
    public int WorkingCapitalMonthsCOGS { get; set; }
    public int WorkingCapitalMonthsPayroll { get; set; }
    public int WorkingCapitalMonthsSalesExpenses { get; set; }
    public int WorkingCapitalMonthsAdminExpenses { get; set; }
    public int CapexInclusionMonths { get; set; }

    // Per-category breakdown
    public decimal SalaryAlreadyAcquired { get; set; }
    public decimal SalaryAcquireBefore { get; set; }
    public decimal SalaryAcquireAfter { get; set; }
    public int SalaryDurationMonths { get; set; } = 3;

    public decimal SalesExpAlreadyAcquired { get; set; }
    public decimal SalesExpAcquireBefore { get; set; }
    public decimal SalesExpAcquireAfter { get; set; }
    public int SalesExpDurationMonths { get; set; } = 3;

    public decimal AdminExpAlreadyAcquired { get; set; }
    public decimal AdminExpAcquireBefore { get; set; }
    public decimal AdminExpAcquireAfter { get; set; }
    public int AdminExpDurationMonths { get; set; } = 3;

    public decimal InventoryAlreadyAcquired { get; set; }
    public decimal InventoryAcquireBefore { get; set; }
    public decimal InventoryAcquireAfter { get; set; }
    public int InventoryDurationMonths { get; set; } = 3;

    public decimal CapexAlreadyAcquired { get; set; }
    public decimal CapexAcquireBefore { get; set; }
    public decimal CapexAcquireAfter { get; set; }
    public int CapexDurationMonths { get; set; } = 3;
}

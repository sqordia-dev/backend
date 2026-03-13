using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// Calculated project cost summary: startup costs + working capital + CAPEX.
/// </summary>
public class ProjectCost : BaseAuditableEntity
{
    public Guid FinancialPlanId { get; private set; }

    // Working capital: months of expenses to cover before break-even
    public int WorkingCapitalMonthsCOGS { get; private set; } = 3;
    public int WorkingCapitalMonthsPayroll { get; private set; } = 3;
    public int WorkingCapitalMonthsSalesExpenses { get; private set; } = 3;
    public int WorkingCapitalMonthsAdminExpenses { get; private set; } = 3;

    // CAPEX inclusion: how many months of planned CAPEX to include
    public int CapexInclusionMonths { get; private set; } = 12;

    // === Per-category breakdown: Acquired / Before opening / After opening / Duration ===

    // Salary startup costs
    public decimal SalaryAlreadyAcquired { get; private set; }
    public decimal SalaryAcquireBefore { get; private set; }
    public decimal SalaryAcquireAfter { get; private set; }
    public int SalaryDurationMonths { get; private set; } = 3;

    // Sales expenses startup costs
    public decimal SalesExpAlreadyAcquired { get; private set; }
    public decimal SalesExpAcquireBefore { get; private set; }
    public decimal SalesExpAcquireAfter { get; private set; }
    public int SalesExpDurationMonths { get; private set; } = 3;

    // Admin expenses startup costs
    public decimal AdminExpAlreadyAcquired { get; private set; }
    public decimal AdminExpAcquireBefore { get; private set; }
    public decimal AdminExpAcquireAfter { get; private set; }
    public int AdminExpDurationMonths { get; private set; } = 3;

    // Inventory startup costs
    public decimal InventoryAlreadyAcquired { get; private set; }
    public decimal InventoryAcquireBefore { get; private set; }
    public decimal InventoryAcquireAfter { get; private set; }
    public int InventoryDurationMonths { get; private set; } = 3;

    // Capital assets (immobilisation) startup costs
    public decimal CapexAlreadyAcquired { get; private set; }
    public decimal CapexAcquireBefore { get; private set; }
    public decimal CapexAcquireAfter { get; private set; }
    public int CapexDurationMonths { get; private set; } = 3;

    // Computed totals (persisted for quick reads, recalculated by engine)
    public decimal TotalStartupCosts { get; private set; }
    public decimal TotalWorkingCapital { get; private set; }
    public decimal TotalCapex { get; private set; }
    public decimal TotalProjectCost { get; private set; }

    // Navigation
    public FinancialPlan FinancialPlan { get; private set; } = null!;

    private ProjectCost() { }

    public ProjectCost(Guid financialPlanId)
    {
        FinancialPlanId = financialPlanId;
    }

    public void UpdateDurationSettings(
        int workingCapitalMonthsCOGS,
        int workingCapitalMonthsPayroll,
        int workingCapitalMonthsSalesExpenses,
        int workingCapitalMonthsAdminExpenses,
        int capexInclusionMonths)
    {
        WorkingCapitalMonthsCOGS = workingCapitalMonthsCOGS;
        WorkingCapitalMonthsPayroll = workingCapitalMonthsPayroll;
        WorkingCapitalMonthsSalesExpenses = workingCapitalMonthsSalesExpenses;
        WorkingCapitalMonthsAdminExpenses = workingCapitalMonthsAdminExpenses;
        CapexInclusionMonths = capexInclusionMonths;
    }

    public void UpdateBreakdown(
        decimal salaryAcquired, decimal salaryBefore, decimal salaryAfter, int salaryDuration,
        decimal salesExpAcquired, decimal salesExpBefore, decimal salesExpAfter, int salesExpDuration,
        decimal adminExpAcquired, decimal adminExpBefore, decimal adminExpAfter, int adminExpDuration,
        decimal inventoryAcquired, decimal inventoryBefore, decimal inventoryAfter, int inventoryDuration,
        decimal capexAcquired, decimal capexBefore, decimal capexAfter, int capexDuration)
    {
        SalaryAlreadyAcquired = salaryAcquired;
        SalaryAcquireBefore = salaryBefore;
        SalaryAcquireAfter = salaryAfter;
        SalaryDurationMonths = salaryDuration;

        SalesExpAlreadyAcquired = salesExpAcquired;
        SalesExpAcquireBefore = salesExpBefore;
        SalesExpAcquireAfter = salesExpAfter;
        SalesExpDurationMonths = salesExpDuration;

        AdminExpAlreadyAcquired = adminExpAcquired;
        AdminExpAcquireBefore = adminExpBefore;
        AdminExpAcquireAfter = adminExpAfter;
        AdminExpDurationMonths = adminExpDuration;

        InventoryAlreadyAcquired = inventoryAcquired;
        InventoryAcquireBefore = inventoryBefore;
        InventoryAcquireAfter = inventoryAfter;
        InventoryDurationMonths = inventoryDuration;

        CapexAlreadyAcquired = capexAcquired;
        CapexAcquireBefore = capexBefore;
        CapexAcquireAfter = capexAfter;
        CapexDurationMonths = capexDuration;
    }

    public void UpdateComputedTotals(
        decimal totalStartupCosts,
        decimal totalWorkingCapital,
        decimal totalCapex,
        decimal totalProjectCost)
    {
        TotalStartupCosts = totalStartupCosts;
        TotalWorkingCapital = totalWorkingCapital;
        TotalCapex = totalCapex;
        TotalProjectCost = totalProjectCost;
    }
}

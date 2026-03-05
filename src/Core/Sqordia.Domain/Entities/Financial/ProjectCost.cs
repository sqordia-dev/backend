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

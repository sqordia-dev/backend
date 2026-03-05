using Sqordia.Domain.Common;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// Fixed administrative overhead expense (rent, insurance, telecom, etc.).
/// </summary>
public class AdminExpenseItem : BaseAuditableEntity
{
    public Guid FinancialPlanId { get; private set; }
    public string Name { get; private set; } = null!;
    public AdminExpenseCategory Category { get; private set; }
    public decimal MonthlyAmount { get; private set; }
    public bool IsTaxable { get; private set; } = true;
    public RecurrenceFrequency Frequency { get; private set; } = RecurrenceFrequency.Monthly;
    public int StartMonth { get; private set; } = 1;
    public int StartYear { get; private set; } = 1;
    public decimal IndexationRate { get; private set; }
    public int SortOrder { get; private set; }

    // Navigation
    public FinancialPlan FinancialPlan { get; private set; } = null!;

    private AdminExpenseItem() { }

    public AdminExpenseItem(
        Guid financialPlanId,
        string name,
        AdminExpenseCategory category,
        decimal monthlyAmount,
        bool isTaxable = true,
        RecurrenceFrequency frequency = RecurrenceFrequency.Monthly,
        int sortOrder = 0)
    {
        FinancialPlanId = financialPlanId;
        Name = name;
        Category = category;
        MonthlyAmount = monthlyAmount;
        IsTaxable = isTaxable;
        Frequency = frequency;
        SortOrder = sortOrder;
    }

    public void Update(
        string name,
        AdminExpenseCategory category,
        decimal monthlyAmount,
        bool isTaxable,
        RecurrenceFrequency frequency,
        int startMonth,
        int startYear,
        decimal indexationRate)
    {
        Name = name;
        Category = category;
        MonthlyAmount = monthlyAmount;
        IsTaxable = isTaxable;
        Frequency = frequency;
        StartMonth = startMonth;
        StartYear = startYear;
        IndexationRate = indexationRate;
    }
}

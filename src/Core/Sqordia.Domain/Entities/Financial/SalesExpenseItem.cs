using Sqordia.Domain.Common;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// Sales-related variable expense (commissions, marketing, trade shows, etc.).
/// </summary>
public class SalesExpenseItem : BaseAuditableEntity
{
    public Guid FinancialPlanId { get; private set; }
    public string Name { get; private set; } = null!;
    public SalesExpenseCategory Category { get; private set; }
    public ExpenseMode ExpenseMode { get; private set; }
    public decimal Amount { get; private set; } // $ or % depending on ExpenseMode
    public RecurrenceFrequency Frequency { get; private set; } = RecurrenceFrequency.Monthly;
    public int StartMonth { get; private set; } = 1;
    public int StartYear { get; private set; } = 1;
    public decimal IndexationRate { get; private set; }
    public int SortOrder { get; private set; }

    // Navigation
    public FinancialPlan FinancialPlan { get; private set; } = null!;

    private SalesExpenseItem() { }

    public SalesExpenseItem(
        Guid financialPlanId,
        string name,
        SalesExpenseCategory category,
        ExpenseMode expenseMode,
        decimal amount,
        RecurrenceFrequency frequency = RecurrenceFrequency.Monthly,
        int sortOrder = 0)
    {
        FinancialPlanId = financialPlanId;
        Name = name;
        Category = category;
        ExpenseMode = expenseMode;
        Amount = amount;
        Frequency = frequency;
        SortOrder = sortOrder;
    }

    public void Update(
        string name,
        SalesExpenseCategory category,
        ExpenseMode expenseMode,
        decimal amount,
        RecurrenceFrequency frequency,
        int startMonth,
        int startYear,
        decimal indexationRate)
    {
        Name = name;
        Category = category;
        ExpenseMode = expenseMode;
        Amount = amount;
        Frequency = frequency;
        StartMonth = startMonth;
        StartYear = startYear;
        IndexationRate = indexationRate;
    }
}

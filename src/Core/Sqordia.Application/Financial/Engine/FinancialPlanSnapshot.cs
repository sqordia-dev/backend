using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Application.Financial.Engine;

/// <summary>
/// Immutable snapshot of all financial plan data needed for calculation.
/// This is the pure input to the calculation engine — no EF, no async.
/// </summary>
public sealed record FinancialPlanSnapshot
{
    public int ProjectionYears { get; init; } = 3;
    public int StartYear { get; init; }

    // Default rates
    public decimal DefaultVolumeGrowthRate { get; init; }
    public decimal DefaultPriceIndexationRate { get; init; }
    public decimal DefaultExpenseIndexationRate { get; init; }
    public decimal DefaultSocialChargeRate { get; init; }
    public decimal DefaultSalesTaxRate { get; init; }

    // Module data
    public IReadOnlyList<ProductSnapshot> Products { get; init; } = [];
    public IReadOnlyList<COGSSnapshot> COGSItems { get; init; } = [];
    public IReadOnlyList<PayrollSnapshot> PayrollItems { get; init; } = [];
    public IReadOnlyList<SalesExpenseSnapshot> SalesExpenses { get; init; } = [];
    public IReadOnlyList<AdminExpenseSnapshot> AdminExpenses { get; init; } = [];
    public IReadOnlyList<CapexSnapshot> CapexAssets { get; init; } = [];
    public IReadOnlyList<FinancingSnapshot> FinancingSources { get; init; } = [];
    public IReadOnlyList<AmortizationSnapshot> AmortizationEntries { get; init; } = [];
    public ProjectCostSnapshot? ProjectCost { get; init; }
}

public sealed record ProductSnapshot
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public decimal UnitPrice { get; init; }
    public PaymentDelay PaymentDelay { get; init; }
    public decimal TaxRate { get; init; }
    public SalesInputMode InputMode { get; init; }
    public decimal VolumeIndexationRate { get; init; }
    public decimal PriceIndexationRate { get; init; }
    public IReadOnlyList<VolumeEntry> Volumes { get; init; } = [];
}

public sealed record VolumeEntry(int Year, int Month, decimal Quantity);

public sealed record COGSSnapshot
{
    public Guid LinkedSalesProductId { get; init; }
    public CostMode CostMode { get; init; }
    public decimal CostValue { get; init; }
    public decimal BeginningInventory { get; init; }
    public decimal CostIndexationRate { get; init; }
}

public sealed record PayrollSnapshot
{
    public string JobTitle { get; init; } = "";
    public PayrollType PayrollType { get; init; }
    public EmploymentStatus EmploymentStatus { get; init; }
    public SalaryFrequency SalaryFrequency { get; init; }
    public decimal SalaryAmount { get; init; }
    public decimal SocialChargeRate { get; init; }
    public int HeadCount { get; init; } = 1;
    public int StartMonth { get; init; } = 1;
    public int StartYear { get; init; } = 1;
    public decimal SalaryIndexationRate { get; init; }

    public decimal GetMonthlySalary() => SalaryFrequency switch
    {
        SalaryFrequency.Hourly => SalaryAmount * 40 * 52 / 12,
        SalaryFrequency.Monthly => SalaryAmount,
        SalaryFrequency.Annual => SalaryAmount / 12,
        _ => SalaryAmount
    };
}

public sealed record SalesExpenseSnapshot
{
    public string Name { get; init; } = "";
    public SalesExpenseCategory Category { get; init; }
    public ExpenseMode ExpenseMode { get; init; }
    public decimal Amount { get; init; }
    public RecurrenceFrequency Frequency { get; init; }
    public int StartMonth { get; init; } = 1;
    public int StartYear { get; init; } = 1;
    public decimal IndexationRate { get; init; }
}

public sealed record AdminExpenseSnapshot
{
    public string Name { get; init; } = "";
    public AdminExpenseCategory Category { get; init; }
    public decimal MonthlyAmount { get; init; }
    public bool IsTaxable { get; init; }
    public RecurrenceFrequency Frequency { get; init; }
    public int StartMonth { get; init; } = 1;
    public int StartYear { get; init; } = 1;
    public decimal IndexationRate { get; init; }
}

public sealed record CapexSnapshot
{
    public string Name { get; init; } = "";
    public AssetType AssetType { get; init; }
    public decimal PurchaseValue { get; init; }
    public int PurchaseMonth { get; init; }
    public int PurchaseYear { get; init; }
    public DepreciationMethod DepreciationMethod { get; init; }
    public int UsefulLifeYears { get; init; }
    public decimal SalvageValue { get; init; }
}

public sealed record FinancingSnapshot
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public FinancingType FinancingType { get; init; }
    public decimal Amount { get; init; }
    public decimal InterestRate { get; init; }
    public int TermMonths { get; init; }
    public int MoratoireMonths { get; init; }
    public int DisbursementMonth { get; init; } = 1;
    public int DisbursementYear { get; init; } = 1;

    public bool RequiresRepayment =>
        FinancingType is FinancingType.BankLoan or FinancingType.LineOfCredit;
}

public sealed record AmortizationSnapshot
{
    public Guid FinancingSourceId { get; init; }
    public int PaymentNumber { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal PaymentAmount { get; init; }
    public decimal PrincipalPortion { get; init; }
    public decimal InterestPortion { get; init; }
    public decimal RemainingBalance { get; init; }
    public bool IsMoratoire { get; init; }
}

public sealed record ProjectCostSnapshot
{
    public int WorkingCapitalMonthsCOGS { get; init; } = 3;
    public int WorkingCapitalMonthsPayroll { get; init; } = 3;
    public int WorkingCapitalMonthsSalesExpenses { get; init; } = 3;
    public int WorkingCapitalMonthsAdminExpenses { get; init; } = 3;
    public int CapexInclusionMonths { get; init; } = 12;
}

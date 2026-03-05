using Sqordia.Domain.Common;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// A funding source (loan, investment, subsidy, etc.) with repayment terms.
/// </summary>
public class FinancingSource : BaseAuditableEntity
{
    public Guid FinancialPlanId { get; private set; }
    public string Name { get; private set; } = null!;
    public FinancingType FinancingType { get; private set; }
    public decimal Amount { get; private set; }
    public decimal InterestRate { get; private set; } // Annual rate as percentage
    public int TermMonths { get; private set; }
    public int MoratoireMonths { get; private set; } // Grace period (interest only)
    public int DisbursementMonth { get; private set; } = 1; // Month funds are received
    public int DisbursementYear { get; private set; } = 1;  // Projection year
    public int SortOrder { get; private set; }

    // Navigation
    public FinancialPlan FinancialPlan { get; private set; } = null!;
    public ICollection<AmortizationEntry> AmortizationEntries { get; private set; } = new List<AmortizationEntry>();

    private FinancingSource() { }

    public FinancingSource(
        Guid financialPlanId,
        string name,
        FinancingType financingType,
        decimal amount,
        decimal interestRate = 0,
        int termMonths = 0,
        int moratoireMonths = 0,
        int sortOrder = 0)
    {
        FinancialPlanId = financialPlanId;
        Name = name;
        FinancingType = financingType;
        Amount = amount;
        InterestRate = interestRate;
        TermMonths = termMonths;
        MoratoireMonths = moratoireMonths;
        SortOrder = sortOrder;
    }

    public void Update(
        string name,
        FinancingType financingType,
        decimal amount,
        decimal interestRate,
        int termMonths,
        int moratoireMonths,
        int disbursementMonth,
        int disbursementYear)
    {
        Name = name;
        FinancingType = financingType;
        Amount = amount;
        InterestRate = interestRate;
        TermMonths = termMonths;
        MoratoireMonths = moratoireMonths;
        DisbursementMonth = disbursementMonth;
        DisbursementYear = disbursementYear;
    }

    /// <summary>
    /// Returns true if this financing type requires repayment (loan vs equity/grant).
    /// </summary>
    public bool RequiresRepayment()
    {
        return FinancingType is FinancingType.BankLoan or FinancingType.LineOfCredit;
    }
}

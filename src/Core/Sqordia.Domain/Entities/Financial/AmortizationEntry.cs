using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// A single payment in a loan amortization schedule.
/// </summary>
public class AmortizationEntry : BaseEntity
{
    public Guid FinancingSourceId { get; private set; }
    public int PaymentNumber { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public decimal PaymentAmount { get; private set; }
    public decimal PrincipalPortion { get; private set; }
    public decimal InterestPortion { get; private set; }
    public decimal RemainingBalance { get; private set; }
    public bool IsMoratoire { get; private set; } // True during grace period

    // Navigation
    public FinancingSource FinancingSource { get; private set; } = null!;

    private AmortizationEntry() { }

    public AmortizationEntry(
        Guid financingSourceId,
        int paymentNumber,
        int year,
        int month,
        decimal paymentAmount,
        decimal principalPortion,
        decimal interestPortion,
        decimal remainingBalance,
        bool isMoratoire = false)
    {
        FinancingSourceId = financingSourceId;
        PaymentNumber = paymentNumber;
        Year = year;
        Month = month;
        PaymentAmount = paymentAmount;
        PrincipalPortion = principalPortion;
        InterestPortion = interestPortion;
        RemainingBalance = remainingBalance;
        IsMoratoire = isMoratoire;
    }
}

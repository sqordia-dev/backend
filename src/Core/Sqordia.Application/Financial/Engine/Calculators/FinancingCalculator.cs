namespace Sqordia.Application.Financial.Engine.Calculators;

/// <summary>
/// Calculates financing-related cash flows: disbursements, loan payments, interest.
/// Uses pre-computed amortization entries from the database.
/// </summary>
public static class FinancingCalculator
{
    public sealed record FinancingResult(
        MonthlyGrid CashInflows,
        MonthlyGrid PrincipalPayments,
        MonthlyGrid InterestPayments,
        MonthlyGrid TotalLoanPayments,
        /// <summary>Outstanding loan balance at end of each month.</summary>
        MonthlyGrid LoanBalance,
        /// <summary>Total invested capital (equity, grants, etc.) at end of each month.</summary>
        MonthlyGrid InvestedCapital);

    public static FinancingResult Calculate(FinancialPlanSnapshot plan, int year, decimal priorYearLoanBalance, decimal priorYearEquity)
    {
        var cashIn = new MonthlyGrid();
        var principal = new MonthlyGrid();
        var interest = new MonthlyGrid();
        var totalPayments = new MonthlyGrid();
        var loanBalance = new MonthlyGrid();
        var investedCapital = new MonthlyGrid();

        decimal runningLoanBalance = priorYearLoanBalance;
        decimal runningEquity = priorYearEquity;

        // Process disbursements
        foreach (var source in plan.FinancingSources)
        {
            if (source.DisbursementYear == year)
            {
                var monthIdx = Math.Max(0, source.DisbursementMonth - 1);
                cashIn[monthIdx] += source.Amount;

                if (source.RequiresRepayment)
                {
                    runningLoanBalance += source.Amount;
                }
                else
                {
                    runningEquity += source.Amount;
                }
            }
            else if (source.DisbursementYear < year)
            {
                // Already disbursed in prior year — balance already tracked
            }
        }

        // Process amortization entries for this year
        var yearEntries = plan.AmortizationEntries
            .Where(e => e.Year == year)
            .GroupBy(e => e.Month)
            .ToDictionary(g => g.Key, g => g.ToList());

        for (int m = 0; m < 12; m++)
        {
            var month = m + 1;

            // Add disbursements to running balance
            if (m == 0)
            {
                // Apply initial balance adjustments for this month's disbursements
            }

            if (yearEntries.TryGetValue(month, out var entries))
            {
                foreach (var entry in entries)
                {
                    principal[m] += entry.PrincipalPortion;
                    interest[m] += entry.InterestPortion;
                    totalPayments[m] += entry.PaymentAmount;
                    runningLoanBalance -= entry.PrincipalPortion;
                }
            }

            // Factor in this month's new disbursements
            foreach (var source in plan.FinancingSources)
            {
                if (source.DisbursementYear == year && source.DisbursementMonth == month)
                {
                    if (source.RequiresRepayment)
                    {
                        // Already added above, but handle the timing
                    }
                    else
                    {
                        // Equity disbursement
                    }
                }
            }

            loanBalance[m] = Math.Max(0, Math.Round(runningLoanBalance, 2));
            investedCapital[m] = Math.Round(runningEquity, 2);
        }

        return new FinancingResult(cashIn, principal, interest, totalPayments, loanBalance, investedCapital);
    }

    /// <summary>
    /// Calculate total initial financing (for year 1, month 0 / pre-opening).
    /// </summary>
    public static (decimal totalLoans, decimal totalEquity) GetInitialFinancing(FinancialPlanSnapshot plan)
    {
        decimal loans = 0;
        decimal equity = 0;

        foreach (var source in plan.FinancingSources)
        {
            if (source.DisbursementYear <= 1 && source.DisbursementMonth <= 1)
            {
                if (source.RequiresRepayment)
                    loans += source.Amount;
                else
                    equity += source.Amount;
            }
        }

        return (loans, equity);
    }
}

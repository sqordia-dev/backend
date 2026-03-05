using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Application.Financial.Engine.Calculators;

/// <summary>
/// Calculates sales expenses and admin expenses per month.
/// Sales expenses can be % of revenue or fixed dollar.
/// Admin expenses are fixed monthly amounts with optional frequency expansion.
/// </summary>
public static class ExpenseCalculator
{
    public sealed record ExpenseResult(
        List<LineResult> Lines,
        MonthlyGrid Total);

    public static ExpenseResult CalculateSalesExpenses(
        FinancialPlanSnapshot plan,
        int year,
        MonthlyGrid totalRevenue)
    {
        var lines = new List<LineResult>();
        var total = new MonthlyGrid();

        foreach (var expense in plan.SalesExpenses)
        {
            var grid = new MonthlyGrid();

            // Apply indexation
            var indexFactor = (decimal)Math.Pow(1 + (double)(expense.IndexationRate / 100), year - 1);

            for (int m = 0; m < 12; m++)
            {
                var month = m + 1;

                // Check start date
                if (year < expense.StartYear || (year == expense.StartYear && month < expense.StartMonth))
                    continue;

                // Check frequency
                if (!IsActiveMonth(expense.Frequency, month))
                    continue;

                decimal amount;
                if (expense.ExpenseMode == ExpenseMode.PercentageOfSales)
                {
                    amount = totalRevenue[m] * (expense.Amount / 100) * indexFactor;
                }
                else
                {
                    amount = expense.Amount * indexFactor;
                }

                grid[m] = Math.Round(amount, 2);
                total[m] += Math.Round(amount, 2);
            }

            lines.Add(new LineResult(expense.Name, grid, IndentLevel: 1));
        }

        return new ExpenseResult(lines, total);
    }

    public static ExpenseResult CalculateAdminExpenses(FinancialPlanSnapshot plan, int year)
    {
        var lines = new List<LineResult>();
        var total = new MonthlyGrid();

        foreach (var expense in plan.AdminExpenses)
        {
            var grid = new MonthlyGrid();

            // Apply indexation
            var indexFactor = (decimal)Math.Pow(1 + (double)(expense.IndexationRate / 100), year - 1);

            for (int m = 0; m < 12; m++)
            {
                var month = m + 1;

                // Check start date
                if (year < expense.StartYear || (year == expense.StartYear && month < expense.StartMonth))
                    continue;

                // Check frequency
                if (!IsActiveMonth(expense.Frequency, month))
                    continue;

                var amount = expense.MonthlyAmount * indexFactor;
                grid[m] = Math.Round(amount, 2);
                total[m] += Math.Round(amount, 2);
            }

            lines.Add(new LineResult(expense.Name, grid, IndentLevel: 1));
        }

        return new ExpenseResult(lines, total);
    }

    private static bool IsActiveMonth(RecurrenceFrequency frequency, int month) => frequency switch
    {
        RecurrenceFrequency.Monthly => true,
        RecurrenceFrequency.Quarterly => month % 3 == 1, // Jan, Apr, Jul, Oct
        RecurrenceFrequency.SemiAnnual => month == 1 || month == 7,
        RecurrenceFrequency.Annual => month == 1,
        RecurrenceFrequency.OneTime => month == 1, // Only first month of first applicable year
        _ => true
    };
}

namespace Sqordia.Application.Financial.Engine.Calculators;

/// <summary>
/// Calculates payroll costs (salary + social charges) per employee per month.
/// Respects start dates and salary indexation.
/// </summary>
public static class PayrollCalculator
{
    public sealed record PayrollResult(
        List<LineResult> Lines,
        MonthlyGrid TotalPayroll,
        MonthlyGrid TotalSalariesOnly,
        MonthlyGrid TotalSocialCharges);

    public static PayrollResult Calculate(FinancialPlanSnapshot plan, int year)
    {
        var lines = new List<LineResult>();
        var totalPayroll = new MonthlyGrid();
        var totalSalaries = new MonthlyGrid();
        var totalCharges = new MonthlyGrid();

        foreach (var item in plan.PayrollItems)
        {
            var grid = new MonthlyGrid();

            // Apply salary indexation: Year N = base * (1 + rate/100)^(N-1)
            var monthlySalary = item.GetMonthlySalary() *
                (decimal)Math.Pow(1 + (double)(item.SalaryIndexationRate / 100), year - 1);

            var monthlyTotal = monthlySalary * item.HeadCount * (1 + item.SocialChargeRate / 100);
            var monthlySalaryTotal = monthlySalary * item.HeadCount;
            var monthlyChargesTotal = monthlySalaryTotal * (item.SocialChargeRate / 100);

            for (int m = 0; m < 12; m++)
            {
                var month = m + 1;
                // Check if employee has started
                if (year < item.StartYear || (year == item.StartYear && month < item.StartMonth))
                    continue;

                grid[m] = Math.Round(monthlyTotal, 2);
                totalPayroll[m] += Math.Round(monthlyTotal, 2);
                totalSalaries[m] += Math.Round(monthlySalaryTotal, 2);
                totalCharges[m] += Math.Round(monthlyChargesTotal, 2);
            }

            lines.Add(new LineResult($"{item.JobTitle} (x{item.HeadCount})", grid, IndentLevel: 1));
        }

        return new PayrollResult(lines, totalPayroll, totalSalaries, totalCharges);
    }
}

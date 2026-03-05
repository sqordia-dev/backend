namespace Sqordia.Application.Financial.Engine.Calculators;

/// <summary>
/// Calculates financial ratios from multi-year results.
/// </summary>
public static class RatioCalculator
{
    public sealed record RatioResult(
        decimal DebtRatio,
        decimal LiquidityRatio,
        decimal GrossMargin,
        decimal NetMargin,
        int? BreakEvenMonth,
        decimal WorkingCapitalRatio);

    public static RatioResult Calculate(Dictionary<int, YearCalculationResult> yearResults)
    {
        if (yearResults.Count == 0)
            return new RatioResult(0, 0, 0, 0, null, 0);

        // Use last year's final month for balance sheet ratios
        var lastYear = yearResults.Keys.Max();
        var lastResult = yearResults[lastYear];

        var totalAssets = lastResult.TotalAssets[11];
        var totalLiabilities = lastResult.TotalLiabilities[11];
        var totalEquity = lastResult.TotalEquity[11];
        var cash = lastResult.Cash[11];
        var currentAssets = cash + lastResult.AccountsReceivable[11] + lastResult.Inventory[11];

        // Debt Ratio = Total Liabilities / Total Assets (target: < 1.0)
        var debtRatio = totalAssets > 0 ? totalLiabilities / totalAssets : 0;

        // Liquidity Ratio = Current Assets / Current Liabilities (target: 1.7-2.0)
        // For simplicity, use total liabilities as proxy for current liabilities
        var liquidityRatio = totalLiabilities > 0 ? currentAssets / totalLiabilities : 0;

        // Use Year 1 for margin calculations
        var year1 = yearResults.GetValueOrDefault(1);
        decimal grossMargin = 0;
        decimal netMargin = 0;

        if (year1 != null)
        {
            var totalRevenue = year1.TotalRevenue.Total;
            if (totalRevenue > 0)
            {
                grossMargin = year1.GrossProfit.Total / totalRevenue;
                netMargin = year1.NetIncome.Total / totalRevenue;
            }
        }

        // Break-even month: first month where cumulative cash flow > 0
        int? breakEvenMonth = null;
        int cumulativeMonthIndex = 0;
        foreach (var yr in yearResults.OrderBy(kv => kv.Key))
        {
            for (int m = 0; m < 12; m++)
            {
                cumulativeMonthIndex++;
                if (yr.Value.CumulativeCash[m] > 0 && breakEvenMonth == null)
                {
                    breakEvenMonth = cumulativeMonthIndex;
                }
            }
        }

        // Working Capital Ratio = Current Assets / Current Liabilities
        var workingCapitalRatio = totalLiabilities > 0 ? currentAssets / totalLiabilities : currentAssets;

        return new RatioResult(
            Math.Round(debtRatio, 4),
            Math.Round(liquidityRatio, 4),
            Math.Round(grossMargin, 4),
            Math.Round(netMargin, 4),
            breakEvenMonth,
            Math.Round(workingCapitalRatio, 4));
    }
}

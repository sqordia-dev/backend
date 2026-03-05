using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Application.Financial.Engine.Calculators;

/// <summary>
/// Calculates sales revenue per product per month, applying indexation.
/// Also computes cash collection timing based on payment delays.
/// </summary>
public static class SalesRevenueCalculator
{
    public sealed record RevenueResult(
        List<LineResult> Lines,
        MonthlyGrid TotalRevenue,
        MonthlyGrid CashCollected,
        MonthlyGrid AccountsReceivableChange,
        /// <summary>Per-product revenue grids, keyed by product ID.</summary>
        Dictionary<Guid, MonthlyGrid> ProductRevenue);

    public static RevenueResult Calculate(FinancialPlanSnapshot plan, int year)
    {
        var lines = new List<LineResult>();
        var totalRevenue = new MonthlyGrid();
        var cashCollected = new MonthlyGrid();
        var productRevenue = new Dictionary<Guid, MonthlyGrid>();

        // Track revenue that will be collected with delay (spills into future months/years)
        // For now we handle within-year delays; cross-year delays add to AR
        var totalAR = new MonthlyGrid();

        foreach (var product in plan.Products)
        {
            var grid = new MonthlyGrid();
            var productCash = new MonthlyGrid();

            // Apply price indexation: Year 1 = base, Year N = base * (1 + rate/100)^(N-1)
            var indexedPrice = product.UnitPrice *
                (decimal)Math.Pow(1 + (double)(product.PriceIndexationRate / 100), year - 1);

            // Get volumes for this year
            var yearVolumes = product.Volumes
                .Where(v => v.Year == year)
                .ToDictionary(v => v.Month, v => v.Quantity);

            for (int m = 0; m < 12; m++)
            {
                var month = m + 1; // 1-based month
                var quantity = yearVolumes.GetValueOrDefault(month, 0);

                // Revenue = quantity * price (for Quantity mode) or just the dollar amount
                var revenue = product.InputMode == SalesInputMode.Quantity
                    ? quantity * indexedPrice
                    : quantity; // In Dollars mode, quantity IS the dollar amount

                grid[m] = Math.Round(revenue, 2);

                // Cash collection: shift by payment delay months
                var delayMonths = (int)product.PaymentDelay;
                var collectionMonth = m + delayMonths;

                if (collectionMonth < 12)
                {
                    productCash[collectionMonth] += Math.Round(revenue, 2);
                }
                // else: spills into next year → becomes AR at year-end
            }

            productRevenue[product.Id] = grid;

            for (int m = 0; m < 12; m++)
            {
                totalRevenue[m] += grid[m];
                cashCollected[m] += productCash[m];
            }

            lines.Add(new LineResult(product.Name, grid, IndentLevel: 1));
        }

        // AR change = revenue earned but not yet collected
        var arChange = new MonthlyGrid();
        for (int m = 0; m < 12; m++)
        {
            // Cumulative revenue minus cumulative cash = AR balance
            arChange[m] = totalRevenue[m] - cashCollected[m];
        }

        return new RevenueResult(lines, totalRevenue, cashCollected, arChange, productRevenue);
    }

    /// <summary>
    /// Calculate uncollected revenue from previous year that arrives this year.
    /// </summary>
    public static MonthlyGrid CalculateCarryoverCash(FinancialPlanSnapshot plan, int currentYear)
    {
        if (currentYear <= 1) return new MonthlyGrid();

        var carryover = new MonthlyGrid();
        var prevYear = currentYear - 1;

        foreach (var product in plan.Products)
        {
            var indexedPrice = product.UnitPrice *
                (decimal)Math.Pow(1 + (double)(product.PriceIndexationRate / 100), prevYear - 1);

            var yearVolumes = product.Volumes
                .Where(v => v.Year == prevYear)
                .ToDictionary(v => v.Month, v => v.Quantity);

            var delayMonths = (int)product.PaymentDelay;
            if (delayMonths == 0) continue;

            for (int m = 0; m < 12; m++)
            {
                var quantity = yearVolumes.GetValueOrDefault(m + 1, 0);
                var revenue = product.InputMode == SalesInputMode.Quantity
                    ? quantity * indexedPrice
                    : quantity;

                var collectionMonth = m + delayMonths;
                if (collectionMonth >= 12)
                {
                    var spillMonth = collectionMonth - 12;
                    if (spillMonth < 12)
                    {
                        carryover[spillMonth] += Math.Round(revenue, 2);
                    }
                }
            }
        }

        return carryover;
    }
}

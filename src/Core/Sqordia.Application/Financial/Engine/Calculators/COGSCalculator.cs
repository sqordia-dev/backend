using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Application.Financial.Engine.Calculators;

/// <summary>
/// Calculates Cost of Goods Sold per product per month.
/// Inventory: Beginning + Purchases - COGS = Ending.
/// </summary>
public static class COGSCalculator
{
    public sealed record COGSResult(
        List<LineResult> Lines,
        MonthlyGrid TotalCOGS,
        MonthlyGrid EndingInventory);

    public static COGSResult Calculate(
        FinancialPlanSnapshot plan,
        int year,
        Dictionary<Guid, MonthlyGrid> productRevenue)
    {
        var lines = new List<LineResult>();
        var totalCOGS = new MonthlyGrid();
        var totalInventory = new MonthlyGrid();

        foreach (var cogs in plan.COGSItems)
        {
            var product = plan.Products.FirstOrDefault(p => p.Id == cogs.LinkedSalesProductId);
            if (product == null) continue;

            if (!productRevenue.TryGetValue(product.Id, out var revenueGrid))
                continue;

            var grid = new MonthlyGrid();
            var indexedPrice = product.UnitPrice *
                (decimal)Math.Pow(1 + (double)(product.PriceIndexationRate / 100), year - 1);

            // Apply cost indexation
            var costIndexFactor = (decimal)Math.Pow(1 + (double)(cogs.CostIndexationRate / 100), year - 1);

            var yearVolumes = product.Volumes
                .Where(v => v.Year == year)
                .ToDictionary(v => v.Month, v => v.Quantity);

            decimal runningInventory = year == 1 ? cogs.BeginningInventory : 0;

            for (int m = 0; m < 12; m++)
            {
                var quantity = yearVolumes.GetValueOrDefault(m + 1, 0);

                decimal costPerUnit;
                if (cogs.CostMode == CostMode.FixedDollars)
                {
                    costPerUnit = cogs.CostValue * costIndexFactor;
                }
                else // PercentageOfPrice
                {
                    costPerUnit = indexedPrice * (cogs.CostValue / 100);
                }

                var monthCOGS = Math.Round(quantity * costPerUnit, 2);
                grid[m] = monthCOGS;
                totalCOGS[m] += monthCOGS;

                // Simple inventory tracking: purchases = COGS (just-in-time assumption)
                // Ending Inventory = Beginning + Purchases - COGS
                // With JIT: Purchases ≈ COGS, so inventory stays roughly constant
                runningInventory = Math.Max(0, runningInventory);
                totalInventory[m] = runningInventory;
            }

            var label = product.Name;
            lines.Add(new LineResult(label, grid, IndentLevel: 1));
        }

        return new COGSResult(lines, totalCOGS, totalInventory);
    }
}

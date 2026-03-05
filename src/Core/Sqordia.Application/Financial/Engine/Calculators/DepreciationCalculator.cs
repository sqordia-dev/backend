using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Application.Financial.Engine.Calculators;

/// <summary>
/// Calculates monthly depreciation for all CAPEX assets.
/// Supports straight-line and declining balance methods.
/// </summary>
public static class DepreciationCalculator
{
    public sealed record DepreciationResult(
        MonthlyGrid TotalDepreciation,
        MonthlyGrid CapexPurchases,
        /// <summary>Net book value of all fixed assets at end of each month.</summary>
        MonthlyGrid NetFixedAssets);

    public static DepreciationResult Calculate(FinancialPlanSnapshot plan, int year, decimal priorYearNetAssets)
    {
        var totalDepr = new MonthlyGrid();
        var purchases = new MonthlyGrid();
        var netAssets = new MonthlyGrid();

        decimal cumulativeDeprThisYear = 0;
        decimal cumulativePurchases = 0;

        // Calculate total asset base from all prior years + this year's purchases
        decimal totalAssetCost = 0;
        decimal totalPriorDepreciation = 0;

        foreach (var asset in plan.CapexAssets)
        {
            // Determine if this asset was purchased before or during this year
            var purchaseYearNum = asset.PurchaseYear; // 0=pre-opening, 1-3
            if (purchaseYearNum == 0) purchaseYearNum = 1; // pre-opening counted in year 1

            if (purchaseYearNum > year) continue; // Not yet purchased

            totalAssetCost += asset.PurchaseValue;

            // This year's purchases
            if (purchaseYearNum == year)
            {
                var purchaseMonthIdx = Math.Max(0, asset.PurchaseMonth - 1);
                purchases[purchaseMonthIdx] += asset.PurchaseValue;
            }

            // Calculate depreciation for this asset this year
            var monthlyDepr = CalculateAssetDepreciation(asset, year);
            for (int m = 0; m < 12; m++)
            {
                totalDepr[m] += monthlyDepr[m];
            }
        }

        // Build net fixed assets month by month
        decimal runningNet = priorYearNetAssets;
        for (int m = 0; m < 12; m++)
        {
            runningNet += purchases[m] - totalDepr[m];
            netAssets[m] = Math.Max(0, Math.Round(runningNet, 2));
        }

        return new DepreciationResult(totalDepr, purchases, netAssets);
    }

    private static MonthlyGrid CalculateAssetDepreciation(CapexSnapshot asset, int year)
    {
        var grid = new MonthlyGrid();
        var purchaseYear = asset.PurchaseYear == 0 ? 1 : asset.PurchaseYear;

        if (purchaseYear > year) return grid; // Not yet purchased

        var depreciableAmount = asset.PurchaseValue - asset.SalvageValue;
        if (depreciableAmount <= 0 || asset.UsefulLifeYears <= 0) return grid;

        if (asset.DepreciationMethod == DepreciationMethod.StraightLine)
        {
            var annualDepr = depreciableAmount / asset.UsefulLifeYears;
            var monthlyDepr = Math.Round(annualDepr / 12, 2);

            // Check if asset is still within useful life
            var yearsOwned = year - purchaseYear;
            if (yearsOwned >= asset.UsefulLifeYears) return grid; // Fully depreciated

            for (int m = 0; m < 12; m++)
            {
                // In purchase year, only depreciate from purchase month onward
                if (year == purchaseYear && m < (asset.PurchaseMonth == 0 ? 0 : asset.PurchaseMonth - 1))
                    continue;

                grid[m] = monthlyDepr;
            }
        }
        else // DecliningBalance
        {
            // Declining balance: rate = 1 / useful life * 2 (double declining)
            var annualRate = 2.0m / asset.UsefulLifeYears;
            var bookValueStart = depreciableAmount;

            // Reduce book value by prior years' depreciation
            for (int y = purchaseYear; y < year; y++)
            {
                var yearDepr = bookValueStart * annualRate;
                bookValueStart -= yearDepr;
                if (bookValueStart <= asset.SalvageValue)
                {
                    bookValueStart = asset.SalvageValue;
                    break;
                }
            }

            if (bookValueStart <= asset.SalvageValue) return grid;

            var thisYearDepr = bookValueStart * annualRate;
            // Don't depreciate below salvage
            if (bookValueStart - thisYearDepr < asset.SalvageValue)
                thisYearDepr = bookValueStart - asset.SalvageValue;

            var monthlyDepr = Math.Round(thisYearDepr / 12, 2);

            for (int m = 0; m < 12; m++)
            {
                if (year == purchaseYear && m < (asset.PurchaseMonth == 0 ? 0 : asset.PurchaseMonth - 1))
                    continue;

                grid[m] = monthlyDepr;
            }
        }

        return grid;
    }
}

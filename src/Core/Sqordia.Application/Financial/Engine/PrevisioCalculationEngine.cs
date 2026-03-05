using Sqordia.Application.Financial.Engine.Calculators;

namespace Sqordia.Application.Financial.Engine;

/// <summary>
/// Pure calculation engine for financial projections.
/// No I/O, no async, no side effects. Takes a snapshot, returns statements.
/// </summary>
public static class PrevisioCalculationEngine
{
    public static CalculationOutput Calculate(FinancialPlanSnapshot plan)
    {
        var yearResults = new Dictionary<int, YearCalculationResult>();

        // Track cross-year state
        decimal priorYearNetAssets = 0;
        decimal priorYearLoanBalance = 0;
        decimal priorYearEquity = 0;
        decimal priorYearCash = 0;
        decimal priorYearRetainedEarnings = 0;
        decimal priorYearAR = 0;
        decimal priorYearInventory = 0;

        // Get initial financing
        var (initialLoans, initialEquity) = FinancingCalculator.GetInitialFinancing(plan);
        priorYearLoanBalance = 0; // Will be set by year-1 calculations
        priorYearEquity = 0;

        for (int year = 1; year <= plan.ProjectionYears; year++)
        {
            var result = CalculateYear(plan, year,
                priorYearNetAssets, priorYearLoanBalance, priorYearEquity,
                priorYearCash, priorYearRetainedEarnings, priorYearAR, priorYearInventory);

            yearResults[year] = result;

            // Carry forward end-of-year values
            priorYearNetAssets = result.FixedAssetsNet[11];
            priorYearLoanBalance = result.LoansPayable[11];
            priorYearEquity = result.InvestedCapital[11];
            priorYearCash = result.Cash[11];
            priorYearRetainedEarnings = result.RetainedEarnings[11];
            priorYearAR = result.AccountsReceivable[11];
            priorYearInventory = result.Inventory[11];
        }

        // Calculate ratios
        var ratios = RatioCalculator.Calculate(yearResults);

        return new CalculationOutput
        {
            YearResults = yearResults,
            DebtRatio = ratios.DebtRatio,
            LiquidityRatio = ratios.LiquidityRatio,
            GrossMargin = ratios.GrossMargin,
            NetMargin = ratios.NetMargin,
            BreakEvenMonth = ratios.BreakEvenMonth,
            WorkingCapitalRatio = ratios.WorkingCapitalRatio
        };
    }

    private static YearCalculationResult CalculateYear(
        FinancialPlanSnapshot plan, int year,
        decimal priorNetAssets, decimal priorLoanBalance, decimal priorEquity,
        decimal priorCash, decimal priorRetainedEarnings, decimal priorAR, decimal priorInventory)
    {
        // 1. Revenue
        var revenue = SalesRevenueCalculator.Calculate(plan, year);
        var carryoverCash = SalesRevenueCalculator.CalculateCarryoverCash(plan, year);

        // 2. COGS
        var cogs = COGSCalculator.Calculate(plan, year, revenue.ProductRevenue);

        // 3. Gross Profit
        var grossProfit = revenue.TotalRevenue - cogs.TotalCOGS;

        // 4. Payroll
        var payroll = PayrollCalculator.Calculate(plan, year);

        // 5. Sales Expenses
        var salesExpenses = ExpenseCalculator.CalculateSalesExpenses(plan, year, revenue.TotalRevenue);

        // 6. Admin Expenses
        var adminExpenses = ExpenseCalculator.CalculateAdminExpenses(plan, year);

        // 7. Depreciation
        var depreciation = DepreciationCalculator.Calculate(plan, year, priorNetAssets);

        // 8. Financing
        var financing = FinancingCalculator.Calculate(plan, year, priorLoanBalance, priorEquity);

        // 9. P&L Assembly
        var totalOpEx = new MonthlyGrid();
        for (int m = 0; m < 12; m++)
        {
            totalOpEx[m] = payroll.TotalPayroll[m] + salesExpenses.Total[m] +
                           adminExpenses.Total[m] + depreciation.TotalDepreciation[m];
        }

        var ebit = grossProfit - totalOpEx + new MonthlyGrid(); // clone via addition
        // Correct: EBIT = Gross Profit - Total OpEx
        var ebitGrid = new MonthlyGrid();
        for (int m = 0; m < 12; m++)
            ebitGrid[m] = grossProfit[m] - totalOpEx[m];

        var interestGrid = new MonthlyGrid();
        for (int m = 0; m < 12; m++)
            interestGrid[m] = financing.InterestPayments[m];

        var netIncomeGrid = new MonthlyGrid();
        for (int m = 0; m < 12; m++)
            netIncomeGrid[m] = ebitGrid[m] - interestGrid[m];

        // 10. Cash Flow Assembly
        var cashFromSales = new MonthlyGrid();
        for (int m = 0; m < 12; m++)
            cashFromSales[m] = revenue.CashCollected[m] + carryoverCash[m];

        var netCashFlow = new MonthlyGrid();
        for (int m = 0; m < 12; m++)
        {
            netCashFlow[m] = cashFromSales[m]
                           - cogs.TotalCOGS[m]                 // COGS paid immediately
                           - payroll.TotalPayroll[m]            // Payroll paid immediately
                           - salesExpenses.Total[m]             // Sales expenses paid immediately
                           - adminExpenses.Total[m]             // Admin expenses paid immediately
                           - depreciation.CapexPurchases[m]     // CAPEX cash outflow
                           + financing.CashInflows[m]           // Financing received
                           - financing.TotalLoanPayments[m];    // Loan payments (principal + interest)
        }

        var cumulativeCash = new MonthlyGrid();
        decimal runningCash = priorCash;
        for (int m = 0; m < 12; m++)
        {
            runningCash += netCashFlow[m];
            cumulativeCash[m] = Math.Round(runningCash, 2);
        }

        // 11. Balance Sheet Assembly
        var cashGrid = cumulativeCash; // Cash = cumulative cash position

        // Accounts Receivable = Revenue earned but not yet collected
        var arGrid = new MonthlyGrid();
        decimal cumulativeRevenue = priorAR; // Start with prior year's outstanding AR
        decimal cumulativeCashCollected = 0;
        for (int m = 0; m < 12; m++)
        {
            cumulativeRevenue += revenue.TotalRevenue[m];
            cumulativeCashCollected += cashFromSales[m];
            arGrid[m] = Math.Max(0, Math.Round(cumulativeRevenue - cumulativeCashCollected, 2));
        }

        // Inventory
        var inventoryGrid = cogs.EndingInventory;

        // Fixed Assets Net
        var fixedAssetsGrid = depreciation.NetFixedAssets;

        // Total Assets
        var totalAssetsGrid = new MonthlyGrid();
        for (int m = 0; m < 12; m++)
        {
            totalAssetsGrid[m] = Math.Round(
                cashGrid[m] + arGrid[m] + inventoryGrid[m] + fixedAssetsGrid[m], 2);
        }

        // Liabilities = Loan balances
        var loansGrid = financing.LoanBalance;
        var totalLiabilitiesGrid = loansGrid; // Simplified: only loans

        // Equity
        var investedCapitalGrid = financing.InvestedCapital;
        var retainedEarningsGrid = new MonthlyGrid();
        decimal cumulativeNetIncome = priorRetainedEarnings;
        for (int m = 0; m < 12; m++)
        {
            cumulativeNetIncome += netIncomeGrid[m];
            retainedEarningsGrid[m] = Math.Round(cumulativeNetIncome, 2);
        }

        var totalEquityGrid = new MonthlyGrid();
        for (int m = 0; m < 12; m++)
        {
            totalEquityGrid[m] = Math.Round(
                investedCapitalGrid[m] + retainedEarningsGrid[m], 2);
        }

        return new YearCalculationResult
        {
            Year = year,

            // Revenue
            RevenueLines = revenue.Lines,
            TotalRevenue = revenue.TotalRevenue,

            // COGS
            COGSLines = cogs.Lines,
            TotalCOGS = cogs.TotalCOGS,
            GrossProfit = grossProfit,

            // Payroll
            PayrollLines = payroll.Lines,
            TotalPayroll = payroll.TotalPayroll,

            // Expenses
            SalesExpenseLines = salesExpenses.Lines,
            TotalSalesExpenses = salesExpenses.Total,
            AdminExpenseLines = adminExpenses.Lines,
            TotalAdminExpenses = adminExpenses.Total,

            // Depreciation
            TotalDepreciation = depreciation.TotalDepreciation,

            // P&L
            TotalOperatingExpenses = totalOpEx,
            EBIT = ebitGrid,
            InterestExpense = interestGrid,
            NetIncome = netIncomeGrid,

            // Cash Flow
            CashFromSales = cashFromSales,
            CashOutCOGS = cogs.TotalCOGS,
            CashOutPayroll = payroll.TotalPayroll,
            CashOutSalesExpenses = salesExpenses.Total,
            CashOutAdminExpenses = adminExpenses.Total,
            CashOutCapex = depreciation.CapexPurchases,
            CashInFinancing = financing.CashInflows,
            CashOutLoanPayments = financing.PrincipalPayments,
            CashOutInterest = financing.InterestPayments,
            NetCashFlow = netCashFlow,
            CumulativeCash = cumulativeCash,

            // Balance Sheet
            Cash = cashGrid,
            AccountsReceivable = arGrid,
            Inventory = inventoryGrid,
            FixedAssetsNet = fixedAssetsGrid,
            TotalAssets = totalAssetsGrid,
            LoansPayable = loansGrid,
            TotalLiabilities = totalLiabilitiesGrid,
            InvestedCapital = investedCapitalGrid,
            RetainedEarnings = retainedEarningsGrid,
            TotalEquity = totalEquityGrid
        };
    }
}

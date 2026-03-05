using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Engine;
using Sqordia.Application.Financial.Services.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Services.Implementations.Financial;

/// <summary>
/// Orchestrates the calculation engine and serves financial statement reports.
/// Loads all plan data, builds an immutable snapshot, runs the pure engine, maps to DTOs.
/// </summary>
public class FinancialStatementsServiceImpl : IFinancialStatementsService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FinancialStatementsServiceImpl> _logger;

    public FinancialStatementsServiceImpl(IApplicationDbContext context, ILogger<FinancialStatementsServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<FinancialStatementsResponse>> RecalculateAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default)
    {
        var snapshot = await BuildSnapshotAsync(businessPlanId, cancellationToken);
        if (snapshot == null)
            return Result.Failure<FinancialStatementsResponse>(
                Error.NotFound("FinancialPlan.NotFound", "Financial plan not found for this business plan"));

        var output = PrevisioCalculationEngine.Calculate(snapshot);
        var labels = StatementLabels.Get(language);

        var response = new FinancialStatementsResponse
        {
            ProfitLoss = MapProfitLoss(output, 1, labels),
            CashFlow = MapCashFlow(output, 1, labels),
            BalanceSheet = MapBalanceSheet(output, 1, labels),
            Ratios = MapRatios(output)
        };

        return Result.Success(response);
    }

    public async Task<Result<ProfitLossStatement>> GetProfitLossAsync(Guid businessPlanId, int year, string language = "fr", CancellationToken cancellationToken = default)
    {
        var snapshot = await BuildSnapshotAsync(businessPlanId, cancellationToken);
        if (snapshot == null)
            return Result.Failure<ProfitLossStatement>(
                Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var output = PrevisioCalculationEngine.Calculate(snapshot);
        return Result.Success(MapProfitLoss(output, year, StatementLabels.Get(language)));
    }

    public async Task<Result<CashFlowStatement>> GetCashFlowAsync(Guid businessPlanId, int year, string language = "fr", CancellationToken cancellationToken = default)
    {
        var snapshot = await BuildSnapshotAsync(businessPlanId, cancellationToken);
        if (snapshot == null)
            return Result.Failure<CashFlowStatement>(
                Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var output = PrevisioCalculationEngine.Calculate(snapshot);
        return Result.Success(MapCashFlow(output, year, StatementLabels.Get(language)));
    }

    public async Task<Result<BalanceSheetStatement>> GetBalanceSheetAsync(Guid businessPlanId, int year, string language = "fr", CancellationToken cancellationToken = default)
    {
        var snapshot = await BuildSnapshotAsync(businessPlanId, cancellationToken);
        if (snapshot == null)
            return Result.Failure<BalanceSheetStatement>(
                Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var output = PrevisioCalculationEngine.Calculate(snapshot);
        return Result.Success(MapBalanceSheet(output, year, StatementLabels.Get(language)));
    }

    public async Task<Result<FinancialRatiosResponse>> GetRatiosAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var snapshot = await BuildSnapshotAsync(businessPlanId, cancellationToken);
        if (snapshot == null)
            return Result.Failure<FinancialRatiosResponse>(
                Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var output = PrevisioCalculationEngine.Calculate(snapshot);
        return Result.Success(MapRatios(output));
    }

    // ──────────────────── Snapshot Builder ────────────────────

    private async Task<FinancialPlanSnapshot?> BuildSnapshotAsync(Guid businessPlanId, CancellationToken ct)
    {
        var plan = await _context.FinancialPlansPrevisio
            .AsNoTracking()
            .Include(fp => fp.SalesProducts)
                .ThenInclude(sp => sp.SalesVolumes)
            .Include(fp => fp.CostOfGoodsSoldItems)
            .Include(fp => fp.PayrollItems)
            .Include(fp => fp.SalesExpenseItems)
            .Include(fp => fp.AdminExpenseItems)
            .Include(fp => fp.CapexAssets)
            .Include(fp => fp.FinancingSources)
                .ThenInclude(fs => fs.AmortizationEntries)
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId && !fp.IsDeleted, ct);

        if (plan == null) return null;

        return new FinancialPlanSnapshot
        {
            ProjectionYears = plan.ProjectionYears,
            StartYear = plan.StartYear,
            DefaultVolumeGrowthRate = plan.DefaultVolumeGrowthRate,
            DefaultPriceIndexationRate = plan.DefaultPriceIndexationRate,
            DefaultExpenseIndexationRate = plan.DefaultExpenseIndexationRate,
            DefaultSocialChargeRate = plan.DefaultSocialChargeRate,
            DefaultSalesTaxRate = plan.DefaultSalesTaxRate,

            Products = plan.SalesProducts.Where(sp => !sp.IsDeleted).Select(sp => new ProductSnapshot
            {
                Id = sp.Id,
                Name = sp.Name,
                UnitPrice = sp.UnitPrice,
                PaymentDelay = sp.PaymentDelay,
                TaxRate = sp.TaxRate,
                InputMode = sp.InputMode,
                VolumeIndexationRate = sp.VolumeIndexationRate,
                PriceIndexationRate = sp.PriceIndexationRate,
                Volumes = sp.SalesVolumes.Select(sv => new VolumeEntry(sv.Year, sv.Month, sv.Quantity)).ToList()
            }).ToList(),

            COGSItems = plan.CostOfGoodsSoldItems.Where(c => !c.IsDeleted).Select(c => new COGSSnapshot
            {
                LinkedSalesProductId = c.LinkedSalesProductId,
                CostMode = c.CostMode,
                CostValue = c.CostValue,
                BeginningInventory = c.BeginningInventory,
                CostIndexationRate = c.CostIndexationRate
            }).ToList(),

            PayrollItems = plan.PayrollItems.Where(p => !p.IsDeleted).Select(p => new PayrollSnapshot
            {
                JobTitle = p.JobTitle,
                PayrollType = p.PayrollType,
                EmploymentStatus = p.EmploymentStatus,
                SalaryFrequency = p.SalaryFrequency,
                SalaryAmount = p.SalaryAmount,
                SocialChargeRate = p.SocialChargeRate,
                HeadCount = p.HeadCount,
                StartMonth = p.StartMonth,
                StartYear = p.StartYear,
                SalaryIndexationRate = p.SalaryIndexationRate
            }).ToList(),

            SalesExpenses = plan.SalesExpenseItems.Where(e => !e.IsDeleted).Select(e => new SalesExpenseSnapshot
            {
                Name = e.Name,
                Category = e.Category,
                ExpenseMode = e.ExpenseMode,
                Amount = e.Amount,
                Frequency = e.Frequency,
                StartMonth = e.StartMonth,
                StartYear = e.StartYear,
                IndexationRate = e.IndexationRate
            }).ToList(),

            AdminExpenses = plan.AdminExpenseItems.Where(e => !e.IsDeleted).Select(e => new AdminExpenseSnapshot
            {
                Name = e.Name,
                Category = e.Category,
                MonthlyAmount = e.MonthlyAmount,
                IsTaxable = e.IsTaxable,
                Frequency = e.Frequency,
                StartMonth = e.StartMonth,
                StartYear = e.StartYear,
                IndexationRate = e.IndexationRate
            }).ToList(),

            CapexAssets = plan.CapexAssets.Where(a => !a.IsDeleted).Select(a => new CapexSnapshot
            {
                Name = a.Name,
                AssetType = a.AssetType,
                PurchaseValue = a.PurchaseValue,
                PurchaseMonth = a.PurchaseMonth,
                PurchaseYear = a.PurchaseYear,
                DepreciationMethod = a.DepreciationMethod,
                UsefulLifeYears = a.UsefulLifeYears,
                SalvageValue = a.SalvageValue
            }).ToList(),

            FinancingSources = plan.FinancingSources.Where(f => !f.IsDeleted).Select(f => new FinancingSnapshot
            {
                Id = f.Id,
                Name = f.Name,
                FinancingType = f.FinancingType,
                Amount = f.Amount,
                InterestRate = f.InterestRate,
                TermMonths = f.TermMonths,
                MoratoireMonths = f.MoratoireMonths,
                DisbursementMonth = f.DisbursementMonth,
                DisbursementYear = f.DisbursementYear
            }).ToList(),

            AmortizationEntries = plan.FinancingSources
                .Where(f => !f.IsDeleted)
                .SelectMany(f => f.AmortizationEntries.Select(ae => new AmortizationSnapshot
                {
                    FinancingSourceId = ae.FinancingSourceId,
                    PaymentNumber = ae.PaymentNumber,
                    Year = ae.Year,
                    Month = ae.Month,
                    PaymentAmount = ae.PaymentAmount,
                    PrincipalPortion = ae.PrincipalPortion,
                    InterestPortion = ae.InterestPortion,
                    RemainingBalance = ae.RemainingBalance,
                    IsMoratoire = ae.IsMoratoire
                })).ToList(),

            ProjectCost = await GetProjectCostSnapshotAsync(plan.Id, ct)
        };
    }

    private async Task<ProjectCostSnapshot?> GetProjectCostSnapshotAsync(Guid planId, CancellationToken ct)
    {
        var pc = await _context.ProjectCosts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.FinancialPlanId == planId && !p.IsDeleted, ct);

        if (pc == null) return null;

        return new ProjectCostSnapshot
        {
            WorkingCapitalMonthsCOGS = pc.WorkingCapitalMonthsCOGS,
            WorkingCapitalMonthsPayroll = pc.WorkingCapitalMonthsPayroll,
            WorkingCapitalMonthsSalesExpenses = pc.WorkingCapitalMonthsSalesExpenses,
            WorkingCapitalMonthsAdminExpenses = pc.WorkingCapitalMonthsAdminExpenses,
            CapexInclusionMonths = pc.CapexInclusionMonths
        };
    }

    // ──────────────────── DTO Mapping ────────────────────

    private static ProfitLossStatement MapProfitLoss(CalculationOutput output, int year, StatementLabels.Labels l)
    {
        if (!output.YearResults.TryGetValue(year, out var yr))
            yr = new YearCalculationResult { Year = year };

        return new ProfitLossStatement
        {
            Revenue = yr.RevenueLines.Select(li => ToLineItem(li)).ToList(),
            CostOfGoodsSold = yr.COGSLines.Select(li => ToLineItem(li)).ToList(),
            GrossProfit = ToLineItem(l.GrossProfit, yr.GrossProfit, isBold: true),
            Payroll = yr.PayrollLines.Select(li => ToLineItem(li)).ToList(),
            SalesExpenses = yr.SalesExpenseLines.Select(li => ToLineItem(li)).ToList(),
            AdminExpenses = yr.AdminExpenseLines.Select(li => ToLineItem(li)).ToList(),
            Depreciation = ToLineItem(l.Depreciation, yr.TotalDepreciation),
            TotalOperatingExpenses = ToLineItem(l.TotalOperatingExpenses, yr.TotalOperatingExpenses, isBold: true),
            EBIT = ToLineItem(l.EBIT, yr.EBIT, isBold: true),
            InterestExpense = ToLineItem(l.InterestExpense, yr.InterestExpense),
            NetIncome = ToLineItem(l.NetIncome, yr.NetIncome, isBold: true)
        };
    }

    private static CashFlowStatement MapCashFlow(CalculationOutput output, int year, StatementLabels.Labels l)
    {
        if (!output.YearResults.TryGetValue(year, out var yr))
            yr = new YearCalculationResult { Year = year };

        var inflows = new List<StatementLineItem>
        {
            ToLineItem(l.SalesCollected, yr.CashFromSales),
            ToLineItem(l.FinancingReceived, yr.CashInFinancing)
        };

        var outflows = new List<StatementLineItem>
        {
            ToLineItem(l.COGSCash, yr.CashOutCOGS),
            ToLineItem(l.PayrollCash, yr.CashOutPayroll),
            ToLineItem(l.SalesExpensesCash, yr.CashOutSalesExpenses),
            ToLineItem(l.AdminExpensesCash, yr.CashOutAdminExpenses),
            ToLineItem(l.CapexCash, yr.CashOutCapex),
            ToLineItem(l.LoanRepayment, yr.CashOutLoanPayments),
            ToLineItem(l.Interest, yr.CashOutInterest)
        };

        return new CashFlowStatement
        {
            CashInflows = inflows,
            CashOutflows = outflows,
            NetCashFlow = ToLineItem(l.NetCashFlow, yr.NetCashFlow, isBold: true),
            CumulativeCashFlow = ToLineItem(l.CumulativeCash, yr.CumulativeCash, isBold: true)
        };
    }

    private static BalanceSheetStatement MapBalanceSheet(CalculationOutput output, int year, StatementLabels.Labels l)
    {
        if (!output.YearResults.TryGetValue(year, out var yr))
            yr = new YearCalculationResult { Year = year };

        var assets = new List<StatementLineItem>
        {
            ToLineItem(l.Cash, yr.Cash),
            ToLineItem(l.AccountsReceivable, yr.AccountsReceivable),
            ToLineItem(l.Inventory, yr.Inventory),
            ToLineItem(l.FixedAssetsNet, yr.FixedAssetsNet)
        };

        var liabilities = new List<StatementLineItem>
        {
            ToLineItem(l.LoansPayable, yr.LoansPayable)
        };

        var equity = new List<StatementLineItem>
        {
            ToLineItem(l.InvestedCapital, yr.InvestedCapital),
            ToLineItem(l.RetainedEarnings, yr.RetainedEarnings)
        };

        var totalAssets = yr.TotalAssets[11];
        var totalLiabEquity = yr.TotalLiabilities[11] + yr.TotalEquity[11];

        return new BalanceSheetStatement
        {
            Assets = assets,
            TotalAssets = ToLineItem(l.TotalAssets, yr.TotalAssets, isBold: true),
            Liabilities = liabilities,
            TotalLiabilities = ToLineItem(l.TotalLiabilities, yr.TotalLiabilities, isBold: true),
            Equity = equity,
            TotalEquity = ToLineItem(l.TotalEquity, yr.TotalEquity, isBold: true),
            TotalLiabilitiesAndEquity = ToLineItem(l.TotalLiabilitiesAndEquity,
                SumGrids(yr.TotalLiabilities, yr.TotalEquity), isBold: true),
            IsBalanced = Math.Abs(totalAssets - totalLiabEquity) < 0.01m
        };
    }

    private static FinancialRatiosResponse MapRatios(CalculationOutput output) => new()
    {
        DebtRatio = output.DebtRatio,
        LiquidityRatio = output.LiquidityRatio,
        GrossMargin = output.GrossMargin,
        NetMargin = output.NetMargin,
        BreakEvenMonth = output.BreakEvenMonth,
        WorkingCapitalRatio = output.WorkingCapitalRatio
    };

    // ──────────────────── Helpers ────────────────────

    private static StatementLineItem ToLineItem(LineResult lr) => new()
    {
        Label = lr.Label,
        MonthlyValues = lr.Grid.ToArray(),
        AnnualTotal = lr.Grid.Total,
        IndentLevel = lr.IndentLevel,
        IsBold = lr.IsBold,
        IsHeader = lr.IsHeader
    };

    private static StatementLineItem ToLineItem(string label, MonthlyGrid grid, bool isBold = false, int indent = 0) => new()
    {
        Label = label,
        MonthlyValues = grid.ToArray(),
        AnnualTotal = grid.Total,
        IsBold = isBold,
        IndentLevel = indent
    };

    private static MonthlyGrid SumGrids(MonthlyGrid a, MonthlyGrid b)
    {
        var result = new MonthlyGrid();
        for (int i = 0; i < 12; i++)
            result[i] = a[i] + b[i];
        return result;
    }
}

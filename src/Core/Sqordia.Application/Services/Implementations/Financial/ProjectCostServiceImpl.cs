using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Services.Previsio;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Application.Services.Implementations.Financial;

public class ProjectCostServiceImpl : IProjectCostService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ProjectCostServiceImpl> _logger;

    public ProjectCostServiceImpl(IApplicationDbContext context, ILogger<ProjectCostServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ProjectCostResponse>> GetAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<ProjectCostResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var projectCost = await _context.ProjectCosts
            .FirstOrDefaultAsync(pc => pc.FinancialPlanId == plan.Id, cancellationToken);

        if (projectCost == null)
        {
            projectCost = new ProjectCost(plan.Id);
            _context.ProjectCosts.Add(projectCost);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Compute working capital breakdown from module data
        var breakdown = await ComputeBreakdownAsync(plan.Id, projectCost, cancellationToken);

        return Result.Success(new ProjectCostResponse
        {
            Id = projectCost.Id,
            WorkingCapitalMonthsCOGS = projectCost.WorkingCapitalMonthsCOGS,
            WorkingCapitalMonthsPayroll = projectCost.WorkingCapitalMonthsPayroll,
            WorkingCapitalMonthsSalesExpenses = projectCost.WorkingCapitalMonthsSalesExpenses,
            WorkingCapitalMonthsAdminExpenses = projectCost.WorkingCapitalMonthsAdminExpenses,
            CapexInclusionMonths = projectCost.CapexInclusionMonths,
            TotalStartupCosts = projectCost.TotalStartupCosts,
            TotalWorkingCapital = projectCost.TotalWorkingCapital,
            TotalCapex = projectCost.TotalCapex,
            TotalProjectCost = projectCost.TotalProjectCost,
            Breakdown = breakdown
        });
    }

    public async Task<Result<ProjectCostResponse>> UpdateSettingsAsync(Guid businessPlanId, UpdateProjectCostSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<ProjectCostResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var projectCost = await _context.ProjectCosts
            .FirstOrDefaultAsync(pc => pc.FinancialPlanId == plan.Id, cancellationToken);

        if (projectCost == null)
            return Result.Failure<ProjectCostResponse>(Error.NotFound("ProjectCost.NotFound", "Project cost not found"));

        projectCost.UpdateDurationSettings(
            request.WorkingCapitalMonthsCOGS,
            request.WorkingCapitalMonthsPayroll,
            request.WorkingCapitalMonthsSalesExpenses,
            request.WorkingCapitalMonthsAdminExpenses,
            request.CapexInclusionMonths);

        await _context.SaveChangesAsync(cancellationToken);
        return await GetAsync(businessPlanId, cancellationToken);
    }

    private async Task<ProjectCostBreakdownResponse> ComputeBreakdownAsync(Guid planId, ProjectCost projectCost, CancellationToken ct)
    {
        // Average monthly COGS from first year
        var cogsItems = await _context.CostOfGoodsSoldItems
            .Where(c => c.FinancialPlanId == planId)
            .Include(c => c.LinkedSalesProduct)
            .ToListAsync(ct);

        // Average monthly payroll
        var payrollItems = await _context.PayrollItems
            .Where(p => p.FinancialPlanId == planId)
            .ToListAsync(ct);

        var monthlyPayroll = payrollItems.Sum(p => p.GetMonthlyTotalCost());

        // Average monthly expenses
        var salesExpenses = await _context.SalesExpenseItems
            .Where(e => e.FinancialPlanId == planId)
            .ToListAsync(ct);

        var adminExpenses = await _context.AdminExpenseItems
            .Where(e => e.FinancialPlanId == planId)
            .ToListAsync(ct);

        var monthlySalesExpenses = salesExpenses.Sum(e => e.Amount);
        var monthlyAdminExpenses = adminExpenses.Sum(e => e.MonthlyAmount);

        // CAPEX in inclusion window
        var capexAssets = await _context.CapexAssets
            .Where(a => a.FinancialPlanId == planId)
            .ToListAsync(ct);

        var wcCOGS = 0m; // Simplified: will be computed by engine
        var wcPayroll = monthlyPayroll * projectCost.WorkingCapitalMonthsPayroll;
        var wcSalesExp = monthlySalesExpenses * projectCost.WorkingCapitalMonthsSalesExpenses;
        var wcAdminExp = monthlyAdminExpenses * projectCost.WorkingCapitalMonthsAdminExpenses;
        var totalCapex = capexAssets.Sum(a => a.PurchaseValue);

        return new ProjectCostBreakdownResponse
        {
            WorkingCapitalCOGS = wcCOGS,
            WorkingCapitalPayroll = wcPayroll,
            WorkingCapitalSalesExpenses = wcSalesExp,
            WorkingCapitalAdminExpenses = wcAdminExp,
            CapexItems = capexAssets.Select(a => new CapexBreakdownItem { Name = a.Name, Amount = a.PurchaseValue }).ToList()
        };
    }
}

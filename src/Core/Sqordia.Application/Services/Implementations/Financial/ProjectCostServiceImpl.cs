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

        // Auto-populate from business plan data if all breakdown values are 0
        var isBlank = projectCost.SalaryAlreadyAcquired == 0 && projectCost.SalaryAcquireBefore == 0
                   && projectCost.SalesExpAlreadyAcquired == 0 && projectCost.SalesExpAcquireBefore == 0
                   && projectCost.AdminExpAlreadyAcquired == 0 && projectCost.AdminExpAcquireBefore == 0
                   && projectCost.InventoryAlreadyAcquired == 0 && projectCost.InventoryAcquireBefore == 0
                   && projectCost.CapexAlreadyAcquired == 0 && projectCost.CapexAcquireBefore == 0;

        if (isBlank)
        {
            await AutoPopulateFromPlanAsync(plan.Id, projectCost, cancellationToken);
        }

        // Compute working capital breakdown from module data
        var breakdown = await ComputeBreakdownAsync(plan.Id, projectCost, cancellationToken);

        // Recompute totals
        var salaryTotal = projectCost.SalaryAlreadyAcquired + projectCost.SalaryAcquireBefore + projectCost.SalaryAcquireAfter;
        var salesExpTotal = projectCost.SalesExpAlreadyAcquired + projectCost.SalesExpAcquireBefore + projectCost.SalesExpAcquireAfter;
        var adminExpTotal = projectCost.AdminExpAlreadyAcquired + projectCost.AdminExpAcquireBefore + projectCost.AdminExpAcquireAfter;
        var inventoryTotal = projectCost.InventoryAlreadyAcquired + projectCost.InventoryAcquireBefore + projectCost.InventoryAcquireAfter;
        var capexTotal = projectCost.CapexAlreadyAcquired + projectCost.CapexAcquireBefore + projectCost.CapexAcquireAfter;

        var startupSubtotal = salaryTotal + salesExpTotal + adminExpTotal;
        var totalProjectCost = startupSubtotal + inventoryTotal + capexTotal;

        projectCost.UpdateComputedTotals(startupSubtotal, startupSubtotal, capexTotal, totalProjectCost);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToResponse(projectCost, breakdown));
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

        projectCost.UpdateBreakdown(
            request.SalaryAlreadyAcquired, request.SalaryAcquireBefore, request.SalaryAcquireAfter, request.SalaryDurationMonths,
            request.SalesExpAlreadyAcquired, request.SalesExpAcquireBefore, request.SalesExpAcquireAfter, request.SalesExpDurationMonths,
            request.AdminExpAlreadyAcquired, request.AdminExpAcquireBefore, request.AdminExpAcquireAfter, request.AdminExpDurationMonths,
            request.InventoryAlreadyAcquired, request.InventoryAcquireBefore, request.InventoryAcquireAfter, request.InventoryDurationMonths,
            request.CapexAlreadyAcquired, request.CapexAcquireBefore, request.CapexAcquireAfter, request.CapexDurationMonths);

        await _context.SaveChangesAsync(cancellationToken);
        return await GetAsync(businessPlanId, cancellationToken);
    }

    /// <summary>
    /// Auto-populates breakdown from existing financial plan data (payroll, expenses, COGS, CAPEX).
    /// Places computed monthly × duration values in the "AcquireBefore" column as initial estimates.
    /// </summary>
    private async Task AutoPopulateFromPlanAsync(Guid planId, ProjectCost projectCost, CancellationToken ct)
    {
        var payrollItems = await _context.PayrollItems
            .Where(p => p.FinancialPlanId == planId)
            .ToListAsync(ct);
        var monthlyPayroll = payrollItems.Sum(p => p.GetMonthlyTotalCost());

        var salesExpenses = await _context.SalesExpenseItems
            .Where(e => e.FinancialPlanId == planId)
            .ToListAsync(ct);
        var monthlySalesExp = salesExpenses.Sum(e => e.Amount);

        var adminExpenses = await _context.AdminExpenseItems
            .Where(e => e.FinancialPlanId == planId)
            .ToListAsync(ct);
        var monthlyAdminExp = adminExpenses.Sum(e => e.MonthlyAmount);

        var cogsItems = await _context.CostOfGoodsSoldItems
            .Where(c => c.FinancialPlanId == planId)
            .ToListAsync(ct);
        var monthlyCogs = cogsItems.Sum(c => c.CostValue);

        var capexAssets = await _context.CapexAssets
            .Where(a => a.FinancialPlanId == planId)
            .ToListAsync(ct);
        var totalCapex = capexAssets.Sum(a => a.PurchaseValue);

        projectCost.UpdateBreakdown(
            salaryAcquired: 0,
            salaryBefore: monthlyPayroll * projectCost.SalaryDurationMonths,
            salaryAfter: 0,
            salaryDuration: projectCost.SalaryDurationMonths,
            salesExpAcquired: 0,
            salesExpBefore: monthlySalesExp * projectCost.SalesExpDurationMonths,
            salesExpAfter: 0,
            salesExpDuration: projectCost.SalesExpDurationMonths,
            adminExpAcquired: 0,
            adminExpBefore: monthlyAdminExp * projectCost.AdminExpDurationMonths,
            adminExpAfter: 0,
            adminExpDuration: projectCost.AdminExpDurationMonths,
            inventoryAcquired: 0,
            inventoryBefore: monthlyCogs * projectCost.InventoryDurationMonths,
            inventoryAfter: 0,
            inventoryDuration: projectCost.InventoryDurationMonths,
            capexAcquired: 0,
            capexBefore: totalCapex,
            capexAfter: 0,
            capexDuration: projectCost.CapexDurationMonths);

        await _context.SaveChangesAsync(ct);
    }

    private async Task<ProjectCostBreakdownResponse> ComputeBreakdownAsync(Guid planId, ProjectCost projectCost, CancellationToken ct)
    {
        var payrollItems = await _context.PayrollItems
            .Where(p => p.FinancialPlanId == planId)
            .ToListAsync(ct);
        var monthlyPayroll = payrollItems.Sum(p => p.GetMonthlyTotalCost());

        var salesExpenses = await _context.SalesExpenseItems
            .Where(e => e.FinancialPlanId == planId)
            .ToListAsync(ct);
        var monthlySalesExp = salesExpenses.Sum(e => e.Amount);

        var adminExpenses = await _context.AdminExpenseItems
            .Where(e => e.FinancialPlanId == planId)
            .ToListAsync(ct);
        var monthlyAdminExp = adminExpenses.Sum(e => e.MonthlyAmount);

        var capexAssets = await _context.CapexAssets
            .Where(a => a.FinancialPlanId == planId)
            .ToListAsync(ct);

        return new ProjectCostBreakdownResponse
        {
            WorkingCapitalCOGS = 0, // Will be computed by engine
            WorkingCapitalPayroll = monthlyPayroll * projectCost.WorkingCapitalMonthsPayroll,
            WorkingCapitalSalesExpenses = monthlySalesExp * projectCost.WorkingCapitalMonthsSalesExpenses,
            WorkingCapitalAdminExpenses = monthlyAdminExp * projectCost.WorkingCapitalMonthsAdminExpenses,
            CapexItems = capexAssets.Select(a => new CapexBreakdownItem { Name = a.Name, Amount = a.PurchaseValue }).ToList()
        };
    }

    private static ProjectCostResponse MapToResponse(ProjectCost pc, ProjectCostBreakdownResponse breakdown)
    {
        return new ProjectCostResponse
        {
            Id = pc.Id,
            WorkingCapitalMonthsCOGS = pc.WorkingCapitalMonthsCOGS,
            WorkingCapitalMonthsPayroll = pc.WorkingCapitalMonthsPayroll,
            WorkingCapitalMonthsSalesExpenses = pc.WorkingCapitalMonthsSalesExpenses,
            WorkingCapitalMonthsAdminExpenses = pc.WorkingCapitalMonthsAdminExpenses,
            CapexInclusionMonths = pc.CapexInclusionMonths,
            SalaryAlreadyAcquired = pc.SalaryAlreadyAcquired,
            SalaryAcquireBefore = pc.SalaryAcquireBefore,
            SalaryAcquireAfter = pc.SalaryAcquireAfter,
            SalaryDurationMonths = pc.SalaryDurationMonths,
            SalesExpAlreadyAcquired = pc.SalesExpAlreadyAcquired,
            SalesExpAcquireBefore = pc.SalesExpAcquireBefore,
            SalesExpAcquireAfter = pc.SalesExpAcquireAfter,
            SalesExpDurationMonths = pc.SalesExpDurationMonths,
            AdminExpAlreadyAcquired = pc.AdminExpAlreadyAcquired,
            AdminExpAcquireBefore = pc.AdminExpAcquireBefore,
            AdminExpAcquireAfter = pc.AdminExpAcquireAfter,
            AdminExpDurationMonths = pc.AdminExpDurationMonths,
            InventoryAlreadyAcquired = pc.InventoryAlreadyAcquired,
            InventoryAcquireBefore = pc.InventoryAcquireBefore,
            InventoryAcquireAfter = pc.InventoryAcquireAfter,
            InventoryDurationMonths = pc.InventoryDurationMonths,
            CapexAlreadyAcquired = pc.CapexAlreadyAcquired,
            CapexAcquireBefore = pc.CapexAcquireBefore,
            CapexAcquireAfter = pc.CapexAcquireAfter,
            CapexDurationMonths = pc.CapexDurationMonths,
            TotalStartupCosts = pc.TotalStartupCosts,
            TotalWorkingCapital = pc.TotalWorkingCapital,
            TotalCapex = pc.TotalCapex,
            TotalProjectCost = pc.TotalProjectCost,
            Breakdown = breakdown
        };
    }
}

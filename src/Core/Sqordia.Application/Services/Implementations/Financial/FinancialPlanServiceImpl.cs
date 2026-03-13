using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Services.Previsio;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;
using Sqordia.Domain.Entities.Financial;

namespace Sqordia.Application.Services.Implementations.Financial;

public class FinancialPlanServiceImpl : IFinancialPlanService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FinancialPlanServiceImpl> _logger;

    public FinancialPlanServiceImpl(IApplicationDbContext context, ILogger<FinancialPlanServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<FinancialPlanResponse>> CreateAsync(Guid businessPlanId, CreateFinancialPlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _context.FinancialPlansPrevisio
                .AnyAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

            if (exists)
                return Result.Failure<FinancialPlanResponse>(Error.Conflict("FinancialPlan.AlreadyExists", "A financial plan already exists for this business plan"));

            var plan = new FinancialPlan(businessPlanId, request.StartYear, request.ProjectionYears);
            _context.FinancialPlansPrevisio.Add(plan);

            // Auto-create project cost
            var projectCost = new ProjectCost(plan.Id);
            _context.ProjectCosts.Add(projectCost);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created financial plan {PlanId} for business plan {BusinessPlanId}", plan.Id, businessPlanId);
            return Result.Success(MapToResponse(plan));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating financial plan for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<FinancialPlanResponse>(Error.InternalServerError("FinancialPlan.CreateError", "Failed to create financial plan"));
        }
    }

    public async Task<Result<FinancialPlanResponse>> GetByBusinessPlanIdAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .Include(fp => fp.SalesProducts)
            .Include(fp => fp.PayrollItems)
            .Include(fp => fp.FinancingSources)
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<FinancialPlanResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        return Result.Success(MapToResponse(plan));
    }

    public async Task<Result<FinancialPlanResponse>> UpdateSettingsAsync(Guid businessPlanId, UpdateFinancialPlanSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<FinancialPlanResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        plan.UpdateSettings(
            request.ProjectionYears,
            request.DefaultVolumeGrowthRate,
            request.DefaultPriceIndexationRate,
            request.DefaultExpenseIndexationRate,
            request.DefaultSocialChargeRate,
            request.DefaultSalesTaxRate,
            request.StartMonth,
            request.SalesTaxFrequency,
            request.IsAlreadyOperating);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(MapToResponse(plan));
    }

    public async Task<Result> DeleteAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        plan.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static FinancialPlanResponse MapToResponse(FinancialPlan plan)
    {
        return new FinancialPlanResponse
        {
            Id = plan.Id,
            BusinessPlanId = plan.BusinessPlanId,
            ProjectionYears = plan.ProjectionYears,
            StartYear = plan.StartYear,
            StartMonth = plan.StartMonth,
            SalesTaxFrequency = plan.SalesTaxFrequency,
            IsAlreadyOperating = plan.IsAlreadyOperating,
            DefaultVolumeGrowthRate = plan.DefaultVolumeGrowthRate,
            DefaultPriceIndexationRate = plan.DefaultPriceIndexationRate,
            DefaultExpenseIndexationRate = plan.DefaultExpenseIndexationRate,
            DefaultSocialChargeRate = plan.DefaultSocialChargeRate,
            DefaultSalesTaxRate = plan.DefaultSalesTaxRate,
            SalesProductCount = plan.SalesProducts?.Count ?? 0,
            PayrollItemCount = plan.PayrollItems?.Count ?? 0,
            FinancingSourceCount = plan.FinancingSources?.Count ?? 0
        };
    }
}

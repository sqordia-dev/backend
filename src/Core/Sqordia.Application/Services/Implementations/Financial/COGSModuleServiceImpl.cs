using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Services.Previsio;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;
using Sqordia.Domain.Entities.Financial;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Application.Services.Implementations.Financial;

public class COGSModuleServiceImpl : ICOGSModuleService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<COGSModuleServiceImpl> _logger;

    public COGSModuleServiceImpl(IApplicationDbContext context, ILogger<COGSModuleServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<COGSModuleResponse>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<COGSModuleResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var items = await _context.CostOfGoodsSoldItems
            .Where(c => c.FinancialPlanId == plan.Id)
            .Include(c => c.LinkedSalesProduct)
            .ToListAsync(cancellationToken);

        return Result.Success(new COGSModuleResponse
        {
            Items = items.Select(MapToResponse).ToList()
        });
    }

    public async Task<Result<COGSItemResponse>> CreateAsync(Guid businessPlanId, CreateCOGSItemRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<COGSItemResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var product = await _context.SalesProducts
            .FirstOrDefaultAsync(sp => sp.Id == request.LinkedSalesProductId && sp.FinancialPlanId == plan.Id, cancellationToken);

        if (product == null)
            return Result.Failure<COGSItemResponse>(Error.NotFound("COGS.ProductNotFound", "Linked sales product not found"));

        var existingCogs = await _context.CostOfGoodsSoldItems
            .AnyAsync(c => c.LinkedSalesProductId == request.LinkedSalesProductId, cancellationToken);

        if (existingCogs)
            return Result.Failure<COGSItemResponse>(Error.Conflict("COGS.AlreadyExists", "COGS already exists for this product"));

        if (!Enum.TryParse<CostMode>(request.CostMode, true, out var costMode))
            return Result.Failure<COGSItemResponse>(Error.Validation("COGS.InvalidCostMode", "Invalid cost mode"));

        var item = new CostOfGoodsSoldItem(plan.Id, request.LinkedSalesProductId, costMode, request.CostValue, request.BeginningInventory);
        _context.CostOfGoodsSoldItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with navigation
        var saved = await _context.CostOfGoodsSoldItems
            .Include(c => c.LinkedSalesProduct)
            .FirstAsync(c => c.Id == item.Id, cancellationToken);

        return Result.Success(MapToResponse(saved));
    }

    public async Task<Result<COGSItemResponse>> UpdateAsync(Guid businessPlanId, Guid itemId, UpdateCOGSItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(businessPlanId, itemId, cancellationToken);
        if (item == null)
            return Result.Failure<COGSItemResponse>(Error.NotFound("COGS.NotFound", "COGS item not found"));

        if (!Enum.TryParse<CostMode>(request.CostMode, true, out var costMode))
            return Result.Failure<COGSItemResponse>(Error.Validation("COGS.InvalidCostMode", "Invalid cost mode"));

        item.Update(costMode, request.CostValue, request.BeginningInventory, request.CostIndexationRate);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToResponse(item));
    }

    public async Task<Result> DeleteAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(businessPlanId, itemId, cancellationToken);
        if (item == null)
            return Result.Failure(Error.NotFound("COGS.NotFound", "COGS item not found"));

        item.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<CostOfGoodsSoldItem?> GetItemAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return null;

        return await _context.CostOfGoodsSoldItems
            .Include(c => c.LinkedSalesProduct)
            .FirstOrDefaultAsync(c => c.Id == itemId && c.FinancialPlanId == plan.Id, cancellationToken);
    }

    private static COGSItemResponse MapToResponse(CostOfGoodsSoldItem item)
    {
        var effectiveCost = item.CostMode == CostMode.FixedDollars
            ? item.CostValue
            : item.LinkedSalesProduct?.UnitPrice * item.CostValue / 100 ?? 0;

        return new COGSItemResponse
        {
            Id = item.Id,
            LinkedSalesProductId = item.LinkedSalesProductId,
            LinkedProductName = item.LinkedSalesProduct?.Name ?? "",
            LinkedProductPrice = item.LinkedSalesProduct?.UnitPrice ?? 0,
            CostMode = item.CostMode.ToString(),
            CostValue = item.CostValue,
            BeginningInventory = item.BeginningInventory,
            CostIndexationRate = item.CostIndexationRate,
            EffectiveCostPerUnit = effectiveCost
        };
    }
}

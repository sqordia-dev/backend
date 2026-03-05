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

public class CapexModuleServiceImpl : ICapexModuleService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CapexModuleServiceImpl> _logger;

    public CapexModuleServiceImpl(IApplicationDbContext context, ILogger<CapexModuleServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<CapexAssetResponse>>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio.FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<List<CapexAssetResponse>>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var assets = await _context.CapexAssets
            .Where(a => a.FinancialPlanId == plan.Id)
            .OrderBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);

        return Result.Success(assets.Select(MapToResponse).ToList());
    }

    public async Task<Result<CapexAssetResponse>> CreateAsync(Guid businessPlanId, CreateCapexAssetRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio.FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<CapexAssetResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        if (!Enum.TryParse<AssetType>(request.AssetType, true, out var assetType))
            return Result.Failure<CapexAssetResponse>(Error.Validation("Capex.InvalidAssetType", "Invalid asset type"));

        var maxOrder = await _context.CapexAssets.Where(a => a.FinancialPlanId == plan.Id).MaxAsync(a => (int?)a.SortOrder, cancellationToken) ?? 0;

        var asset = new CapexAsset(plan.Id, request.Name, assetType, request.PurchaseValue, request.PurchaseMonth, request.PurchaseYear, maxOrder + 1);
        _context.CapexAssets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToResponse(asset));
    }

    public async Task<Result<CapexAssetResponse>> UpdateAsync(Guid businessPlanId, Guid assetId, UpdateCapexAssetRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio.FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<CapexAssetResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var asset = await _context.CapexAssets.FirstOrDefaultAsync(a => a.Id == assetId && a.FinancialPlanId == plan.Id, cancellationToken);
        if (asset == null) return Result.Failure<CapexAssetResponse>(Error.NotFound("Capex.NotFound", "Asset not found"));

        Enum.TryParse<AssetType>(request.AssetType, true, out var assetType);
        Enum.TryParse<DepreciationMethod>(request.DepreciationMethod, true, out var depMethod);

        asset.Update(request.Name, assetType, request.PurchaseValue, request.PurchaseMonth, request.PurchaseYear, depMethod, request.UsefulLifeYears, request.SalvageValue);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToResponse(asset));
    }

    public async Task<Result> DeleteAsync(Guid businessPlanId, Guid assetId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio.FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var asset = await _context.CapexAssets.FirstOrDefaultAsync(a => a.Id == assetId && a.FinancialPlanId == plan.Id, cancellationToken);
        if (asset == null) return Result.Failure(Error.NotFound("Capex.NotFound", "Asset not found"));

        asset.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static CapexAssetResponse MapToResponse(CapexAsset asset)
    {
        var annualDep = asset.DepreciationMethod == DepreciationMethod.StraightLine
            ? (asset.PurchaseValue - asset.SalvageValue) / Math.Max(asset.UsefulLifeYears, 1)
            : 0; // Declining balance calculated in engine

        return new CapexAssetResponse
        {
            Id = asset.Id,
            Name = asset.Name,
            AssetType = asset.AssetType.ToString(),
            PurchaseValue = asset.PurchaseValue,
            PurchaseMonth = asset.PurchaseMonth,
            PurchaseYear = asset.PurchaseYear,
            DepreciationMethod = asset.DepreciationMethod.ToString(),
            UsefulLifeYears = asset.UsefulLifeYears,
            SalvageValue = asset.SalvageValue,
            SortOrder = asset.SortOrder,
            AnnualDepreciation = annualDep
        };
    }
}

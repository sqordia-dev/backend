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

public class SalesModuleServiceImpl : ISalesModuleService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SalesModuleServiceImpl> _logger;

    public SalesModuleServiceImpl(IApplicationDbContext context, ILogger<SalesModuleServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<SalesModuleResponse>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<SalesModuleResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var products = await _context.SalesProducts
            .Where(sp => sp.FinancialPlanId == plan.Id)
            .Include(sp => sp.CostOfGoodsSoldItem)
            .OrderBy(sp => sp.SortOrder)
            .ToListAsync(cancellationToken);

        var productIds = products.Select(p => p.Id).ToList();
        var volumes = await _context.SalesVolumes
            .Where(sv => productIds.Contains(sv.SalesProductId))
            .ToListAsync(cancellationToken);

        var response = new SalesModuleResponse
        {
            Products = products.Select(p => MapProductToResponse(p)).ToList(),
            VolumeGrids = products.SelectMany(p =>
            {
                var productVolumes = volumes.Where(v => v.SalesProductId == p.Id);
                var years = productVolumes.Select(v => v.Year).Distinct().OrderBy(y => y);
                return years.Select(year => new SalesVolumeGridResponse
                {
                    SalesProductId = p.Id,
                    ProductName = p.Name,
                    Year = year,
                    MonthlyValues = Enumerable.Range(0, 13)
                        .Select(m => new MonthlyValueResponse
                        {
                            Month = m,
                            Value = productVolumes.FirstOrDefault(v => v.Year == year && v.Month == m)?.Quantity ?? 0
                        }).ToList(),
                    YearTotal = productVolumes.Where(v => v.Year == year).Sum(v => v.Quantity)
                });
            }).ToList()
        };

        return Result.Success(response);
    }

    public async Task<Result<SalesProductResponse>> CreateProductAsync(Guid businessPlanId, CreateSalesProductRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<SalesProductResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        if (!Enum.TryParse<PaymentDelay>(request.PaymentDelay, true, out var paymentDelay))
            return Result.Failure<SalesProductResponse>(Error.Validation("Sales.InvalidPaymentDelay", "Invalid payment delay value"));

        if (!Enum.TryParse<SalesInputMode>(request.InputMode, true, out var inputMode))
            inputMode = SalesInputMode.Quantity;

        var maxOrder = await _context.SalesProducts
            .Where(sp => sp.FinancialPlanId == plan.Id)
            .MaxAsync(sp => (int?)sp.SortOrder, cancellationToken) ?? 0;

        var product = new SalesProduct(plan.Id, request.Name, request.UnitPrice, paymentDelay, request.TaxRate, inputMode, maxOrder + 1);
        _context.SalesProducts.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created sales product {ProductId} for plan {PlanId}", product.Id, plan.Id);
        return Result.Success(MapProductToResponse(product));
    }

    public async Task<Result<SalesProductResponse>> UpdateProductAsync(Guid businessPlanId, Guid productId, UpdateSalesProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(businessPlanId, productId, cancellationToken);
        if (product == null)
            return Result.Failure<SalesProductResponse>(Error.NotFound("Sales.ProductNotFound", "Sales product not found"));

        if (!Enum.TryParse<PaymentDelay>(request.PaymentDelay, true, out var paymentDelay))
            return Result.Failure<SalesProductResponse>(Error.Validation("Sales.InvalidPaymentDelay", "Invalid payment delay value"));

        if (!Enum.TryParse<SalesInputMode>(request.InputMode, true, out var inputMode))
            inputMode = SalesInputMode.Quantity;

        product.Update(request.Name, request.UnitPrice, paymentDelay, request.TaxRate, inputMode, request.VolumeIndexationRate, request.PriceIndexationRate);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapProductToResponse(product));
    }

    public async Task<Result> DeleteProductAsync(Guid businessPlanId, Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(businessPlanId, productId, cancellationToken);
        if (product == null)
            return Result.Failure(Error.NotFound("Sales.ProductNotFound", "Sales product not found"));

        product.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<SalesVolumeGridResponse>> GetVolumeGridAsync(Guid businessPlanId, Guid productId, int year, CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(businessPlanId, productId, cancellationToken);
        if (product == null)
            return Result.Failure<SalesVolumeGridResponse>(Error.NotFound("Sales.ProductNotFound", "Sales product not found"));

        var volumes = await _context.SalesVolumes
            .Where(sv => sv.SalesProductId == productId && sv.Year == year)
            .ToListAsync(cancellationToken);

        return Result.Success(new SalesVolumeGridResponse
        {
            SalesProductId = productId,
            ProductName = product.Name,
            Year = year,
            MonthlyValues = Enumerable.Range(0, 13)
                .Select(m => new MonthlyValueResponse
                {
                    Month = m,
                    Value = volumes.FirstOrDefault(v => v.Month == m)?.Quantity ?? 0
                }).ToList(),
            YearTotal = volumes.Sum(v => v.Quantity)
        });
    }

    public async Task<Result<SalesVolumeGridResponse>> UpdateVolumeGridAsync(Guid businessPlanId, UpdateSalesVolumeGridRequest request, CancellationToken cancellationToken = default)
    {
        var product = await GetProductAsync(businessPlanId, request.SalesProductId, cancellationToken);
        if (product == null)
            return Result.Failure<SalesVolumeGridResponse>(Error.NotFound("Sales.ProductNotFound", "Sales product not found"));

        var existingVolumes = await _context.SalesVolumes
            .Where(sv => sv.SalesProductId == request.SalesProductId && sv.Year == request.Year)
            .ToListAsync(cancellationToken);

        foreach (var mv in request.MonthlyValues)
        {
            var existing = existingVolumes.FirstOrDefault(v => v.Month == mv.Month);
            if (existing != null)
            {
                existing.UpdateQuantity(mv.Value);
            }
            else if (mv.Value != 0)
            {
                _context.SalesVolumes.Add(new SalesVolume(request.SalesProductId, request.Year, mv.Month, mv.Value));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return await GetVolumeGridAsync(businessPlanId, request.SalesProductId, request.Year, cancellationToken);
    }

    public async Task<Result> ReplicateYearAsync(Guid businessPlanId, ReplicateYearRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var productIds = request.ProductIds;
        if (productIds == null || !productIds.Any())
        {
            productIds = await _context.SalesProducts
                .Where(sp => sp.FinancialPlanId == plan.Id)
                .Select(sp => sp.Id)
                .ToListAsync(cancellationToken);
        }

        var sourceVolumes = await _context.SalesVolumes
            .Where(sv => productIds.Contains(sv.SalesProductId) && sv.Year == request.SourceYear)
            .ToListAsync(cancellationToken);

        // Remove existing target year volumes
        var targetVolumes = await _context.SalesVolumes
            .Where(sv => productIds.Contains(sv.SalesProductId) && sv.Year == request.TargetYear)
            .ToListAsync(cancellationToken);

        foreach (var tv in targetVolumes)
        {
            _context.SalesVolumes.Remove(tv);
        }

        // Create new volumes with augmentation
        var multiplier = 1 + (request.AugmentationRate / 100);
        foreach (var sv in sourceVolumes)
        {
            _context.SalesVolumes.Add(new SalesVolume(
                sv.SalesProductId,
                request.TargetYear,
                sv.Month,
                Math.Round(sv.Quantity * multiplier, 2)));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<SalesProduct?> GetProductAsync(Guid businessPlanId, Guid productId, CancellationToken cancellationToken)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null) return null;

        return await _context.SalesProducts
            .FirstOrDefaultAsync(sp => sp.Id == productId && sp.FinancialPlanId == plan.Id, cancellationToken);
    }

    private static SalesProductResponse MapProductToResponse(SalesProduct product)
    {
        return new SalesProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            UnitPrice = product.UnitPrice,
            PaymentDelay = product.PaymentDelay.ToString(),
            TaxRate = product.TaxRate,
            InputMode = product.InputMode.ToString(),
            VolumeIndexationRate = product.VolumeIndexationRate,
            PriceIndexationRate = product.PriceIndexationRate,
            SortOrder = product.SortOrder,
            HasCOGS = product.CostOfGoodsSoldItem != null
        };
    }
}

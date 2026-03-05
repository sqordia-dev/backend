using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

public interface ISalesModuleService
{
    Task<Result<SalesModuleResponse>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<SalesProductResponse>> CreateProductAsync(Guid businessPlanId, CreateSalesProductRequest request, CancellationToken cancellationToken = default);
    Task<Result<SalesProductResponse>> UpdateProductAsync(Guid businessPlanId, Guid productId, UpdateSalesProductRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteProductAsync(Guid businessPlanId, Guid productId, CancellationToken cancellationToken = default);
    Task<Result<SalesVolumeGridResponse>> GetVolumeGridAsync(Guid businessPlanId, Guid productId, int year, CancellationToken cancellationToken = default);
    Task<Result<SalesVolumeGridResponse>> UpdateVolumeGridAsync(Guid businessPlanId, UpdateSalesVolumeGridRequest request, CancellationToken cancellationToken = default);
    Task<Result> ReplicateYearAsync(Guid businessPlanId, ReplicateYearRequest request, CancellationToken cancellationToken = default);
}

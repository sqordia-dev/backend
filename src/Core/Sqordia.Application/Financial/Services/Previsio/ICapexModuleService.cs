using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

public interface ICapexModuleService
{
    Task<Result<List<CapexAssetResponse>>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<CapexAssetResponse>> CreateAsync(Guid businessPlanId, CreateCapexAssetRequest request, CancellationToken cancellationToken = default);
    Task<Result<CapexAssetResponse>> UpdateAsync(Guid businessPlanId, Guid assetId, UpdateCapexAssetRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid businessPlanId, Guid assetId, CancellationToken cancellationToken = default);
}

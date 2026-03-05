using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

public interface ICOGSModuleService
{
    Task<Result<COGSModuleResponse>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<COGSItemResponse>> CreateAsync(Guid businessPlanId, CreateCOGSItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<COGSItemResponse>> UpdateAsync(Guid businessPlanId, Guid itemId, UpdateCOGSItemRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken = default);
}

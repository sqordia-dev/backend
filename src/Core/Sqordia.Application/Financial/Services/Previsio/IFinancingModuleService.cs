using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

public interface IFinancingModuleService
{
    Task<Result<FinancingModuleResponse>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<FinancingSourceResponse>> CreateAsync(Guid businessPlanId, CreateFinancingSourceRequest request, CancellationToken cancellationToken = default);
    Task<Result<FinancingSourceResponse>> UpdateAsync(Guid businessPlanId, Guid sourceId, UpdateFinancingSourceRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid businessPlanId, Guid sourceId, CancellationToken cancellationToken = default);
    Task<Result<List<AmortizationEntryResponse>>> GetAmortizationScheduleAsync(Guid businessPlanId, Guid sourceId, CancellationToken cancellationToken = default);
}

using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

public interface IFinancialPlanService
{
    Task<Result<FinancialPlanResponse>> CreateAsync(Guid businessPlanId, CreateFinancialPlanRequest request, CancellationToken cancellationToken = default);
    Task<Result<FinancialPlanResponse>> GetByBusinessPlanIdAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<FinancialPlanResponse>> UpdateSettingsAsync(Guid businessPlanId, UpdateFinancialPlanSettingsRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
}

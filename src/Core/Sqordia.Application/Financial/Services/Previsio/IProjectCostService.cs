using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

public interface IProjectCostService
{
    Task<Result<ProjectCostResponse>> GetAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<ProjectCostResponse>> UpdateSettingsAsync(Guid businessPlanId, UpdateProjectCostSettingsRequest request, CancellationToken cancellationToken = default);
}

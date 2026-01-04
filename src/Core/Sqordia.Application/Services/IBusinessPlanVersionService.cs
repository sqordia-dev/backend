using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace Sqordia.Application.Services;

public interface IBusinessPlanVersionService
{
    Task<Result<BusinessPlanVersionResponse>> CreateVersionAsync(Guid businessPlanId, string? comment = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BusinessPlanVersionResponse>>> GetVersionsAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<BusinessPlanVersionResponse>> GetVersionAsync(Guid businessPlanId, int versionNumber, CancellationToken cancellationToken = default);
    Task<Result<BusinessPlanResponse>> RestoreVersionAsync(Guid businessPlanId, int versionNumber, CancellationToken cancellationToken = default);
}


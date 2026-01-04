using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace Sqordia.Application.Services;

public interface IBusinessPlanShareService
{
    Task<Result<BusinessPlanShareResponse>> ShareBusinessPlanAsync(Guid businessPlanId, ShareBusinessPlanRequest request, CancellationToken cancellationToken = default);
    Task<Result<BusinessPlanShareResponse>> CreatePublicShareAsync(Guid businessPlanId, CreatePublicShareRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<BusinessPlanShareResponse>>> GetSharesAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result> RevokeShareAsync(Guid businessPlanId, Guid shareId, CancellationToken cancellationToken = default);
    Task<Result<BusinessPlanResponse>> GetBusinessPlanByPublicTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Result> UpdateSharePermissionAsync(Guid businessPlanId, Guid shareId, UpdateSharePermissionRequest request, CancellationToken cancellationToken = default);
}


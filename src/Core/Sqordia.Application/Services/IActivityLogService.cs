using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.ActivityLog;
using Sqordia.Contracts.Responses.ActivityLog;

namespace Sqordia.Application.Services;

public interface IActivityLogService
{
    Task<Result<ActivityLogResponse>> LogActivityAsync(
        CreateActivityLogRequest request,
        CancellationToken cancellationToken = default);
}

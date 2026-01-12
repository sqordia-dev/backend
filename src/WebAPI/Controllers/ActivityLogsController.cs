using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.ActivityLog;

namespace WebAPI.Controllers;

/// <summary>
/// Activity logging endpoint for tracking user actions
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/activity-logs")]
[Authorize]
public class ActivityLogsController : BaseApiController
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Log a user activity
    /// </summary>
    /// <param name="request">Activity details</param>
    /// <returns>Activity log confirmation</returns>
    [HttpPost]
    public async Task<IActionResult> LogActivity([FromBody] CreateActivityLogRequest request)
    {
        var result = await _activityLogService.LogActivityAsync(request);
        return HandleResult(result);
    }
}

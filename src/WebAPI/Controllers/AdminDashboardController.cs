using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;

namespace WebAPI.Controllers;

/// <summary>
/// Admin Dashboard API for system management and analytics
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : BaseApiController
{
    private readonly IAdminDashboardService _adminDashboardService;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(
        IAdminDashboardService adminDashboardService,
        ILogger<AdminDashboardController> logger)
    {
        _adminDashboardService = adminDashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive system overview and key metrics
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetSystemOverview(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Admin requesting system overview");
        var result = await _adminDashboardService.GetSystemOverviewAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get paginated list of users with filtering and sorting options
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] UserStatus? status = null,
        [FromQuery] string? userType = null,
        [FromQuery] DateTime? createdAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] bool? emailVerified = null,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters. Page must be >= 1, PageSize must be between 1 and 100." });
        }

        var request = new AdminUserRequest
        {
            SearchTerm = searchTerm,
            Status = status,
            UserType = userType,
            CreatedAfter = createdAfter,
            CreatedBefore = createdBefore,
            EmailVerified = emailVerified,
            SortBy = sortBy,
            SortDescending = sortDescending,
            Page = page,
            PageSize = pageSize
        };

        var result = await _adminDashboardService.GetUsersAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get paginated list of organizations with filtering options
    /// </summary>
    [HttpGet("organizations")]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? organizationType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] DateTime? createdAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters. Page must be >= 1, PageSize must be between 1 and 100." });
        }

        var request = new AdminOrganizationRequest
        {
            SearchTerm = searchTerm,
            OrganizationType = organizationType,
            IsActive = isActive,
            CreatedAfter = createdAfter,
            CreatedBefore = createdBefore,
            SortBy = sortBy,
            SortDescending = sortDescending,
            Page = page,
            PageSize = pageSize
        };

        var result = await _adminDashboardService.GetOrganizationsAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get paginated list of business plans with comprehensive filtering
    /// </summary>
    [HttpGet("business-plans")]
    public async Task<IActionResult> GetBusinessPlans(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? planType = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? createdAfter = null,
        [FromQuery] DateTime? createdBefore = null,
        [FromQuery] Guid? organizationId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters. Page must be >= 1, PageSize must be between 1 and 100." });
        }

        var request = new AdminBusinessPlanRequest
        {
            SearchTerm = searchTerm,
            PlanType = planType,
            Status = status,
            CreatedAfter = createdAfter,
            CreatedBefore = createdBefore,
            OrganizationId = organizationId,
            UserId = userId,
            SortBy = sortBy,
            SortDescending = sortDescending,
            Page = page,
            PageSize = pageSize
        };

        var result = await _adminDashboardService.GetBusinessPlansAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update user status (activate, deactivate, suspend, etc.)
    /// </summary>
    [HttpPut("users/{userId}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        Guid userId,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid user ID." });
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { message = "Reason is required for status changes." });
        }

        var result = await _adminDashboardService.UpdateUserStatusAsync(userId, request.Status, request.Reason, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update organization status (activate/deactivate)
    /// </summary>
    [HttpPut("organizations/{organizationId}/status")]
    public async Task<IActionResult> UpdateOrganizationStatus(
        Guid organizationId,
        [FromBody] UpdateOrganizationStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid organization ID." });
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return BadRequest(new { message = "Reason is required for status changes." });
        }

        var result = await _adminDashboardService.UpdateOrganizationStatusAsync(organizationId, request.IsActive, request.Reason, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get system activity logs with filtering options
    /// </summary>
    [HttpGet("activity-logs")]
    public async Task<IActionResult> GetActivityLogs(
        [FromQuery] string? action = null,
        [FromQuery] string? entity = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool? isSuccess = null,
        [FromQuery] string? ipAddress = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "Invalid pagination parameters. Page must be >= 1, PageSize must be between 1 and 100." });
        }

        var request = new AdminActivityLogRequest
        {
            Action = action,
            Entity = entity,
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate,
            IsSuccess = isSuccess,
            IPAddress = ipAddress,
            Page = page,
            PageSize = pageSize
        };

        var result = await _adminDashboardService.GetActivityLogsAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get system health status and performance metrics
    /// </summary>
    [HttpGet("system-health")]
    public async Task<IActionResult> GetSystemHealth(CancellationToken cancellationToken = default)
    {
        var result = await _adminDashboardService.GetSystemHealthAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Generate comprehensive system reports
    /// </summary>
    [HttpPost("reports/{reportType}")]
    public async Task<IActionResult> GenerateReport(
        AdminReportType reportType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string format = "pdf",
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object>();

        if (startDate.HasValue)
            parameters["startDate"] = startDate.Value;

        if (endDate.HasValue)
            parameters["endDate"] = endDate.Value;

        parameters["format"] = format;

        var result = await _adminDashboardService.GenerateSystemReportAsync(reportType, parameters, cancellationToken);

        if (result.IsSuccess && result.Value != null)
        {
            var report = result.Value;
            return File(report.ReportData ?? Array.Empty<byte>(), report.ContentType ?? "application/octet-stream", report.FileName ?? "report");
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Get AI usage statistics and cost analysis
    /// </summary>
    [HttpGet("ai-usage-stats")]
    public async Task<IActionResult> GetAIUsageStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        if (start > end)
        {
            return BadRequest(new { message = "Start date must be before end date." });
        }

        if ((end - start).TotalDays > 365)
        {
            return BadRequest(new { message = "Date range cannot exceed 365 days." });
        }

        var result = await _adminDashboardService.GetAIUsageStatsAsync(start, end, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Force regeneration of business plan sections using AI
    /// </summary>
    [HttpPost("business-plans/{businessPlanId}/regenerate")]
    public async Task<IActionResult> ForceBusinessPlanRegeneration(
        Guid businessPlanId,
        [FromBody] ForceRegenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (businessPlanId == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid business plan ID." });
        }

        var result = await _adminDashboardService.ForceBusinessPlanRegenerationAsync(businessPlanId, request.Sections, cancellationToken);
        return HandleResult(result);
    }
}

/// <summary>
/// Request model for updating user status
/// </summary>
public class UpdateUserStatusRequest
{
    /// <summary>
    /// New user status
    /// </summary>
    public UserStatus Status { get; set; }

    /// <summary>
    /// Reason for status change (required for audit trail)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating organization status
/// </summary>
public class UpdateOrganizationStatusRequest
{
    /// <summary>
    /// New active status
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Reason for status change (required for audit trail)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Request model for forcing business plan regeneration
/// </summary>
public class ForceRegenerationRequest
{
    /// <summary>
    /// Specific sections to regenerate (null or empty for all sections)
    /// </summary>
    public List<string>? Sections { get; set; }

    /// <summary>
    /// Reason for regeneration (optional)
    /// </summary>
    public string? Reason { get; set; }
}
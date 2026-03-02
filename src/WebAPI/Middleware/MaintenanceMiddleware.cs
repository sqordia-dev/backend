using System.Text.Json;
using Sqordia.Application.Services;

namespace WebAPI.Middleware;

/// <summary>
/// Middleware that blocks requests when maintenance mode is enabled.
/// Returns HTTP 503 with maintenance status details.
/// </summary>
public class MaintenanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MaintenanceMiddleware> _logger;

    /// <summary>
    /// Paths that should bypass maintenance mode.
    /// These endpoints remain accessible during maintenance.
    /// </summary>
    private static readonly string[] BypassPaths = new[]
    {
        "/health",
        "/api/health",
        "/api/v1/maintenance/status",
        "/api/v1/maintenance/ci/",
        "/swagger",
        "/favicon.ico"
    };

    public MaintenanceMiddleware(RequestDelegate next, ILogger<MaintenanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IMaintenanceService maintenanceService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Check if path should bypass maintenance mode
        if (ShouldBypass(path))
        {
            await _next(context);
            return;
        }

        // Check maintenance status
        var statusResult = await maintenanceService.GetStatusAsync(context.RequestAborted);

        // If we can't get status or maintenance is disabled, proceed normally
        if (!statusResult.IsSuccess || statusResult.Value?.IsEnabled != true)
        {
            await _next(context);
            return;
        }

        var status = statusResult.Value;

        // Check if admin bypass is allowed and user is admin
        if (status.AllowAdminAccess && IsAdmin(context))
        {
            _logger.LogDebug("Admin user bypassing maintenance mode: {User}", context.User.Identity?.Name);
            await _next(context);
            return;
        }

        // Return 503 Service Unavailable with maintenance details
        _logger.LogInformation("Request blocked due to maintenance mode. Path: {Path}", path);

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "application/json";

        // Set Retry-After header (in seconds)
        if (status.EstimatedEnd.HasValue)
        {
            var retryAfterSeconds = Math.Max(60, (int)(status.EstimatedEnd.Value - DateTime.UtcNow).TotalSeconds);
            context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
        }
        else
        {
            context.Response.Headers["Retry-After"] = "300"; // Default 5 minutes
        }

        var response = new
        {
            error = "Service temporarily unavailable",
            message = "The application is currently under maintenance. Please try again later.",
            maintenance = new
            {
                isEnabled = status.IsEnabled,
                reason = status.Reason,
                estimatedEnd = status.EstimatedEnd,
                progressPercent = status.ProgressPercent,
                currentStep = status.CurrentStep,
                type = status.Type
            }
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static bool ShouldBypass(string path)
    {
        return BypassPaths.Any(bypassPath =>
            path.StartsWith(bypassPath, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsAdmin(HttpContext context)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        return context.User.IsInRole("Admin") || context.User.IsInRole("Administrator");
    }
}

using System.Diagnostics;

namespace WebAPI.Middleware;

/// <summary>
/// Performance monitoring middleware
/// Tracks request duration and logs slow requests
/// </summary>
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        // Log slow requests (over 1 second)
        if (elapsedMilliseconds > 1000)
        {
            _logger.LogWarning("Slow request: {Method} {Path} took {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                elapsedMilliseconds);
        }

        // Add performance header only if response hasn't started
        if (!context.Response.HasStarted)
        {
            context.Response.Headers["X-Response-Time"] = $"{elapsedMilliseconds}ms";
        }
    }
}

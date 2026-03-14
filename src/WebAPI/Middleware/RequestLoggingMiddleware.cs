using System.Diagnostics;
using Serilog.Context;

namespace WebAPI.Middleware;

/// <summary>
/// Request logging middleware with correlation ID support.
/// Generates or reads X-Request-ID, enriches Serilog context, and logs timing.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate or read correlation ID
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault()
                     ?? Guid.NewGuid().ToString("N")[..12];
        context.TraceIdentifier = requestId;
        context.Response.Headers["X-Request-ID"] = requestId;

        using (LogContext.PushProperty("RequestId", requestId))
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("HTTP {Method} {Path} started",
                context.Request.Method,
                context.Request.Path);

            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation("HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}

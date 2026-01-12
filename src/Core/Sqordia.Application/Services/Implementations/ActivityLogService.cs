using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.ActivityLog;
using Sqordia.Contracts.Responses.ActivityLog;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Application.Services.Implementations;

public class ActivityLogService : IActivityLogService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ActivityLogService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<ActivityLogResponse>> LogActivityAsync(
        CreateActivityLogRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current user
            var userIdString = _currentUserService.GetUserId();
            Guid? userId = null;
            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            // Get IP address from HttpContext
            var ipAddress = GetClientIpAddress();

            // Serialize metadata to JSON if provided
            string? additionalData = null;
            if (request.Metadata != null && request.Metadata.Count > 0)
            {
                additionalData = JsonSerializer.Serialize(request.Metadata);
            }

            // Create audit log entity
            var auditLog = new AuditLog(
                userId: userId,
                action: request.Action,
                entityType: request.EntityType ?? "general",
                entityId: request.EntityId,
                ipAddress: ipAddress,
                userAgent: request.UserAgent,
                success: true,
                additionalData: additionalData
            );

            // Save to database
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Activity logged: {Action} by user {UserId} on {EntityType} {EntityId}",
                request.Action, userId, request.EntityType, request.EntityId);

            // Map to response
            var response = new ActivityLogResponse
            {
                Id = auditLog.Id,
                UserId = auditLog.UserId,
                Action = auditLog.Action,
                EntityType = auditLog.EntityType,
                EntityId = auditLog.EntityId,
                Timestamp = auditLog.Timestamp,
                Success = auditLog.Success
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging activity: {Action}", request.Action);
            return Result.Failure<ActivityLogResponse>(
                Error.InternalServerError(
                    "ActivityLog.Error",
                    "An error occurred while logging activity"));
        }
    }

    private string? GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // Try X-Forwarded-For header first (for proxies/load balancers)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        // Try X-Real-IP header
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to remote IP address
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}

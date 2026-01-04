using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Security;

namespace Sqordia.Application.Services.Implementations;

public class SecurityManagementService : ISecurityManagementService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SecurityManagementService> _logger;
    private readonly ILocalizationService _localizationService;

    public SecurityManagementService(
        IApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SecurityManagementService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<List<ActiveSessionResponse>>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentSessionToken = GetCurrentSessionToken();
            
            var now = DateTime.UtcNow;
            var sessions = await _context.ActiveSessions
                .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > now)
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync(cancellationToken);

            var response = sessions.Select(s => new ActiveSessionResponse
            {
                Id = s.Id,
                CreatedAt = s.CreatedAt,
                LastActivityAt = s.LastActivityAt,
                ExpiresAt = s.ExpiresAt,
                IsActive = s.IsActive,
                IsCurrentSession = s.SessionToken == currentSessionToken,
                IpAddress = s.IpAddress,
                DeviceType = s.DeviceType,
                Browser = s.Browser,
                OperatingSystem = s.OperatingSystem,
                Country = s.Country,
                City = s.City
            }).ToList();

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active sessions for user {UserId}", userId);
            return Result.Failure<List<ActiveSessionResponse>>(
                Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _context.ActiveSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

            if (session == null)
            {
                return Result.Failure(Error.NotFound("Security.Error.SessionNotFound", _localizationService.GetString("Security.Error.SessionNotFound")));
            }

            var ipAddress = GetClientIpAddress();
            session.Revoke(ipAddress);
            
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Session {SessionId} revoked for user {UserId}", sessionId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking session {SessionId} for user {UserId}", sessionId, userId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> RevokeAllSessionsExceptCurrentAsync(Guid userId, string currentSessionToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _context.ActiveSessions
                .Where(s => s.UserId == userId && s.IsActive && s.SessionToken != currentSessionToken)
                .ToListAsync(cancellationToken);

            var ipAddress = GetClientIpAddress();
            foreach (var session in sessions)
            {
                session.Revoke(ipAddress);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Revoked {Count} sessions for user {UserId} (except current)", sessions.Count, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all sessions except current for user {UserId}", userId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> RevokeAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _context.ActiveSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync(cancellationToken);

            var ipAddress = GetClientIpAddress();
            foreach (var session in sessions)
            {
                session.Revoke(ipAddress);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Revoked all {Count} sessions for user {UserId}", sessions.Count, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all sessions for user {UserId}", userId);
            return Result.Failure(Error.InternalServerError("Security.RevokeAllSessions.Failed", "Failed to revoke all sessions"));
        }
    }

    public async Task<Result<List<LoginHistoryResponse>>> GetLoginHistoryAsync(
        Guid userId, 
        int pageNumber = 1, 
        int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var skip = (pageNumber - 1) * pageSize;
            
            var loginHistory = await _context.LoginHistories
                .Where(lh => lh.UserId == userId)
                .OrderByDescending(lh => lh.LoginAttemptAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var response = loginHistory.Select(lh => new LoginHistoryResponse
            {
                Id = lh.Id,
                LoginAttemptAt = lh.LoginAttemptAt,
                IsSuccessful = lh.IsSuccessful,
                FailureReason = lh.FailureReason,
                IpAddress = lh.IpAddress,
                DeviceType = lh.DeviceType,
                Browser = lh.Browser,
                OperatingSystem = lh.OperatingSystem,
                Country = lh.Country,
                City = lh.City
            }).ToList();

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting login history for user {UserId}", userId);
            return Result.Failure<List<LoginHistoryResponse>>(
                Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> UnlockAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            if (!user.IsLockedOut)
            {
                return Result.Failure(Error.Validation("Security.Error.NotLocked", _localizationService.GetString("Security.Error.NotLocked")));
            }

            user.UnlockAccount();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Account unlocked for user {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking account for user {UserId}", userId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> ForcePasswordChangeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Result.Failure(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            user.ForcePasswordChange();
            
            // Revoke all active sessions to force re-login
            var sessions = await _context.ActiveSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync(cancellationToken);

            var ipAddress = GetClientIpAddress();
            foreach (var session in sessions)
            {
                session.Revoke(ipAddress);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Forced password change for user {UserId}", userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forcing password change for user {UserId}", userId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    private string GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Connection?.RemoteIpAddress != null)
        {
            return httpContext.Connection.RemoteIpAddress.ToString();
        }
        return "Unknown";
    }

    private string? GetCurrentSessionToken()
    {
        // Try to get the refresh token from the request (if available)
        // This is a simplified version - in production you might want to track this differently
        return _httpContextAccessor.HttpContext?.Request.Headers["X-Session-Token"].ToString();
    }
}


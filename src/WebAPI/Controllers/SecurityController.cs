using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using WebAPI.Controllers;

namespace WebAPI.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/security")]
public class SecurityController : BaseApiController
{
    private readonly ISecurityManagementService _securityManagementService;

    public SecurityController(ISecurityManagementService securityManagementService)
    {
        _securityManagementService = securityManagementService;
    }

    /// <summary>
    /// Get all active sessions for the current user
    /// </summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetActiveSessions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _securityManagementService.GetActiveSessionsAsync(userId.Value, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Revoke a specific session
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _securityManagementService.RevokeSessionAsync(userId.Value, sessionId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Revoke all sessions except the current one
    /// </summary>
    [HttpPost("sessions/revoke-others")]
    public async Task<IActionResult> RevokeAllOtherSessions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Get the current refresh token from request header or body
        var currentSessionToken = Request.Headers["X-Session-Token"].ToString();
        
        var result = await _securityManagementService.RevokeAllSessionsExceptCurrentAsync(
            userId.Value, 
            currentSessionToken, 
            cancellationToken);
        
        return HandleResult(result);
    }

    /// <summary>
    /// Revoke all sessions (including current)
    /// </summary>
    [HttpPost("sessions/revoke-all")]
    public async Task<IActionResult> RevokeAllSessions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _securityManagementService.RevokeAllSessionsAsync(userId.Value, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get login history for the current user
    /// </summary>
    [HttpGet("login-history")]
    public async Task<IActionResult> GetLoginHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _securityManagementService.GetLoginHistoryAsync(
            userId.Value, 
            pageNumber, 
            pageSize, 
            cancellationToken);
        
        return HandleResult(result);
    }

    /// <summary>
    /// Unlock a user account (Admin only)
    /// </summary>
    [HttpPost("unlock-account/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UnlockAccount(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _securityManagementService.UnlockAccountAsync(userId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Force a user to change their password on next login (Admin only)
    /// </summary>
    [HttpPost("force-password-change/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ForcePasswordChange(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _securityManagementService.ForcePasswordChangeAsync(userId, cancellationToken);
        return HandleResult(result);
    }
}


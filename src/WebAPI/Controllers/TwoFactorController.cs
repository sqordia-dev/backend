using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.TwoFactor;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/2fa")]
public class TwoFactorController : BaseApiController
{
    private readonly ITwoFactorService _twoFactorService;

    public TwoFactorController(ITwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    /// <summary>
    /// Setup two-factor authentication (get QR code and secret)
    /// </summary>
    /// <returns>2FA setup information including QR code</returns>
    [HttpPost("setup")]
    [Authorize]
    public async Task<IActionResult> Setup()
    {
        var result = await _twoFactorService.SetupTwoFactorAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Enable two-factor authentication by verifying a code
    /// </summary>
    /// <param name="request">Verification code from authenticator app</param>
    /// <returns>Backup codes</returns>
    [HttpPost("enable")]
    [Authorize]
    public async Task<IActionResult> Enable([FromBody] EnableTwoFactorRequest request)
    {
        var result = await _twoFactorService.EnableTwoFactorAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    /// <returns>Success or failure</returns>
    [HttpPost("disable")]
    [Authorize]
    public async Task<IActionResult> Disable()
    {
        var result = await _twoFactorService.DisableTwoFactorAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Get two-factor authentication status
    /// </summary>
    /// <returns>2FA status</returns>
    [HttpGet("status")]
    [Authorize]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _twoFactorService.GetTwoFactorStatusAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Regenerate backup codes
    /// </summary>
    /// <returns>New backup codes</returns>
    [HttpPost("backup-codes/regenerate")]
    [Authorize]
    public async Task<IActionResult> RegenerateBackupCodes()
    {
        var result = await _twoFactorService.RegenerateBackupCodesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Verify a two-factor code
    /// </summary>
    /// <param name="request">Code to verify</param>
    /// <returns>Success or failure</returns>
    [HttpPost("verify")]
    [Authorize]
    public async Task<IActionResult> Verify([FromBody] VerifyTwoFactorRequest request)
    {
        var result = await _twoFactorService.VerifyTwoFactorCodeAsync(request);
        return HandleResult(result);
    }
}


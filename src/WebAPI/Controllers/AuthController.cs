using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Auth;
using WebAPI.Constants;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authenticationService.RegisterAsync(request);
        return HandleResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authenticationService.LoginAsync(request);
        return HandleResult(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authenticationService.RefreshTokenAsync(request);
        return HandleResult(result);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var result = await _authenticationService.RevokeTokenAsync(request);
        return HandleResult(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var result = await _authenticationService.LogoutAsync(request);
        return HandleResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _authenticationService.GetCurrentUserAsync(userId.Value);
        return HandleResult(result);
    }

    [HttpPost("send-verification-email")]
    [AllowAnonymous]
    public async Task<IActionResult> SendVerificationEmail([FromBody] SendEmailVerificationRequest request)
    {
        var result = await _authenticationService.SendEmailVerificationAsync(request);
        return HandleResult(result);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var result = await _authenticationService.VerifyEmailAsync(request);
        return HandleResult(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authenticationService.ForgotPasswordAsync(request);
        return HandleResult(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authenticationService.ResetPasswordAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticate with Google OAuth
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> AuthenticateWithGoogle([FromBody] GoogleAuthRequest request)
    {
        // Note: This is a simplified implementation. In production, validate the Google ID token.
        var googleId = "google_" + Guid.NewGuid().ToString("N")[..8];
        var email = "user@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var profilePictureUrl = "https://via.placeholder.com/150";

        var result = await _authenticationService.AuthenticateWithGoogleAsync(
            googleId, email, firstName, lastName, profilePictureUrl);
        
        return HandleResult(result);
    }

    /// <summary>
    /// Link Google account to existing user
    /// </summary>
    [HttpPost("google/link")]
    [Authorize]
    public async Task<IActionResult> LinkGoogleAccount([FromBody] LinkGoogleAccountRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Note: In production, extract from validated Google token
        var googleId = "google_" + Guid.NewGuid().ToString("N")[..8];
        var profilePictureUrl = "https://via.placeholder.com/150";

        var result = await _authenticationService.LinkGoogleAccountAsync(
            userId.Value, googleId, profilePictureUrl);
        
        return HandleResult(result);
    }

    /// <summary>
    /// Unlink Google account from user
    /// </summary>
    [HttpPost("google/unlink")]
    [Authorize]
    public async Task<IActionResult> UnlinkGoogleAccount()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _authenticationService.UnlinkGoogleAccountAsync(userId.Value);
        return HandleResult(result);
    }
}

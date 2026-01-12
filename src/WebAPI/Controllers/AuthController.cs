using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Auth;
using System.Text.Json;
using WebAPI.Configuration;
using WebAPI.Constants;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthenticationService _authenticationService;
    private readonly GoogleOAuthSettings _googleOAuthSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        IOptions<GoogleOAuthSettings> googleOAuthSettings,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _googleOAuthSettings = googleOAuthSettings.Value;
        _logger = logger;
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
        try
        {
            if (string.IsNullOrWhiteSpace(request.IdToken) && string.IsNullOrWhiteSpace(request.AccessToken))
            {
                return BadRequest(new { errorMessage = "Google ID token or access token is required" });
            }

            string googleId;
            string email;
            string firstName;
            string lastName;
            string? profilePictureUrl;

            // Try to validate as ID token first (preferred method)
            if (!string.IsNullOrWhiteSpace(request.IdToken))
            {
                GoogleJsonWebSignature.Payload? payload = null;
                try
                {
                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _googleOAuthSettings.ClientId }
                    };

                    payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                }
                catch (InvalidJwtException ex)
                {
                    _logger.LogError(ex, "Invalid Google ID token. ClientId: {ClientId}, Token length: {TokenLength}", 
                        _googleOAuthSettings.ClientId, 
                        request.IdToken?.Length ?? 0);
                    return BadRequest(new { 
                        errorMessage = "Invalid Google ID token", 
                        details = ex.Message 
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating Google ID token. ClientId: {ClientId}", 
                        _googleOAuthSettings.ClientId);
                    return BadRequest(new { 
                        errorMessage = "Failed to validate Google ID token", 
                        details = ex.Message 
                    });
                }

                if (payload != null)
                {
                    // Successfully validated ID token
                    googleId = payload.Subject ?? string.Empty;
                    email = payload.Email ?? string.Empty;
                    firstName = payload.GivenName ?? string.Empty;
                    lastName = payload.FamilyName ?? string.Empty;
                    profilePictureUrl = payload.Picture;

                    if (string.IsNullOrWhiteSpace(googleId) || string.IsNullOrWhiteSpace(email))
                    {
                        return BadRequest(new { errorMessage = "Invalid Google ID token: missing required fields" });
                    }

                    _logger.LogInformation("Validated Google ID token for user: {Email}, GoogleId: {GoogleId}", email, googleId);

                    var result = await _authenticationService.AuthenticateWithGoogleAsync(
                        googleId, email, firstName, lastName, profilePictureUrl);
                    
                    return HandleResult(result);
                }
            }

            // Fallback: Validate using access token
            if (!string.IsNullOrWhiteSpace(request.AccessToken))
            {
                try
                {
                    using var httpClient = new HttpClient();
                    var userInfoResponse = await httpClient.GetAsync(
                        $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={request.AccessToken}");

                    if (!userInfoResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to get user info from Google with access token. Status: {Status}", userInfoResponse.StatusCode);
                        return BadRequest(new { errorMessage = "Invalid Google access token" });
                    }

                    var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoJson);

                    googleId = userInfo.GetProperty("sub").GetString() ?? string.Empty;
                    email = userInfo.GetProperty("email").GetString() ?? string.Empty;
                    firstName = userInfo.TryGetProperty("given_name", out var givenName) ? givenName.GetString() ?? string.Empty : string.Empty;
                    lastName = userInfo.TryGetProperty("family_name", out var familyName) ? familyName.GetString() ?? string.Empty : string.Empty;
                    profilePictureUrl = userInfo.TryGetProperty("picture", out var picture) ? picture.GetString() : null;

                    if (string.IsNullOrWhiteSpace(googleId) || string.IsNullOrWhiteSpace(email))
                    {
                        return BadRequest(new { errorMessage = "Invalid user info from Google access token" });
                    }

                    _logger.LogInformation("Validated Google access token for user: {Email}, GoogleId: {GoogleId}", email, googleId);

                    var result = await _authenticationService.AuthenticateWithGoogleAsync(
                        googleId, email, firstName, lastName, profilePictureUrl);
                    
                    return HandleResult(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating Google access token");
                    return BadRequest(new { errorMessage = "Failed to validate Google access token" });
                }
            }

            return BadRequest(new { errorMessage = "Unable to validate Google token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return StatusCode(500, new { errorMessage = "An error occurred during Google authentication" });
        }
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

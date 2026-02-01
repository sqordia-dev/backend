using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthenticationService authenticationService,
        IOptions<GoogleOAuthSettings> googleOAuthSettings,
        ILogger<AuthController> logger,
        IConfiguration configuration)
    {
        _authenticationService = authenticationService;
        _googleOAuthSettings = googleOAuthSettings.Value;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.RegisterAsync(request);
        return HandleResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.LoginAsync(request);
        return HandleResult(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.RefreshTokenAsync(request);
        return HandleResult(result);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.RevokeTokenAsync(request);
        return HandleResult(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.LogoutAsync(request);
        return HandleResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
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
    public async Task<IActionResult> SendVerificationEmail([FromBody] SendEmailVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.SendEmailVerificationAsync(request);
        return HandleResult(result);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.VerifyEmailAsync(request);
        return HandleResult(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.ForgotPasswordAsync(request);
        return HandleResult(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authenticationService.ResetPasswordAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticate with Google OAuth
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> AuthenticateWithGoogle([FromBody] GoogleAuthRequest request, CancellationToken cancellationToken = default)
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
                    firstName = !string.IsNullOrWhiteSpace(payload.GivenName) ? payload.GivenName : email.Split('@')[0];
                    lastName = !string.IsNullOrWhiteSpace(payload.FamilyName) ? payload.FamilyName : "User";
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
                    var givenNameStr = userInfo.TryGetProperty("given_name", out var givenName) ? givenName.GetString() : null;
                    var familyNameStr = userInfo.TryGetProperty("family_name", out var familyName) ? familyName.GetString() : null;
                    firstName = !string.IsNullOrWhiteSpace(givenNameStr) ? givenNameStr : email.Split('@')[0];
                    lastName = !string.IsNullOrWhiteSpace(familyNameStr) ? familyNameStr : "User";
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
    /// Google OAuth callback endpoint (handles redirect from Google)
    /// </summary>
    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(CancellationToken cancellationToken = default)
    {
        try
        {
            // This endpoint is called by Google OAuth middleware after authentication
            // The middleware handles the OAuth flow and sets up the user claims
            // For API-based OAuth, we typically use the /google endpoint with tokens instead
            // This callback is mainly for web-based OAuth flows
            
            _logger.LogInformation("Google OAuth callback received");
            
            // Redirect to frontend with success message
            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") 
                           ?? _configuration["Frontend:BaseUrl"]
                           ?? "https://sqordia.app";
            
            return Redirect($"{frontendUrl}/auth/google/callback?success=true");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Google OAuth callback");
            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") 
                           ?? _configuration["Frontend:BaseUrl"]
                           ?? "https://sqordia.app";
            return Redirect($"{frontendUrl}/auth/google/callback?success=false&error=callback_failed");
        }
    }

    /// <summary>
    /// Link Google account to existing user
    /// </summary>
    [HttpPost("google/link")]
    [Authorize]
    public async Task<IActionResult> LinkGoogleAccount([FromBody] LinkGoogleAccountRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            if (string.IsNullOrWhiteSpace(request.IdToken) && string.IsNullOrWhiteSpace(request.AccessToken))
            {
                return BadRequest(new { errorMessage = "Google ID token or access token is required" });
            }

            string googleId;
            string? profilePictureUrl = null;

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
                    _logger.LogError(ex, "Invalid Google ID token in link request");
                    return BadRequest(new { 
                        errorMessage = "Invalid Google ID token", 
                        details = ex.Message 
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating Google ID token in link request");
                    return BadRequest(new { 
                        errorMessage = "Failed to validate Google ID token", 
                        details = ex.Message 
                    });
                }

                if (payload != null)
                {
                    googleId = payload.Subject ?? string.Empty;
                    profilePictureUrl = payload.Picture;

                    if (string.IsNullOrWhiteSpace(googleId))
                    {
                        return BadRequest(new { errorMessage = "Invalid Google ID token: missing Google ID" });
                    }

                    _logger.LogInformation("Validated Google ID token for linking. GoogleId: {GoogleId}", googleId);
                }
                else
                {
                    return BadRequest(new { errorMessage = "Unable to validate Google ID token" });
                }
            }
            // Fallback: Validate using access token
            else if (!string.IsNullOrWhiteSpace(request.AccessToken))
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
                    profilePictureUrl = userInfo.TryGetProperty("picture", out var picture) ? picture.GetString() : null;

                    if (string.IsNullOrWhiteSpace(googleId))
                    {
                        return BadRequest(new { errorMessage = "Invalid user info from Google access token" });
                    }

                    _logger.LogInformation("Validated Google access token for linking. GoogleId: {GoogleId}", googleId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating Google access token in link request");
                    return BadRequest(new { errorMessage = "Failed to validate Google access token" });
                }
            }
            else
            {
                return BadRequest(new { errorMessage = "Google ID token or access token is required" });
            }

            var result = await _authenticationService.LinkGoogleAccountAsync(
                userId.Value, googleId, profilePictureUrl);
            
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google account linking");
            return StatusCode(500, new { errorMessage = "An error occurred during Google account linking" });
        }
    }

    /// <summary>
    /// Unlink Google account from user
    /// </summary>
    [HttpPost("google/unlink")]
    [Authorize]
    public async Task<IActionResult> UnlinkGoogleAccount(CancellationToken cancellationToken = default)
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

using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sqordia.Application.Services;
using Sqordia.Application.Services.Implementations;
using Sqordia.Contracts.Requests.Auth;
using System.Net.Http.Headers;
using System.Text.Json;
using WebAPI.Configuration;

namespace WebAPI.Controllers;

/// <summary>
/// OAuth authentication controller for Google and Microsoft login
/// Note: Use /api/v1/auth/google for Google login (handled by AuthController)
/// This controller provides alternative OAuth endpoints under /oauth
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/oauth")]
public class OAuthController : BaseApiController
{
    private readonly IOAuthService _oAuthService;
    private readonly GoogleOAuthSettings _googleOAuthSettings;
    private readonly ILogger<OAuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public OAuthController(
        IOAuthService oAuthService,
        IOptions<GoogleOAuthSettings> googleOAuthSettings,
        ILogger<OAuthController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _oAuthService = oAuthService;
        _googleOAuthSettings = googleOAuthSettings.Value;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Authenticate with Google OAuth - validates token, creates/links user, returns JWT
    /// </summary>
    /// <param name="request">Google login request containing the ID token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token and user info</returns>
    /// <remarks>
    /// This endpoint validates a Google ID token, creates a new user or links an existing one,
    /// and returns a JWT access token for authentication.
    ///
    /// Sample request:
    ///     POST /api/v1/auth/google
    ///     {
    ///         "idToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6..."
    ///     }
    ///
    /// Sample response:
    ///     {
    ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    ///         "refreshToken": "abc123...",
    ///         "expiresAt": "2025-01-24T12:00:00Z",
    ///         "user": {
    ///             "id": "...",
    ///             "firstName": "John",
    ///             "lastName": "Doe",
    ///             "email": "john@gmail.com",
    ///             "userName": "john@gmail.com",
    ///             "roles": ["User"]
    ///         }
    ///     }
    /// </remarks>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid Google ID token</response>
    /// <response code="409">Email already linked to different account</response>
    [HttpPost("google")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AuthenticateWithGoogle(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                return BadRequest(new { errorMessage = "Google ID token is required" });
            }

            // Validate Google ID token
            GoogleJsonWebSignature.Payload payload;
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
                _logger.LogError(ex, "Invalid Google ID token. ClientId: {ClientId}", _googleOAuthSettings.ClientId);
                return BadRequest(new { errorMessage = "Invalid Google ID token", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google ID token. ClientId: {ClientId}", _googleOAuthSettings.ClientId);
                return BadRequest(new { errorMessage = "Failed to validate Google ID token", details = ex.Message });
            }

            var googleId = payload.Subject ?? string.Empty;
            var email = payload.Email ?? string.Empty;
            var firstName = payload.GivenName ?? string.Empty;
            var lastName = payload.FamilyName ?? string.Empty;
            var profilePictureUrl = payload.Picture;

            if (string.IsNullOrWhiteSpace(googleId) || string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { errorMessage = "Invalid Google ID token: missing required fields" });
            }

            _logger.LogInformation("Validated Google ID token for user: {Email}, GoogleId: {GoogleId}", email, googleId);

            // Use the OAuthService to authenticate the user
            if (_oAuthService is OAuthService oAuthServiceImpl)
            {
                var result = await oAuthServiceImpl.AuthenticateOAuthUserAsync(
                    "google", googleId, email, firstName, lastName, profilePictureUrl, cancellationToken);
                return HandleResult(result);
            }

            return BadRequest(new { errorMessage = "OAuth service not properly configured" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication");
            return StatusCode(500, new { errorMessage = "An error occurred during Google authentication" });
        }
    }

    /// <summary>
    /// Authenticate with Microsoft OAuth - validates token, creates/links user, returns JWT
    /// </summary>
    /// <param name="request">Microsoft login request containing the access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token and user info</returns>
    /// <remarks>
    /// This endpoint validates a Microsoft access token by calling Microsoft Graph API,
    /// creates a new user or links an existing one, and returns a JWT access token.
    ///
    /// Sample request:
    ///     POST /api/v1/auth/microsoft
    ///     {
    ///         "accessToken": "eyJ0eXAiOiJKV1QiLCJub25jZSI6..."
    ///     }
    ///
    /// Sample response:
    ///     {
    ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    ///         "refreshToken": "abc123...",
    ///         "expiresAt": "2025-01-24T12:00:00Z",
    ///         "user": {
    ///             "id": "...",
    ///             "firstName": "John",
    ///             "lastName": "Doe",
    ///             "email": "john@outlook.com",
    ///             "userName": "john@outlook.com",
    ///             "roles": ["User"]
    ///         }
    ///     }
    /// </remarks>
    /// <response code="200">Authentication successful</response>
    /// <response code="400">Invalid Microsoft access token</response>
    /// <response code="409">Email already linked to different account</response>
    [HttpPost("microsoft")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AuthenticateWithMicrosoft(
        [FromBody] MicrosoftLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken))
            {
                return BadRequest(new { errorMessage = "Microsoft access token is required" });
            }

            // Validate Microsoft access token by calling Microsoft Graph API
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);

            var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to validate Microsoft access token. Status: {Status}", response.StatusCode);
                return BadRequest(new { errorMessage = "Invalid Microsoft access token" });
            }

            var userInfoJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoJson);

            var microsoftId = userInfo.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;
            var email = userInfo.TryGetProperty("mail", out var mailProp) ? mailProp.GetString() ?? string.Empty : string.Empty;

            // If mail is empty, try userPrincipalName
            if (string.IsNullOrWhiteSpace(email))
            {
                email = userInfo.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() ?? string.Empty : string.Empty;
            }

            var firstName = userInfo.TryGetProperty("givenName", out var givenNameProp) ? givenNameProp.GetString() ?? string.Empty : string.Empty;
            var lastName = userInfo.TryGetProperty("surname", out var surnameProp) ? surnameProp.GetString() ?? string.Empty : string.Empty;
            var displayName = userInfo.TryGetProperty("displayName", out var displayNameProp) ? displayNameProp.GetString() : null;

            // If firstName is empty but displayName exists, try to split it
            if (string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(displayName))
            {
                var parts = displayName.Split(' ', 2);
                firstName = parts[0];
                lastName = parts.Length > 1 ? parts[1] : string.Empty;
            }

            if (string.IsNullOrWhiteSpace(microsoftId) || string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { errorMessage = "Invalid Microsoft token: missing required user information" });
            }

            _logger.LogInformation("Validated Microsoft access token for user: {Email}, MicrosoftId: {MicrosoftId}", email, microsoftId);

            // Use the OAuthService to authenticate the user
            if (_oAuthService is OAuthService oAuthServiceImpl)
            {
                var result = await oAuthServiceImpl.AuthenticateOAuthUserAsync(
                    "microsoft", microsoftId, email, firstName, lastName, null, cancellationToken);
                return HandleResult(result);
            }

            return BadRequest(new { errorMessage = "OAuth service not properly configured" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Microsoft authentication");
            return StatusCode(500, new { errorMessage = "An error occurred during Microsoft authentication" });
        }
    }
}

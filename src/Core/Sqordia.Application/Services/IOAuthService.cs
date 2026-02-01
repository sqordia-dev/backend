using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Auth;
using Sqordia.Contracts.Responses.Auth;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for handling OAuth authentication with external providers
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Authenticate with Google OAuth - validates token, creates/links user, returns JWT
    /// </summary>
    /// <param name="request">Google login request containing the ID token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(
        GoogleLoginRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticate with Microsoft OAuth - validates token, creates/links user, returns JWT
    /// </summary>
    /// <param name="request">Microsoft login request containing the access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with JWT token</returns>
    Task<Result<AuthResponse>> AuthenticateWithMicrosoftAsync(
        MicrosoftLoginRequest request,
        CancellationToken cancellationToken = default);
}

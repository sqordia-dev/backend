using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Auth;

/// <summary>
/// Request for Google OAuth authentication
/// </summary>
public class GoogleAuthRequest
{
    /// <summary>
    /// Google ID token from OAuth flow
    /// </summary>
    [Required]
    public string IdToken { get; set; } = string.Empty;

    /// <summary>
    /// Google access token from OAuth flow
    /// </summary>
    [Required]
    public string AccessToken { get; set; } = string.Empty;
}

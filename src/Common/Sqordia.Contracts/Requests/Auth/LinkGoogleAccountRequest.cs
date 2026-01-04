using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Auth;

/// <summary>
/// Request to link a Google account to an existing user
/// </summary>
public class LinkGoogleAccountRequest
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

using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Auth;

/// <summary>
/// Request for Google OAuth login
/// </summary>
public class GoogleLoginRequest
{
    /// <summary>
    /// Google ID token from OAuth flow
    /// </summary>
    [Required]
    public string IdToken { get; set; } = string.Empty;
}

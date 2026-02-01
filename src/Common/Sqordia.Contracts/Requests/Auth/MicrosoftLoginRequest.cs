using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Auth;

/// <summary>
/// Request for Microsoft OAuth login
/// </summary>
public class MicrosoftLoginRequest
{
    /// <summary>
    /// Microsoft access token from OAuth flow
    /// </summary>
    [Required]
    public string AccessToken { get; set; } = string.Empty;
}

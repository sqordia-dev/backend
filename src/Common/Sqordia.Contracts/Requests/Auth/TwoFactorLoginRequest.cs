using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Auth;

/// <summary>
/// Request to complete login with a 2FA code after initial credential validation.
/// </summary>
public class TwoFactorLoginRequest
{
    [Required]
    public string TwoFactorToken { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

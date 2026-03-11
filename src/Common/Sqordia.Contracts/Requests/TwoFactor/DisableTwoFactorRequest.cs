using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.TwoFactor;

/// <summary>
/// Request to disable 2FA. Requires a valid TOTP code or backup code for security.
/// </summary>
public class DisableTwoFactorRequest
{
    [Required]
    [StringLength(10, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

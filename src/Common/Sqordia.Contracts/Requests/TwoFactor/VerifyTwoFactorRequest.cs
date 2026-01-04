using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.TwoFactor;

public class VerifyTwoFactorRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}


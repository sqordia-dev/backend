using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.TwoFactor;

public class EnableTwoFactorRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string VerificationCode { get; set; } = string.Empty;
}


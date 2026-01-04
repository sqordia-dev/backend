using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Auth;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
}

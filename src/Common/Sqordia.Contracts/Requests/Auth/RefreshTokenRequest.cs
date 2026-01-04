using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Auth;

public class RefreshTokenRequest
{
    [Required]
    public required string Token { get; set; }

    [Required]
    public required string RefreshToken { get; set; }
}

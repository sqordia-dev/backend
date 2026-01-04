using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Auth;

/// <summary>
/// User login request model
/// </summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }
}

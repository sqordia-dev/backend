using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Auth;

/// <summary>
/// User registration request model
/// </summary>
public class RegisterRequest
{
    [Required]
    [MinLength(2)]
    public required string FirstName { get; set; }

    [Required]
    [MinLength(2)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }

    [Required]
    [Compare("Password")]
    public required string ConfirmPassword { get; set; }

    [Required]
    [MinLength(3)]
    public required string UserName { get; set; }

    [Required]
    public required string UserType { get; set; } // "Entrepreneur", "OBNL", or "Consultant"
}

using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.User;

public class UpdateProfileRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(50, MinimumLength = 3)]
    public string? UserName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [Url]
    public string? ProfilePictureUrl { get; set; }
}


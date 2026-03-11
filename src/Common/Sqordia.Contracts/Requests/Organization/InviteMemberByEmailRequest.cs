using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Organization;

public class InviteMemberByEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Member";
}

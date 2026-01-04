using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Organization;

public class UpdateMemberRoleRequest
{
    [Required]
    public required string Role { get; set; } // "Owner", "Admin", "Member", "Viewer"
}


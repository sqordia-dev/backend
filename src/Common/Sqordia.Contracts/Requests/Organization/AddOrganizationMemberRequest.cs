using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Organization;

public class AddOrganizationMemberRequest
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public required string Role { get; set; } // "Owner", "Admin", "Member", "Viewer"
}


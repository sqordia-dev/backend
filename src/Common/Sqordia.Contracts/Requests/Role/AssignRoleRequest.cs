using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Role;

public class AssignRoleRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid RoleId { get; set; }
}


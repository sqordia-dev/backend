using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Role;

public class CreateRoleRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;

    public List<Guid> PermissionIds { get; set; } = new();
}


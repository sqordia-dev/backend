using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Identity;

/// <summary>
/// Represents the many-to-many relationship between roles and permissions
/// </summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    // Navigation properties
    public Role Role { get; private set; }
    public Permission Permission { get; private set; }

    private RolePermission() 
    { 
        Role = null!; // EF Core constructor
        Permission = null!;
    }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        Role = null!; // Will be set by EF Core
        Permission = null!; // Will be set by EF Core
    }
}

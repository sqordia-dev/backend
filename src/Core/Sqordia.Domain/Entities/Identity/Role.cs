using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Identity;

/// <summary>
/// Role entity for user authorization
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsSystemRole { get; private set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Role() 
    { 
        Name = string.Empty; // EF Core constructor
        Description = string.Empty;
    }

    public Role(string name, string description, bool isSystemRole = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        IsSystemRole = isSystemRole;
    }

    public void Update(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
}

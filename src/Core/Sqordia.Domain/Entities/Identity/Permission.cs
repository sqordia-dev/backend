using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Identity;

/// <summary>
/// Represents a permission that can be assigned to roles
/// </summary>
public class Permission : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Permission() 
    { 
        Name = string.Empty; // EF Core constructor
        Description = string.Empty;
        Category = string.Empty;
    }

    public Permission(string name, string description, string category)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Category = category ?? throw new ArgumentNullException(nameof(category));
    }

    public void Update(string name, string description, string category)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Category = category ?? throw new ArgumentNullException(nameof(category));
    }
}

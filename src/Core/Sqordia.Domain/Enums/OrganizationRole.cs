namespace Sqordia.Domain.Enums;

/// <summary>
/// Defines the roles a user can have within an organization
/// </summary>
public enum OrganizationRole
{
    /// <summary>
    /// Organization owner with full administrative access
    /// </summary>
    Owner = 0,
    
    /// <summary>
    /// Administrator with management capabilities
    /// </summary>
    Admin = 1,
    
    /// <summary>
    /// Regular member with standard access
    /// </summary>
    Member = 2,
    
    /// <summary>
    /// Read-only viewer with limited access
    /// </summary>
    Viewer = 3
}


namespace Sqordia.Domain.Enums;

/// <summary>
/// Permission levels for sharing business plans
/// </summary>
public enum SharePermission
{
    /// <summary>
    /// Read-only access - can view but not edit
    /// </summary>
    ReadOnly = 0,
    
    /// <summary>
    /// Can view and edit the business plan
    /// </summary>
    Edit = 1,
    
    /// <summary>
    /// Full access including sharing and deletion
    /// </summary>
    FullAccess = 2
}


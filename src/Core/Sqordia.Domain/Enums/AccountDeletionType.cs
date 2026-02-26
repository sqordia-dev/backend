namespace Sqordia.Domain.Enums;

/// <summary>
/// Types of account deletion (Quebec Bill 25 compliance)
/// </summary>
public enum AccountDeletionType
{
    /// <summary>
    /// Soft delete - account is deactivated but can be reactivated within 30 days
    /// </summary>
    Deactivate = 0,

    /// <summary>
    /// Hard delete - all user data is permanently and irreversibly deleted
    /// </summary>
    Permanent = 1
}

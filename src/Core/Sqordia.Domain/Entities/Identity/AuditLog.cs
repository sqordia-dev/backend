using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Identity;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime Timestamp { get; private set; }
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? AdditionalData { get; private set; }

    // Navigation property
    public User? User { get; private set; }

    private AuditLog() 
    { 
        Action = string.Empty; // EF Core constructor
        EntityType = string.Empty;
    }

    public AuditLog(
        Guid? userId,
        string action,
        string entityType,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        bool success = true,
        string? errorMessage = null,
        string? additionalData = null)
    {
        UserId = userId;
        Action = action ?? throw new ArgumentNullException(nameof(action));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Timestamp = DateTime.UtcNow;
        Success = success;
        ErrorMessage = errorMessage;
        AdditionalData = additionalData;
    }
}

public static class AuditActions
{
    // Authentication actions
    public const string Login = "LOGIN";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string Logout = "LOGOUT";
    public const string Register = "REGISTER";
    public const string PasswordChange = "PASSWORD_CHANGE";
    public const string PasswordReset = "PASSWORD_RESET";
    public const string EmailVerification = "EMAIL_VERIFICATION";
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string AccountUnlocked = "ACCOUNT_UNLOCKED";
    public const string TokenRefresh = "TOKEN_REFRESH";
    public const string TokenRevoke = "TOKEN_REVOKE";

    // Account actions
    public const string AccountCreated = "ACCOUNT_CREATED";
    public const string AccountUpdated = "ACCOUNT_UPDATED";
    public const string AccountDeactivated = "ACCOUNT_DEACTIVATED";
    public const string AccountActivated = "ACCOUNT_ACTIVATED";

    // Permission actions
    public const string RoleAssigned = "ROLE_ASSIGNED";
    public const string RoleRemoved = "ROLE_REMOVED";
    public const string PermissionGranted = "PERMISSION_GRANTED";
    public const string PermissionRevoked = "PERMISSION_REVOKED";

    // Data access
    public const string DataAccessed = "DATA_ACCESSED";
    public const string DataCreated = "DATA_CREATED";
    public const string DataUpdated = "DATA_UPDATED";
    public const string DataDeleted = "DATA_DELETED";
}
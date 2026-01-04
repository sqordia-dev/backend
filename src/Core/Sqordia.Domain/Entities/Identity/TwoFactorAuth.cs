using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Identity;

public class TwoFactorAuth : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public string SecretKey { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime? EnabledAt { get; private set; }
    public string? BackupCodes { get; private set; } // JSON array of backup codes
    public int FailedAttempts { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private TwoFactorAuth() 
    { 
        SecretKey = string.Empty; // EF Core constructor
    }

    public TwoFactorAuth(Guid userId, string secretKey)
    {
        UserId = userId;
        SecretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
        IsEnabled = false;
        FailedAttempts = 0;
    }

    public void Enable(string backupCodes)
    {
        IsEnabled = true;
        EnabledAt = DateTime.UtcNow;
        BackupCodes = backupCodes ?? throw new ArgumentNullException(nameof(backupCodes));
        FailedAttempts = 0;
    }

    public void Disable()
    {
        IsEnabled = false;
        EnabledAt = null;
        FailedAttempts = 0;
    }

    public void RecordFailedAttempt()
    {
        FailedAttempts++;
        LastAttemptAt = DateTime.UtcNow;
    }

    public void ResetFailedAttempts()
    {
        FailedAttempts = 0;
        LastAttemptAt = null;
    }

    public void RegenerateBackupCodes(string backupCodes)
    {
        BackupCodes = backupCodes ?? throw new ArgumentNullException(nameof(backupCodes));
    }

    public void UpdateSecretKey(string newSecretKey)
    {
        SecretKey = newSecretKey ?? throw new ArgumentNullException(nameof(newSecretKey));
        IsEnabled = false;
        EnabledAt = null;
        FailedAttempts = 0;
    }
}


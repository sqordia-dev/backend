using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Identity;

public class EmailVerificationToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public string? UsedByIp { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;
    public bool IsActive => !IsUsed && !IsExpired;

    // Navigation property
    public User User { get; private set; } = null!;

    private EmailVerificationToken() 
    { 
        Token = string.Empty; // EF Core constructor
    }

    public EmailVerificationToken(Guid userId, string token, DateTime expiresAt)
    {
        UserId = userId;
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        IsUsed = false;
    }

    public void MarkAsUsed(string? ipAddress = null)
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        UsedByIp = ipAddress;
    }

    public void Deactivate()
    {
        MarkAsUsed();
    }

    public static string GenerateToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}
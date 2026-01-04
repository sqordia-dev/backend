using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Identity;

/// <summary>
/// Refresh token entity for JWT token refresh functionality
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string CreatedByIp { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt == null && !IsExpired;

    // Navigation property
    public User User { get; private set; }

    private RefreshToken() 
    { 
        Token = string.Empty; // EF Core constructor
        CreatedByIp = string.Empty;
        User = null!;
    }

    public RefreshToken(Guid userId, string token, DateTime expiresAt, string createdByIp)
    {
        UserId = userId;
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = createdByIp ?? throw new ArgumentNullException(nameof(createdByIp));
        User = null!; // Will be set by EF Core
    }

    public void Revoke(string revokedByIp, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
    }
}

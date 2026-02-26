using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.Identity;

/// <summary>
/// Tracks user consent for terms of service, privacy policy, etc. (Quebec Bill 25 compliance)
/// </summary>
public class UserConsent : BaseAuditableEntity
{
    public Guid UserId { get; private set; }
    public ConsentType Type { get; private set; }
    public string Version { get; private set; } = null!;
    public bool IsAccepted { get; private set; }
    public DateTime AcceptedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    private UserConsent() { } // EF Core constructor

    public UserConsent(Guid userId, ConsentType type, string version, string? ipAddress = null, string? userAgent = null)
    {
        UserId = userId;
        Type = type;
        Version = version ?? throw new ArgumentNullException(nameof(version));
        IsAccepted = true;
        AcceptedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    /// <summary>
    /// Withdraw consent (user revokes their acceptance)
    /// </summary>
    public void Withdraw()
    {
        IsAccepted = false;
    }
}

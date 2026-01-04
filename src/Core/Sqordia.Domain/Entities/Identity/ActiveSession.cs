using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Identity;

public class ActiveSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public string SessionToken { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public string? DeviceType { get; private set; }
    public string? Browser { get; private set; }
    public string? OperatingSystem { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private ActiveSession() { } // EF Core constructor

    public ActiveSession(
        Guid userId,
        string sessionToken,
        DateTime expiresAt,
        string ipAddress,
        string? userAgent = null)
    {
        UserId = userId;
        SessionToken = sessionToken ?? throw new ArgumentNullException(nameof(sessionToken));
        CreatedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IsActive = true;
        IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
        UserAgent = userAgent;
    }

    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    public void SetDeviceInfo(string? deviceType, string? browser, string? operatingSystem)
    {
        DeviceType = deviceType;
        Browser = browser;
        OperatingSystem = operatingSystem;
    }

    public void SetLocationInfo(string? country, string? city)
    {
        Country = country;
        City = city;
    }

    public void Revoke(string? ipAddress = null)
    {
        IsActive = false;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}


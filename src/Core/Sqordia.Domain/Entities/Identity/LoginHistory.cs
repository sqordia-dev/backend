using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Identity;

public class LoginHistory : BaseEntity
{
    public Guid UserId { get; private set; }
    public DateTime LoginAttemptAt { get; private set; }
    public bool IsSuccessful { get; private set; }
    public string? FailureReason { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public string? DeviceType { get; private set; }
    public string? Browser { get; private set; }
    public string? OperatingSystem { get; private set; }
    public string? Country { get; private set; }
    public string? City { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private LoginHistory() { } // EF Core constructor

    public LoginHistory(
        Guid userId,
        bool isSuccessful,
        string ipAddress,
        string? userAgent = null,
        string? failureReason = null)
    {
        UserId = userId;
        LoginAttemptAt = DateTime.UtcNow;
        IsSuccessful = isSuccessful;
        FailureReason = failureReason;
        IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
        UserAgent = userAgent;
    }

    public void SetDeviceInfo(string? deviceType, string? browser, string? operatingSystem)
    {
        DeviceType = deviceType;
        Browser = browser;
        OperatingSystem = operatingSystem;
    }

    public void SetLocationInfo(string? country, string? city, double? latitude, double? longitude)
    {
        Country = country;
        City = city;
        Latitude = latitude;
        Longitude = longitude;
    }
}


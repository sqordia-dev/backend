namespace Sqordia.Contracts.Responses.Security;

public class LoginHistoryResponse
{
    public Guid Id { get; set; }
    public DateTime LoginAttemptAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}


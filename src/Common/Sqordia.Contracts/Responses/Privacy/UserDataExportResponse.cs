namespace Sqordia.Contracts.Responses.Privacy;

/// <summary>
/// Response containing exported user data (Quebec Bill 25 compliance - data portability)
/// </summary>
public class UserDataExportResponse
{
    public ExportMetadata Metadata { get; set; } = new();
    public ProfileData Profile { get; set; } = new();
    public List<ConsentRecord> Consents { get; set; } = new();
}

/// <summary>
/// Metadata about the data export
/// </summary>
public class ExportMetadata
{
    public DateTime ExportedAt { get; set; }
    public string ExportVersion { get; set; } = "1.0";
    public string RequestedBy { get; set; } = string.Empty;
}

/// <summary>
/// User profile data for export
/// </summary>
public class ProfileData
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Persona { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}

/// <summary>
/// Record of a consent acceptance
/// </summary>
public class ConsentRecord
{
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
    public DateTime AcceptedAt { get; set; }
}

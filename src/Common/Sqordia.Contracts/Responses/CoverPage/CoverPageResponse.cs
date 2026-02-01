namespace Sqordia.Contracts.Responses.CoverPage;

/// <summary>
/// Response containing cover page settings for a business plan
/// </summary>
public class CoverPageResponse
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }

    // Branding
    public string? LogoUrl { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string DocumentTitle { get; set; } = "Business Plan";
    public string PrimaryColor { get; set; } = "#2563EB";
    public string LayoutStyle { get; set; } = "classic";

    // Contact Information
    public string? ContactName { get; set; }
    public string? ContactTitle { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Website { get; set; }

    // Business Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // Metadata
    public DateTime PreparedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// JSON-serialized extended style settings (background, typography, decorations, etc.)
    /// </summary>
    public string? StyleSettingsJson { get; set; }
}

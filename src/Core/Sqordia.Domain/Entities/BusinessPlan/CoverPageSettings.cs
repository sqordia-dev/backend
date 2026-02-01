using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Cover page settings for a business plan
/// Contains branding, contact information, and styling options
/// </summary>
public class CoverPageSettings : BaseAuditableEntity
{
    public Guid BusinessPlanId { get; private set; }

    // Branding
    public string? LogoUrl { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public string DocumentTitle { get; private set; } = "Business Plan";
    public string PrimaryColor { get; private set; } = "#2563EB";
    public string LayoutStyle { get; private set; } = "classic"; // classic, modern, minimal, bold, creative, elegant

    /// <summary>
    /// JSON-serialized extended style settings (background, typography, decorations, etc.)
    /// This allows flexible storage of all enhanced styling options without schema changes
    /// </summary>
    public string? StyleSettingsJson { get; private set; }

    // Contact Information
    public string? ContactName { get; private set; }
    public string? ContactTitle { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? Website { get; private set; }

    // Business Address
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? StateProvince { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }

    // Metadata
    public DateTime PreparedDate { get; private set; } = DateTime.UtcNow;

    // Navigation
    public BusinessPlan BusinessPlan { get; private set; } = null!;

    private CoverPageSettings() { } // EF Core constructor

    public CoverPageSettings(Guid businessPlanId, string companyName)
    {
        BusinessPlanId = businessPlanId;
        CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
        PreparedDate = DateTime.UtcNow;
    }

    public void UpdateBranding(string companyName, string documentTitle, string primaryColor, string layoutStyle)
    {
        CompanyName = companyName ?? throw new ArgumentNullException(nameof(companyName));
        DocumentTitle = documentTitle ?? "Business Plan";
        PrimaryColor = primaryColor ?? "#2563EB";
        LayoutStyle = layoutStyle ?? "classic";
    }

    public void UpdateLogo(string? logoUrl)
    {
        LogoUrl = logoUrl;
    }

    public void UpdateContactInfo(string? name, string? title, string? phone, string? email, string? website)
    {
        ContactName = name;
        ContactTitle = title;
        ContactPhone = phone;
        ContactEmail = email;
        Website = website;
    }

    public void UpdateAddress(string? line1, string? line2, string? city, string? stateProvince, string? postalCode, string? country)
    {
        AddressLine1 = line1;
        AddressLine2 = line2;
        City = city;
        StateProvince = stateProvince;
        PostalCode = postalCode;
        Country = country;
    }

    public void UpdatePreparedDate(DateTime preparedDate)
    {
        PreparedDate = preparedDate;
    }

    public void UpdateStyleSettings(string? styleSettingsJson)
    {
        StyleSettingsJson = styleSettingsJson;
    }
}

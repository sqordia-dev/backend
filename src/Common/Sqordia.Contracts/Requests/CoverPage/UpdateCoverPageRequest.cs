using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.CoverPage;

/// <summary>
/// Request to update cover page settings for a business plan
/// </summary>
public class UpdateCoverPageRequest
{
    // Branding
    public string? LogoUrl { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string CompanyName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string DocumentTitle { get; set; }

    [Required]
    [StringLength(7, MinimumLength = 7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Primary color must be a valid hex color (e.g., #2563EB)")]
    public required string PrimaryColor { get; set; }

    [Required]
    [RegularExpression(@"^(classic|modern|minimal|bold|creative|elegant)$", ErrorMessage = "Layout style must be 'classic', 'modern', 'minimal', 'bold', 'creative', or 'elegant'")]
    public required string LayoutStyle { get; set; }

    // Contact Information
    [StringLength(100)]
    public string? ContactName { get; set; }

    [StringLength(100)]
    public string? ContactTitle { get; set; }

    [StringLength(30)]
    public string? ContactPhone { get; set; }

    [StringLength(200)]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    [StringLength(200)]
    public string? Website { get; set; }

    // Business Address
    [StringLength(200)]
    public string? AddressLine1 { get; set; }

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? StateProvince { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    // Metadata
    public DateTime? PreparedDate { get; set; }

    /// <summary>
    /// JSON-serialized extended style settings (background, typography, decorations, etc.)
    /// </summary>
    public string? StyleSettingsJson { get; set; }
}

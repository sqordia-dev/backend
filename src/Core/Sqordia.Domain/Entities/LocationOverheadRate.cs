using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Stores overhead rates by Canadian province/territory for consultant financial projections
/// </summary>
public class LocationOverheadRate : BaseEntity
{
    public string Province { get; set; } = string.Empty;        // e.g., "Ontario", "Quebec"
    public string ProvinceCode { get; set; } = string.Empty;    // e.g., "ON", "QC"
    public decimal OverheadRate { get; set; }                   // Percentage (e.g., 12.0 for 12%)
    public decimal InsuranceRate { get; set; }                  // Monthly amount in CAD
    public decimal TaxRate { get; set; }                        // Percentage (e.g., 20.0 for 20%)
    public decimal OfficeCost { get; set; }                     // Monthly amount in CAD
    public string Currency { get; set; } = "CAD";
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

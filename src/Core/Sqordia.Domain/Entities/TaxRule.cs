using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class TaxRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty; // Income, VAT, GST, Corporate, etc.
    public decimal Rate { get; set; } // Tax rate as percentage
    public decimal MinAmount { get; set; } // Minimum amount for tax to apply
    public decimal MaxAmount { get; set; } // Maximum amount for tax to apply
    public bool IsPercentage { get; set; } // True for percentage, false for fixed amount
    public string CalculationMethod { get; set; } = string.Empty; // Progressive, Flat, etc.
    public string ApplicableTo { get; set; } = string.Empty; // Business type, revenue range, etc.
    public bool IsActive { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string LegalReference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Currency? Currency { get; set; }
    public List<TaxCalculation> TaxCalculations { get; set; } = new();
}

using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class TaxCalculation : BaseEntity
{
    public Guid FinancialProjectionId { get; set; }
    public Guid TaxRuleId { get; set; }
    public string TaxName { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CalculationMethod { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime TaxPeriod { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public FinancialProjectionItem FinancialProjection { get; set; } = null!;
    public TaxRule TaxRule { get; set; } = null!;
}

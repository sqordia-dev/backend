namespace Sqordia.Application.Financial.Queries;

public class TaxCalculationDto
{
    public Guid Id { get; set; }
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
}

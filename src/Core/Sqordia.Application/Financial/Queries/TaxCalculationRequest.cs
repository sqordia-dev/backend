namespace Sqordia.Application.Financial.Queries;

public class TaxCalculationRequest
{
    public Guid FinancialProjectionId { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime TaxPeriod { get; set; }
    public string BusinessType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

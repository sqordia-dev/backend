using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty; // USD, EUR, GBP, etc.
    public string Name { get; set; } = string.Empty; // US Dollar, Euro, etc.
    public string Symbol { get; set; } = string.Empty; // $, €, £, etc.
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DecimalPlaces { get; set; }
    public decimal ExchangeRate { get; set; } // Base rate (usually USD = 1)
    public DateTime LastUpdated { get; set; }
    public string Source { get; set; } = string.Empty; // API source for rates
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public List<FinancialProjectionItem> FinancialProjectionItems { get; set; } = new();
    public List<ExchangeRate> FromExchangeRates { get; set; } = new();
    public List<ExchangeRate> ToExchangeRates { get; set; } = new();
    public List<TaxRule> TaxRules { get; set; } = new();
}

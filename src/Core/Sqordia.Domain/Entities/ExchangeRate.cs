using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class ExchangeRate : BaseEntity
{
    public Guid FromCurrencyId { get; set; }
    public Guid ToCurrencyId { get; set; }
    public decimal Rate { get; set; }
    public decimal InverseRate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Source { get; set; } = string.Empty; // API source
    public bool IsActive { get; set; }
    public string Provider { get; set; } = string.Empty; // Exchange rate provider
    public decimal Spread { get; set; } // Bid-ask spread
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Currency FromCurrency { get; set; } = null!;
    public Currency ToCurrency { get; set; } = null!;
}

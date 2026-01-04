namespace Sqordia.Application.Financial.Queries;

public class ExchangeRateDto
{
    public Guid Id { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal InverseRate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Provider { get; set; } = string.Empty;
    public decimal Spread { get; set; }
}

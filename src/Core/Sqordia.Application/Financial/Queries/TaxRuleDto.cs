namespace Sqordia.Application.Financial.Queries;

public class TaxRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public bool IsPercentage { get; set; }
    public string CalculationMethod { get; set; } = string.Empty;
    public string ApplicableTo { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string LegalReference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

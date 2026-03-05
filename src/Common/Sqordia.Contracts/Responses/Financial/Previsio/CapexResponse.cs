namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class CapexAssetResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string AssetType { get; set; } = null!;
    public decimal PurchaseValue { get; set; }
    public int PurchaseMonth { get; set; }
    public int PurchaseYear { get; set; }
    public string DepreciationMethod { get; set; } = null!;
    public int UsefulLifeYears { get; set; }
    public decimal SalvageValue { get; set; }
    public int SortOrder { get; set; }
    public decimal AnnualDepreciation { get; set; }
}

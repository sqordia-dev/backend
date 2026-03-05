namespace Sqordia.Contracts.Requests.Financial.Previsio;

public class CreateCapexAssetRequest
{
    public string Name { get; set; } = null!;
    public string AssetType { get; set; } = null!;
    public decimal PurchaseValue { get; set; }
    public int PurchaseMonth { get; set; }
    public int PurchaseYear { get; set; }
}

public class UpdateCapexAssetRequest
{
    public string Name { get; set; } = null!;
    public string AssetType { get; set; } = null!;
    public decimal PurchaseValue { get; set; }
    public int PurchaseMonth { get; set; }
    public int PurchaseYear { get; set; }
    public string DepreciationMethod { get; set; } = null!;
    public int UsefulLifeYears { get; set; }
    public decimal SalvageValue { get; set; }
}

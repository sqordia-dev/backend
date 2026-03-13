namespace Sqordia.Contracts.Requests.Financial.Previsio;

public class CreateFinancialPlanRequest
{
    public int StartYear { get; set; }
    public int ProjectionYears { get; set; } = 3;
}

public class UpdateFinancialPlanSettingsRequest
{
    public int ProjectionYears { get; set; }
    public decimal DefaultVolumeGrowthRate { get; set; }
    public decimal DefaultPriceIndexationRate { get; set; }
    public decimal DefaultExpenseIndexationRate { get; set; }
    public decimal DefaultSocialChargeRate { get; set; }
    public decimal DefaultSalesTaxRate { get; set; }
    public int? StartMonth { get; set; }
    public string? SalesTaxFrequency { get; set; }
    public bool? IsAlreadyOperating { get; set; }
}

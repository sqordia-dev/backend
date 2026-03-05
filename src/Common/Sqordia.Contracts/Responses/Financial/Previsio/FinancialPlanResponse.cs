namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class FinancialPlanResponse
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public int ProjectionYears { get; set; }
    public int StartYear { get; set; }
    public decimal DefaultVolumeGrowthRate { get; set; }
    public decimal DefaultPriceIndexationRate { get; set; }
    public decimal DefaultExpenseIndexationRate { get; set; }
    public decimal DefaultSocialChargeRate { get; set; }
    public decimal DefaultSalesTaxRate { get; set; }
    public int SalesProductCount { get; set; }
    public int PayrollItemCount { get; set; }
    public int FinancingSourceCount { get; set; }
}

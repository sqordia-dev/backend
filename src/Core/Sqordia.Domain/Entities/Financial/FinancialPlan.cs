using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// Aggregate root for Previsio financial projections. 1:1 with BusinessPlan.
/// </summary>
public class FinancialPlan : BaseAuditableEntity
{
    public Guid BusinessPlanId { get; private set; }
    public int ProjectionYears { get; private set; } = 3;
    public int StartYear { get; private set; }
    public int StartMonth { get; private set; } = 1; // 1-12
    public string? SalesTaxFrequency { get; private set; } // none, monthly, quarterly, annually
    public bool IsAlreadyOperating { get; private set; }

    // Default rates (Quebec defaults)
    public decimal DefaultVolumeGrowthRate { get; private set; } = 5.0m;
    public decimal DefaultPriceIndexationRate { get; private set; } = 2.0m;
    public decimal DefaultExpenseIndexationRate { get; private set; } = 2.0m;
    public decimal DefaultSocialChargeRate { get; private set; } = 15.0m;
    public decimal DefaultSalesTaxRate { get; private set; } = 14.98m;

    // Navigation properties
    public BusinessPlan.BusinessPlan BusinessPlan { get; private set; } = null!;
    public ICollection<SalesProduct> SalesProducts { get; private set; } = new List<SalesProduct>();
    public ICollection<CostOfGoodsSoldItem> CostOfGoodsSoldItems { get; private set; } = new List<CostOfGoodsSoldItem>();
    public ICollection<PayrollItem> PayrollItems { get; private set; } = new List<PayrollItem>();
    public ICollection<SalesExpenseItem> SalesExpenseItems { get; private set; } = new List<SalesExpenseItem>();
    public ICollection<AdminExpenseItem> AdminExpenseItems { get; private set; } = new List<AdminExpenseItem>();
    public ICollection<CapexAsset> CapexAssets { get; private set; } = new List<CapexAsset>();
    public ICollection<FinancingSource> FinancingSources { get; private set; } = new List<FinancingSource>();

    private FinancialPlan() { }

    public FinancialPlan(Guid businessPlanId, int startYear, int projectionYears = 3)
    {
        BusinessPlanId = businessPlanId;
        StartYear = startYear;
        ProjectionYears = projectionYears;
    }

    public void UpdateSettings(
        int projectionYears,
        decimal volumeGrowthRate,
        decimal priceIndexationRate,
        decimal expenseIndexationRate,
        decimal socialChargeRate,
        decimal salesTaxRate,
        int? startMonth = null,
        string? salesTaxFrequency = null,
        bool? isAlreadyOperating = null)
    {
        ProjectionYears = projectionYears;
        DefaultVolumeGrowthRate = volumeGrowthRate;
        DefaultPriceIndexationRate = priceIndexationRate;
        DefaultExpenseIndexationRate = expenseIndexationRate;
        DefaultSocialChargeRate = socialChargeRate;
        DefaultSalesTaxRate = salesTaxRate;
        if (startMonth.HasValue) StartMonth = Math.Clamp(startMonth.Value, 1, 12);
        if (salesTaxFrequency != null) SalesTaxFrequency = salesTaxFrequency;
        if (isAlreadyOperating.HasValue) IsAlreadyOperating = isAlreadyOperating.Value;
    }
}

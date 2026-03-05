using Sqordia.Domain.Common;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// Capital expenditure asset with depreciation tracking.
/// </summary>
public class CapexAsset : BaseAuditableEntity
{
    public Guid FinancialPlanId { get; private set; }
    public string Name { get; private set; } = null!;
    public AssetType AssetType { get; private set; }
    public decimal PurchaseValue { get; private set; }
    public int PurchaseMonth { get; private set; }
    public int PurchaseYear { get; private set; } // Projection year (0=pre-opening, 1-3)
    public DepreciationMethod DepreciationMethod { get; private set; } = DepreciationMethod.StraightLine;
    public int UsefulLifeYears { get; private set; }
    public decimal SalvageValue { get; private set; }
    public int SortOrder { get; private set; }

    // Navigation
    public FinancialPlan FinancialPlan { get; private set; } = null!;

    private CapexAsset() { }

    public CapexAsset(
        Guid financialPlanId,
        string name,
        AssetType assetType,
        decimal purchaseValue,
        int purchaseMonth,
        int purchaseYear,
        int sortOrder = 0)
    {
        FinancialPlanId = financialPlanId;
        Name = name;
        AssetType = assetType;
        PurchaseValue = purchaseValue;
        PurchaseMonth = purchaseMonth;
        PurchaseYear = purchaseYear;
        UsefulLifeYears = GetDefaultUsefulLife(assetType);
        SortOrder = sortOrder;
    }

    public void Update(
        string name,
        AssetType assetType,
        decimal purchaseValue,
        int purchaseMonth,
        int purchaseYear,
        DepreciationMethod depreciationMethod,
        int usefulLifeYears,
        decimal salvageValue)
    {
        Name = name;
        AssetType = assetType;
        PurchaseValue = purchaseValue;
        PurchaseMonth = purchaseMonth;
        PurchaseYear = purchaseYear;
        DepreciationMethod = depreciationMethod;
        UsefulLifeYears = usefulLifeYears;
        SalvageValue = salvageValue;
    }

    /// <summary>
    /// Returns the default useful life in years based on asset type (Quebec CCA rules).
    /// </summary>
    public static int GetDefaultUsefulLife(AssetType assetType)
    {
        return assetType switch
        {
            AssetType.IT => 3,
            AssetType.Furniture => 5,
            AssetType.Equipment => 5,
            AssetType.Vehicle => 5,
            AssetType.LeaseholdImprovements => 10,
            _ => 5
        };
    }
}

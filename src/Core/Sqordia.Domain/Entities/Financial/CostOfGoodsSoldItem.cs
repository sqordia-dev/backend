using Sqordia.Domain.Common;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// COGS item linked to a sales product. Tracks cost per unit and beginning inventory.
/// </summary>
public class CostOfGoodsSoldItem : BaseAuditableEntity
{
    public Guid FinancialPlanId { get; private set; }
    public Guid LinkedSalesProductId { get; private set; }
    public CostMode CostMode { get; private set; }
    public decimal CostValue { get; private set; } // $ per unit or % of price
    public decimal BeginningInventory { get; private set; }
    public decimal CostIndexationRate { get; private set; }

    // Navigation
    public FinancialPlan FinancialPlan { get; private set; } = null!;
    public SalesProduct LinkedSalesProduct { get; private set; } = null!;

    private CostOfGoodsSoldItem() { }

    public CostOfGoodsSoldItem(
        Guid financialPlanId,
        Guid linkedSalesProductId,
        CostMode costMode,
        decimal costValue,
        decimal beginningInventory = 0)
    {
        FinancialPlanId = financialPlanId;
        LinkedSalesProductId = linkedSalesProductId;
        CostMode = costMode;
        CostValue = costValue;
        BeginningInventory = beginningInventory;
    }

    public void Update(CostMode costMode, decimal costValue, decimal beginningInventory, decimal costIndexationRate)
    {
        CostMode = costMode;
        CostValue = costValue;
        BeginningInventory = beginningInventory;
        CostIndexationRate = costIndexationRate;
    }
}

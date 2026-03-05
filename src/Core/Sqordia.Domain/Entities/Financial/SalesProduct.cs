using Sqordia.Domain.Common;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// A product or service with pricing and payment terms for revenue projection.
/// </summary>
public class SalesProduct : BaseAuditableEntity
{
    public Guid FinancialPlanId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public PaymentDelay PaymentDelay { get; private set; }
    public decimal TaxRate { get; private set; }
    public SalesInputMode InputMode { get; private set; } = SalesInputMode.Quantity;
    public decimal VolumeIndexationRate { get; private set; }
    public decimal PriceIndexationRate { get; private set; }
    public int SortOrder { get; private set; }

    // Navigation properties
    public FinancialPlan FinancialPlan { get; private set; } = null!;
    public ICollection<SalesVolume> SalesVolumes { get; private set; } = new List<SalesVolume>();
    public CostOfGoodsSoldItem? CostOfGoodsSoldItem { get; private set; }

    private SalesProduct() { }

    public SalesProduct(
        Guid financialPlanId,
        string name,
        decimal unitPrice,
        PaymentDelay paymentDelay,
        decimal taxRate,
        SalesInputMode inputMode,
        int sortOrder = 0)
    {
        FinancialPlanId = financialPlanId;
        Name = name;
        UnitPrice = unitPrice;
        PaymentDelay = paymentDelay;
        TaxRate = taxRate;
        InputMode = inputMode;
        SortOrder = sortOrder;
    }

    public void Update(
        string name,
        decimal unitPrice,
        PaymentDelay paymentDelay,
        decimal taxRate,
        SalesInputMode inputMode,
        decimal volumeIndexationRate,
        decimal priceIndexationRate)
    {
        Name = name;
        UnitPrice = unitPrice;
        PaymentDelay = paymentDelay;
        TaxRate = taxRate;
        InputMode = inputMode;
        VolumeIndexationRate = volumeIndexationRate;
        PriceIndexationRate = priceIndexationRate;
    }
}

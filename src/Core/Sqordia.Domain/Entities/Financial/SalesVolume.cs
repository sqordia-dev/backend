using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// Monthly sales volume for a product. Month 0 = pre-opening period.
/// </summary>
public class SalesVolume : BaseEntity
{
    public Guid SalesProductId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; } // 0 = pre-opening, 1-12 = calendar months
    public decimal Quantity { get; private set; }

    // Navigation
    public SalesProduct SalesProduct { get; private set; } = null!;

    private SalesVolume() { }

    public SalesVolume(Guid salesProductId, int year, int month, decimal quantity)
    {
        SalesProductId = salesProductId;
        Year = year;
        Month = month;
        Quantity = quantity;
    }

    public void UpdateQuantity(decimal quantity)
    {
        Quantity = quantity;
    }
}

namespace Sqordia.Contracts.Requests.Financial.Previsio;

public class CreateCOGSItemRequest
{
    public Guid LinkedSalesProductId { get; set; }
    public string CostMode { get; set; } = null!;
    public decimal CostValue { get; set; }
    public decimal BeginningInventory { get; set; }
}

public class UpdateCOGSItemRequest
{
    public string CostMode { get; set; } = null!;
    public decimal CostValue { get; set; }
    public decimal BeginningInventory { get; set; }
    public decimal CostIndexationRate { get; set; }
}

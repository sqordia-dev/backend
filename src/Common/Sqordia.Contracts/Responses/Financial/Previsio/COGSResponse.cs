namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class COGSItemResponse
{
    public Guid Id { get; set; }
    public Guid LinkedSalesProductId { get; set; }
    public string LinkedProductName { get; set; } = null!;
    public decimal LinkedProductPrice { get; set; }
    public string CostMode { get; set; } = null!;
    public decimal CostValue { get; set; }
    public decimal BeginningInventory { get; set; }
    public decimal CostIndexationRate { get; set; }
    public decimal EffectiveCostPerUnit { get; set; }
}

public class COGSModuleResponse
{
    public List<COGSItemResponse> Items { get; set; } = new();
}

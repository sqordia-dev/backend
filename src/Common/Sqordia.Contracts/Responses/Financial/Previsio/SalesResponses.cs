namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class SalesProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public string PaymentDelay { get; set; } = null!;
    public decimal TaxRate { get; set; }
    public string InputMode { get; set; } = null!;
    public decimal VolumeIndexationRate { get; set; }
    public decimal PriceIndexationRate { get; set; }
    public int SortOrder { get; set; }
    public bool HasCOGS { get; set; }
}

public class SalesVolumeGridResponse
{
    public Guid SalesProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int Year { get; set; }
    public List<MonthlyValueResponse> MonthlyValues { get; set; } = new();
    public decimal YearTotal { get; set; }
}

public class MonthlyValueResponse
{
    public int Month { get; set; }
    public decimal Value { get; set; }
}

public class SalesModuleResponse
{
    public List<SalesProductResponse> Products { get; set; } = new();
    public List<SalesVolumeGridResponse> VolumeGrids { get; set; } = new();
}

namespace Sqordia.Contracts.Requests.Financial.Previsio;

public class CreateSalesProductRequest
{
    public string Name { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public string PaymentDelay { get; set; } = null!;
    public decimal TaxRate { get; set; }
    public string InputMode { get; set; } = "Quantity";
}

public class UpdateSalesProductRequest
{
    public string Name { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public string PaymentDelay { get; set; } = null!;
    public decimal TaxRate { get; set; }
    public string InputMode { get; set; } = null!;
    public decimal VolumeIndexationRate { get; set; }
    public decimal PriceIndexationRate { get; set; }
}

public class UpdateSalesVolumeGridRequest
{
    public Guid SalesProductId { get; set; }
    public int Year { get; set; }
    public List<MonthlyValue> MonthlyValues { get; set; } = new();
}

public class MonthlyValue
{
    public int Month { get; set; }
    public decimal Value { get; set; }
}

public class ReplicateYearRequest
{
    public int SourceYear { get; set; }
    public int TargetYear { get; set; }
    public decimal AugmentationRate { get; set; }
    public List<Guid>? ProductIds { get; set; }
}

namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class SalesExpenseItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string ExpenseMode { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = null!;
    public int StartMonth { get; set; }
    public int StartYear { get; set; }
    public decimal IndexationRate { get; set; }
    public int SortOrder { get; set; }
}

public class AdminExpenseItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal MonthlyAmount { get; set; }
    public bool IsTaxable { get; set; }
    public string Frequency { get; set; } = null!;
    public int StartMonth { get; set; }
    public int StartYear { get; set; }
    public decimal IndexationRate { get; set; }
    public int SortOrder { get; set; }
}

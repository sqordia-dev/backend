namespace Sqordia.Contracts.Requests.Financial.Previsio;

public class CreateSalesExpenseRequest
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string ExpenseMode { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = "Monthly";
}

public class UpdateSalesExpenseRequest
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string ExpenseMode { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = null!;
    public int StartMonth { get; set; }
    public int StartYear { get; set; }
    public decimal IndexationRate { get; set; }
}

public class CreateAdminExpenseRequest
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal MonthlyAmount { get; set; }
    public bool IsTaxable { get; set; } = true;
    public string Frequency { get; set; } = "Monthly";
}

public class UpdateAdminExpenseRequest
{
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal MonthlyAmount { get; set; }
    public bool IsTaxable { get; set; }
    public string Frequency { get; set; } = null!;
    public int StartMonth { get; set; }
    public int StartYear { get; set; }
    public decimal IndexationRate { get; set; }
}

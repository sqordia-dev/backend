namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class FinancingSourceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string FinancingType { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public int MoratoireMonths { get; set; }
    public int DisbursementMonth { get; set; }
    public int DisbursementYear { get; set; }
    public int SortOrder { get; set; }
    public bool RequiresRepayment { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalInterest { get; set; }
}

public class AmortizationEntryResponse
{
    public int PaymentNumber { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal PrincipalPortion { get; set; }
    public decimal InterestPortion { get; set; }
    public decimal RemainingBalance { get; set; }
    public bool IsMoratoire { get; set; }
}

public class FinancingModuleResponse
{
    public List<FinancingSourceResponse> Sources { get; set; } = new();
    public decimal TotalFinancing { get; set; }
    public decimal TotalProjectCost { get; set; }
    public decimal FinancingGap { get; set; }
}

namespace Sqordia.Contracts.Requests.Financial.Previsio;

public class CreateFinancingSourceRequest
{
    public string Name { get; set; } = null!;
    public string FinancingType { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public int MoratoireMonths { get; set; }
    public int DisbursementMonth { get; set; }
    public int DisbursementYear { get; set; }
}

public class UpdateFinancingSourceRequest
{
    public string Name { get; set; } = null!;
    public string FinancingType { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public int MoratoireMonths { get; set; }
    public int DisbursementMonth { get; set; }
    public int DisbursementYear { get; set; }
}

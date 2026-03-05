namespace Sqordia.Contracts.Requests.Financial.Previsio;

public class CreatePayrollItemRequest
{
    public string JobTitle { get; set; } = null!;
    public string PayrollType { get; set; } = null!;
    public string EmploymentStatus { get; set; } = null!;
    public string SalaryFrequency { get; set; } = null!;
    public decimal SalaryAmount { get; set; }
    public decimal SocialChargeRate { get; set; }
    public int HeadCount { get; set; } = 1;
}

public class UpdatePayrollItemRequest
{
    public string JobTitle { get; set; } = null!;
    public string PayrollType { get; set; } = null!;
    public string EmploymentStatus { get; set; } = null!;
    public string SalaryFrequency { get; set; } = null!;
    public decimal SalaryAmount { get; set; }
    public decimal SocialChargeRate { get; set; }
    public int HeadCount { get; set; }
    public int StartMonth { get; set; }
    public int StartYear { get; set; }
    public decimal SalaryIndexationRate { get; set; }
}

public class CalculateSalaryRequest
{
    public decimal Amount { get; set; }
    public string FromFrequency { get; set; } = null!;
    public string ToFrequency { get; set; } = null!;
}

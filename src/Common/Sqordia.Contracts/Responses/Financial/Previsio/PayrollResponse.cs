namespace Sqordia.Contracts.Responses.Financial.Previsio;

public class PayrollItemResponse
{
    public Guid Id { get; set; }
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
    public int SortOrder { get; set; }
    public decimal MonthlySalary { get; set; }
    public decimal MonthlyTotalCost { get; set; }
}

public class SalaryCalculationResponse
{
    public decimal Hourly { get; set; }
    public decimal Monthly { get; set; }
    public decimal Annual { get; set; }
}

public class PayrollModuleResponse
{
    public List<PayrollItemResponse> Items { get; set; } = new();
    public decimal TotalMonthlyPayroll { get; set; }
    public decimal TotalMonthlySocialCharges { get; set; }
}

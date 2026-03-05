using Sqordia.Domain.Common;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Domain.Entities.Financial;

/// <summary>
/// Employee or contractor payroll entry with salary and social charges.
/// </summary>
public class PayrollItem : BaseAuditableEntity
{
    public Guid FinancialPlanId { get; private set; }
    public string JobTitle { get; private set; } = null!;
    public PayrollType PayrollType { get; private set; }
    public EmploymentStatus EmploymentStatus { get; private set; }
    public SalaryFrequency SalaryFrequency { get; private set; }
    public decimal SalaryAmount { get; private set; }
    public decimal SocialChargeRate { get; private set; }
    public int HeadCount { get; private set; } = 1;
    public int StartMonth { get; private set; } = 1; // Month employment begins (1-12)
    public int StartYear { get; private set; } = 1;  // Projection year (1-3)
    public decimal SalaryIndexationRate { get; private set; }
    public int SortOrder { get; private set; }

    // Navigation
    public FinancialPlan FinancialPlan { get; private set; } = null!;

    private PayrollItem() { }

    public PayrollItem(
        Guid financialPlanId,
        string jobTitle,
        PayrollType payrollType,
        EmploymentStatus employmentStatus,
        SalaryFrequency salaryFrequency,
        decimal salaryAmount,
        decimal socialChargeRate,
        int headCount = 1,
        int sortOrder = 0)
    {
        FinancialPlanId = financialPlanId;
        JobTitle = jobTitle;
        PayrollType = payrollType;
        EmploymentStatus = employmentStatus;
        SalaryFrequency = salaryFrequency;
        SalaryAmount = salaryAmount;
        SocialChargeRate = socialChargeRate;
        HeadCount = headCount;
        SortOrder = sortOrder;
    }

    public void Update(
        string jobTitle,
        PayrollType payrollType,
        EmploymentStatus employmentStatus,
        SalaryFrequency salaryFrequency,
        decimal salaryAmount,
        decimal socialChargeRate,
        int headCount,
        int startMonth,
        int startYear,
        decimal salaryIndexationRate)
    {
        JobTitle = jobTitle;
        PayrollType = payrollType;
        EmploymentStatus = employmentStatus;
        SalaryFrequency = salaryFrequency;
        SalaryAmount = salaryAmount;
        SocialChargeRate = socialChargeRate;
        HeadCount = headCount;
        StartMonth = startMonth;
        StartYear = startYear;
        SalaryIndexationRate = salaryIndexationRate;
    }

    /// <summary>
    /// Calculates monthly salary based on frequency.
    /// </summary>
    public decimal GetMonthlySalary()
    {
        return SalaryFrequency switch
        {
            SalaryFrequency.Hourly => SalaryAmount * 40 * 52 / 12, // 40h/week, 52 weeks
            SalaryFrequency.Monthly => SalaryAmount,
            SalaryFrequency.Annual => SalaryAmount / 12,
            _ => SalaryAmount
        };
    }

    /// <summary>
    /// Calculates total monthly cost including social charges.
    /// </summary>
    public decimal GetMonthlyTotalCost()
    {
        var monthlySalary = GetMonthlySalary();
        return monthlySalary * HeadCount * (1 + SocialChargeRate / 100);
    }
}

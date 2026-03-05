using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Services.Previsio;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;
using Sqordia.Domain.Entities.Financial;
using Sqordia.Domain.Enums.Financial;

namespace Sqordia.Application.Services.Implementations.Financial;

public class PayrollModuleServiceImpl : IPayrollModuleService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PayrollModuleServiceImpl> _logger;

    public PayrollModuleServiceImpl(IApplicationDbContext context, ILogger<PayrollModuleServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PayrollModuleResponse>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<PayrollModuleResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var items = await _context.PayrollItems
            .Where(p => p.FinancialPlanId == plan.Id)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        return Result.Success(new PayrollModuleResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalMonthlyPayroll = items.Sum(i => i.GetMonthlySalary() * i.HeadCount),
            TotalMonthlySocialCharges = items.Sum(i => i.GetMonthlyTotalCost() - i.GetMonthlySalary() * i.HeadCount)
        });
    }

    public async Task<Result<PayrollItemResponse>> CreateAsync(Guid businessPlanId, CreatePayrollItemRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (plan == null)
            return Result.Failure<PayrollItemResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        if (!Enum.TryParse<PayrollType>(request.PayrollType, true, out var payrollType))
            return Result.Failure<PayrollItemResponse>(Error.Validation("Payroll.InvalidType", "Invalid payroll type"));

        if (!Enum.TryParse<EmploymentStatus>(request.EmploymentStatus, true, out var status))
            return Result.Failure<PayrollItemResponse>(Error.Validation("Payroll.InvalidStatus", "Invalid employment status"));

        if (!Enum.TryParse<SalaryFrequency>(request.SalaryFrequency, true, out var frequency))
            return Result.Failure<PayrollItemResponse>(Error.Validation("Payroll.InvalidFrequency", "Invalid salary frequency"));

        var maxOrder = await _context.PayrollItems
            .Where(p => p.FinancialPlanId == plan.Id)
            .MaxAsync(p => (int?)p.SortOrder, cancellationToken) ?? 0;

        var item = new PayrollItem(
            plan.Id, request.JobTitle, payrollType, status, frequency,
            request.SalaryAmount, request.SocialChargeRate, request.HeadCount, maxOrder + 1);

        _context.PayrollItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToResponse(item));
    }

    public async Task<Result<PayrollItemResponse>> UpdateAsync(Guid businessPlanId, Guid itemId, UpdatePayrollItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(businessPlanId, itemId, cancellationToken);
        if (item == null)
            return Result.Failure<PayrollItemResponse>(Error.NotFound("Payroll.NotFound", "Payroll item not found"));

        if (!Enum.TryParse<PayrollType>(request.PayrollType, true, out var payrollType) ||
            !Enum.TryParse<EmploymentStatus>(request.EmploymentStatus, true, out var status) ||
            !Enum.TryParse<SalaryFrequency>(request.SalaryFrequency, true, out var frequency))
            return Result.Failure<PayrollItemResponse>(Error.Validation("Payroll.InvalidEnum", "Invalid enum value"));

        item.Update(request.JobTitle, payrollType, status, frequency, request.SalaryAmount,
            request.SocialChargeRate, request.HeadCount, request.StartMonth, request.StartYear, request.SalaryIndexationRate);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(MapToResponse(item));
    }

    public async Task<Result> DeleteAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(businessPlanId, itemId, cancellationToken);
        if (item == null)
            return Result.Failure(Error.NotFound("Payroll.NotFound", "Payroll item not found"));

        item.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public Task<Result<SalaryCalculationResponse>> CalculateSalaryAsync(CalculateSalaryRequest request, CancellationToken cancellationToken = default)
    {
        var annualAmount = request.FromFrequency.ToLowerInvariant() switch
        {
            "hourly" => request.Amount * 40 * 52,
            "monthly" => request.Amount * 12,
            "annual" => request.Amount,
            _ => request.Amount
        };

        return Task.FromResult(Result.Success(new SalaryCalculationResponse
        {
            Hourly = Math.Round(annualAmount / (40 * 52), 2),
            Monthly = Math.Round(annualAmount / 12, 2),
            Annual = Math.Round(annualAmount, 2)
        }));
    }

    private async Task<PayrollItem?> GetItemAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken)
    {
        var plan = await _context.FinancialPlansPrevisio
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return null;

        return await _context.PayrollItems
            .FirstOrDefaultAsync(p => p.Id == itemId && p.FinancialPlanId == plan.Id, cancellationToken);
    }

    private static PayrollItemResponse MapToResponse(PayrollItem item)
    {
        return new PayrollItemResponse
        {
            Id = item.Id,
            JobTitle = item.JobTitle,
            PayrollType = item.PayrollType.ToString(),
            EmploymentStatus = item.EmploymentStatus.ToString(),
            SalaryFrequency = item.SalaryFrequency.ToString(),
            SalaryAmount = item.SalaryAmount,
            SocialChargeRate = item.SocialChargeRate,
            HeadCount = item.HeadCount,
            StartMonth = item.StartMonth,
            StartYear = item.StartYear,
            SalaryIndexationRate = item.SalaryIndexationRate,
            SortOrder = item.SortOrder,
            MonthlySalary = item.GetMonthlySalary(),
            MonthlyTotalCost = item.GetMonthlyTotalCost()
        };
    }
}

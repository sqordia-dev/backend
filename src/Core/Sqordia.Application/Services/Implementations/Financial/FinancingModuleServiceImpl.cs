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

public class FinancingModuleServiceImpl : IFinancingModuleService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FinancingModuleServiceImpl> _logger;

    public FinancingModuleServiceImpl(IApplicationDbContext context, ILogger<FinancingModuleServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<FinancingModuleResponse>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio.FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<FinancingModuleResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var sources = await _context.FinancingSources
            .Where(fs => fs.FinancialPlanId == plan.Id)
            .OrderBy(fs => fs.SortOrder)
            .ToListAsync(cancellationToken);

        var projectCost = await _context.ProjectCosts
            .FirstOrDefaultAsync(pc => pc.FinancialPlanId == plan.Id, cancellationToken);

        var totalFinancing = sources.Sum(s => s.Amount);
        var totalProjectCost = projectCost?.TotalProjectCost ?? 0;

        return Result.Success(new FinancingModuleResponse
        {
            Sources = sources.Select(MapToResponse).ToList(),
            TotalFinancing = totalFinancing,
            TotalProjectCost = totalProjectCost,
            FinancingGap = totalProjectCost - totalFinancing
        });
    }

    public async Task<Result<FinancingSourceResponse>> CreateAsync(Guid businessPlanId, CreateFinancingSourceRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio.FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<FinancingSourceResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        if (!Enum.TryParse<FinancingType>(request.FinancingType, true, out var financingType))
            return Result.Failure<FinancingSourceResponse>(Error.Validation("Financing.InvalidType", "Invalid financing type"));

        var maxOrder = await _context.FinancingSources.Where(fs => fs.FinancialPlanId == plan.Id).MaxAsync(fs => (int?)fs.SortOrder, cancellationToken) ?? 0;

        var source = new FinancingSource(plan.Id, request.Name, financingType, request.Amount, request.InterestRate, request.TermMonths, request.MoratoireMonths, maxOrder + 1, request.DisbursementMonth, request.DisbursementYear);
        _context.FinancingSources.Add(source);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate amortization schedule if loan
        if (source.RequiresRepayment() && source.TermMonths > 0)
            await GenerateAmortizationScheduleAsync(source, cancellationToken);

        return Result.Success(MapToResponse(source));
    }

    public async Task<Result<FinancingSourceResponse>> UpdateAsync(Guid businessPlanId, Guid sourceId, UpdateFinancingSourceRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio.FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<FinancingSourceResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var source = await _context.FinancingSources.FirstOrDefaultAsync(fs => fs.Id == sourceId && fs.FinancialPlanId == plan.Id, cancellationToken);
        if (source == null) return Result.Failure<FinancingSourceResponse>(Error.NotFound("Financing.NotFound", "Financing source not found"));

        Enum.TryParse<FinancingType>(request.FinancingType, true, out var financingType);

        source.Update(request.Name, financingType, request.Amount, request.InterestRate, request.TermMonths, request.MoratoireMonths, request.DisbursementMonth, request.DisbursementYear);
        await _context.SaveChangesAsync(cancellationToken);

        // Regenerate amortization
        var oldEntries = await _context.AmortizationEntries.Where(ae => ae.FinancingSourceId == source.Id).ToListAsync(cancellationToken);
        foreach (var entry in oldEntries) _context.AmortizationEntries.Remove(entry);

        if (source.RequiresRepayment() && source.TermMonths > 0)
            await GenerateAmortizationScheduleAsync(source, cancellationToken);

        return Result.Success(MapToResponse(source));
    }

    public async Task<Result> DeleteAsync(Guid businessPlanId, Guid sourceId, CancellationToken cancellationToken = default)
    {
        var plan = await _context.FinancialPlansPrevisio.FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var source = await _context.FinancingSources.FirstOrDefaultAsync(fs => fs.Id == sourceId && fs.FinancialPlanId == plan.Id, cancellationToken);
        if (source == null) return Result.Failure(Error.NotFound("Financing.NotFound", "Financing source not found"));

        source.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<List<AmortizationEntryResponse>>> GetAmortizationScheduleAsync(Guid businessPlanId, Guid sourceId, CancellationToken cancellationToken = default)
    {
        var entries = await _context.AmortizationEntries
            .Where(ae => ae.FinancingSourceId == sourceId)
            .OrderBy(ae => ae.PaymentNumber)
            .ToListAsync(cancellationToken);

        return Result.Success(entries.Select(e => new AmortizationEntryResponse
        {
            PaymentNumber = e.PaymentNumber,
            Year = e.Year,
            Month = e.Month,
            PaymentAmount = e.PaymentAmount,
            PrincipalPortion = e.PrincipalPortion,
            InterestPortion = e.InterestPortion,
            RemainingBalance = e.RemainingBalance,
            IsMoratoire = e.IsMoratoire
        }).ToList());
    }

    private async Task GenerateAmortizationScheduleAsync(FinancingSource source, CancellationToken cancellationToken)
    {
        var principal = source.Amount;
        var monthlyRate = source.InterestRate / 100 / 12;
        var termPayments = source.TermMonths - source.MoratoireMonths;
        var balance = principal;

        // Calculate PMT for non-moratoire period
        decimal monthlyPayment = 0;
        if (termPayments > 0 && monthlyRate > 0)
        {
            var factor = (double)monthlyRate * Math.Pow(1 + (double)monthlyRate, termPayments);
            var divisor = Math.Pow(1 + (double)monthlyRate, termPayments) - 1;
            monthlyPayment = principal * (decimal)(factor / divisor);
        }
        else if (termPayments > 0)
        {
            monthlyPayment = principal / termPayments;
        }

        var startMonth = source.DisbursementMonth;
        var startYear = source.DisbursementYear;

        for (int i = 1; i <= source.TermMonths; i++)
        {
            var currentMonth = ((startMonth - 1 + i - 1) % 12) + 1;
            var currentYear = startYear + (startMonth - 1 + i - 1) / 12;

            var interest = Math.Round(balance * monthlyRate, 2);
            bool isMoratoire = i <= source.MoratoireMonths;

            decimal principalPortion;
            decimal payment;

            if (isMoratoire)
            {
                principalPortion = 0;
                payment = interest;
            }
            else
            {
                payment = Math.Round(monthlyPayment, 2);
                principalPortion = payment - interest;
            }

            balance = Math.Max(0, balance - principalPortion);

            _context.AmortizationEntries.Add(new AmortizationEntry(
                source.Id, i, currentYear, currentMonth, payment, principalPortion, interest, balance, isMoratoire));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static FinancingSourceResponse MapToResponse(FinancingSource source)
    {
        decimal monthlyPayment = 0;
        if (source.RequiresRepayment() && source.TermMonths > 0)
        {
            var termPayments = source.TermMonths - source.MoratoireMonths;
            var monthlyRate = source.InterestRate / 100 / 12;
            if (termPayments > 0 && monthlyRate > 0)
            {
                var factor = (double)monthlyRate * Math.Pow(1 + (double)monthlyRate, termPayments);
                var divisor = Math.Pow(1 + (double)monthlyRate, termPayments) - 1;
                monthlyPayment = source.Amount * (decimal)(factor / divisor);
            }
            else if (termPayments > 0)
            {
                monthlyPayment = source.Amount / termPayments;
            }
        }

        return new FinancingSourceResponse
        {
            Id = source.Id,
            Name = source.Name,
            FinancingType = source.FinancingType.ToString(),
            Amount = source.Amount,
            InterestRate = source.InterestRate,
            TermMonths = source.TermMonths,
            MoratoireMonths = source.MoratoireMonths,
            DisbursementMonth = source.DisbursementMonth,
            DisbursementYear = source.DisbursementYear,
            SortOrder = source.SortOrder,
            RequiresRepayment = source.RequiresRepayment(),
            MonthlyPayment = Math.Round(monthlyPayment, 2),
            TotalInterest = Math.Round(monthlyPayment * (source.TermMonths - source.MoratoireMonths) - source.Amount, 2)
        };
    }
}

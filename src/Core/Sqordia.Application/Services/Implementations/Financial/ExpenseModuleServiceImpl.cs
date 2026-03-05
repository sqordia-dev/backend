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

public class ExpenseModuleServiceImpl : IExpenseModuleService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ExpenseModuleServiceImpl> _logger;

    public ExpenseModuleServiceImpl(IApplicationDbContext context, ILogger<ExpenseModuleServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    // === Sales Expenses ===

    public async Task<Result<List<SalesExpenseItemResponse>>> GetSalesExpensesAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<List<SalesExpenseItemResponse>>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var items = await _context.SalesExpenseItems
            .Where(e => e.FinancialPlanId == plan.Id)
            .OrderBy(e => e.SortOrder)
            .ToListAsync(cancellationToken);

        return Result.Success(items.Select(MapSalesExpense).ToList());
    }

    public async Task<Result<SalesExpenseItemResponse>> CreateSalesExpenseAsync(Guid businessPlanId, CreateSalesExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<SalesExpenseItemResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        if (!Enum.TryParse<SalesExpenseCategory>(request.Category, true, out var category))
            return Result.Failure<SalesExpenseItemResponse>(Error.Validation("Expense.InvalidCategory", "Invalid expense category"));

        if (!Enum.TryParse<ExpenseMode>(request.ExpenseMode, true, out var mode))
            return Result.Failure<SalesExpenseItemResponse>(Error.Validation("Expense.InvalidMode", "Invalid expense mode"));

        Enum.TryParse<RecurrenceFrequency>(request.Frequency, true, out var frequency);

        var maxOrder = await _context.SalesExpenseItems
            .Where(e => e.FinancialPlanId == plan.Id)
            .MaxAsync(e => (int?)e.SortOrder, cancellationToken) ?? 0;

        var item = new SalesExpenseItem(plan.Id, request.Name, category, mode, request.Amount, frequency, maxOrder + 1);
        _context.SalesExpenseItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapSalesExpense(item));
    }

    public async Task<Result<SalesExpenseItemResponse>> UpdateSalesExpenseAsync(Guid businessPlanId, Guid itemId, UpdateSalesExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<SalesExpenseItemResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var item = await _context.SalesExpenseItems.FirstOrDefaultAsync(e => e.Id == itemId && e.FinancialPlanId == plan.Id, cancellationToken);
        if (item == null) return Result.Failure<SalesExpenseItemResponse>(Error.NotFound("Expense.NotFound", "Expense not found"));

        Enum.TryParse<SalesExpenseCategory>(request.Category, true, out var category);
        Enum.TryParse<ExpenseMode>(request.ExpenseMode, true, out var mode);
        Enum.TryParse<RecurrenceFrequency>(request.Frequency, true, out var frequency);

        item.Update(request.Name, category, mode, request.Amount, frequency, request.StartMonth, request.StartYear, request.IndexationRate);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapSalesExpense(item));
    }

    public async Task<Result> DeleteSalesExpenseAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var item = await _context.SalesExpenseItems.FirstOrDefaultAsync(e => e.Id == itemId && e.FinancialPlanId == plan.Id, cancellationToken);
        if (item == null) return Result.Failure(Error.NotFound("Expense.NotFound", "Expense not found"));

        item.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    // === Admin Expenses ===

    public async Task<Result<List<AdminExpenseItemResponse>>> GetAdminExpensesAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<List<AdminExpenseItemResponse>>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var items = await _context.AdminExpenseItems
            .Where(e => e.FinancialPlanId == plan.Id)
            .OrderBy(e => e.SortOrder)
            .ToListAsync(cancellationToken);

        return Result.Success(items.Select(MapAdminExpense).ToList());
    }

    public async Task<Result<AdminExpenseItemResponse>> CreateAdminExpenseAsync(Guid businessPlanId, CreateAdminExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<AdminExpenseItemResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        if (!Enum.TryParse<AdminExpenseCategory>(request.Category, true, out var category))
            return Result.Failure<AdminExpenseItemResponse>(Error.Validation("Expense.InvalidCategory", "Invalid expense category"));

        Enum.TryParse<RecurrenceFrequency>(request.Frequency, true, out var frequency);

        var maxOrder = await _context.AdminExpenseItems
            .Where(e => e.FinancialPlanId == plan.Id)
            .MaxAsync(e => (int?)e.SortOrder, cancellationToken) ?? 0;

        var item = new AdminExpenseItem(plan.Id, request.Name, category, request.MonthlyAmount, request.IsTaxable, frequency, maxOrder + 1);
        _context.AdminExpenseItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapAdminExpense(item));
    }

    public async Task<Result<AdminExpenseItemResponse>> UpdateAdminExpenseAsync(Guid businessPlanId, Guid itemId, UpdateAdminExpenseRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure<AdminExpenseItemResponse>(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var item = await _context.AdminExpenseItems.FirstOrDefaultAsync(e => e.Id == itemId && e.FinancialPlanId == plan.Id, cancellationToken);
        if (item == null) return Result.Failure<AdminExpenseItemResponse>(Error.NotFound("Expense.NotFound", "Expense not found"));

        Enum.TryParse<AdminExpenseCategory>(request.Category, true, out var category);
        Enum.TryParse<RecurrenceFrequency>(request.Frequency, true, out var frequency);

        item.Update(request.Name, category, request.MonthlyAmount, request.IsTaxable, frequency, request.StartMonth, request.StartYear, request.IndexationRate);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapAdminExpense(item));
    }

    public async Task<Result> DeleteAdminExpenseAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var plan = await GetPlanAsync(businessPlanId, cancellationToken);
        if (plan == null) return Result.Failure(Error.NotFound("FinancialPlan.NotFound", "Financial plan not found"));

        var item = await _context.AdminExpenseItems.FirstOrDefaultAsync(e => e.Id == itemId && e.FinancialPlanId == plan.Id, cancellationToken);
        if (item == null) return Result.Failure(Error.NotFound("Expense.NotFound", "Expense not found"));

        item.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<FinancialPlan?> GetPlanAsync(Guid businessPlanId, CancellationToken ct) =>
        await _context.FinancialPlansPrevisio.FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, ct);

    private static SalesExpenseItemResponse MapSalesExpense(SalesExpenseItem item) => new()
    {
        Id = item.Id, Name = item.Name, Category = item.Category.ToString(), ExpenseMode = item.ExpenseMode.ToString(),
        Amount = item.Amount, Frequency = item.Frequency.ToString(), StartMonth = item.StartMonth,
        StartYear = item.StartYear, IndexationRate = item.IndexationRate, SortOrder = item.SortOrder
    };

    private static AdminExpenseItemResponse MapAdminExpense(AdminExpenseItem item) => new()
    {
        Id = item.Id, Name = item.Name, Category = item.Category.ToString(), MonthlyAmount = item.MonthlyAmount,
        IsTaxable = item.IsTaxable, Frequency = item.Frequency.ToString(), StartMonth = item.StartMonth,
        StartYear = item.StartYear, IndexationRate = item.IndexationRate, SortOrder = item.SortOrder
    };
}

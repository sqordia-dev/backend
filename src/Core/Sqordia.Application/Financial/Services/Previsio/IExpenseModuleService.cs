using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

public interface IExpenseModuleService
{
    // Sales Expenses
    Task<Result<List<SalesExpenseItemResponse>>> GetSalesExpensesAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<SalesExpenseItemResponse>> CreateSalesExpenseAsync(Guid businessPlanId, CreateSalesExpenseRequest request, CancellationToken cancellationToken = default);
    Task<Result<SalesExpenseItemResponse>> UpdateSalesExpenseAsync(Guid businessPlanId, Guid itemId, UpdateSalesExpenseRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteSalesExpenseAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken = default);

    // Admin Expenses
    Task<Result<List<AdminExpenseItemResponse>>> GetAdminExpensesAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<AdminExpenseItemResponse>> CreateAdminExpenseAsync(Guid businessPlanId, CreateAdminExpenseRequest request, CancellationToken cancellationToken = default);
    Task<Result<AdminExpenseItemResponse>> UpdateAdminExpenseAsync(Guid businessPlanId, Guid itemId, UpdateAdminExpenseRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAdminExpenseAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken = default);
}

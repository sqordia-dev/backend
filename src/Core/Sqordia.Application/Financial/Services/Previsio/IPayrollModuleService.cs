using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Financial.Previsio;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

public interface IPayrollModuleService
{
    Task<Result<PayrollModuleResponse>> GetAllAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
    Task<Result<PayrollItemResponse>> CreateAsync(Guid businessPlanId, CreatePayrollItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<PayrollItemResponse>> UpdateAsync(Guid businessPlanId, Guid itemId, UpdatePayrollItemRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid businessPlanId, Guid itemId, CancellationToken cancellationToken = default);
    Task<Result<SalaryCalculationResponse>> CalculateSalaryAsync(CalculateSalaryRequest request, CancellationToken cancellationToken = default);
}

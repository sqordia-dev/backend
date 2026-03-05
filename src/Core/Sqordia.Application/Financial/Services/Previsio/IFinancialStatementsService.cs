using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Financial.Services.Previsio;

public interface IFinancialStatementsService
{
    Task<Result<FinancialStatementsResponse>> RecalculateAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default);
    Task<Result<ProfitLossStatement>> GetProfitLossAsync(Guid businessPlanId, int year, string language = "fr", CancellationToken cancellationToken = default);
    Task<Result<CashFlowStatement>> GetCashFlowAsync(Guid businessPlanId, int year, string language = "fr", CancellationToken cancellationToken = default);
    Task<Result<BalanceSheetStatement>> GetBalanceSheetAsync(Guid businessPlanId, int year, string language = "fr", CancellationToken cancellationToken = default);
    Task<Result<FinancialRatiosResponse>> GetRatiosAsync(Guid businessPlanId, CancellationToken cancellationToken = default);
}

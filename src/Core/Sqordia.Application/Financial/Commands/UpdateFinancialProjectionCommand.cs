using MediatR;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Queries;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Financial.Commands;

public record UpdateFinancialProjectionCommand : IRequest<Result<FinancialProjectionDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ProjectionType { get; init; } = string.Empty;
    public ScenarioType Scenario { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public FinancialCategory Category { get; init; }
    public string SubCategory { get; init; } = string.Empty;
    public bool IsRecurring { get; init; }
    public string Frequency { get; init; } = string.Empty;
    public decimal GrowthRate { get; init; }
    public string Assumptions { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
}

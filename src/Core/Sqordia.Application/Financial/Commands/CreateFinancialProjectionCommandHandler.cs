using MediatR;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Queries;
using Sqordia.Domain.Entities;

namespace Sqordia.Application.Financial.Commands;

public class CreateFinancialProjectionCommandHandler : IRequestHandler<CreateFinancialProjectionCommand, Result<FinancialProjectionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateFinancialProjectionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FinancialProjectionDto>> Handle(CreateFinancialProjectionCommand request, CancellationToken cancellationToken)
    {
        var projection = new FinancialProjectionItem
        {
            BusinessPlanId = request.BusinessPlanId,
            Name = request.Name,
            Description = request.Description,
            ProjectionType = request.ProjectionType,
            Scenario = request.Scenario.ToString(),
            Year = request.Year,
            Month = request.Month,
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode,
            BaseAmount = request.Amount, // Will be updated with exchange rate
            Category = request.Category.ToString(),
            SubCategory = request.SubCategory,
            IsRecurring = request.IsRecurring,
            Frequency = request.Frequency,
            GrowthRate = request.GrowthRate,
            Assumptions = request.Assumptions,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId ?? "System",
            UpdatedBy = _currentUserService.UserId ?? "System"
        };

        _context.FinancialProjectionItems.Add(projection);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new FinancialProjectionDto
        {
            Id = projection.Id,
            BusinessPlanId = projection.BusinessPlanId,
            Name = projection.Name,
            Description = projection.Description,
            ProjectionType = projection.ProjectionType,
            Scenario = projection.Scenario,
            Year = projection.Year,
            Month = projection.Month,
            Amount = projection.Amount,
            CurrencyCode = projection.CurrencyCode,
            BaseAmount = projection.BaseAmount,
            Category = projection.Category,
            SubCategory = projection.SubCategory,
            IsRecurring = projection.IsRecurring,
            Frequency = projection.Frequency,
            GrowthRate = projection.GrowthRate,
            CreatedAt = projection.CreatedAt,
            UpdatedAt = projection.UpdatedAt
        };

        return Result<FinancialProjectionDto>.Success(dto);
    }
}

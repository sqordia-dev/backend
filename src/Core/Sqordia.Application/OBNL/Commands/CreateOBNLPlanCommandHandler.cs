using MediatR;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.Entities;
using Sqordia.Domain.ValueObjects;

namespace Sqordia.Application.OBNL.Commands;

public class CreateOBNLPlanCommandHandler : IRequestHandler<CreateOBNLPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateOBNLPlanCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateOBNLPlanCommand request, CancellationToken cancellationToken)
    {
        var obnlPlan = new OBNLBusinessPlan
        {
            OrganizationId = request.OrganizationId,
            OBNLType = request.OBNLType,
            Mission = request.Mission,
            Vision = request.Vision,
            Values = request.Values,
            FundingRequirements = request.FundingRequirements,
            FundingPurpose = request.FundingPurpose,
            LegalStructure = request.LegalStructure,
            RegistrationNumber = request.RegistrationNumber,
            RegistrationDate = request.RegistrationDate,
            GoverningBody = request.GoverningBody,
            BoardComposition = request.BoardComposition,
            StakeholderEngagement = request.StakeholderEngagement,
            ImpactMeasurement = request.ImpactMeasurement,
            SustainabilityStrategy = request.SustainabilityStrategy,
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId ?? "System",
            UpdatedBy = _currentUserService.UserId ?? "System"
        };

        _context.OBNLBusinessPlans.Add(obnlPlan);
        await _context.SaveChangesAsync(cancellationToken);

        return obnlPlan.Id;
    }
}

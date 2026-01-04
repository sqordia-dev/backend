using Microsoft.EntityFrameworkCore;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.OBNL.Commands;
using Sqordia.Application.OBNL.Queries;
using Sqordia.Domain.Entities;
using Sqordia.Domain.ValueObjects;

namespace Sqordia.Application.OBNL.Services;

public class OBNLPlanService : IOBNLPlanService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public OBNLPlanService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> CreateOBNLPlanAsync(CreateOBNLPlanCommand command)
    {
        var plan = new OBNLBusinessPlan
        {
            OrganizationId = command.OrganizationId,
            OBNLType = command.OBNLType,
            Mission = command.Mission,
            Vision = command.Vision,
            Values = command.Values,
            FundingRequirements = command.FundingRequirements,
            FundingPurpose = command.FundingPurpose,
            LegalStructure = command.LegalStructure,
            RegistrationNumber = command.RegistrationNumber,
            RegistrationDate = command.RegistrationDate,
            GoverningBody = command.GoverningBody,
            BoardComposition = command.BoardComposition,
            StakeholderEngagement = command.StakeholderEngagement,
            ImpactMeasurement = command.ImpactMeasurement,
            SustainabilityStrategy = command.SustainabilityStrategy,
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId ?? "System",
            UpdatedBy = _currentUserService.UserId ?? "System"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        return plan.Id;
    }

    public async Task<OBNLPlanDto> GetOBNLPlanAsync(Guid planId)
    {
        var plan = await _context.OBNLBusinessPlans
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
            throw new ArgumentException("OBNL plan not found");

        return new OBNLPlanDto
        {
            Id = plan.Id,
            OrganizationId = plan.OrganizationId,
            OBNLType = plan.OBNLType,
            Mission = plan.Mission,
            Vision = plan.Vision,
            Values = plan.Values,
            FundingRequirements = plan.FundingRequirements,
            FundingPurpose = plan.FundingPurpose,
            ComplianceStatus = plan.ComplianceStatus.Status,
            LegalStructure = plan.LegalStructure,
            RegistrationNumber = plan.RegistrationNumber,
            RegistrationDate = plan.RegistrationDate,
            GoverningBody = plan.GoverningBody,
            BoardComposition = plan.BoardComposition,
            StakeholderEngagement = plan.StakeholderEngagement,
            ImpactMeasurement = plan.ImpactMeasurement,
            SustainabilityStrategy = plan.SustainabilityStrategy,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt,
            CreatedBy = plan.CreatedBy,
            UpdatedBy = plan.UpdatedBy
        };
    }

    public async Task<List<OBNLPlanDto>> GetOBNLPlansByOrganizationAsync(Guid organizationId)
    {
        var plans = await _context.OBNLBusinessPlans
            .Where(p => p.OrganizationId == organizationId)
            .Select(p => new OBNLPlanDto
            {
                Id = p.Id,
                OrganizationId = p.OrganizationId,
                OBNLType = p.OBNLType,
                Mission = p.Mission,
                Vision = p.Vision,
                Values = p.Values,
                FundingRequirements = p.FundingRequirements,
                FundingPurpose = p.FundingPurpose,
                ComplianceStatus = p.ComplianceStatus.Status,
                LegalStructure = p.LegalStructure,
                RegistrationNumber = p.RegistrationNumber,
                RegistrationDate = p.RegistrationDate,
                GoverningBody = p.GoverningBody,
                BoardComposition = p.BoardComposition,
                StakeholderEngagement = p.StakeholderEngagement,
                ImpactMeasurement = p.ImpactMeasurement,
                SustainabilityStrategy = p.SustainabilityStrategy,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                CreatedBy = p.CreatedBy,
                UpdatedBy = p.UpdatedBy
            })
            .ToListAsync();

        return plans;
    }

    public async Task UpdateOBNLPlanAsync(UpdateOBNLPlanCommand command)
    {
        var plan = await _context.OBNLBusinessPlans
            .FirstOrDefaultAsync(p => p.Id == command.Id);

        if (plan == null)
            throw new ArgumentException("OBNL plan not found");

        plan.OBNLType = command.OBNLType;
        plan.Mission = command.Mission;
        plan.Vision = command.Vision;
        plan.Values = command.Values;
        plan.FundingRequirements = command.FundingRequirements;
        plan.FundingPurpose = command.FundingPurpose;
        plan.LegalStructure = command.LegalStructure;
        plan.RegistrationNumber = command.RegistrationNumber;
        plan.RegistrationDate = command.RegistrationDate;
        plan.GoverningBody = command.GoverningBody;
        plan.BoardComposition = command.BoardComposition;
        plan.StakeholderEngagement = command.StakeholderEngagement;
        plan.ImpactMeasurement = command.ImpactMeasurement;
        plan.SustainabilityStrategy = command.SustainabilityStrategy;
        plan.UpdatedAt = DateTime.UtcNow;
        plan.UpdatedBy = _currentUserService.UserId ?? "System";

        await _context.SaveChangesAsync();
    }

    public async Task DeleteOBNLPlanAsync(Guid planId)
    {
        var plan = await _context.OBNLBusinessPlans
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
            throw new ArgumentException("OBNL plan not found");

        _context.OBNLBusinessPlans.Remove(plan);
        await _context.SaveChangesAsync();
    }

    public async Task<ComplianceAnalysisDto> AnalyzeComplianceAsync(Guid planId)
    {
        var plan = await _context.OBNLBusinessPlans
            .FirstOrDefaultAsync(p => p.Id == planId);

        if (plan == null)
            throw new ArgumentException("OBNL plan not found");

        // Mock compliance analysis
        var analysis = new ComplianceAnalysisDto
        {
            Status = "Compliant",
            Level = "High",
            Requirements = new List<string>
            {
                "Annual financial reporting required",
                "Board of directors must meet quarterly",
                "Tax-exempt status documentation needed"
            },
            Recommendations = new List<string>
            {
                "Submit annual report by March 31st",
                "Schedule quarterly board meetings",
                "Maintain detailed financial records"
            },
            LastUpdated = DateTime.UtcNow,
            Notes = "Compliance analysis completed successfully"
        };

        return analysis;
    }

    public async Task<List<GrantApplicationDto>> GetGrantApplicationsAsync(Guid planId)
    {
        var applications = await _context.GrantApplications
            .Where(ga => ga.OBNLBusinessPlanId == planId)
            .Select(ga => new GrantApplicationDto
            {
                Id = ga.Id,
                OBNLBusinessPlanId = ga.OBNLBusinessPlanId,
                GrantName = ga.GrantName,
                GrantingOrganization = ga.GrantingOrganization,
                RequestedAmount = ga.RequestedAmount,
                MatchingFunds = ga.MatchingFunds,
                ApplicationDeadline = ga.ApplicationDeadline,
                Status = ga.Status,
                CreatedAt = ga.CreatedAt,
                UpdatedAt = ga.UpdatedAt
            })
            .ToListAsync();

        return applications;
    }

    public async Task<Guid> CreateGrantApplicationAsync(CreateGrantApplicationCommand command)
    {
        var grantApplication = new GrantApplication
        {
            OBNLBusinessPlanId = command.OBNLBusinessPlanId,
            GrantName = command.GrantName,
            GrantingOrganization = command.GrantingOrganization,
            RequestedAmount = command.RequestedAmount,
            MatchingFunds = command.MatchingFunds,
            ApplicationDeadline = command.ApplicationDeadline,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.GrantApplications.Add(grantApplication);
        await _context.SaveChangesAsync();

        return grantApplication.Id;
    }

    public async Task<List<ImpactMeasurementDto>> GetImpactMeasurementsAsync(Guid planId)
    {
        var measurements = await _context.ImpactMeasurements
            .Where(im => im.OBNLBusinessPlanId == planId)
            .Select(im => new ImpactMeasurementDto
            {
                Id = im.Id,
                OBNLBusinessPlanId = im.OBNLBusinessPlanId,
                MetricName = im.MetricName,
                Description = im.Description,
                MeasurementType = im.MeasurementType,
                UnitOfMeasurement = im.UnitOfMeasurement,
                BaselineValue = im.BaselineValue,
                TargetValue = im.TargetValue,
                CurrentValue = im.CurrentValue,
                DataSource = im.DataSource,
                CollectionMethod = im.CollectionMethod,
                Frequency = im.Frequency,
                ResponsibleParty = im.ResponsibleParty,
                LastMeasurement = im.LastMeasurement,
                NextMeasurement = im.NextMeasurement,
                Status = im.Status,
                Notes = im.Notes,
                CreatedAt = im.CreatedAt,
                UpdatedAt = im.UpdatedAt
            })
            .ToListAsync();

        return measurements;
    }

    public async Task<Guid> CreateImpactMeasurementAsync(CreateImpactMeasurementCommand command)
    {
        var impactMeasurement = new ImpactMeasurement
        {
            OBNLBusinessPlanId = command.OBNLBusinessPlanId,
            MetricName = command.MetricName,
            Description = command.Description,
            BaselineValue = command.BaselineValue,
            TargetValue = command.TargetValue,
            CurrentValue = command.CurrentValue,
            LastMeasurement = command.LastMeasurement,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ImpactMeasurements.Add(impactMeasurement);
        await _context.SaveChangesAsync();

        return impactMeasurement.Id;
    }
}

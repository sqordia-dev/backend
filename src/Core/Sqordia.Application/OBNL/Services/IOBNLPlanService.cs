using Sqordia.Application.OBNL.Commands;
using Sqordia.Application.OBNL.Queries;

namespace Sqordia.Application.OBNL.Services;

public interface IOBNLPlanService
{
    Task<Guid> CreateOBNLPlanAsync(CreateOBNLPlanCommand command);
    Task<OBNLPlanDto> GetOBNLPlanAsync(Guid planId);
    Task<List<OBNLPlanDto>> GetOBNLPlansByOrganizationAsync(Guid organizationId);
    Task UpdateOBNLPlanAsync(UpdateOBNLPlanCommand command);
    Task DeleteOBNLPlanAsync(Guid planId);
    Task<ComplianceAnalysisDto> AnalyzeComplianceAsync(Guid planId);
    Task<List<GrantApplicationDto>> GetGrantApplicationsAsync(Guid planId);
    Task<Guid> CreateGrantApplicationAsync(CreateGrantApplicationCommand command);
    Task<List<ImpactMeasurementDto>> GetImpactMeasurementsAsync(Guid planId);
    Task<Guid> CreateImpactMeasurementAsync(CreateImpactMeasurementCommand command);
}

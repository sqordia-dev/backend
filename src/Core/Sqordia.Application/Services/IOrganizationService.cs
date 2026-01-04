using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Organization;
using Sqordia.Contracts.Responses.Organization;

namespace Sqordia.Application.Services;

public interface IOrganizationService
{
    // Organization CRUD
    Task<Result<OrganizationResponse>> CreateOrganizationAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<Result<OrganizationResponse>> GetOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OrganizationResponse>>> GetUserOrganizationsAsync(CancellationToken cancellationToken = default);
    Task<Result<OrganizationDetailResponse>> GetOrganizationDetailAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Result<OrganizationResponse>> UpdateOrganizationAsync(Guid organizationId, UpdateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Result> DeactivateOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Result> ReactivateOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    
    // Organization Settings
    Task<Result<OrganizationResponse>> UpdateOrganizationSettingsAsync(Guid organizationId, UpdateOrganizationSettingsRequest request, CancellationToken cancellationToken = default);
    
    // Member Management
    Task<Result<OrganizationMemberResponse>> AddMemberAsync(Guid organizationId, AddOrganizationMemberRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OrganizationMemberResponse>>> GetMembersAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<Result<OrganizationMemberResponse>> UpdateMemberRoleAsync(Guid organizationId, Guid memberId, UpdateMemberRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveMemberAsync(Guid organizationId, Guid memberId, CancellationToken cancellationToken = default);
}


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Organization;
using Sqordia.Contracts.Responses.Organization;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

public class OrganizationService : IOrganizationService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<OrganizationService> _logger;
    private readonly ILocalizationService _localizationService;

    public OrganizationService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<OrganizationService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<OrganizationResponse>> CreateOrganizationAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<OrganizationResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var user = await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken);
            if (user == null)
            {
                return Result.Failure<OrganizationResponse>(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Parse organization type
            if (!Enum.TryParse<Domain.Enums.OrganizationType>(request.OrganizationType, out var orgType))
            {
                return Result.Failure<OrganizationResponse>(Error.Validation("Validation.Required", _localizationService.GetString("Validation.Required")));
            }

            var organization = new Organization(request.Name, orgType, request.Description, request.Website);
            organization.CreatedBy = userId.Value.ToString();
            
            _context.Organizations.Add(organization);

            // Add the creator as an owner
            var member = new OrganizationMember(organization.Id, userId.Value, OrganizationRole.Owner);
            _context.OrganizationMembers.Add(member);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Organization {OrganizationName} created by user {UserId}", organization.Name, userId.Value);

            return Result.Success(MapToOrganizationResponse(organization));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization");
            return Result.Failure<OrganizationResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<OrganizationResponse>> GetOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<OrganizationResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            if (organization == null)
            {
                return Result.Failure<OrganizationResponse>(Error.NotFound("Organization.Error.NotFound", _localizationService.GetString("Organization.Error.NotFound")));
            }

            // Check if user is a member
            if (!await IsUserMemberAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure<OrganizationResponse>(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            return Result.Success(MapToOrganizationResponse(organization));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization {OrganizationId}", organizationId);
            return Result.Failure<OrganizationResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<IEnumerable<OrganizationResponse>>> GetUserOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<IEnumerable<OrganizationResponse>>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            // First get the organization IDs the user is a member of
            var organizationIds = await _context.OrganizationMembers
                .Where(om => om.UserId == userId.Value && om.IsActive)
                .Select(om => om.OrganizationId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!organizationIds.Any())
            {
                return Result.Success(Enumerable.Empty<OrganizationResponse>());
            }

            // Then get the organizations with their members
            var organizations = await _context.Organizations
                .Where(o => organizationIds.Contains(o.Id) && !o.IsDeleted)
                .Include(o => o.Members.Where(m => m.IsActive))
                .ToListAsync(cancellationToken);

            var responses = organizations.Select(MapToOrganizationResponse);
            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user organizations");
            return Result.Failure<IEnumerable<OrganizationResponse>>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<OrganizationDetailResponse>> GetOrganizationDetailAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<OrganizationDetailResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var organization = await _context.Organizations
                .Include(o => o.Members.Where(m => m.IsActive))
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            if (organization == null)
            {
                return Result.Failure<OrganizationDetailResponse>(Error.NotFound("Organization.Error.NotFound", _localizationService.GetString("Organization.Error.NotFound")));
            }

            // Check if user is a member
            if (!await IsUserMemberAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure<OrganizationDetailResponse>(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            var response = new OrganizationDetailResponse
            {
                Id = organization.Id,
                Name = organization.Name,
                OrganizationType = organization.OrganizationType.ToString(),
                Description = organization.Description,
                Website = organization.Website,
                LogoUrl = organization.LogoUrl,
                IsActive = organization.IsActive,
                DeactivatedAt = organization.DeactivatedAt,
                MaxMembers = organization.MaxMembers,
                AllowMemberInvites = organization.AllowMemberInvites,
                RequireEmailVerification = organization.RequireEmailVerification,
                Created = organization.Created,
                CreatedBy = organization.CreatedBy,
                Members = organization.Members.Select(m => new OrganizationMemberResponse
                {
                    Id = m.Id,
                    OrganizationId = m.OrganizationId,
                    UserId = m.UserId,
                    Role = m.Role.ToString(),
                    IsActive = m.IsActive,
                    JoinedAt = m.JoinedAt,
                    LeftAt = m.LeftAt,
                    InvitedBy = m.InvitedBy,
                    FirstName = m.User.FirstName,
                    LastName = m.User.LastName,
                    Email = m.User.Email.Value
                })
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization detail {OrganizationId}", organizationId);
            return Result.Failure<OrganizationDetailResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<OrganizationResponse>> UpdateOrganizationAsync(Guid organizationId, UpdateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<OrganizationResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            if (organization == null)
            {
                return Result.Failure<OrganizationResponse>(Error.NotFound("Organization.Error.NotFound", _localizationService.GetString("Organization.Error.NotFound")));
            }

            // Check if user is owner or admin
            if (!await IsUserOwnerOrAdminAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure<OrganizationResponse>(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            organization.UpdateDetails(request.Name, request.Description, request.Website);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Organization {OrganizationId} updated by user {UserId}", organizationId, userId.Value);

            return Result.Success(MapToOrganizationResponse(organization));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization {OrganizationId}", organizationId);
            return Result.Failure<OrganizationResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> DeleteOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            if (organization == null)
            {
                return Result.Failure(Error.NotFound("Organization.Error.NotFound", _localizationService.GetString("Organization.Error.NotFound")));
            }

            // Only owner can delete
            if (!await IsUserOwnerAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            organization.SoftDelete();
            organization.DeletedBy = userId.Value.ToString();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Organization {OrganizationId} deleted by user {UserId}", organizationId, userId.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organization {OrganizationId}", organizationId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> DeactivateOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            if (organization == null)
            {
                return Result.Failure(Error.NotFound("Organization.Error.NotFound", _localizationService.GetString("Organization.Error.NotFound")));
            }

            // Only owner can deactivate
            if (!await IsUserOwnerAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            organization.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Organization {OrganizationId} deactivated by user {UserId}", organizationId, userId.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating organization {OrganizationId}", organizationId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> ReactivateOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            if (organization == null)
            {
                return Result.Failure(Error.NotFound("Organization.Error.NotFound", _localizationService.GetString("Organization.Error.NotFound")));
            }

            // Only owner can reactivate
            if (!await IsUserOwnerAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            organization.Reactivate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Organization {OrganizationId} reactivated by user {UserId}", organizationId, userId.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating organization {OrganizationId}", organizationId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<OrganizationResponse>> UpdateOrganizationSettingsAsync(Guid organizationId, UpdateOrganizationSettingsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<OrganizationResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            if (organization == null)
            {
                return Result.Failure<OrganizationResponse>(Error.NotFound("Organization.Error.NotFound", _localizationService.GetString("Organization.Error.NotFound")));
            }

            // Check if user is owner or admin
            if (!await IsUserOwnerOrAdminAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure<OrganizationResponse>(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            organization.UpdateSettings(request.MaxMembers, request.AllowMemberInvites, request.RequireEmailVerification);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Organization {OrganizationId} settings updated by user {UserId}", organizationId, userId.Value);

            return Result.Success(MapToOrganizationResponse(organization));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization settings {OrganizationId}", organizationId);
            return Result.Failure<OrganizationResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<OrganizationMemberResponse>> AddMemberAsync(Guid organizationId, AddOrganizationMemberRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<OrganizationMemberResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var organization = await _context.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            if (organization == null)
            {
                return Result.Failure<OrganizationMemberResponse>(Error.NotFound("Organization.Error.NotFound", _localizationService.GetString("Organization.Error.NotFound")));
            }

            // Check if user is owner or admin
            if (!await IsUserOwnerOrAdminAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure<OrganizationMemberResponse>(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            // Check if organization can add more members
            if (!organization.CanAddMoreMembers())
            {
                return Result.Failure<OrganizationMemberResponse>(Error.Validation("Organization.Error.MaxMembersReached", _localizationService.GetString("Organization.Error.MaxMembersReached")));
            }

            // Check if user exists
            var userToAdd = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
            if (userToAdd == null)
            {
                return Result.Failure<OrganizationMemberResponse>(Error.NotFound("Auth.Error.UserNotFound", _localizationService.GetString("Auth.Error.UserNotFound")));
            }

            // Check if user is already a member
            var existingMember = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == organizationId && om.UserId == request.UserId && om.IsActive, cancellationToken);

            if (existingMember != null)
            {
                return Result.Failure<OrganizationMemberResponse>(Error.Validation("Organization.Error.MemberAlreadyExists", _localizationService.GetString("Organization.Error.MemberAlreadyExists")));
            }

            // Parse role
            if (!Enum.TryParse<OrganizationRole>(request.Role, out var role))
            {
                return Result.Failure<OrganizationMemberResponse>(Error.Validation("Validation.Required", _localizationService.GetString("Validation.Required")));
            }

            var member = new OrganizationMember(organizationId, request.UserId, role, userId.Value);
            _context.OrganizationMembers.Add(member);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserId} added to organization {OrganizationId} by {InvitedBy}", request.UserId, organizationId, userId.Value);

            return Result.Success(new OrganizationMemberResponse
            {
                Id = member.Id,
                OrganizationId = member.OrganizationId,
                UserId = member.UserId,
                Role = member.Role.ToString(),
                IsActive = member.IsActive,
                JoinedAt = member.JoinedAt,
                LeftAt = member.LeftAt,
                InvitedBy = member.InvitedBy,
                FirstName = userToAdd.FirstName,
                LastName = userToAdd.LastName,
                Email = userToAdd.Email.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to organization {OrganizationId}", organizationId);
            return Result.Failure<OrganizationMemberResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<IEnumerable<OrganizationMemberResponse>>> GetMembersAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<IEnumerable<OrganizationMemberResponse>>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            // Check if user is a member
            if (!await IsUserMemberAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure<IEnumerable<OrganizationMemberResponse>>(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            var members = await _context.OrganizationMembers
                .Where(om => om.OrganizationId == organizationId && om.IsActive)
                .Include(om => om.User)
                .ToListAsync(cancellationToken);

            var responses = members.Select(m => new OrganizationMemberResponse
            {
                Id = m.Id,
                OrganizationId = m.OrganizationId,
                UserId = m.UserId,
                Role = m.Role.ToString(),
                IsActive = m.IsActive,
                JoinedAt = m.JoinedAt,
                LeftAt = m.LeftAt,
                InvitedBy = m.InvitedBy,
                FirstName = m.User.FirstName,
                LastName = m.User.LastName,
                Email = m.User.Email.Value
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members for organization {OrganizationId}", organizationId);
            return Result.Failure<IEnumerable<OrganizationMemberResponse>>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<OrganizationMemberResponse>> UpdateMemberRoleAsync(Guid organizationId, Guid memberId, UpdateMemberRoleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure<OrganizationMemberResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var member = await _context.OrganizationMembers
                .Include(om => om.User)
                .FirstOrDefaultAsync(om => om.Id == memberId && om.OrganizationId == organizationId, cancellationToken);

            if (member == null)
            {
                return Result.Failure<OrganizationMemberResponse>(Error.NotFound("Organization.Error.MemberNotFound", _localizationService.GetString("Organization.Error.MemberNotFound")));
            }

            // Only owner can change roles
            if (!await IsUserOwnerAsync(organizationId, userId.Value, cancellationToken))
            {
                return Result.Failure<OrganizationMemberResponse>(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            // Parse role
            if (!Enum.TryParse<OrganizationRole>(request.Role, out var role))
            {
                return Result.Failure<OrganizationMemberResponse>(Error.Validation("Validation.Required", _localizationService.GetString("Validation.Required")));
            }

            member.UpdateRole(role);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Member {MemberId} role updated to {Role} in organization {OrganizationId} by {UserId}", 
                memberId, role, organizationId, userId.Value);

            return Result.Success(new OrganizationMemberResponse
            {
                Id = member.Id,
                OrganizationId = member.OrganizationId,
                UserId = member.UserId,
                Role = member.Role.ToString(),
                IsActive = member.IsActive,
                JoinedAt = member.JoinedAt,
                LeftAt = member.LeftAt,
                InvitedBy = member.InvitedBy,
                FirstName = member.User.FirstName,
                LastName = member.User.LastName,
                Email = member.User.Email.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member role {MemberId} in organization {OrganizationId}", memberId, organizationId);
            return Result.Failure<OrganizationMemberResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result> RemoveMemberAsync(Guid organizationId, Guid memberId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
            {
                return Result.Failure(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.Id == memberId && om.OrganizationId == organizationId, cancellationToken);

            if (member == null)
            {
                return Result.Failure(Error.NotFound("Organization.Error.MemberNotFound", _localizationService.GetString("Organization.Error.MemberNotFound")));
            }

            // Owner or admin can remove members, or user can remove themselves
            var isOwnerOrAdmin = await IsUserOwnerOrAdminAsync(organizationId, userId.Value, cancellationToken);
            var isSelf = member.UserId == userId.Value;

            if (!isOwnerOrAdmin && !isSelf)
            {
                return Result.Failure(Error.Forbidden("Organization.Error.Forbidden", _localizationService.GetString("Organization.Error.Forbidden")));
            }

            // Prevent removing the last owner
            if (member.Role == OrganizationRole.Owner)
            {
                var ownerCount = await _context.OrganizationMembers
                    .CountAsync(om => om.OrganizationId == organizationId && om.Role == OrganizationRole.Owner && om.IsActive, cancellationToken);

                if (ownerCount <= 1)
                {
                    return Result.Failure(Error.Validation("Organization.Error.CannotRemoveLastOwner", _localizationService.GetString("Organization.Error.CannotRemoveLastOwner")));
                }
            }

            member.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Member {MemberId} removed from organization {OrganizationId} by {UserId}", memberId, organizationId, userId.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member {MemberId} from organization {OrganizationId}", memberId, organizationId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    // Helper methods
    private async Task<bool> IsUserMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == userId && om.IsActive, cancellationToken);
    }

    private async Task<bool> IsUserOwnerAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == userId && om.IsActive && om.Role == OrganizationRole.Owner, cancellationToken);
    }

    private async Task<bool> IsUserOwnerOrAdminAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == organizationId && om.UserId == userId && om.IsActive && 
                (om.Role == OrganizationRole.Owner || om.Role == OrganizationRole.Admin), cancellationToken);
    }

    private static OrganizationResponse MapToOrganizationResponse(Organization organization)
    {
        return new OrganizationResponse
        {
            Id = organization.Id,
            Name = organization.Name,
            OrganizationType = organization.OrganizationType.ToString(),
            Description = organization.Description,
            Website = organization.Website,
            LogoUrl = organization.LogoUrl,
            IsActive = organization.IsActive,
            DeactivatedAt = organization.DeactivatedAt,
            MaxMembers = organization.MaxMembers,
            AllowMemberInvites = organization.AllowMemberInvites,
            RequireEmailVerification = organization.RequireEmailVerification,
            MemberCount = organization.Members.Count(m => m.IsActive),
            Created = organization.Created,
            CreatedBy = organization.CreatedBy
        };
    }
}


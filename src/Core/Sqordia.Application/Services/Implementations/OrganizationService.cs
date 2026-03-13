using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Organization;
using Sqordia.Contracts.Responses.Organization;
using Sqordia.Domain.Constants;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

public class OrganizationService : IOrganizationService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<OrganizationService> _logger;
    private readonly ILocalizationService _localizationService;
    private readonly IOrganizationMembershipCache _membershipCache;
    private readonly INotificationService _notificationService;
    private readonly IFeatureGateService _featureGate;

    public OrganizationService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<OrganizationService> logger,
        ILocalizationService localizationService,
        IOrganizationMembershipCache membershipCache,
        INotificationService notificationService,
        IFeatureGateService featureGate)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _localizationService = localizationService;
        _membershipCache = membershipCache;
        _notificationService = notificationService;
        _featureGate = featureGate;
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

            // Check max organizations limit
            var userOrgCount = await _context.OrganizationMembers
                .CountAsync(om => om.UserId == userId.Value && om.IsActive, cancellationToken);

            if (userOrgCount > 0)
            {
                var primaryOrgId = await _context.OrganizationMembers
                    .Where(om => om.UserId == userId.Value && om.IsActive)
                    .OrderBy(om => om.Created)
                    .Select(om => om.OrganizationId)
                    .FirstAsync(cancellationToken);

                var orgLimit = await _featureGate.GetLimitAsync(primaryOrgId, PlanFeatures.MaxOrganizations, cancellationToken);
                if (orgLimit != -1 && userOrgCount >= orgLimit)
                {
                    return Result.Failure<OrganizationResponse>(Error.Validation(
                        "Organization.Error.MaxOrganizationsReached",
                        $"Your plan allows a maximum of {orgLimit} organization(s). Upgrade your plan to create more."));
                }
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
                CreatedAt = organization.Created,
                CreatedBy = organization.CreatedBy,
                Industry = organization.Industry,
                Sector = organization.Sector,
                LegalForm = organization.LegalForm,
                TeamSize = organization.TeamSize,
                FundingStatus = organization.FundingStatus,
                TargetMarket = organization.TargetMarket,
                BusinessStage = organization.BusinessStage,
                GoalsJson = organization.GoalsJson,
                City = organization.City,
                Province = organization.Province,
                Country = organization.Country,
                ProfileCompletenessScore = organization.ProfileCompletenessScore,
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

            // Update business context if any fields are provided
            if (request.Industry != null || request.Sector != null || request.LegalForm != null || request.TeamSize != null ||
                request.FundingStatus != null || request.TargetMarket != null || request.BusinessStage != null ||
                request.GoalsJson != null || request.City != null || request.Province != null || request.Country != null)
            {
                organization.UpdateBusinessContext(
                    request.Industry ?? organization.Industry,
                    request.Sector ?? organization.Sector,
                    request.TeamSize ?? organization.TeamSize,
                    request.FundingStatus ?? organization.FundingStatus,
                    request.TargetMarket ?? organization.TargetMarket,
                    request.BusinessStage ?? organization.BusinessStage,
                    request.GoalsJson ?? organization.GoalsJson,
                    request.City ?? organization.City,
                    request.Province ?? organization.Province,
                    request.Country ?? organization.Country,
                    request.LegalForm ?? organization.LegalForm);
            }

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

            // Check team member limit via feature gate
            var memberLimit = await _featureGate.GetLimitAsync(organizationId, PlanFeatures.MaxTeamMembers, cancellationToken);
            var activeMemberCount = organization.Members.Count(m => m.IsActive);
            if (memberLimit != -1 && activeMemberCount >= memberLimit)
            {
                return Result.Failure<OrganizationMemberResponse>(Error.Validation(
                    "Organization.Error.MaxMembersReached",
                    $"Your plan allows a maximum of {memberLimit} team member(s). Upgrade your plan to add more."));
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

            // Invalidate membership cache for the new member
            _membershipCache.InvalidateMembership(organizationId, request.UserId);

            _logger.LogInformation("User {UserId} added to organization {OrganizationId} by {InvitedBy}", request.UserId, organizationId, userId.Value);

            // Send in-app notification to the new member
            try
            {
                await _notificationService.CreateNotificationAsync(
                    new CreateNotificationCommand(
                        request.UserId,
                        NotificationType.OrganizationInvitation,
                        NotificationCategory.Organization,
                        $"Vous avez été ajouté à {organization.Name}",
                        $"You have been added to {organization.Name}",
                        $"Vous avez rejoint l'organisation {organization.Name} en tant que {role}.",
                        $"You have joined the organization {organization.Name} as {role}.",
                        ActionUrl: "/dashboard",
                        RelatedEntityId: organizationId),
                    cancellationToken);
            }
            catch (Exception notifEx)
            {
                _logger.LogWarning(notifEx, "Failed to create organization invitation notification for user {UserId}", request.UserId);
            }

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

            // Invalidate membership cache for the removed member
            _membershipCache.InvalidateMembership(organizationId, member.UserId);

            _logger.LogInformation("Member {MemberId} removed from organization {OrganizationId} by {UserId}", memberId, organizationId, userId.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member {MemberId} from organization {OrganizationId}", memberId, organizationId);
            return Result.Failure(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    // Helper methods - using cached membership service for high-frequency checks
    private async Task<bool> IsUserMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        return await _membershipCache.IsUserMemberAsync(organizationId, userId, cancellationToken);
    }

    private async Task<bool> IsUserOwnerAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        return await _membershipCache.IsUserOwnerAsync(organizationId, userId, cancellationToken);
    }

    private async Task<bool> IsUserOwnerOrAdminAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        var role = await _membershipCache.GetUserRoleAsync(organizationId, userId, cancellationToken);
        return role == OrganizationRole.Owner.ToString() || role == OrganizationRole.Admin.ToString();
    }

    // ── Invitation Management ────────────────────────────────────────────────

    public async Task<Result<InvitationResponse>> InviteMemberByEmailAsync(
        Guid organizationId, InviteMemberByEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure<InvitationResponse>(Error.Unauthorized("Organization.Error.Unauthorized", "User not authenticated"));

            if (!await IsUserOwnerOrAdminAsync(organizationId, userId.Value, cancellationToken))
                return Result.Failure<InvitationResponse>(Error.Forbidden("Organization.Error.Forbidden", "Only owners and admins can invite members"));

            var organization = await _context.Organizations
                .Include(o => o.Members.Where(m => m.IsActive))
                .FirstOrDefaultAsync(o => o.Id == organizationId && !o.IsDeleted, cancellationToken);

            if (organization == null)
                return Result.Failure<InvitationResponse>(Error.NotFound("Organization.Error.NotFound", "Organization not found"));

            var email = request.Email.ToLowerInvariant();

            // Check if user is already a member
            var existingMember = await _context.OrganizationMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId
                    && m.User.Email != null && m.User.Email.Value.ToLower() == email
                    && m.IsActive && !m.IsDeleted, cancellationToken);

            if (existingMember != null)
                return Result.Failure<InvitationResponse>(Error.Conflict("Organization.Error.MemberAlreadyExists", "User is already a member of this organization"));

            // Check for existing pending invitation
            var existingInvitation = await _context.OrganizationInvitations
                .FirstOrDefaultAsync(i => i.OrganizationId == organizationId
                    && i.Email == email
                    && i.Status == InvitationStatus.Pending
                    && !i.IsDeleted, cancellationToken);

            if (existingInvitation != null)
                return Result.Failure<InvitationResponse>(Error.Conflict("Organization.Error.InvitationAlreadyPending", "An invitation is already pending for this email"));

            // Check member limit
            if (!organization.CanAddMoreMembers())
            {
                var limitCheck = await _featureGate.CheckUsageLimitAsync(organizationId, PlanFeatures.MaxTeamMembers, cancellationToken);
                if (limitCheck.IsSuccess && !limitCheck.Value!.Allowed)
                    return Result.Failure<InvitationResponse>(Error.Validation("Organization.Error.MaxMembersReached", "Organization has reached its member limit"));
            }

            // Parse role
            if (!Enum.TryParse<OrganizationRole>(request.Role, true, out var role))
                return Result.Failure<InvitationResponse>(Error.Validation("Organization.Error.InvalidRole", "Invalid role. Must be: Admin, Member, or Viewer"));

            // Prevent inviting as Owner
            if (role == OrganizationRole.Owner)
                return Result.Failure<InvitationResponse>(Error.Validation("Organization.Error.CannotInviteAsOwner", "Cannot invite users as Owner. Use role transfer instead."));

            var invitation = new OrganizationInvitation(organizationId, email, role, userId.Value);
            _context.OrganizationInvitations.Add(invitation);
            await _context.SaveChangesAsync(cancellationToken);

            // Send notification
            try
            {
                var inviter = await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken);
                var inviterName = inviter != null ? $"{inviter.FirstName} {inviter.LastName}".Trim() : "A team member";

                await _notificationService.CreateNotificationAsync(
                    new CreateNotificationCommand(
                        userId.Value,
                        NotificationType.OrganizationInvitation,
                        NotificationCategory.Organization,
                        $"Invitation envoyée à {email}",
                        $"Invitation sent to {email}",
                        $"{inviterName} a invité {email} à rejoindre {organization.Name}.",
                        $"{inviterName} invited {email} to join {organization.Name}.",
                        ActionUrl: "/dashboard",
                        RelatedEntityId: organizationId),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send invitation notification for {Email}", email);
            }

            var invitedBy = await _context.Users.FindAsync(new object[] { userId.Value }, cancellationToken);

            return Result.Success(new InvitationResponse
            {
                Id = invitation.Id,
                OrganizationId = organizationId,
                Email = invitation.Email,
                Role = invitation.Role.ToString(),
                Status = invitation.Status.ToString(),
                InvitedByName = invitedBy != null ? $"{invitedBy.FirstName} {invitedBy.LastName}".Trim() : "",
                CreatedAt = invitation.Created,
                ExpiresAt = invitation.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inviting member by email to organization {OrganizationId}", organizationId);
            return Result.Failure<InvitationResponse>(Error.Failure("Organization.Error.InviteFailed", "Failed to send invitation"));
        }
    }

    public async Task<Result<IEnumerable<InvitationResponse>>> GetPendingInvitationsAsync(
        Guid organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure<IEnumerable<InvitationResponse>>(Error.Unauthorized("Organization.Error.Unauthorized", "User not authenticated"));

            if (!await IsUserOwnerOrAdminAsync(organizationId, userId.Value, cancellationToken))
                return Result.Failure<IEnumerable<InvitationResponse>>(Error.Forbidden("Organization.Error.Forbidden", "Only owners and admins can view invitations"));

            var invitations = await _context.OrganizationInvitations
                .Include(i => i.InvitedByUser)
                .Where(i => i.OrganizationId == organizationId
                    && i.Status == InvitationStatus.Pending
                    && !i.IsDeleted)
                .OrderByDescending(i => i.Created)
                .ToListAsync(cancellationToken);

            // Auto-expire
            foreach (var inv in invitations.Where(i => i.IsExpired()))
            {
                inv.MarkExpired();
            }
            await _context.SaveChangesAsync(cancellationToken);

            var result = invitations
                .Where(i => i.Status == InvitationStatus.Pending)
                .Select(i => new InvitationResponse
                {
                    Id = i.Id,
                    OrganizationId = i.OrganizationId,
                    Email = i.Email,
                    Role = i.Role.ToString(),
                    Status = i.Status.ToString(),
                    InvitedByName = i.InvitedByUser != null ? $"{i.InvitedByUser.FirstName} {i.InvitedByUser.LastName}".Trim() : "",
                    CreatedAt = i.Created,
                    ExpiresAt = i.ExpiresAt
                });

            return Result.Success(result.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending invitations for organization {OrganizationId}", organizationId);
            return Result.Failure<IEnumerable<InvitationResponse>>(Error.Failure("Organization.Error.GetInvitationsFailed", "Failed to get invitations"));
        }
    }

    public async Task<Result> CancelInvitationAsync(
        Guid organizationId, Guid invitationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUserService.GetUserIdAsGuid();
            if (userId == null)
                return Result.Failure(Error.Unauthorized("Organization.Error.Unauthorized", "User not authenticated"));

            if (!await IsUserOwnerOrAdminAsync(organizationId, userId.Value, cancellationToken))
                return Result.Failure(Error.Forbidden("Organization.Error.Forbidden", "Only owners and admins can cancel invitations"));

            var invitation = await _context.OrganizationInvitations
                .FirstOrDefaultAsync(i => i.Id == invitationId
                    && i.OrganizationId == organizationId
                    && !i.IsDeleted, cancellationToken);

            if (invitation == null)
                return Result.Failure(Error.NotFound("Organization.Error.InvitationNotFound", "Invitation not found"));

            if (invitation.Status != InvitationStatus.Pending)
                return Result.Failure(Error.Validation("Organization.Error.InvitationNotPending", "Only pending invitations can be cancelled"));

            invitation.Cancel();
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invitation {InvitationId}", invitationId);
            return Result.Failure(Error.Failure("Organization.Error.CancelInvitationFailed", "Failed to cancel invitation"));
        }
    }

    public async Task<Result<OrganizationMemberResponse>> AcceptInvitationAsync(
        Guid token, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invitation = await _context.OrganizationInvitations
                .Include(i => i.Organization)
                .FirstOrDefaultAsync(i => i.Token == token && !i.IsDeleted, cancellationToken);

            if (invitation == null)
                return Result.Failure<OrganizationMemberResponse>(Error.NotFound("Organization.Error.InvitationNotFound", "Invitation not found or has been revoked"));

            if (invitation.Status != InvitationStatus.Pending)
                return Result.Failure<OrganizationMemberResponse>(Error.Validation("Organization.Error.InvitationNotPending", $"This invitation has been {invitation.Status.ToString().ToLower()}"));

            if (invitation.IsExpired())
            {
                invitation.MarkExpired();
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Failure<OrganizationMemberResponse>(Error.Validation("Organization.Error.InvitationExpired", "This invitation has expired"));
            }

            // Verify user email matches invitation
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
                return Result.Failure<OrganizationMemberResponse>(Error.NotFound("Organization.Error.UserNotFound", "User not found"));

            if (user.Email?.ToString().ToLowerInvariant() != invitation.Email)
                return Result.Failure<OrganizationMemberResponse>(Error.Validation("Organization.Error.EmailMismatch", "Your email does not match the invitation"));

            // Check if already a member
            var existingMember = await _context.OrganizationMembers
                .FirstOrDefaultAsync(m => m.OrganizationId == invitation.OrganizationId
                    && m.UserId == userId
                    && m.IsActive && !m.IsDeleted, cancellationToken);

            if (existingMember != null)
            {
                invitation.Accept(userId);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Failure<OrganizationMemberResponse>(Error.Conflict("Organization.Error.MemberAlreadyExists", "You are already a member of this organization"));
            }

            // Create membership
            var member = new OrganizationMember(invitation.OrganizationId, userId, invitation.Role, invitation.InvitedByUserId);
            _context.OrganizationMembers.Add(member);

            invitation.Accept(userId);
            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _membershipCache.InvalidateMembership(invitation.OrganizationId, userId);

            return Result.Success(new OrganizationMemberResponse
            {
                Id = member.Id,
                OrganizationId = member.OrganizationId,
                UserId = member.UserId,
                Role = member.Role.ToString(),
                IsActive = member.IsActive,
                JoinedAt = member.JoinedAt,
                InvitedBy = member.InvitedBy,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email?.ToString() ?? ""
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation {Token}", token);
            return Result.Failure<OrganizationMemberResponse>(Error.Failure("Organization.Error.AcceptInvitationFailed", "Failed to accept invitation"));
        }
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
            CreatedAt = organization.Created,
            CreatedBy = organization.CreatedBy,
            Industry = organization.Industry,
            Sector = organization.Sector,
            LegalForm = organization.LegalForm,
            TeamSize = organization.TeamSize,
            FundingStatus = organization.FundingStatus,
            TargetMarket = organization.TargetMarket,
            BusinessStage = organization.BusinessStage,
            GoalsJson = organization.GoalsJson,
            City = organization.City,
            Province = organization.Province,
            Country = organization.Country,
            ProfileCompletenessScore = organization.ProfileCompletenessScore
        };
    }
}


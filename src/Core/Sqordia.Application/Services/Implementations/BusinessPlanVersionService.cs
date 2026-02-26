using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

public class BusinessPlanVersionService : IBusinessPlanVersionService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BusinessPlanVersionService> _logger;
    private readonly ILocalizationService _localizationService;

    public BusinessPlanVersionService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<BusinessPlanVersionService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<BusinessPlanVersionResponse>> CreateVersionAsync(
        Guid businessPlanId,
        string? comment = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<BusinessPlanVersionResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<BusinessPlanVersionResponse>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has access
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == businessPlan.OrganizationId && 
                                          om.UserId == currentUserId.Value && 
                                          om.IsActive, cancellationToken);

            if (member == null)
            {
                return Result.Failure<BusinessPlanVersionResponse>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            // Get next version number
            var maxVersion = await _context.BusinessPlanVersions
                .Where(bv => bv.BusinessPlanId == businessPlanId)
                .Select(bv => bv.VersionNumber)
                .DefaultIfEmpty(0)
                .MaxAsync(cancellationToken);

            var versionNumber = maxVersion + 1;

            // Create version snapshot
            var version = new BusinessPlanVersion(businessPlanId, versionNumber, businessPlan, comment);
            version.CreatedBy = currentUserId.Value.ToString();
            _context.BusinessPlanVersions.Add(version);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Version {VersionNumber} created for business plan {PlanId} by {UserId}", 
                versionNumber, businessPlanId, currentUserId.Value);

            var response = new BusinessPlanVersionResponse
            {
                Id = version.Id,
                BusinessPlanId = version.BusinessPlanId,
                VersionNumber = version.VersionNumber,
                Comment = version.Comment,
                Title = version.Title,
                Description = version.Description,
                PlanType = version.PlanType,
                Status = version.Status,
                Created = version.Created,
                CreatedBy = version.CreatedBy ?? string.Empty,
                ContentPreview = version.ExecutiveSummary != null && version.ExecutiveSummary.Length > 200 
                    ? version.ExecutiveSummary.Substring(0, 200) + "..." 
                    : version.ExecutiveSummary
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating version for business plan {PlanId}", businessPlanId);
            return Result.Failure<BusinessPlanVersionResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<IEnumerable<BusinessPlanVersionResponse>>> GetVersionsAsync(
        Guid businessPlanId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<IEnumerable<BusinessPlanVersionResponse>>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<IEnumerable<BusinessPlanVersionResponse>>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has access
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == businessPlan.OrganizationId && 
                                          om.UserId == currentUserId.Value && 
                                          om.IsActive, cancellationToken);

            if (member == null)
            {
                return Result.Failure<IEnumerable<BusinessPlanVersionResponse>>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            var versions = await _context.BusinessPlanVersions
                .Where(bv => bv.BusinessPlanId == businessPlanId)
                .OrderByDescending(bv => bv.VersionNumber)
                .ToListAsync(cancellationToken);

            var responses = versions.Select(version => new BusinessPlanVersionResponse
            {
                Id = version.Id,
                BusinessPlanId = version.BusinessPlanId,
                VersionNumber = version.VersionNumber,
                Comment = version.Comment,
                Title = version.Title,
                Description = version.Description,
                PlanType = version.PlanType,
                Status = version.Status,
                Created = version.Created,
                CreatedBy = version.CreatedBy ?? string.Empty,
                ContentPreview = version.ExecutiveSummary != null && version.ExecutiveSummary.Length > 200 
                    ? version.ExecutiveSummary.Substring(0, 200) + "..." 
                    : version.ExecutiveSummary
            });

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting versions for business plan {PlanId}", businessPlanId);
            return Result.Failure<IEnumerable<BusinessPlanVersionResponse>>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<BusinessPlanVersionResponse>> GetVersionAsync(
        Guid businessPlanId, 
        int versionNumber, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<BusinessPlanVersionResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<BusinessPlanVersionResponse>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has access
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == businessPlan.OrganizationId && 
                                          om.UserId == currentUserId.Value && 
                                          om.IsActive, cancellationToken);

            if (member == null)
            {
                return Result.Failure<BusinessPlanVersionResponse>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            var version = await _context.BusinessPlanVersions
                .FirstOrDefaultAsync(bv => bv.BusinessPlanId == businessPlanId && bv.VersionNumber == versionNumber, cancellationToken);

            if (version == null)
            {
                return Result.Failure<BusinessPlanVersionResponse>(Error.NotFound("Version.Error.NotFound", "Version not found"));
            }

            var response = new BusinessPlanVersionResponse
            {
                Id = version.Id,
                BusinessPlanId = version.BusinessPlanId,
                VersionNumber = version.VersionNumber,
                Comment = version.Comment,
                Title = version.Title,
                Description = version.Description,
                PlanType = version.PlanType,
                Status = version.Status,
                Created = version.Created,
                CreatedBy = version.CreatedBy ?? string.Empty,
                ContentPreview = version.ExecutiveSummary != null && version.ExecutiveSummary.Length > 200 
                    ? version.ExecutiveSummary.Substring(0, 200) + "..." 
                    : version.ExecutiveSummary
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version {VersionNumber} for business plan {PlanId}", versionNumber, businessPlanId);
            return Result.Failure<BusinessPlanVersionResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }

    public async Task<Result<BusinessPlanResponse>> RestoreVersionAsync(
        Guid businessPlanId, 
        int versionNumber, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<BusinessPlanResponse>(Error.Unauthorized("General.Unauthorized", _localizationService.GetString("General.Unauthorized")));
            }

            var businessPlan = await _context.BusinessPlans
                .Include(bp => bp.Organization)
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<BusinessPlanResponse>(Error.NotFound("BusinessPlan.Error.NotFound", _localizationService.GetString("BusinessPlan.Error.NotFound")));
            }

            // Verify user has access and is Owner or Admin
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(om => om.OrganizationId == businessPlan.OrganizationId && 
                                          om.UserId == currentUserId.Value && 
                                          om.IsActive, cancellationToken);

            if (member == null || (member.Role != OrganizationRole.Owner && member.Role != OrganizationRole.Admin))
            {
                return Result.Failure<BusinessPlanResponse>(Error.Forbidden("BusinessPlan.Error.Forbidden", _localizationService.GetString("BusinessPlan.Error.Forbidden")));
            }

            var version = await _context.BusinessPlanVersions
                .FirstOrDefaultAsync(bv => bv.BusinessPlanId == businessPlanId && bv.VersionNumber == versionNumber, cancellationToken);

            if (version == null)
            {
                return Result.Failure<BusinessPlanResponse>(Error.NotFound("Version.Error.NotFound", "Version not found"));
            }

            // Create a new version snapshot of current state before restoring
            var maxVersion = await _context.BusinessPlanVersions
                .Where(bv => bv.BusinessPlanId == businessPlanId)
                .Select(bv => bv.VersionNumber)
                .DefaultIfEmpty(0)
                .MaxAsync(cancellationToken);

            var backupVersion = new BusinessPlanVersion(businessPlanId, maxVersion + 1, businessPlan, "Backup before restore");
            backupVersion.CreatedBy = currentUserId.Value.ToString();
            _context.BusinessPlanVersions.Add(backupVersion);

            // Restore content from version
            businessPlan.UpdateTitle(version.Title);
            businessPlan.UpdateDescription(version.Description);
            businessPlan.UpdateExecutiveSummary(version.ExecutiveSummary);
            businessPlan.UpdateProblemStatement(version.ProblemStatement);
            businessPlan.UpdateSolution(version.Solution);
            businessPlan.UpdateMarketAnalysis(version.MarketAnalysis);
            businessPlan.UpdateCompetitiveAnalysis(version.CompetitiveAnalysis);
            businessPlan.UpdateSwotAnalysis(version.SwotAnalysis);
            businessPlan.UpdateBusinessModel(version.BusinessModel);
            businessPlan.UpdateMarketingStrategy(version.MarketingStrategy);
            businessPlan.UpdateBrandingStrategy(version.BrandingStrategy);
            businessPlan.UpdateOperationsPlan(version.OperationsPlan);
            businessPlan.UpdateManagementTeam(version.ManagementTeam);
            businessPlan.UpdateFinancialProjections(version.FinancialProjections);
            businessPlan.UpdateFundingRequirements(version.FundingRequirements);
            businessPlan.UpdateRiskAnalysis(version.RiskAnalysis);
            businessPlan.UpdateExitStrategy(version.ExitStrategy);
            businessPlan.UpdateAppendixData(version.AppendixData);
            businessPlan.UpdateMissionStatement(version.MissionStatement);
            businessPlan.UpdateSocialImpact(version.SocialImpact);
            businessPlan.UpdateBeneficiaryProfile(version.BeneficiaryProfile);
            businessPlan.UpdateGrantStrategy(version.GrantStrategy);
            businessPlan.UpdateSustainabilityPlan(version.SustainabilityPlan);
            
            businessPlan.IncrementVersion();
            businessPlan.LastModifiedBy = currentUserId.Value.ToString();

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Business plan {PlanId} restored to version {VersionNumber} by {UserId}", 
                businessPlanId, versionNumber, currentUserId.Value);

            var response = new BusinessPlanResponse
            {
                Id = businessPlan.Id,
                Title = businessPlan.Title,
                Description = businessPlan.Description,
                PlanType = businessPlan.PlanType.ToString(),
                Status = businessPlan.Status.ToString(),
                OrganizationId = businessPlan.OrganizationId,
                OrganizationName = businessPlan.Organization.Name,
                Version = businessPlan.Version,
                TotalQuestions = businessPlan.TotalQuestions,
                CompletedQuestions = businessPlan.CompletedQuestions,
                CompletionPercentage = businessPlan.CompletionPercentage,
                QuestionnaireCompletedAt = businessPlan.QuestionnaireCompletedAt,
                GenerationStartedAt = businessPlan.GenerationStartedAt,
                GenerationCompletedAt = businessPlan.GenerationCompletedAt,
                FinalizedAt = businessPlan.FinalizedAt,
                Created = businessPlan.Created,
                LastModified = businessPlan.LastModified,
                CreatedBy = businessPlan.CreatedBy ?? string.Empty
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring version {VersionNumber} for business plan {PlanId}", versionNumber, businessPlanId);
            return Result.Failure<BusinessPlanResponse>(Error.InternalServerError("General.InternalServerError", _localizationService.GetString("General.InternalServerError")));
        }
    }
}


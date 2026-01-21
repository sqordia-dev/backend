using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.V2.StrategyMap;
using Sqordia.Contracts.Responses.V2.StrategyMap;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Application.Services.V2.Implementations;

/// <summary>
/// Strategy map management service
/// Handles saving/retrieving strategy maps and triggering recalculations
/// </summary>
public class StrategyMapService : IStrategyMapService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IReadinessScoreService _readinessScoreService;
    private readonly ILogger<StrategyMapService> _logger;

    public StrategyMapService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IReadinessScoreService readinessScoreService,
        ILogger<StrategyMapService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _readinessScoreService = readinessScoreService;
        _logger = logger;
    }

    public async Task<Result<StrategyMapResponse>> SaveStrategyMapAsync(
        Guid businessPlanId,
        SaveStrategyMapRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Saving strategy map for business plan {PlanId}", businessPlanId);

            var businessPlan = await GetBusinessPlanWithAccessCheckAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<StrategyMapResponse>(
                    Error.NotFound("BusinessPlan.NotFound", "Business plan not found or access denied."));
            }

            // Validate JSON structure
            if (!IsValidJson(request.StrategyMapJson))
            {
                return Result.Failure<StrategyMapResponse>(
                    Error.Validation("StrategyMap.InvalidJson", "The strategy map JSON is not valid."));
            }

            // Update the strategy map
            businessPlan.UpdateStrategyMap(request.StrategyMapJson);

            // Trigger recalculation if requested
            if (request.TriggerRecalculation)
            {
                _logger.LogInformation("Triggering readiness score recalculation for business plan {PlanId}", businessPlanId);
                var recalcResult = await _readinessScoreService.RecalculateAndSaveAsync(businessPlanId, cancellationToken);
                if (recalcResult.IsSuccess)
                {
                    businessPlan.UpdateReadinessScore(recalcResult.Value);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Strategy map saved successfully for business plan {PlanId}", businessPlanId);

            return Result.Success(MapToResponse(businessPlan));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving strategy map for business plan {PlanId}", businessPlanId);
            return Result.Failure<StrategyMapResponse>(
                Error.InternalServerError("StrategyMap.SaveError", "An error occurred while saving the strategy map."));
        }
    }

    public async Task<Result<StrategyMapResponse>> GetStrategyMapAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting strategy map for business plan {PlanId}", businessPlanId);

            var businessPlan = await GetBusinessPlanWithAccessCheckAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<StrategyMapResponse>(
                    Error.NotFound("BusinessPlan.NotFound", "Business plan not found or access denied."));
            }

            return Result.Success(MapToResponse(businessPlan));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting strategy map for business plan {PlanId}", businessPlanId);
            return Result.Failure<StrategyMapResponse>(
                Error.InternalServerError("StrategyMap.GetError", "An error occurred while retrieving the strategy map."));
        }
    }

    private async Task<BusinessPlan?> GetBusinessPlanWithAccessCheckAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            return null;
        }

        var businessPlan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null)
        {
            return null;
        }

        var isMember = await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                           om.UserId == currentUserId &&
                           om.IsActive, cancellationToken);

        return isMember ? businessPlan : null;
    }

    private static StrategyMapResponse MapToResponse(BusinessPlan businessPlan)
    {
        return new StrategyMapResponse
        {
            BusinessPlanId = businessPlan.Id,
            StrategyMapJson = businessPlan.StrategyMapJson,
            ReadinessScore = businessPlan.ReadinessScore,
            HealthMetrics = businessPlan.HealthMetrics != null
                ? new FinancialHealthMetricsResponse
                {
                    PivotPointMonth = businessPlan.HealthMetrics.PivotPointMonth,
                    RunwayMonths = businessPlan.HealthMetrics.RunwayMonths,
                    MonthlyBurnRate = businessPlan.HealthMetrics.MonthlyBurnRate,
                    TargetCAC = businessPlan.HealthMetrics.TargetCAC
                }
                : null,
            LastUpdated = businessPlan.LastModified ?? businessPlan.Created
        };
    }

    private static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

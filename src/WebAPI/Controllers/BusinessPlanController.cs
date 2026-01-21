using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Application.Services.V2;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Requests.V2.StrategyMap;
using Sqordia.Contracts.Responses.BusinessPlan;
using Sqordia.Contracts.Responses;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans")]
[Authorize]
public class BusinessPlanController : BaseApiController
{
    private readonly IBusinessPlanService _businessPlanService;
    private readonly IStrategyMapService _strategyMapService;
    private readonly IReadinessScoreService _readinessScoreService;
    private readonly IAuditService _auditService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<BusinessPlanController> _logger;

    public BusinessPlanController(
        IBusinessPlanService businessPlanService,
        IStrategyMapService strategyMapService,
        IReadinessScoreService readinessScoreService,
        IAuditService auditService,
        IApplicationDbContext context,
        ILogger<BusinessPlanController> logger)
    {
        _businessPlanService = businessPlanService;
        _strategyMapService = strategyMapService;
        _readinessScoreService = readinessScoreService;
        _auditService = auditService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new business plan
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBusinessPlan([FromBody] CreateBusinessPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessPlanService.CreateBusinessPlanAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a business plan by ID
    /// </summary>
    /// <param name="id">The business plan ID</param>
    /// <param name="sections">Whether to include structured sections in the response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business plan with optional structured sections</returns>
    /// <remarks>
    /// Returns a business plan by ID. When the sections parameter is true, the response will include
    /// structured section data with content, metadata, and completion status.
    /// 
    /// Query parameters:
    /// - sections (optional): Include structured sections in the response
    /// 
    /// Sample request:
    /// GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6?sections=true
    /// 
    /// Sample response with sections:
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "title": "My Business Plan",
    ///   "description": "A comprehensive business plan",
    ///   "planType": "BusinessPlan",
    ///   "status": "Draft",
    ///   "sections": {
    ///     "executive-summary": {
    ///       "content": "Our company provides...",
    ///       "hasContent": true,
    ///       "wordCount": 150,
    ///       "lastUpdated": "2025-01-14T10:30:00Z",
    ///       "isAIGenerated": true,
    ///       "status": "draft"
    ///     }
    ///   }
    /// }
    /// </remarks>
    /// <response code="200">Business plan details</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusinessPlan(
        Guid id, 
        [FromQuery] bool sections = false, 
        CancellationToken cancellationToken = default)
    {
        var result = await _businessPlanService.GetBusinessPlanAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all business plans for the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserBusinessPlans(CancellationToken cancellationToken)
    {
        var result = await _businessPlanService.GetUserBusinessPlansAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all business plans for an organization
    /// </summary>
    [HttpGet("organizations/{organizationId}")]
    public async Task<IActionResult> GetOrganizationBusinessPlans(Guid organizationId, CancellationToken cancellationToken)
    {
        var result = await _businessPlanService.GetOrganizationBusinessPlansAsync(organizationId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update a business plan
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBusinessPlan(Guid id, [FromBody] UpdateBusinessPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessPlanService.UpdateBusinessPlanAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a business plan
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBusinessPlan(Guid id, CancellationToken cancellationToken)
    {
        var result = await _businessPlanService.DeleteBusinessPlanAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Archive a business plan
    /// </summary>
    [HttpPost("{id}/archive")]
    public async Task<IActionResult> ArchiveBusinessPlan(Guid id, CancellationToken cancellationToken)
    {
        var result = await _businessPlanService.ArchiveBusinessPlanAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Unarchive a business plan
    /// </summary>
    [HttpPost("{id}/unarchive")]
    public async Task<IActionResult> UnarchiveBusinessPlan(Guid id, CancellationToken cancellationToken)
    {
        var result = await _businessPlanService.UnarchiveBusinessPlanAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Duplicate a business plan
    /// </summary>
    /// <param name="id">The business plan ID to duplicate</param>
    /// <param name="request">Optional new title for the duplicated plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The duplicated business plan</returns>
    /// <remarks>
    /// Creates a complete copy of a business plan including all sections, questionnaire responses, and financial projections.
    /// The new plan will have "Copie de [original title]" as the default title unless a custom title is provided.
    /// 
    /// Sample request:
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/duplicate
    ///     {
    ///         "newTitle": "My Business Plan - Copy"
    ///     }
    /// </remarks>
    /// <response code="200">Business plan duplicated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="403">Forbidden - user doesn't have access to the business plan</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("{id}/duplicate")]
    [ProducesResponseType(typeof(BusinessPlanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DuplicateBusinessPlan(
        Guid id, 
        [FromBody] DuplicateBusinessPlanRequest? request = null, 
        CancellationToken cancellationToken = default)
    {
        var result = await _businessPlanService.DuplicateBusinessPlanAsync(id, request?.NewTitle, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all available business plan types with descriptions
    /// </summary>
    /// <returns>List of available business plan types</returns>
    /// <remarks>
    /// Returns all available business plan types with their descriptions, focus areas, and characteristics.
    /// This endpoint helps users understand which plan type is most suitable for their needs.
    /// 
    /// Available plan types:
    /// - BusinessPlan (0): Traditional business plan for startups/SMEs
    /// - StrategicPlan (1): Strategic plan for non-profits (OBNL)
    /// - LeanCanvas (2): One-page lean business plan
    /// 
    /// Sample response:
    /// [
    ///   {
    ///     "id": 0,
    ///     "name": "BusinessPlan",
    ///     "description": "Traditional business plan for startups/SMEs",
    ///     "focus": "Revenue, market, profitability, growth",
    ///     "targetAudience": "Startups, SMEs, investors",
    ///     "useCases": "Funding applications, business development, strategic planning",
    ///     "estimatedHours": 8,
    ///     "typicalSections": 15,
    ///     "isNonProfitFriendly": false,
    ///     "isStartupFriendly": true,
    ///     "isEstablishedBusinessFriendly": true
    ///   }
    /// ]
    /// </remarks>
    /// <response code="200">List of business plan types</response>
    [HttpGet("plan-types")]
    [ProducesResponseType(typeof(List<PlanTypeDto>), StatusCodes.Status200OK)]
    public IActionResult GetPlanTypes()
    {
        var planTypes = new List<PlanTypeDto>
        {
            new PlanTypeDto
            {
                Id = (int)BusinessPlanType.BusinessPlan,
                Name = "BusinessPlan",
                Description = "Traditional business plan for startups/SMEs",
                Focus = "Revenue, market, profitability, growth",
                TargetAudience = "Startups, SMEs, investors",
                UseCases = "Funding applications, business development, strategic planning",
                EstimatedHours = 8,
                TypicalSections = 15,
                IsNonProfitFriendly = false,
                IsStartupFriendly = true,
                IsEstablishedBusinessFriendly = true
            },
            new PlanTypeDto
            {
                Id = (int)BusinessPlanType.StrategicPlan,
                Name = "StrategicPlan",
                Description = "Strategic plan for non-profits (OBNL)",
                Focus = "Mission, impact, grants, beneficiaries",
                TargetAudience = "Non-profits, OBNL organizations, grant providers",
                UseCases = "Grant applications, impact reporting, strategic planning",
                EstimatedHours = 6,
                TypicalSections = 12,
                IsNonProfitFriendly = true,
                IsStartupFriendly = false,
                IsEstablishedBusinessFriendly = false
            },
            new PlanTypeDto
            {
                Id = (int)BusinessPlanType.LeanCanvas,
                Name = "LeanCanvas",
                Description = "One-page lean business plan",
                Focus = "Quick validation, MVP, iteration",
                TargetAudience = "Startups, entrepreneurs, accelerators",
                UseCases = "Rapid prototyping, pitch decks, validation",
                EstimatedHours = 2,
                TypicalSections = 9,
                IsNonProfitFriendly = false,
                IsStartupFriendly = true,
                IsEstablishedBusinessFriendly = false
            }
        };

        return Ok(planTypes);
    }

    /// <summary>
    /// Update strategy map for a business plan (v1 compatibility endpoint)
    /// Proxies to V2 strategy map service
    /// </summary>
    /// <param name="id">The business plan ID</param>
    /// <param name="request">Strategy map update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("{id}/strategy-map/update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStrategyMap(
        Guid id,
        [FromBody] SaveStrategyMapRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating strategy map for plan {PlanId}", id);

        var result = await _strategyMapService.SaveStrategyMapAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Perform comprehensive plan audit (v1 compatibility endpoint)
    /// Combines readiness score and audit summary
    /// </summary>
    /// <param name="id">The business plan ID</param>
    /// <param name="language">Language code: fr (default) or en</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Combined audit response with readiness score and issues</returns>
    [HttpPost("{id}/audit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AuditPlan(
        Guid id,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Performing comprehensive audit for plan {PlanId}", id);

        // Get readiness score
        var readinessResult = await _readinessScoreService.CalculateReadinessScoreAsync(id, cancellationToken);
        if (!readinessResult.IsSuccess)
        {
            return HandleResult(readinessResult);
        }

        // Get audit summary
        var auditResult = await _auditService.GetAuditSummaryAsync(id, language, cancellationToken);
        if (!auditResult.IsSuccess)
        {
            return HandleResult(auditResult);
        }

        // Get business plan for financial metrics
        var plan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == id && !bp.IsDeleted, cancellationToken);

        var readiness = readinessResult.Value!;
        var audit = auditResult.Value!;

        // Combine into single response
        var response = new PlanAuditResponse
        {
            ReadinessScore = readiness.OverallScore,
            ReadinessComponents = new ReadinessComponents
            {
                ConsistencyScore = readiness.ConsistencyScore,
                RiskMitigationScore = readiness.RiskMitigationScore,
                CompletenessScore = readiness.CompletenessScore
            },
            PivotPointMonth = plan?.HealthMetrics?.PivotPointMonth,
            RunwayMonths = plan?.HealthMetrics?.RunwayMonths,
            Issues = audit.Sections.SelectMany(s => 
            {
                // Convert section summaries to issues format expected by frontend
                var issues = new List<AuditIssue>();
                if (s.IssueCount > 0)
                {
                    issues.Add(new AuditIssue
                    {
                        Category = "Strategic", // Default category
                        Severity = s.Score < 50 ? "error" : s.Score < 70 ? "warning" : "info",
                        Message = $"{s.SectionName} has {s.IssueCount} issue(s). Score: {s.Score}%",
                        Nudge = $"What improvements can be made to {s.SectionName}?",
                        Suggestions = new Suggestions
                        {
                            OptionA = $"Review and strengthen {s.SectionName}",
                            OptionB = $"Add more detail to {s.SectionName}",
                            OptionC = $"Get feedback on {s.SectionName}"
                        }
                    });
                }
                return issues;
            }).Concat(audit.CriticalIssues.Select(issue => new AuditIssue
            {
                Category = "Strategic",
                Severity = "error",
                Message = issue,
                Nudge = "How will you address this critical issue?",
                Suggestions = new Suggestions
                {
                    OptionA = "Address immediately",
                    OptionB = "Add to risk mitigation plan",
                    OptionC = "Seek expert advice"
                }
            })).ToList()
        };

        return Ok(response);
    }
}


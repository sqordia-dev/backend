using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans")]
[Authorize]
public class BusinessPlanGenerationController : BaseApiController
{
    private readonly IBusinessPlanGenerationService _generationService;
    private readonly ILogger<BusinessPlanGenerationController> _logger;

    public BusinessPlanGenerationController(
        IBusinessPlanGenerationService generationService,
        ILogger<BusinessPlanGenerationController> logger)
    {
        _generationService = generationService;
        _logger = logger;
    }

    /// <summary>
    /// Generate AI-powered content for all sections of a business plan
    /// </summary>
    /// <param name="id">The business plan ID</param>
    /// <param name="language">Language for generation (fr or en). Defaults to fr.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated business plan with generated content</returns>
    /// <remarks>
    /// This endpoint uses AI to generate professional content for all sections of the business plan
    /// based on the completed questionnaire responses. The generation process typically takes 1-3 minutes.
    /// 
    /// Prerequisites:
    /// - The business plan must exist
    /// - The questionnaire must be completed
    /// - The user must have access to the business plan's organization
    /// 
    /// Sample request:
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/generate?language=fr
    /// </remarks>
    /// <response code="200">Business plan generated successfully</response>
    /// <response code="400">Invalid request or questionnaire not completed</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("{id}/generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateBusinessPlan(
        Guid id,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generate business plan request for ID: {BusinessPlanId}, Language: {Language}", 
            id, language);

        // Validate language parameter
        if (language != "fr" && language != "en")
        {
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });
        }

        var result = await _generationService.GenerateBusinessPlanAsync(id, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Regenerate a specific section of a business plan
    /// </summary>
    /// <param name="id">The business plan ID</param>
    /// <param name="sectionName">The name of the section to regenerate (e.g., ExecutiveSummary, MarketAnalysis)</param>
    /// <param name="language">Language for generation (fr or en). Defaults to fr.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated business plan</returns>
    /// <remarks>
    /// This endpoint allows you to regenerate a specific section if you're not satisfied with the initial generation.
    /// The section name must match one of the available sections for the business plan type.
    /// 
    /// Available sections for Startups (BusinessPlan):
    /// - ExecutiveSummary
    /// - ProblemStatement
    /// - Solution
    /// - MarketAnalysis
    /// - CompetitiveAnalysis
    /// - SwotAnalysis
    /// - BusinessModel
    /// - MarketingStrategy
    /// - BrandingStrategy
    /// - OperationsPlan
    /// - ManagementTeam
    /// - FinancialProjections
    /// - FundingRequirements
    /// - RiskAnalysis
    /// - ExitStrategy
    /// 
    /// Additional sections for OBNL (StrategicPlan):
    /// - MissionStatement
    /// - SocialImpact
    /// - BeneficiaryProfile
    /// - GrantStrategy
    /// - SustainabilityPlan
    /// 
    /// Sample request:
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/regenerate/ExecutiveSummary?language=en
    /// </remarks>
    /// <response code="200">Section regenerated successfully</response>
    /// <response code="400">Invalid section name or request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("{id}/regenerate/{sectionName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegenerateSection(
        Guid id,
        string sectionName,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Regenerate section request for business plan ID: {BusinessPlanId}, Section: {Section}, Language: {Language}",
            id, sectionName, language);

        // Validate language parameter
        if (language != "fr" && language != "en")
        {
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });
        }

        // Validate section name (check if it exists in available sections)
        // Note: The service will validate against the specific business plan type
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            return BadRequest(new { error = "Section name is required" });
        }

        var result = await _generationService.RegenerateSectionAsync(id, sectionName, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the generation status of a business plan
    /// </summary>
    /// <param name="id">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generation status information including progress and completion percentage</returns>
    /// <remarks>
    /// This endpoint provides real-time status information about the business plan generation process.
    /// Use this to monitor progress during generation or to check completion status.
    /// 
    /// The response includes:
    /// - Current status (Draft, Generating, Generated, etc.)
    /// - Total number of sections
    /// - Number of completed sections
    /// - Completion percentage
    /// - Start and completion timestamps
    /// 
    /// Sample request:
    ///     GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/generation-status
    /// 
    /// Sample response:
    /// {
    ///     "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "status": "Generated",
    ///     "startedAt": "2025-10-14T19:30:00Z",
    ///     "completedAt": "2025-10-14T19:32:15Z",
    ///     "totalSections": 15,
    ///     "completedSections": 15,
    ///     "completionPercentage": 100.0
    /// }
    /// </remarks>
    /// <response code="200">Status retrieved successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet("{id}/generation-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGenerationStatus(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Get generation status request for business plan ID: {BusinessPlanId}", id);

        var result = await _generationService.GetGenerationStatusAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get available sections that can be generated for a business plan type
    /// </summary>
    /// <param name="planType">The business plan type (0 = BusinessPlan/Startup, 2 = StrategicPlan/OBNL)</param>
    /// <returns>List of available section names</returns>
    /// <remarks>
    /// This endpoint returns the list of sections that can be generated for a specific business plan type.
    /// Use this to know which section names are valid for the RegenerateSection endpoint.
    /// 
    /// Sample request:
    ///     GET /api/v1/business-plans/available-sections?planType=0
    /// 
    /// Sample response:
    /// [
    ///     "ExecutiveSummary",
    ///     "ProblemStatement",
    ///     "Solution",
    ///     "MarketAnalysis",
    ///     ...
    /// ]
    /// </remarks>
    /// <response code="200">Section list retrieved successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("available-sections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetAvailableSections([FromQuery] string planType = "0")
    {
        _logger.LogInformation("Get available sections request for plan type: {PlanType}", planType);

        var sections = _generationService.GetAvailableSections(planType);
        return Ok(sections);
    }
}


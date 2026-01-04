using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.AI;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/ai-analysis")]
[Authorize]
public class AIAnalysisController : BaseApiController
{
    private readonly IAIAnalysisService _aiAnalysisService;
    private readonly ILogger<AIAnalysisController> _logger;

    public AIAnalysisController(
        IAIAnalysisService aiAnalysisService,
        ILogger<AIAnalysisController> logger)
    {
        _aiAnalysisService = aiAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// Generate AI strategy suggestions for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="request">Strategy suggestion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated strategy suggestions</returns>
    [HttpPost("strategy-suggestions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateStrategySuggestions(
        Guid businessPlanId,
        [FromBody] StrategySuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Strategy suggestions request for business plan {BusinessPlanId}", businessPlanId);

        request.BusinessPlanId = businessPlanId;
        var result = await _aiAnalysisService.GenerateStrategySuggestionsAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Perform risk mitigation analysis for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="request">Risk mitigation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated risk analysis with mitigation strategies</returns>
    [HttpPost("risk-analysis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalyzeRisks(
        Guid businessPlanId,
        [FromBody] RiskMitigationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Risk analysis request for business plan {BusinessPlanId}", businessPlanId);

        request.BusinessPlanId = businessPlanId;
        var result = await _aiAnalysisService.AnalyzeRisksAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Perform comprehensive business mentor analysis
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="request">Business mentor request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive business analysis with opportunities and weaknesses</returns>
    [HttpPost("business-mentor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PerformBusinessMentorAnalysis(
        Guid businessPlanId,
        [FromBody] BusinessMentorRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Business mentor analysis request for business plan {BusinessPlanId}", businessPlanId);

        request.BusinessPlanId = businessPlanId;
        var result = await _aiAnalysisService.PerformBusinessMentorAnalysisAsync(request, cancellationToken);
        return Ok(result);
    }
}


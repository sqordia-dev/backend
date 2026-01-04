using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Pricing;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/pricing-analysis")]
[Authorize]
public class PricingAnalysisController : BaseApiController
{
    private readonly IPricingAnalysisService _pricingAnalysisService;
    private readonly ILogger<PricingAnalysisController> _logger;

    public PricingAnalysisController(
        IPricingAnalysisService pricingAnalysisService,
        ILogger<PricingAnalysisController> logger)
    {
        _pricingAnalysisService = pricingAnalysisService;
        _logger = logger;
    }

    /// <summary>
    /// Generate pricing analysis and competitive report
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalyzePricing(
        Guid businessPlanId,
        [FromBody] PricingAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pricing analysis request for business plan {BusinessPlanId}", businessPlanId);

        request.BusinessPlanId = businessPlanId;
        var result = await _pricingAnalysisService.AnalyzePricingAsync(request, cancellationToken);
        return Ok(result);
    }
}


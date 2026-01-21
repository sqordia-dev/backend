using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Services;
using Sqordia.Contracts.Requests.Financial;

namespace WebAPI.Controllers;

/// <summary>
/// Business plan financial management endpoints
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/plans/{planId}/financials")]
[Authorize]
public class BusinessPlanFinancialController : BaseApiController
{
    private readonly IFinancialService _financialService;
    private readonly ILogger<BusinessPlanFinancialController> _logger;

    public BusinessPlanFinancialController(
        IFinancialService financialService,
        ILogger<BusinessPlanFinancialController> logger)
    {
        _financialService = financialService;
        _logger = logger;
    }

    /// <summary>
    /// Update a financial table cell and recalculate dependent values
    /// </summary>
    /// <param name="planId">The business plan ID</param>
    /// <param name="request">Cell update request</param>
    /// <returns>Updated financial table data</returns>
    [HttpPost("update-cell")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFinancialCell(
        Guid planId,
        [FromBody] UpdateFinancialCellRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating financial cell for plan {PlanId}", planId);

        var result = await _financialService.UpdateFinancialCellAsync(planId, request);
        return HandleResult(result);
    }
}

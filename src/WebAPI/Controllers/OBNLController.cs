using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.OBNL.Commands;
using Sqordia.Application.OBNL.Queries;
using Sqordia.Application.OBNL.Services;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/obnl")]
[Authorize]
public class OBNLController : BaseApiController
{
    private readonly IOBNLPlanService _obnlPlanService;

    public OBNLController(IOBNLPlanService obnlPlanService)
    {
        _obnlPlanService = obnlPlanService;
    }

    [HttpPost("plans")]
    public async Task<IActionResult> CreateOBNLPlan([FromBody] CreateOBNLPlanCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var planId = await _obnlPlanService.CreateOBNLPlanAsync(command);
            return CreatedAtAction(nameof(GetOBNLPlan), new { planId }, new { Id = planId, Message = "OBNL plan created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("plans/{planId}")]
    public async Task<IActionResult> GetOBNLPlan(Guid planId, CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _obnlPlanService.GetOBNLPlanAsync(planId);
            return Ok(plan);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpGet("organizations/{organizationId}/plans")]
    public async Task<IActionResult> GetOBNLPlansByOrganization(Guid organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var plans = await _obnlPlanService.GetOBNLPlansByOrganizationAsync(organizationId);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("plans/{planId}/compliance/analyze")]
    public async Task<IActionResult> AnalyzeCompliance(Guid planId, CancellationToken cancellationToken = default)
    {
        try
        {
            var analysis = await _obnlPlanService.AnalyzeComplianceAsync(planId);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("plans/{planId}/grants")]
    public async Task<IActionResult> GetGrantApplications(Guid planId, CancellationToken cancellationToken = default)
    {
        try
        {
            var grants = await _obnlPlanService.GetGrantApplicationsAsync(planId);
            return Ok(grants);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("plans/{planId}/grants")]
    public async Task<IActionResult> CreateGrantApplication(Guid planId, [FromBody] CreateGrantApplicationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var grantId = await _obnlPlanService.CreateGrantApplicationAsync(command);
            return Ok(new { Id = grantId, Message = "Grant application created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("plans/{planId}/impact-measurements")]
    public async Task<IActionResult> GetImpactMeasurements(Guid planId, CancellationToken cancellationToken = default)
    {
        try
        {
            var measurements = await _obnlPlanService.GetImpactMeasurementsAsync(planId);
            return Ok(measurements);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("plans/{planId}/impact-measurements")]
    public async Task<IActionResult> CreateImpactMeasurement(Guid planId, [FromBody] CreateImpactMeasurementCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var measurementId = await _obnlPlanService.CreateImpactMeasurementAsync(command);
            return Ok(new { Id = measurementId, Message = "Impact measurement created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("plans/{planId}")]
    public async Task<IActionResult> UpdateOBNLPlan(Guid planId, [FromBody] UpdateOBNLPlanCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var updateCommand = command with { Id = planId };
            await _obnlPlanService.UpdateOBNLPlanAsync(updateCommand);
            return Ok(new { Message = "OBNL plan updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("plans/{planId}")]
    public async Task<IActionResult> DeleteOBNLPlan(Guid planId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _obnlPlanService.DeleteOBNLPlanAsync(planId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

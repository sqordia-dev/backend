using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.OBNL.Commands;
using Sqordia.Application.OBNL.Queries;
using Sqordia.Application.OBNL.Services;

namespace WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/obnl")]
public class OBNLController : ControllerBase
{
    private readonly IOBNLPlanService _obnlPlanService;

    public OBNLController(IOBNLPlanService obnlPlanService)
    {
        _obnlPlanService = obnlPlanService;
    }

    [HttpPost("plans")]
    public async Task<IActionResult> CreateOBNLPlan([FromBody] CreateOBNLPlanCommand command)
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
    public async Task<IActionResult> GetOBNLPlan(Guid planId)
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
    public async Task<IActionResult> GetOBNLPlansByOrganization(Guid organizationId)
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
    public async Task<IActionResult> AnalyzeCompliance(Guid planId)
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
    public async Task<IActionResult> GetGrantApplications(Guid planId)
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
    public async Task<IActionResult> CreateGrantApplication(Guid planId, [FromBody] CreateGrantApplicationCommand command)
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
    public async Task<IActionResult> GetImpactMeasurements(Guid planId)
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
    public async Task<IActionResult> CreateImpactMeasurement(Guid planId, [FromBody] CreateImpactMeasurementCommand command)
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
    public async Task<IActionResult> UpdateOBNLPlan(Guid planId, [FromBody] UpdateOBNLPlanCommand command)
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
    public async Task<IActionResult> DeleteOBNLPlan(Guid planId)
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

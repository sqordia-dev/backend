using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/sections")]
[Authorize]
public class BusinessPlanSectionController : BaseApiController
{
    private readonly ISectionService _sectionService;

    public BusinessPlanSectionController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }
    /// <summary>
    /// Get all sections for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all business plan sections</returns>
    /// <remarks>
    /// Returns all sections for a business plan with their content, metadata, and completion status.
    /// This endpoint provides a comprehensive view of all sections in the business plan.
    /// 
    /// Sample response:
    /// {
    ///   "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "businessPlanTitle": "My Startup Business Plan",
    ///   "planType": "BusinessPlan",
    ///   "sections": [
    ///     {
    ///       "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "sectionName": "executive-summary",
    ///       "title": "Executive Summary",
    ///       "content": "Our company provides innovative solutions...",
    ///       "hasContent": true,
    ///       "wordCount": 150,
    ///       "characterCount": 850,
    ///       "lastUpdated": "2025-01-14T10:30:00Z",
    ///       "lastUpdatedBy": "user@example.com",
    ///       "isRequired": true,
    ///       "order": 1,
    ///       "description": "Brief overview of the business",
    ///       "isAIGenerated": true,
    ///       "isManuallyEdited": false,
    ///       "status": "draft",
    ///       "tags": ["overview", "summary"]
    ///     }
    ///   ],
    ///   "totalSections": 15,
    ///   "sectionsWithContent": 8,
    ///   "completionPercentage": 53.3,
    ///   "lastUpdated": "2025-01-14T10:30:00Z"
    /// }
    /// </remarks>
    /// <response code="200">List of business plan sections</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(BusinessPlanSectionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSections(Guid businessPlanId, CancellationToken cancellationToken)
    {
        try
        {
            var sections = await _sectionService.GetSectionsAsync(businessPlanId, cancellationToken);
            return Ok(sections);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific section of a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionName">The section name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Specific business plan section</returns>
    /// <remarks>
    /// Returns a specific section of a business plan with its content and metadata.
    /// 
    /// Common section names:
    /// - executive-summary
    /// - market-analysis
    /// - competitive-analysis
    /// - business-model
    /// - marketing-strategy
    /// - operations-plan
    /// - management-team
    /// - financial-projections
    /// - funding-requirements
    /// - risk-analysis
    /// 
    /// Sample response:
    /// {
    ///   "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "sectionName": "executive-summary",
    ///   "title": "Executive Summary",
    ///   "content": "Our company provides innovative solutions...",
    ///   "hasContent": true,
    ///   "wordCount": 150,
    ///   "characterCount": 850,
    ///   "lastUpdated": "2025-01-14T10:30:00Z",
    ///   "lastUpdatedBy": "user@example.com",
    ///   "isRequired": true,
    ///   "order": 1,
    ///   "description": "Brief overview of the business",
    ///   "isAIGenerated": true,
    ///   "isManuallyEdited": false,
    ///   "status": "draft",
    ///   "tags": ["overview", "summary"]
    /// }
    /// </remarks>
    /// <response code="200">Business plan section</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan or section not found</response>
    [HttpGet("{sectionName}")]
    [ProducesResponseType(typeof(BusinessPlanSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSection(Guid businessPlanId, string sectionName, CancellationToken cancellationToken)
    {
        try
        {
            var section = await _sectionService.GetSectionAsync(businessPlanId, sectionName, cancellationToken);
            return Ok(section);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update a specific section of a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionName">The section name</param>
    /// <param name="request">The section update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated business plan section</returns>
    /// <remarks>
    /// Updates a specific section of a business plan with new content.
    /// The section will be marked as manually edited if the user provides the content.
    /// 
    /// Sample request:
    /// {
    ///   "content": "Updated executive summary content...",
    ///   "isAIGenerated": false,
    ///   "isManualEdit": true,
    ///   "status": "review",
    ///   "tags": ["updated", "review"],
    ///   "notes": "Updated based on feedback"
    /// }
    /// </remarks>
    /// <response code="200">Updated business plan section</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan or section not found</response>
    [HttpPut("{sectionName}")]
    [ProducesResponseType(typeof(BusinessPlanSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSection(
        Guid businessPlanId, 
        string sectionName, 
        [FromBody] UpdateSectionRequest request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedSection = await _sectionService.UpdateSectionAsync(businessPlanId, sectionName, request, cancellationToken);
            return Ok(updatedSection);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

}

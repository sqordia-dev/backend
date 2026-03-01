using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V3;
using Sqordia.Contracts.Requests.Admin.SectionHierarchy;
using Sqordia.Contracts.Responses.Admin.SectionHierarchy;

namespace WebAPI.Controllers;

/// <summary>
/// Admin controller for managing business plan section hierarchy
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/sections")]
[Authorize(Roles = "Admin")]
public class AdminSectionHierarchyController : BaseApiController
{
    private readonly ISectionHierarchyService _service;

    public AdminSectionHierarchyController(ISectionHierarchyService service)
    {
        _service = service;
    }

    #region Main Sections

    /// <summary>
    /// Get all main sections with their sub-sections
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MainSectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllMainSections(CancellationToken cancellationToken)
    {
        var result = await _service.GetAllMainSectionsAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get main sections list (lightweight)
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<MainSectionListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMainSectionList(CancellationToken cancellationToken)
    {
        var result = await _service.GetMainSectionListAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a main section by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MainSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMainSectionById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetMainSectionByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a main section by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(MainSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMainSectionByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _service.GetMainSectionByCodeAsync(code, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new main section
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMainSection(
        [FromBody] CreateMainSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateMainSectionAsync(request, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetMainSectionById), new { id = result.Value }, new { Id = result.Value });
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Update a main section
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMainSection(
        Guid id,
        [FromBody] UpdateMainSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateMainSectionAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete (deactivate) a main section
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMainSection(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteMainSectionAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reorder main sections
    /// </summary>
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderMainSections(
        [FromBody] ReorderSectionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ReorderMainSectionsAsync(request, cancellationToken);
        return HandleResult(result);
    }

    #endregion

    #region Sub-Sections

    /// <summary>
    /// Get sub-sections for a main section
    /// </summary>
    [HttpGet("{mainSectionId:guid}/subsections")]
    [ProducesResponseType(typeof(List<SubSectionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubSections(Guid mainSectionId, CancellationToken cancellationToken)
    {
        var result = await _service.GetSubSectionsAsync(mainSectionId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a sub-section by ID
    /// </summary>
    [HttpGet("subsections/{id:guid}")]
    [ProducesResponseType(typeof(SubSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubSectionById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetSubSectionByIdAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a sub-section by code
    /// </summary>
    [HttpGet("subsections/by-code/{code}")]
    [ProducesResponseType(typeof(SubSectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubSectionByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _service.GetSubSectionByCodeAsync(code, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new sub-section
    /// </summary>
    [HttpPost("{mainSectionId:guid}/subsections")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubSection(
        Guid mainSectionId,
        [FromBody] CreateSubSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateSubSectionAsync(mainSectionId, request, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetSubSectionById), new { id = result.Value }, new { Id = result.Value });
        }
        return HandleResult(result);
    }

    /// <summary>
    /// Update a sub-section
    /// </summary>
    [HttpPut("subsections/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSubSection(
        Guid id,
        [FromBody] UpdateSubSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateSubSectionAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete (deactivate) a sub-section
    /// </summary>
    [HttpDelete("subsections/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSubSection(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteSubSectionAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reorder sub-sections within a main section
    /// </summary>
    [HttpPut("{mainSectionId:guid}/subsections/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderSubSections(
        Guid mainSectionId,
        [FromBody] ReorderSectionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.ReorderSubSectionsAsync(mainSectionId, request, cancellationToken);
        return HandleResult(result);
    }

    #endregion
}

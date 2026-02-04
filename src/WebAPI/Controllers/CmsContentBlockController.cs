using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Requests.Cms;
using Sqordia.Contracts.Responses.Cms;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/cms/versions/{versionId:guid}/blocks")]
[Authorize(Roles = "Admin")]
public class CmsContentBlockController : BaseApiController
{
    private readonly ICmsContentBlockService _blockService;
    private readonly ILogger<CmsContentBlockController> _logger;

    public CmsContentBlockController(
        ICmsContentBlockService blockService,
        ILogger<CmsContentBlockController> logger)
    {
        _blockService = blockService;
        _logger = logger;
    }

    /// <summary>
    /// Get all content blocks for a version, optionally filtered by section key and language
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CmsContentBlockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBlocksByVersion(
        Guid versionId,
        [FromQuery] string? sectionKey,
        [FromQuery] string? language,
        CancellationToken cancellationToken = default)
    {
        var result = await _blockService.GetBlocksByVersionAsync(versionId, sectionKey, language, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a specific content block by ID within a version
    /// </summary>
    [HttpGet("{blockId:guid}")]
    [ProducesResponseType(typeof(CmsContentBlockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBlock(Guid versionId, Guid blockId, CancellationToken cancellationToken = default)
    {
        var result = await _blockService.GetBlockAsync(blockId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new content block within a version
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CmsContentBlockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBlock(
        Guid versionId,
        [FromBody] CreateContentBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _blockService.CreateBlockAsync(versionId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing content block
    /// </summary>
    [HttpPut("{blockId:guid}")]
    [ProducesResponseType(typeof(CmsContentBlockResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateBlock(
        Guid versionId,
        Guid blockId,
        [FromBody] UpdateContentBlockRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _blockService.UpdateBlockAsync(blockId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update multiple content blocks in a single operation
    /// </summary>
    [HttpPut("bulk")]
    [ProducesResponseType(typeof(List<CmsContentBlockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BulkUpdateBlocks(
        Guid versionId,
        [FromBody] BulkUpdateContentBlocksRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _blockService.BulkUpdateBlocksAsync(versionId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Reorder content blocks within a version
    /// </summary>
    [HttpPut("reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReorderBlocks(
        Guid versionId,
        [FromBody] ReorderContentBlocksRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _blockService.ReorderBlocksAsync(versionId, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a content block
    /// </summary>
    [HttpDelete("{blockId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteBlock(Guid versionId, Guid blockId, CancellationToken cancellationToken = default)
    {
        var result = await _blockService.DeleteBlockAsync(blockId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Clone content blocks from the currently published version into this draft version
    /// </summary>
    [HttpPost("clone-published")]
    [ProducesResponseType(typeof(List<CmsContentBlockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClonePublished(Guid versionId, CancellationToken cancellationToken = default)
    {
        var result = await _blockService.CloneBlocksFromPublishedAsync(versionId, cancellationToken);
        return HandleResult(result);
    }
}

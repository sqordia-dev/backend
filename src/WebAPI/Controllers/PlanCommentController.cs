using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Comment;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/comments")]
[Authorize]
public class PlanCommentController : BaseApiController
{
    private readonly IPlanCommentService _commentService;
    private readonly ILogger<PlanCommentController> _logger;

    public PlanCommentController(
        IPlanCommentService commentService,
        ILogger<PlanCommentController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a comment on a section
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateComment(
        Guid businessPlanId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        request.BusinessPlanId = businessPlanId;
        var result = await _commentService.CreateCommentAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get all comments for a business plan
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBusinessPlanComments(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var result = await _commentService.GetBusinessPlanCommentsAsync(businessPlanId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get comments for a specific section
    /// </summary>
    [HttpGet("sections/{sectionName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSectionComments(
        Guid businessPlanId,
        string sectionName,
        CancellationToken cancellationToken = default)
    {
        var result = await _commentService.GetSectionCommentsAsync(businessPlanId, sectionName, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Resolve a comment
    /// </summary>
    [HttpPut("{commentId}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResolveComment(
        Guid businessPlanId,
        Guid commentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _commentService.ResolveCommentAsync(commentId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a comment
    /// </summary>
    [HttpDelete("{commentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteComment(
        Guid businessPlanId,
        Guid commentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _commentService.DeleteCommentAsync(commentId, cancellationToken);
        return Ok(result);
    }
}


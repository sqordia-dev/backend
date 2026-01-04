using Sqordia.Contracts.Requests.Comment;
using Sqordia.Contracts.Responses.Comment;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for managing comments on business plan sections
/// </summary>
public interface IPlanCommentService
{
    /// <summary>
    /// Create a comment on a section
    /// </summary>
    Task<CommentResponse> CreateCommentAsync(
        CreateCommentRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all comments for a business plan section
    /// </summary>
    Task<List<CommentResponse>> GetSectionCommentsAsync(
        Guid businessPlanId,
        string sectionName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all comments for a business plan
    /// </summary>
    Task<List<CommentResponse>> GetBusinessPlanCommentsAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resolve a comment
    /// </summary>
    Task<CommentResponse> ResolveCommentAsync(
        Guid commentId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a comment
    /// </summary>
    Task<bool> DeleteCommentAsync(
        Guid commentId,
        CancellationToken cancellationToken = default);
}


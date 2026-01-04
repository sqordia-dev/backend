using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Comment;
using Sqordia.Contracts.Responses.Comment;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Application.Services.Implementations;

public class PlanCommentService : IPlanCommentService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PlanCommentService> _logger;

    public PlanCommentService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<PlanCommentService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CommentResponse> CreateCommentAsync(
        CreateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        // Verify business plan access
        var businessPlan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == request.BusinessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null)
        {
            throw new ArgumentException($"Business plan {request.BusinessPlanId} not found");
        }

        var isMember = await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                           om.UserId == currentUserId &&
                           om.IsActive, cancellationToken);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("User does not have access to this business plan");
        }

        var comment = new PlanSectionComment(
            request.BusinessPlanId,
            request.SectionName,
            request.CommentText,
            request.ParentCommentId);

        comment.CreatedBy = currentUserIdString;
        _context.PlanSectionComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Comment created on section {SectionName} for business plan {BusinessPlanId}",
            request.SectionName, request.BusinessPlanId);

        return MapToResponse(comment);
    }

    public async Task<List<CommentResponse>> GetSectionCommentsAsync(
        Guid businessPlanId,
        string sectionName,
        CancellationToken cancellationToken = default)
    {
        var comments = await _context.PlanSectionComments
            .Where(c => c.BusinessPlanId == businessPlanId && 
                       c.SectionName == sectionName &&
                       c.ParentCommentId == null) // Only top-level comments
            .Include(c => c.Replies)
            .OrderByDescending(c => c.Created)
            .ToListAsync(cancellationToken);

        return comments.Select(MapToResponse).ToList();
    }

    public async Task<List<CommentResponse>> GetBusinessPlanCommentsAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var comments = await _context.PlanSectionComments
            .Where(c => c.BusinessPlanId == businessPlanId && c.ParentCommentId == null)
            .Include(c => c.Replies)
            .OrderByDescending(c => c.Created)
            .ToListAsync(cancellationToken);

        return comments.Select(MapToResponse).ToList();
    }

    public async Task<CommentResponse> ResolveCommentAsync(
        Guid commentId,
        CancellationToken cancellationToken = default)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User must be authenticated");
        }

        var comment = await _context.PlanSectionComments
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment == null)
        {
            throw new ArgumentException($"Comment {commentId} not found");
        }

        comment.Resolve(currentUserId);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(comment);
    }

    public async Task<bool> DeleteCommentAsync(
        Guid commentId,
        CancellationToken cancellationToken = default)
    {
        var comment = await _context.PlanSectionComments
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment == null)
        {
            return false;
        }

        _context.PlanSectionComments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private CommentResponse MapToResponse(PlanSectionComment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            BusinessPlanId = comment.BusinessPlanId,
            SectionName = comment.SectionName,
            CommentText = comment.CommentText,
            ParentCommentId = comment.ParentCommentId,
            IsResolved = comment.IsResolved,
            ResolvedAt = comment.ResolvedAt,
            ResolvedByUserId = comment.ResolvedByUserId,
            CreatedBy = comment.CreatedBy ?? "Unknown",
            CreatedAt = comment.Created,
            UpdatedAt = comment.LastModified,
            Replies = comment.Replies.Select(MapToResponse).ToList()
        };
    }
}


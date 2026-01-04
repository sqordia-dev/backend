namespace Sqordia.Contracts.Responses.Comment;

/// <summary>
/// Comment response
/// </summary>
public class CommentResponse
{
    public required Guid Id { get; set; }
    public required Guid BusinessPlanId { get; set; }
    public required string SectionName { get; set; }
    public required string CommentText { get; set; }
    public Guid? ParentCommentId { get; set; }
    public required bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public required string CreatedBy { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CommentResponse> Replies { get; set; } = new();
}


using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Comment on a business plan section for collaboration
/// </summary>
public class PlanSectionComment : BaseAuditableEntity
{
    public Guid BusinessPlanId { get; private set; }
    public string SectionName { get; private set; } = null!; // e.g., "executive-summary", "market-analysis"
    public string CommentText { get; private set; } = null!;
    public Guid? ParentCommentId { get; private set; } // For threaded comments/replies
    public bool IsResolved { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public Guid? ResolvedByUserId { get; private set; }
    
    // Navigation properties
    public BusinessPlan BusinessPlan { get; private set; } = null!;
    public PlanSectionComment? ParentComment { get; private set; }
    public ICollection<PlanSectionComment> Replies { get; private set; } = new List<PlanSectionComment>();
    
    private PlanSectionComment() { } // EF Core constructor
    
    public PlanSectionComment(
        Guid businessPlanId,
        string sectionName,
        string commentText,
        Guid? parentCommentId = null)
    {
        BusinessPlanId = businessPlanId;
        SectionName = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
        CommentText = commentText ?? throw new ArgumentNullException(nameof(commentText));
        ParentCommentId = parentCommentId;
        IsResolved = false;
    }
    
    public void Resolve(Guid resolvedByUserId)
    {
        IsResolved = true;
        ResolvedAt = DateTime.UtcNow;
        ResolvedByUserId = resolvedByUserId;
    }
    
    public void Unresolve()
    {
        IsResolved = false;
        ResolvedAt = null;
        ResolvedByUserId = null;
    }
    
    public void UpdateComment(string commentText)
    {
        CommentText = commentText ?? throw new ArgumentNullException(nameof(commentText));
    }
}


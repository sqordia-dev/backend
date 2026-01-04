using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// SMART (Specific, Measurable, Achievable, Relevant, Time-bound) objective for a business plan
/// </summary>
public class SmartObjective : BaseAuditableEntity
{
    public Guid BusinessPlanId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    
    // SMART criteria
    public string Specific { get; private set; } = null!; // What exactly will be accomplished
    public string Measurable { get; private set; } = null!; // How will success be measured
    public string Achievable { get; private set; } = null!; // Is this realistic and attainable
    public string Relevant { get; private set; } = null!; // Why is this important
    public string TimeBound { get; private set; } = null!; // When will this be completed
    
    // Tracking
    public DateTime TargetDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public decimal ProgressPercentage { get; private set; }
    public string Status { get; private set; } = "NotStarted"; // NotStarted, InProgress, Completed, OnHold, Cancelled
    
    // Category
    public string Category { get; private set; } = null!; // Revenue, Marketing, Operations, etc.
    public int Priority { get; private set; } // 1-5, 1 being highest
    
    // Navigation properties
    public BusinessPlan BusinessPlan { get; private set; } = null!;
    
    private SmartObjective() { } // EF Core constructor
    
    public SmartObjective(
        Guid businessPlanId,
        string title,
        string description,
        string specific,
        string measurable,
        string achievable,
        string relevant,
        string timeBound,
        DateTime targetDate,
        string category,
        int priority = 3)
    {
        BusinessPlanId = businessPlanId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Specific = specific ?? throw new ArgumentNullException(nameof(specific));
        Measurable = measurable ?? throw new ArgumentNullException(nameof(measurable));
        Achievable = achievable ?? throw new ArgumentNullException(nameof(achievable));
        Relevant = relevant ?? throw new ArgumentNullException(nameof(relevant));
        TimeBound = timeBound ?? throw new ArgumentNullException(nameof(timeBound));
        TargetDate = targetDate;
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Priority = priority;
        Status = "NotStarted";
        ProgressPercentage = 0;
    }
    
    public void UpdateProgress(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Progress percentage must be between 0 and 100");
        
        ProgressPercentage = percentage;
        
        if (percentage == 100)
        {
            Status = "Completed";
            CompletedDate = DateTime.UtcNow;
        }
        else if (percentage > 0)
        {
            Status = "InProgress";
        }
    }
    
    public void UpdateStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentNullException(nameof(status));
        
        Status = status;
        
        if (status == "Completed" && !CompletedDate.HasValue)
        {
            CompletedDate = DateTime.UtcNow;
            ProgressPercentage = 100;
        }
    }
    
    public void UpdatePriority(int priority)
    {
        if (priority < 1 || priority > 5)
            throw new ArgumentException("Priority must be between 1 and 5");
        
        Priority = priority;
    }
}


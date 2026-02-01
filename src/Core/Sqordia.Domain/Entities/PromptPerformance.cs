using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Entity for tracking prompt template usage metrics and performance
/// Used for A/B testing and prompt optimization
/// </summary>
public class PromptPerformance : BaseEntity
{
    public Guid PromptTemplateId { get; private set; }
    public int UsageCount { get; private set; }
    public int EditCount { get; private set; }        // How many times users edited the result
    public int RegenerateCount { get; private set; }  // How many times users regenerated
    public int AcceptCount { get; private set; }      // Accepted without changes
    public double TotalRating { get; private set; }   // Sum of all ratings for averaging
    public int RatingCount { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Calculated metrics
    public double EditRate => UsageCount > 0 ? (double)EditCount / UsageCount : 0;
    public double RegenerateRate => UsageCount > 0 ? (double)RegenerateCount / UsageCount : 0;
    public double AcceptanceRate => UsageCount > 0 ? (double)AcceptCount / UsageCount : 0;
    public double AverageRating => RatingCount > 0 ? TotalRating / RatingCount : 0;

    // Navigation property
    public virtual PromptTemplate PromptTemplate { get; private set; } = null!;

    // Required for EF Core
    protected PromptPerformance() { }

    public PromptPerformance(Guid promptTemplateId, DateTime periodStart, DateTime periodEnd)
    {
        PromptTemplateId = promptTemplateId;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        UsageCount = 0;
        EditCount = 0;
        RegenerateCount = 0;
        AcceptCount = 0;
        TotalRating = 0;
        RatingCount = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a new usage of the prompt
    /// </summary>
    public void RecordUsage()
    {
        UsageCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records when a user edits the generated content
    /// </summary>
    public void RecordEdit()
    {
        EditCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records when a user regenerates the content
    /// </summary>
    public void RecordRegenerate()
    {
        RegenerateCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records when a user accepts the content without changes
    /// </summary>
    public void RecordAccept()
    {
        AcceptCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a user rating (1-5)
    /// </summary>
    public void RecordRating(int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5");

        TotalRating += rating;
        RatingCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the period end date
    /// </summary>
    public void ExtendPeriod(DateTime newEndDate)
    {
        if (newEndDate <= PeriodEnd)
            throw new ArgumentException("New end date must be after current end date", nameof(newEndDate));

        PeriodEnd = newEndDate;
        UpdatedAt = DateTime.UtcNow;
    }
}

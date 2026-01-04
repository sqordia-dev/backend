namespace Sqordia.Contracts.Responses.BusinessPlan;

/// <summary>
/// Represents a business plan type with its description and characteristics
/// </summary>
public class PlanTypeDto
{
    /// <summary>
    /// The plan type identifier
    /// </summary>
    public required int Id { get; set; }
    
    /// <summary>
    /// The plan type name
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Brief description of the plan type
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Focus areas for this plan type
    /// </summary>
    public required string Focus { get; set; }
    
    /// <summary>
    /// Target audience for this plan type
    /// </summary>
    public required string TargetAudience { get; set; }
    
    /// <summary>
    /// Typical use cases for this plan type
    /// </summary>
    public required string UseCases { get; set; }
    
    /// <summary>
    /// Estimated completion time in hours
    /// </summary>
    public required int EstimatedHours { get; set; }
    
    /// <summary>
    /// Number of typical sections in this plan type
    /// </summary>
    public required int TypicalSections { get; set; }
    
    /// <summary>
    /// Whether this plan type is suitable for non-profits
    /// </summary>
    public required bool IsNonProfitFriendly { get; set; }
    
    /// <summary>
    /// Whether this plan type is suitable for startups
    /// </summary>
    public required bool IsStartupFriendly { get; set; }
    
    /// <summary>
    /// Whether this plan type is suitable for established businesses
    /// </summary>
    public required bool IsEstablishedBusinessFriendly { get; set; }
}

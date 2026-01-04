using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Template for questionnaires based on business plan type
/// Defines the questions to be asked for different plan types
/// </summary>
public class QuestionnaireTemplate : BaseAuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public BusinessPlanType PlanType { get; private set; }
    public bool IsActive { get; private set; }
    public int Version { get; private set; }
    
    // Navigation properties
    public ICollection<QuestionTemplate> Questions { get; private set; } = new List<QuestionTemplate>();
    
    private QuestionnaireTemplate() { } // EF Core constructor
    
    public QuestionnaireTemplate(string name, BusinessPlanType planType, string? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PlanType = planType;
        Description = description;
        IsActive = true;
        Version = 1;
    }
    
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void IncrementVersion() => Version++;
}


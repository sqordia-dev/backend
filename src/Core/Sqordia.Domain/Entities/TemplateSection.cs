using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class TemplateSection : BaseEntity
{
    public Guid TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public bool IsVisible { get; set; }
    public string SectionType { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public string ValidationRules { get; set; } = string.Empty;
    public string HelpText { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Template Template { get; set; } = null!;
    public List<TemplateField> Fields { get; set; } = new();
}

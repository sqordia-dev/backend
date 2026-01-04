using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

public class TemplateField : BaseEntity
{
    public Guid TemplateSectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsVisible { get; set; }
    public int Order { get; set; }
    public string ValidationRules { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty; // JSON for dropdown options
    public string HelpText { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public TemplateSection TemplateSection { get; set; } = null!;
}

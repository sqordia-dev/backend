using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

public class Template : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public TemplateCategory Category { get; set; }
    public TemplateType Type { get; set; }
    public TemplateStatus Status { get; set; }
    public string Industry { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public bool IsDefault { get; set; }
    public int UsageCount { get; set; }
    public decimal Rating { get; set; }
    public int RatingCount { get; set; }
    public string Tags { get; set; } = string.Empty;
    public string PreviewImage { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Changelog { get; set; } = string.Empty;
    public DateTime LastUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public List<TemplateSection> Sections { get; set; } = new();
    public List<TemplateCustomization> Customizations { get; set; } = new();
    public List<TemplateRating> Ratings { get; set; } = new();
    public List<TemplateUsage> Usages { get; set; } = new();
}

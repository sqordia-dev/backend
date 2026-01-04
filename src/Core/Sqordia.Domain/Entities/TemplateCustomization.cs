using Sqordia.Domain.Common;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Domain.Entities;

public class TemplateCustomization : BaseEntity
{
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Customizations { get; set; } = string.Empty; // JSON of customizations
    public bool IsPublic { get; set; }
    public bool IsDefault { get; set; }
    public int UsageCount { get; set; }
    public decimal Rating { get; set; }
    public int RatingCount { get; set; }
    public string Tags { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Changelog { get; set; } = string.Empty;
    public DateTime LastUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Template Template { get; set; } = null!;
    public User User { get; set; } = null!;
}

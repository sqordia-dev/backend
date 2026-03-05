using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Email template entity for admin-managed bilingual email templates
/// </summary>
public class EmailTemplate : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // transactional, marketing, notification
    public string SubjectFr { get; set; } = string.Empty;
    public string SubjectEn { get; set; } = string.Empty;
    public string BodyFr { get; set; } = string.Empty;
    public string BodyEn { get; set; } = string.Empty;
    public string? VariablesJson { get; set; } // JSON array of variable names
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
}

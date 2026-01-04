using Sqordia.Domain.Common;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Domain.Entities;

public class TemplateUsage : BaseEntity
{
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public Guid? BusinessPlanId { get; set; }
    public string UsageType { get; set; } = string.Empty; // View, Download, Use, Customize
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; }
    public int Duration { get; set; } // in seconds
    public string Referrer { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Template Template { get; set; } = null!;
    public User User { get; set; } = null!;
    public Domain.Entities.BusinessPlan.BusinessPlan? BusinessPlan { get; set; }
}

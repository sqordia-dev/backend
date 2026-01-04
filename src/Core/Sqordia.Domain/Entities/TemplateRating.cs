using Sqordia.Domain.Common;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Domain.Entities;

public class TemplateRating : BaseEntity
{
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string Comment { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public Template Template { get; set; } = null!;
    public User User { get; set; } = null!;
}

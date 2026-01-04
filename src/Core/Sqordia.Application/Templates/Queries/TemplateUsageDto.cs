namespace Sqordia.Application.Templates.Queries;

public class TemplateUsageDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public Guid? BusinessPlanId { get; set; }
    public string UsageType { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; }
    public int Duration { get; set; }
    public string Referrer { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

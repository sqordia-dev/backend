namespace Sqordia.Application.Templates.Queries;

public class TemplateAnalyticsDto
{
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int TotalDownloads { get; set; }
    public int TotalUses { get; set; }
    public int TotalCustomizations { get; set; }
    public int TotalRatings { get; set; }
    public decimal AverageRating { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueCountries { get; set; }
    public List<string> TopCountries { get; set; } = new();
    public List<string> TopCities { get; set; } = new();
    public List<string> TopReferrers { get; set; } = new();
    public Dictionary<string, int> UsageByType { get; set; } = new();
    public Dictionary<string, int> UsageByMonth { get; set; } = new();
    public Dictionary<string, int> UsageByDay { get; set; } = new();
    public Dictionary<string, int> UsageByHour { get; set; } = new();
    public int AverageSessionDuration { get; set; }
    public int BounceRate { get; set; }
    public int ConversionRate { get; set; }
    public DateTime LastUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

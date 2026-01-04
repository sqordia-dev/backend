namespace Sqordia.Application.Financial.Queries;

public class FinancialReportDto
{
    public Guid Id { get; set; }
    public Guid BusinessPlanId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // JSON or HTML content
    public string Notes { get; set; } = string.Empty;
}

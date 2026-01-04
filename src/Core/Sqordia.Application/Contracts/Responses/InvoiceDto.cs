namespace Sqordia.Application.Contracts.Responses;

/// <summary>
/// Invoice DTO
/// </summary>
public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "CAD";
    public string Status { get; set; } = null!;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Description { get; set; } = null!;
    public string? PdfUrl { get; set; }
}


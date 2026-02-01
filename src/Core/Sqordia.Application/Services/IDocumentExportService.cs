using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Export;
using Sqordia.Contracts.Responses.Export;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for exporting business plans to various document formats
/// </summary>
public interface IDocumentExportService
{
    /// <summary>
    /// Export a business plan to PDF format
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="language">Export language (fr/en)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the PDF file data</returns>
    Task<Result<ExportResult>> ExportToPdfAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default);

    /// <summary>
    /// Export a business plan to Word (DOCX) format
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="language">Export language (fr/en)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the Word file data</returns>
    Task<Result<ExportResult>> ExportToWordAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default);

    /// <summary>
    /// Export a business plan to HTML format (for preview)
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="language">Export language (fr/en)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the HTML content</returns>
    Task<Result<string>> ExportToHtmlAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default);

    /// <summary>
    /// Export a business plan to Excel (XLSX) format
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="language">Export language (fr/en)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the Excel file data</returns>
    Task<Result<ExportResult>> ExportToExcelAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default);

    /// <summary>
    /// Export a business plan to PowerPoint (PPTX) format
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="language">Export language (fr/en)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the PowerPoint file data</returns>
    Task<Result<ExportResult>> ExportToPowerPointAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available export templates
    /// </summary>
    /// <returns>List of available templates</returns>
    Task<Result<List<ExportTemplate>>> GetAvailableTemplatesAsync();

    /// <summary>
    /// Export a business plan to PDF format with visual elements (charts, tables, metrics)
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="request">Export request with visual element options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the PDF file data with rendered visual elements</returns>
    Task<Result<ExportWithVisualsResponse>> ExportToPdfWithVisualsAsync(
        Guid businessPlanId,
        ExportWithVisualsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export a business plan to Word (DOCX) format with visual elements (charts, tables, metrics)
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="request">Export request with visual element options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the Word file data with rendered visual elements</returns>
    Task<Result<ExportWithVisualsResponse>> ExportToWordWithVisualsAsync(
        Guid businessPlanId,
        ExportWithVisualsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export a business plan to HTML format with visual elements for preview
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="request">Export request with visual element options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the HTML content with embedded visual elements</returns>
    Task<Result<string>> ExportToHtmlWithVisualsAsync(
        Guid businessPlanId,
        ExportWithVisualsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get export preview information including visual element counts
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Export preview information</returns>
    Task<Result<ExportPreviewResponse>> GetExportPreviewAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an export result with file data
/// </summary>
public class ExportResult
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    public string Language { get; set; } = "fr";
    public string Template { get; set; } = "default";
    public ExportVisualStatistics? VisualStatistics { get; set; }
}

/// <summary>
/// Statistics about visual elements in an export
/// </summary>
public class ExportVisualStatistics
{
    public int TotalVisualElements { get; set; }
    public int TableCount { get; set; }
    public int ChartCount { get; set; }
    public int MetricCount { get; set; }
    public int InfographicCount { get; set; }
    public int SectionCount { get; set; }
    public int WordCount { get; set; }
    public int? EstimatedPageCount { get; set; }
    public long ProcessingTimeMs { get; set; }
}

/// <summary>
/// Represents an export template
/// </summary>
public class ExportTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PreviewImageUrl { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public List<string> SupportedFormats { get; set; } = new();
    public List<string> SupportedLanguages { get; set; } = new();
}
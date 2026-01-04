using Sqordia.Application.Common.Models;

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
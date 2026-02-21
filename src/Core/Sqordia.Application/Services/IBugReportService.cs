using Microsoft.AspNetCore.Http;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.BugReport;
using Sqordia.Contracts.Responses.BugReport;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for managing bug reports.
/// </summary>
public interface IBugReportService
{
    /// <summary>
    /// Gets all bug reports with optional filtering.
    /// </summary>
    Task<Result<List<BugReportSummaryResponse>>> GetBugReportsAsync(
        string? status = null,
        string? severity = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a bug report by ID.
    /// </summary>
    Task<Result<BugReportResponse>> GetBugReportAsync(
        Guid bugReportId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a bug report by ticket number.
    /// </summary>
    Task<Result<BugReportResponse>> GetBugReportByTicketNumberAsync(
        string ticketNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new bug report.
    /// </summary>
    Task<Result<BugReportResponse>> CreateBugReportAsync(
        CreateBugReportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing bug report.
    /// </summary>
    Task<Result<BugReportResponse>> UpdateBugReportAsync(
        Guid bugReportId,
        UpdateBugReportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a bug report.
    /// </summary>
    Task<Result> DeleteBugReportAsync(
        Guid bugReportId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an attachment to a bug report.
    /// </summary>
    Task<Result<BugReportAttachmentResponse>> AddAttachmentAsync(
        Guid bugReportId,
        IFormFile file,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an attachment from a bug report.
    /// </summary>
    Task<Result> RemoveAttachmentAsync(
        Guid bugReportId,
        Guid attachmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the next ticket number.
    /// </summary>
    Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a bug report to PDF format.
    /// </summary>
    Task<Result<BugReportExportResult>> ExportToPdfAsync(
        Guid bugReportId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a bug report to Word (DOCX) format.
    /// </summary>
    Task<Result<BugReportExportResult>> ExportToWordAsync(
        Guid bugReportId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an export result for bug reports.
/// </summary>
public class BugReportExportResult
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}

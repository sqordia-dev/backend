using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.BugReport;
using Sqordia.Contracts.Responses.BugReport;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bug-reports")]
[Authorize]
public class BugReportController : BaseApiController
{
    private readonly IBugReportService _bugReportService;
    private readonly ILogger<BugReportController> _logger;

    public BugReportController(
        IBugReportService bugReportService,
        ILogger<BugReportController> logger)
    {
        _bugReportService = bugReportService;
        _logger = logger;
    }

    /// <summary>
    /// Get all bug reports with optional filtering by status and severity
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BugReportSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBugReports(
        [FromQuery] string? status = null,
        [FromQuery] string? severity = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.GetBugReportsAsync(status, severity, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a bug report by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BugReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBugReport(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.GetBugReportAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a bug report by ticket number
    /// </summary>
    [HttpGet("by-ticket/{ticketNumber}")]
    [ProducesResponseType(typeof(BugReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBugReportByTicketNumber(
        string ticketNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.GetBugReportByTicketNumberAsync(ticketNumber, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Create a new bug report
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BugReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBugReport(
        [FromBody] CreateBugReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.CreateBugReportAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update an existing bug report
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BugReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateBugReport(
        Guid id,
        [FromBody] UpdateBugReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.UpdateBugReportAsync(id, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a bug report
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteBugReport(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.DeleteBugReportAsync(id, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Add an attachment to a bug report
    /// </summary>
    [HttpPost("{id:guid}/attachments")]
    [ProducesResponseType(typeof(BugReportAttachmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddAttachment(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.AddAttachmentAsync(id, file, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove an attachment from a bug report
    /// </summary>
    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveAttachment(
        Guid id,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.RemoveAttachmentAsync(id, attachmentId, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get the next ticket number (for preview before submission)
    /// </summary>
    [HttpGet("next-ticket-number")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNextTicketNumber(CancellationToken cancellationToken = default)
    {
        var ticketNumber = await _bugReportService.GenerateTicketNumberAsync(cancellationToken);
        return Ok(new { ticketNumber });
    }

    /// <summary>
    /// Export a bug report to PDF
    /// </summary>
    [HttpGet("{id:guid}/export/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportToPdf(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.ExportToPdfAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var exportResult = result.Value!;
        return File(exportResult.FileData, exportResult.ContentType, exportResult.FileName);
    }

    /// <summary>
    /// Export a bug report to Word (DOCX)
    /// </summary>
    [HttpGet("{id:guid}/export/word")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportToWord(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _bugReportService.ExportToWordAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var exportResult = result.Value!;
        return File(exportResult.FileData, exportResult.ContentType, exportResult.FileName);
    }
}

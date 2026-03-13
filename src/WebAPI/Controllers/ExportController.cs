using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Export;
using Sqordia.Domain.Constants;
using Sqordia.Domain.Enums;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/business-plans/{businessPlanId}/export")]
[Authorize]
public class ExportController : BaseApiController
{
    private readonly IDocumentExportService _exportService;
    private readonly ISlideDeckService _slideDeckService;
    private readonly IThemedPdfService _themedPdfService;
    private readonly IDocumentAgentService _documentAgent;
    private readonly IFeatureGateService _featureGate;
    private readonly INotificationService _notificationService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ExportController> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExportController(
        IDocumentExportService exportService,
        ISlideDeckService slideDeckService,
        IThemedPdfService themedPdfService,
        IDocumentAgentService documentAgent,
        IFeatureGateService featureGate,
        INotificationService notificationService,
        IApplicationDbContext context,
        ILogger<ExportController> logger,
        IServiceScopeFactory scopeFactory)
    {
        _exportService = exportService;
        _slideDeckService = slideDeckService;
        _themedPdfService = themedPdfService;
        _documentAgent = documentAgent;
        _featureGate = featureGate;
        _notificationService = notificationService;
        _context = context;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Export a business plan to PDF format
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="language">Export language (fr or en). Defaults to fr.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file download</returns>
    /// <remarks>
    /// This endpoint exports a complete business plan to a professionally formatted PDF document.
    /// The PDF includes all generated sections with proper styling and branding.
    ///
    /// Prerequisites:
    /// - The business plan must exist
    /// - The user must have access to the business plan's organization
    /// - At least some sections should be generated for a meaningful export
    ///
    /// Sample request:
    ///     GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/export/pdf?language=en
    ///
    /// The response will be a file download with appropriate Content-Disposition headers.
    /// </remarks>
    /// <response code="200">PDF file successfully generated and returned</response>
    /// <response code="400">Invalid request or business plan not ready for export</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet("pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToPdf(
        Guid businessPlanId,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PDF export request for business plan {BusinessPlanId} in {Language}",
            businessPlanId, language);

        if (language != "fr" && language != "en")
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });

        var (denied, orgId) = await CheckExportFeatureAsync(businessPlanId, PlanFeatures.ExportPdf, cancellationToken);
        if (denied != null) return denied;

        var result = await _exportService.ExportToPdfAsync(businessPlanId, language, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var exportResult = result.Value!;
        await RecordExportUsageAsync(orgId, PlanFeatures.ExportPdf, cancellationToken, exportResult.FileData.LongLength);
        FireExportNotification(businessPlanId, "PDF", exportResult.FileName);

        return File(
            exportResult.FileData,
            exportResult.ContentType,
            exportResult.FileName);
    }

    /// <summary>
    /// Export a themed PDF using Puppeteer (selectable text, proper page breaks).
    /// Renders the same HTML template as the frontend preview.
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="language">Export language (fr or en). Defaults to fr.</param>
    /// <param name="themeId">Theme ID (classic, modern, corporate, etc.). Defaults to classic.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file download with selectable text</returns>
    [HttpGet("themed-pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToThemedPdf(
        Guid businessPlanId,
        [FromQuery] string language = "fr",
        [FromQuery] string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Themed PDF export for business plan {BusinessPlanId} in {Language} with theme {ThemeId}",
            businessPlanId, language, themeId ?? "classic");

        if (language != "fr" && language != "en")
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });

        var (denied, orgId) = await CheckExportFeatureAsync(businessPlanId, PlanFeatures.ExportPdf, cancellationToken);
        if (denied != null) return denied;

        var result = await _themedPdfService.GenerateThemedPdfAsync(
            businessPlanId, themeId, language, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var exportResult = result.Value!;
        await RecordExportUsageAsync(orgId, PlanFeatures.ExportPdf, cancellationToken, exportResult.FileData.LongLength);
        FireExportNotification(businessPlanId, "PDF", exportResult.FileName);
        return File(exportResult.FileData, exportResult.ContentType, exportResult.FileName);
    }

    /// <summary>
    /// Export a business plan to Word (DOCX) format
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="language">Export language (fr or en). Defaults to fr.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Word document file download</returns>
    /// <remarks>
    /// This endpoint exports a complete business plan to a Microsoft Word document.
    /// The Word document can be further edited and customized by the user.
    ///
    /// Features:
    /// - Editable Word format (.docx)
    /// - Professional formatting with proper headings
    /// - Bilingual support (French/English)
    /// - All business plan sections included
    ///
    /// Sample request:
    ///     GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/export/word?language=fr
    /// </remarks>
    /// <response code="200">Word document successfully generated and returned</response>
    /// <response code="400">Invalid request or business plan not ready for export</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet("word")]
    [HttpGet("docx")] // Alias for compatibility
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToWord(
        Guid businessPlanId,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Word export request for business plan {BusinessPlanId} in {Language}",
            businessPlanId, language);

        if (language != "fr" && language != "en")
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });

        var (denied, orgId) = await CheckExportFeatureAsync(businessPlanId, PlanFeatures.ExportWord, cancellationToken);
        if (denied != null) return denied;

        var result = await _exportService.ExportToWordAsync(businessPlanId, language, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var exportResult = result.Value!;
        await RecordExportUsageAsync(orgId, PlanFeatures.ExportWord, cancellationToken, exportResult.FileData.LongLength);
        FireExportNotification(businessPlanId, "Word", exportResult.FileName);

        return File(
            exportResult.FileData,
            exportResult.ContentType,
            exportResult.FileName);
    }

    /// <summary>
    /// Export a business plan to HTML format for preview
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="language">Export language (fr or en). Defaults to fr.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTML content for preview</returns>
    /// <remarks>
    /// This endpoint generates an HTML preview of the business plan that can be displayed
    /// in a web browser or embedded in applications. Useful for previewing before downloading.
    ///
    /// The HTML includes:
    /// - Responsive design for mobile and desktop
    /// - Print-friendly CSS styles
    /// - Professional formatting
    /// - Embedded CSS (no external dependencies)
    ///
    /// Sample request:
    ///     GET /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/export/html?language=en
    ///
    /// Sample response:
    /// {
    ///     "html": "&lt;!DOCTYPE html&gt;&lt;html&gt;...&lt;/html&gt;",
    ///     "language": "en",
    ///     "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "generatedAt": "2025-10-15T10:30:00Z"
    /// }
    /// </remarks>
    /// <response code="200">HTML content successfully generated</response>
    /// <response code="400">Invalid request or business plan not ready for export</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet("html")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToHtml(
        Guid businessPlanId,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("HTML export request for business plan {BusinessPlanId} in {Language}",
            businessPlanId, language);

        // Validate language parameter
        if (language != "fr" && language != "en")
        {
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });
        }

        var result = await _exportService.ExportToHtmlAsync(businessPlanId, language, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var htmlContent = result.Value!;

        return Ok(new
        {
            html = htmlContent,
            language = language,
            businessPlanId = businessPlanId,
            generatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get available export templates
    /// </summary>
    /// <returns>List of available export templates</returns>
    /// <remarks>
    /// This endpoint returns the available templates that can be used for exporting business plans.
    /// Each template has different styling and formatting options.
    ///
    /// Sample response:
    /// [
    ///     {
    ///         "id": "default",
    ///         "name": "Default Template",
    ///         "description": "Clean, professional business plan template",
    ///         "isDefault": true,
    ///         "supportedFormats": ["pdf", "docx", "html"],
    ///         "supportedLanguages": ["fr", "en"]
    ///     }
    /// ]
    /// </remarks>
    /// <response code="200">Template list retrieved successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("templates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAvailableTemplates(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving available export templates");

        var result = await _exportService.GetAvailableTemplatesAsync();

        if (!result.IsSuccess)
            return HandleResult(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// Export a business plan to Excel (XLSX) format
    /// </summary>
    [HttpGet("excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToExcel(
        Guid businessPlanId,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Excel export request for business plan {BusinessPlanId} in {Language}",
            businessPlanId, language);

        if (language != "fr" && language != "en")
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });

        var (denied, orgId) = await CheckExportFeatureAsync(businessPlanId, PlanFeatures.ExportExcel, cancellationToken);
        if (denied != null) return denied;

        var result = await _exportService.ExportToExcelAsync(businessPlanId, language, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var exportResult = result.Value!;
        await RecordExportUsageAsync(orgId, PlanFeatures.ExportExcel, cancellationToken, exportResult.FileData.LongLength);
        FireExportNotification(businessPlanId, "Excel", exportResult.FileName);
        return File(exportResult.FileData, exportResult.ContentType, exportResult.FileName);
    }

    /// <summary>
    /// Export a business plan to PowerPoint (PPTX) format.
    /// When the SlideDeckExport feature flag is enabled, generates a full slide deck with AI-summarized content.
    /// Otherwise, falls back to a basic title-slide-only export.
    /// </summary>
    [HttpGet("powerpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToPowerPoint(
        Guid businessPlanId,
        [FromQuery] string language = "fr",
        [FromQuery] string? themeId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PowerPoint export request for business plan {BusinessPlanId} in {Language} with theme {ThemeId}",
            businessPlanId, language, themeId ?? "default");

        if (language != "fr" && language != "en")
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });

        var (denied, orgId) = await CheckExportFeatureAsync(businessPlanId, PlanFeatures.ExportPowerpoint, cancellationToken);
        if (denied != null) return denied;

        // Use SlideDeckService for full AI-powered slide deck, fall back to basic export on failure
        var slideDeckResult = await _slideDeckService.GenerateSlideDeckAsync(
            businessPlanId, themeId, language, cancellationToken);

        if (slideDeckResult.IsSuccess)
        {
            var deckExport = slideDeckResult.Value!;
            await RecordExportUsageAsync(orgId, PlanFeatures.ExportPowerpoint, cancellationToken, deckExport.FileData.LongLength);
            FireExportNotification(businessPlanId, "PowerPoint", deckExport.FileName);
            return File(deckExport.FileData, deckExport.ContentType, deckExport.FileName);
        }

        _logger.LogWarning("SlideDeckService failed, falling back to basic PowerPoint export: {Error}",
            slideDeckResult.Error?.Message ?? "Unknown");

        // Fallback to basic PowerPoint export
        var result = await _exportService.ExportToPowerPointAsync(businessPlanId, language, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var exportResult = result.Value!;
        await RecordExportUsageAsync(orgId, PlanFeatures.ExportPowerpoint, cancellationToken, exportResult.FileData.LongLength);
        FireExportNotification(businessPlanId, "PowerPoint", exportResult.FileName);
        return File(exportResult.FileData, exportResult.ContentType, exportResult.FileName);
    }

    /// <summary>
    /// Get export statistics for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Export readiness information</returns>
    /// <remarks>
    /// This endpoint provides information about how ready a business plan is for export,
    /// including section completion status and recommended export formats.
    ///
    /// Sample response:
    /// {
    ///     "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "isReadyForExport": true,
    ///     "completedSections": 12,
    ///     "totalSections": 15,
    ///     "completionPercentage": 80.0,
    ///     "availableFormats": ["pdf", "docx", "html", "xlsx", "pptx"],
    ///     "estimatedPdfPages": 25,
    ///     "lastUpdated": "2025-10-15T09:30:00Z"
    /// }
    /// </remarks>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetExportStatus(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting export status for business plan {BusinessPlanId}", businessPlanId);

        return Ok(new
        {
            businessPlanId = businessPlanId,
            isReadyForExport = true,
            availableFormats = new[] { "pdf", "docx", "html", "xlsx", "pptx" },
            supportedLanguages = new[] { "fr", "en" },
            lastChecked = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Export a business plan to PDF format with visual elements (charts, tables, metrics)
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="request">Export request with visual element options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PDF file download with embedded visual elements</returns>
    /// <remarks>
    /// This enhanced export endpoint supports visual elements including:
    /// - Tables (financial, SWOT, comparison, etc.)
    /// - Charts (line, bar, pie, donut, area, stacked-bar)
    /// - Metric cards with values and trends
    /// - Infographics (process-flow, icon-list, timeline)
    ///
    /// The request body can include:
    /// - Custom cover page settings
    /// - Section-level visual elements
    /// - Table of contents toggle
    /// - Visual rendering options
    ///
    /// Sample request:
    ///     POST /api/v1/business-plans/3fa85f64-5717-4562-b3fc-2c963f66afa6/export/pdf-with-visuals
    ///     {
    ///         "format": "pdf",
    ///         "language": "en",
    ///         "includeVisuals": true,
    ///         "includeTableOfContents": true,
    ///         "coverPageSettings": {
    ///             "companyName": "Acme Corp",
    ///             "documentTitle": "Business Plan 2025",
    ///             "primaryColor": "#2563EB"
    ///         }
    ///     }
    /// </remarks>
    /// <response code="200">PDF file with visual elements successfully generated</response>
    /// <response code="400">Invalid request or business plan not ready for export</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("pdf-with-visuals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToPdfWithVisuals(
        Guid businessPlanId,
        [FromBody] ExportWithVisualsRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PDF export with visuals request for business plan {BusinessPlanId} in {Language}",
            businessPlanId, request.Language);

        // Validate language parameter
        if (request.Language != "fr" && request.Language != "en")
        {
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });
        }

        var result = await _exportService.ExportToPdfWithVisualsAsync(businessPlanId, request, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var exportResult = result.Value!;

        return File(
            exportResult.FileData,
            exportResult.ContentType,
            exportResult.FileName);
    }

    /// <summary>
    /// Export a business plan to Word (DOCX) format with visual elements
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="request">Export request with visual element options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Word document file download with embedded visual elements</returns>
    /// <remarks>
    /// This enhanced export endpoint creates a Word document with visual elements including
    /// formatted tables, metric summaries, and infographic representations.
    /// Charts are rendered as placeholders with data summaries (interactive chart rendering
    /// requires specialized Word add-ins).
    ///
    /// The Word document includes:
    /// - Professional cover page with branding
    /// - Table of contents
    /// - Sections with prose content and visual elements
    /// - Properly formatted tables with alternating row colors
    /// - Metric cards displayed as styled tables
    /// </remarks>
    /// <response code="200">Word document with visual elements successfully generated</response>
    /// <response code="400">Invalid request or business plan not ready for export</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("word-with-visuals")]
    [HttpPost("docx-with-visuals")] // Alias for compatibility
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToWordWithVisuals(
        Guid businessPlanId,
        [FromBody] ExportWithVisualsRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Word export with visuals request for business plan {BusinessPlanId} in {Language}",
            businessPlanId, request.Language);

        // Validate language parameter
        if (request.Language != "fr" && request.Language != "en")
        {
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });
        }

        var result = await _exportService.ExportToWordWithVisualsAsync(businessPlanId, request, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var exportResult = result.Value!;

        return File(
            exportResult.FileData,
            exportResult.ContentType,
            exportResult.FileName);
    }

    /// <summary>
    /// Export a business plan to HTML format with visual elements for preview
    /// </summary>
    /// <param name="businessPlanId">The business plan ID to export</param>
    /// <param name="request">Export request with visual element options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>HTML content with embedded visual elements</returns>
    /// <remarks>
    /// This endpoint generates an HTML preview with visual elements that can be displayed
    /// in a web browser. The HTML includes embedded CSS for styling and is optimized for
    /// print output.
    ///
    /// Visual elements are rendered as:
    /// - Tables: Full HTML tables with proper styling
    /// - Charts: Placeholder divs (JavaScript charting library needed for interactivity)
    /// - Metrics: Styled card components
    /// - Infographics: Numbered step lists
    /// </remarks>
    /// <response code="200">HTML content with visual elements successfully generated</response>
    /// <response code="400">Invalid request or business plan not ready for export</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpPost("html-with-visuals")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportToHtmlWithVisuals(
        Guid businessPlanId,
        [FromBody] ExportWithVisualsRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("HTML export with visuals request for business plan {BusinessPlanId} in {Language}",
            businessPlanId, request.Language);

        // Validate language parameter
        if (request.Language != "fr" && request.Language != "en")
        {
            return BadRequest(new { error = "Language must be either 'fr' or 'en'" });
        }

        var result = await _exportService.ExportToHtmlWithVisualsAsync(businessPlanId, request, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var htmlContent = result.Value!;

        return Ok(new
        {
            html = htmlContent,
            language = request.Language,
            businessPlanId = businessPlanId,
            includesVisuals = request.IncludeVisuals,
            generatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get detailed export preview information including visual element counts
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed export preview information with section and visual element details</returns>
    /// <remarks>
    /// This endpoint provides detailed information about the business plan's export readiness,
    /// including:
    /// - Section completion status and word counts
    /// - Visual element counts by type
    /// - Estimated page count for PDF
    /// - Available export formats and languages
    ///
    /// Sample response:
    /// {
    ///     "businessPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "isReadyForExport": true,
    ///     "completedSections": 8,
    ///     "totalSections": 10,
    ///     "completionPercentage": 80.0,
    ///     "totalVisualElements": 12,
    ///     "estimatedPdfPages": 25,
    ///     "sections": [
    ///         {
    ///             "sectionKey": "ExecutiveSummary",
    ///             "title": "Executive Summary",
    ///             "hasContent": true,
    ///             "wordCount": 450,
    ///             "visualElementCount": 2,
    ///             "visualElementTypes": ["table", "metric"]
    ///         }
    ///     ]
    /// }
    /// </remarks>
    /// <response code="200">Export preview information retrieved successfully</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="404">Business plan not found</response>
    [HttpGet("preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExportPreview(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting export preview for business plan {BusinessPlanId}", businessPlanId);

        var result = await _exportService.GetExportPreviewAsync(businessPlanId, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        return Ok(result.Value);
    }

    // ── Document Agent Blueprint Endpoints ────────────────────

    /// <summary>
    /// Generate a structured Word document blueprint using the Claude document agent.
    /// Returns a JSON blueprint (headings, paragraphs, tables, SWOT grids, callouts)
    /// that can be rendered into a professional .docx by the frontend or backend renderer.
    /// </summary>
    [HttpPost("agent/word-blueprint")]
    [ProducesResponseType(typeof(WordDocumentBlueprint), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateWordBlueprint(
        Guid businessPlanId,
        [FromBody] DocumentAgentExportRequest request,
        CancellationToken ct = default)
    {
        var agentRequest = await BuildAgentRequestAsync(businessPlanId, request.Language, ct);
        if (agentRequest == null)
            return NotFound(new { error = "Business plan not found or has no content" });

        var result = await _documentAgent.GenerateWordBlueprintAsync(agentRequest, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Generate a structured PDF document blueprint using the Claude document agent.
    /// Returns a JSON blueprint with cover page, header/footer, sections, tables,
    /// SWOT grids, metrics panels, chart placeholders — ready for the Puppeteer/PDF renderer.
    /// </summary>
    [HttpPost("agent/pdf-blueprint")]
    [ProducesResponseType(typeof(PdfDocumentBlueprint), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GeneratePdfBlueprint(
        Guid businessPlanId,
        [FromBody] DocumentAgentExportRequest request,
        CancellationToken ct = default)
    {
        var agentRequest = await BuildAgentRequestAsync(businessPlanId, request.Language, ct);
        if (agentRequest == null)
            return NotFound(new { error = "Business plan not found or has no content" });

        var result = await _documentAgent.GeneratePdfBlueprintAsync(agentRequest, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Generate a structured presentation blueprint using the Claude document agent.
    /// Returns a JSON blueprint (title, content, two-column, SWOT, table, metrics slides)
    /// with speaker notes, ready for the PPTX renderer.
    /// </summary>
    [HttpPost("agent/presentation-blueprint")]
    [ProducesResponseType(typeof(PresentationBlueprint), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GeneratePresentationBlueprint(
        Guid businessPlanId,
        [FromBody] DocumentAgentExportRequest request,
        CancellationToken ct = default)
    {
        var agentRequest = await BuildAgentRequestAsync(businessPlanId, request.Language, ct);
        if (agentRequest == null)
            return NotFound(new { error = "Business plan not found or has no content" });

        var result = await _documentAgent.GeneratePresentationBlueprintAsync(agentRequest, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Generate a structured spreadsheet blueprint using the Claude document agent.
    /// Returns a JSON blueprint (sheets with headers, rows, summary rows, chart hints)
    /// for the Excel renderer.
    /// </summary>
    [HttpPost("agent/spreadsheet-blueprint")]
    [ProducesResponseType(typeof(SpreadsheetBlueprint), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateSpreadsheetBlueprint(
        Guid businessPlanId,
        [FromBody] DocumentAgentExportRequest request,
        CancellationToken ct = default)
    {
        var agentRequest = await BuildAgentRequestAsync(businessPlanId, request.Language, ct);
        if (agentRequest == null)
            return NotFound(new { error = "Business plan not found or has no content" });

        var result = await _documentAgent.GenerateSpreadsheetBlueprintAsync(agentRequest, ct);
        return HandleResult(result);
    }

    private async Task<DocumentAgentRequest?> BuildAgentRequestAsync(
        Guid businessPlanId, string language, CancellationToken ct)
    {
        var bp = await _exportService.GetExportPreviewAsync(businessPlanId, ct);
        if (!bp.IsSuccess) return null;

        // Use the HTML export to get all section content
        var htmlResult = await _exportService.ExportToHtmlAsync(businessPlanId, language, ct);
        if (!htmlResult.IsSuccess) return null;

        // Build sections dict from the business plan via a lightweight query
        // For now, return a request with the HTML content as a single section
        // The agent will parse and structure it
        return new DocumentAgentRequest
        {
            Sections = new Dictionary<string, string>
            {
                ["FullPlan"] = htmlResult.Value ?? ""
            },
            CompanyName = bp.Value?.ToString() ?? "",
            PlanTitle = $"Business Plan",
            Language = language
        };
    }

    /// <summary>
    /// Checks whether the organization that owns the business plan has the given export feature enabled.
    /// Returns null if allowed, or a 403 Forbidden result if denied.
    /// Also records export usage on success.
    /// </summary>
    private async Task<(IActionResult? Denied, Guid OrgId)> CheckExportFeatureAsync(
        Guid businessPlanId, string featureKey, CancellationToken ct)
    {
        var orgId = await _context.BusinessPlans
            .Where(bp => bp.Id == businessPlanId && !bp.IsDeleted)
            .Select(bp => (Guid?)bp.OrganizationId)
            .FirstOrDefaultAsync(ct);

        if (orgId == null)
            return (NotFound(new { error = "Business plan not found" }), Guid.Empty);

        var allowed = await _featureGate.IsFeatureEnabledAsync(orgId.Value, featureKey, ct);
        if (!allowed)
        {
            return (StatusCode(403, new
            {
                error = $"Your plan does not include this export format. Upgrade to unlock it.",
                featureKey
            }), orgId.Value);
        }

        return (null, orgId.Value);
    }

    private async Task RecordExportUsageAsync(Guid orgId, string featureKey, CancellationToken ct, long fileSizeBytes = 0)
    {
        await _featureGate.RecordUsageAsync(orgId, featureKey, 1, ct);
        if (fileSizeBytes > 0)
        {
            await _featureGate.RecordUsageAsync(orgId, PlanFeatures.MaxStorageMb, (int)fileSizeBytes, ct);
        }
    }

    private void FireExportNotification(Guid businessPlanId, string format, string fileName)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return;

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.CreateNotificationAsync(
                    new CreateNotificationCommand(
                        userId.Value,
                        NotificationType.ExportCompleted,
                        NotificationCategory.BusinessPlan,
                        $"Export {format} terminé : {fileName}",
                        $"{format} export completed: {fileName}",
                        "Votre document est prêt.",
                        "Your document is ready.",
                        ActionUrl: $"/business-plan/{businessPlanId}/preview",
                        RelatedEntityId: businessPlanId,
                        GroupKey: $"export-{businessPlanId}"),
                    CancellationToken.None);
            }
            catch { /* Non-critical */ }
        });
    }
}

public class DocumentAgentExportRequest
{
    public string Language { get; set; } = "fr";
}
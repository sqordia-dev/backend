using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXmlDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.BugReport;
using Sqordia.Contracts.Responses.BugReport;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Infrastructure.Services;

public class BugReportService : IBugReportService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStorageService _storageService;
    private readonly ILogger<BugReportService> _logger;

    private static readonly string[] AllowedContentTypes = new[]
    {
        "image/png", "image/jpeg", "image/gif", "image/webp",
        "video/mp4", "video/webm",
        "application/pdf"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public BugReportService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IStorageService storageService,
        ILogger<BugReportService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Result<List<BugReportSummaryResponse>>> GetBugReportsAsync(
        string? status = null,
        string? severity = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.BugReports
                .Include(b => b.Attachments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BugReportStatus>(status, true, out var statusEnum))
            {
                query = query.Where(b => b.Status == statusEnum);
            }

            if (!string.IsNullOrWhiteSpace(severity) && Enum.TryParse<BugReportSeverity>(severity, true, out var severityEnum))
            {
                query = query.Where(b => b.Severity == severityEnum);
            }

            var reports = await query
                .OrderByDescending(b => b.Created)
                .ToListAsync(cancellationToken);

            var userIds = reports.Select(b => b.ReportedByUserId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FirstName + " " + u.LastName, cancellationToken);

            var responses = reports.Select(b => MapToSummaryResponse(b, users)).ToList();

            return Result.Success(responses);
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available (migration may not be applied).");
            return Result.Success(new List<BugReportSummaryResponse>());
        }
    }

    public async Task<Result<BugReportResponse>> GetBugReportAsync(
        Guid bugReportId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.BugReports
                .Include(b => b.Attachments)
                .FirstOrDefaultAsync(b => b.Id == bugReportId, cancellationToken);

            if (report == null)
            {
                return Result.Failure<BugReportResponse>(
                    Error.NotFound("BugReport.NotFound", $"Bug report with ID '{bugReportId}' was not found."));
            }

            var response = await MapToResponseAsync(report, cancellationToken);
            return Result.Success(response);
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available.");
            return Result.Failure<BugReportResponse>(BugReportTablesNotAvailableError);
        }
    }

    public async Task<Result<BugReportResponse>> GetBugReportByTicketNumberAsync(
        string ticketNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.BugReports
                .Include(b => b.Attachments)
                .FirstOrDefaultAsync(b => b.TicketNumber == ticketNumber, cancellationToken);

            if (report == null)
            {
                return Result.Failure<BugReportResponse>(
                    Error.NotFound("BugReport.NotFound", $"Bug report with ticket number '{ticketNumber}' was not found."));
            }

            var response = await MapToResponseAsync(report, cancellationToken);
            return Result.Success(response);
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available.");
            return Result.Failure<BugReportResponse>(BugReportTablesNotAvailableError);
        }
    }

    public async Task<Result<BugReportResponse>> CreateBugReportAsync(
        CreateBugReportRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<BugReportSeverity>(request.Severity, true, out var severity))
            {
                return Result.Failure<BugReportResponse>(
                    Error.Validation("BugReport.InvalidSeverity", $"Invalid severity value: '{request.Severity}'. Valid values: Low, Medium, High, Critical."));
            }

            Guid? userId = null;
            if (_currentUserService.IsAuthenticated && !string.IsNullOrEmpty(_currentUserService.UserId))
            {
                userId = Guid.Parse(_currentUserService.UserId);
            }

            var ticketNumber = await GenerateTicketNumberAsync(cancellationToken);

            var bugReport = new BugReport(
                request.Title,
                request.PageSection,
                severity,
                request.Description,
                ticketNumber,
                userId,
                request.AppVersion,
                request.Browser,
                request.OperatingSystem);

            _context.BugReports.Add(bugReport);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bug report {TicketNumber} created by user {UserId}", ticketNumber, userId);

            var response = await MapToResponseAsync(bugReport, cancellationToken);
            return Result.Success(response);
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available.");
            return Result.Failure<BugReportResponse>(BugReportTablesNotAvailableError);
        }
    }

    public async Task<Result<BugReportResponse>> UpdateBugReportAsync(
        Guid bugReportId,
        UpdateBugReportRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.BugReports
                .Include(b => b.Attachments)
                .FirstOrDefaultAsync(b => b.Id == bugReportId, cancellationToken);

            if (report == null)
            {
                return Result.Failure<BugReportResponse>(
                    Error.NotFound("BugReport.NotFound", $"Bug report with ID '{bugReportId}' was not found."));
            }

            // Update basic fields if provided
            if (!string.IsNullOrWhiteSpace(request.Title) ||
                !string.IsNullOrWhiteSpace(request.PageSection) ||
                !string.IsNullOrWhiteSpace(request.Severity) ||
                !string.IsNullOrWhiteSpace(request.Description))
            {
                var newSeverity = report.Severity;
                if (!string.IsNullOrWhiteSpace(request.Severity))
                {
                    if (!Enum.TryParse<BugReportSeverity>(request.Severity, true, out newSeverity))
                    {
                        return Result.Failure<BugReportResponse>(
                            Error.Validation("BugReport.InvalidSeverity", $"Invalid severity value: '{request.Severity}'."));
                    }
                }

                report.Update(
                    request.Title ?? report.Title,
                    request.PageSection ?? report.PageSection,
                    newSeverity,
                    request.Description ?? report.Description);
            }

            // Update status if provided
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse<BugReportStatus>(request.Status, true, out var newStatus))
                {
                    return Result.Failure<BugReportResponse>(
                        Error.Validation("BugReport.InvalidStatus", $"Invalid status value: '{request.Status}'."));
                }

                var userId = _currentUserService.IsAuthenticated && !string.IsNullOrEmpty(_currentUserService.UserId)
                    ? Guid.Parse(_currentUserService.UserId)
                    : Guid.Empty;

                switch (newStatus)
                {
                    case BugReportStatus.InProgress:
                        report.SetInProgress();
                        break;
                    case BugReportStatus.Resolved:
                        report.Resolve(request.ResolutionNotes ?? string.Empty, userId);
                        break;
                    case BugReportStatus.Closed:
                        report.Close();
                        break;
                    case BugReportStatus.WontFix:
                        report.MarkAsWontFix(request.ResolutionNotes ?? string.Empty, userId);
                        break;
                    case BugReportStatus.Open:
                        report.Reopen();
                        break;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bug report {TicketNumber} updated", report.TicketNumber);

            var response = await MapToResponseAsync(report, cancellationToken);
            return Result.Success(response);
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available.");
            return Result.Failure<BugReportResponse>(BugReportTablesNotAvailableError);
        }
    }

    public async Task<Result> DeleteBugReportAsync(
        Guid bugReportId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.BugReports
                .FirstOrDefaultAsync(b => b.Id == bugReportId, cancellationToken);

            if (report == null)
            {
                return Result.Failure(
                    Error.NotFound("BugReport.NotFound", $"Bug report with ID '{bugReportId}' was not found."));
            }

            report.SoftDelete();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bug report {TicketNumber} soft-deleted", report.TicketNumber);

            return Result.Success();
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available.");
            return Result.Failure(BugReportTablesNotAvailableError);
        }
    }

    public async Task<Result<BugReportAttachmentResponse>> AddAttachmentAsync(
        Guid bugReportId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.BugReports
                .FirstOrDefaultAsync(b => b.Id == bugReportId, cancellationToken);

            if (report == null)
            {
                return Result.Failure<BugReportAttachmentResponse>(
                    Error.NotFound("BugReport.NotFound", $"Bug report with ID '{bugReportId}' was not found."));
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return Result.Failure<BugReportAttachmentResponse>(
                    Error.Validation("BugReport.InvalidFileType", $"File type '{file.ContentType}' is not allowed."));
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return Result.Failure<BugReportAttachmentResponse>(
                    Error.Validation("BugReport.FileTooLarge", $"File size exceeds maximum allowed size of 10 MB."));
            }

            var storageKey = $"bug-reports/{bugReportId}/{Guid.NewGuid()}/{file.FileName}";

            string url;
            using (var stream = file.OpenReadStream())
            {
                url = await _storageService.UploadFileAsync(storageKey, stream, file.ContentType, cancellationToken);
            }

            var attachment = new BugReportAttachment(
                bugReportId,
                file.FileName,
                file.ContentType,
                file.Length,
                url);

            _context.BugReportAttachments.Add(attachment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Attachment '{FileName}' added to bug report {TicketNumber}", file.FileName, report.TicketNumber);

            return Result.Success(MapToAttachmentResponse(attachment));
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available.");
            return Result.Failure<BugReportAttachmentResponse>(BugReportTablesNotAvailableError);
        }
    }

    public async Task<Result> RemoveAttachmentAsync(
        Guid bugReportId,
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var attachment = await _context.BugReportAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.BugReportId == bugReportId, cancellationToken);

            if (attachment == null)
            {
                return Result.Failure(
                    Error.NotFound("BugReport.AttachmentNotFound", $"Attachment with ID '{attachmentId}' was not found."));
            }

            attachment.SoftDelete();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Attachment {AttachmentId} soft-deleted from bug report {BugReportId}", attachmentId, bugReportId);

            return Result.Success();
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available.");
            return Result.Failure(BugReportTablesNotAvailableError);
        }
    }

    public async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var lastTicket = await _context.BugReports
                .IgnoreQueryFilters()
                .OrderByDescending(b => b.Created)
                .Select(b => b.TicketNumber)
                .FirstOrDefaultAsync(cancellationToken);

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastTicket) && lastTicket.StartsWith("BUG-"))
            {
                var numberPart = lastTicket.Substring(4);
                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"BUG-{nextNumber:D4}";
        }
        catch
        {
            // If tables don't exist, start with 1
            return "BUG-0001";
        }
    }

    public async Task<Result<BugReportExportResult>> ExportToPdfAsync(
        Guid bugReportId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.BugReports
                .Include(b => b.Attachments)
                .FirstOrDefaultAsync(b => b.Id == bugReportId, cancellationToken);

            if (report == null)
            {
                return Result.Failure<BugReportExportResult>(
                    Error.NotFound("BugReport.NotFound", $"Bug report with ID '{bugReportId}' was not found."));
            }

            var userName = await GetUserNameAsync(report.ReportedByUserId, cancellationToken);
            var resolverName = await GetUserNameAsync(report.ResolvedByUserId, cancellationToken);

            var pdfBytes = GeneratePdf(report, userName, resolverName);

            return Result.Success(new BugReportExportResult
            {
                FileData = pdfBytes,
                FileName = $"{report.TicketNumber}.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = pdfBytes.Length,
                ExportedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available.");
            return Result.Failure<BugReportExportResult>(BugReportTablesNotAvailableError);
        }
    }

    public async Task<Result<BugReportExportResult>> ExportToWordAsync(
        Guid bugReportId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.BugReports
                .Include(b => b.Attachments)
                .FirstOrDefaultAsync(b => b.Id == bugReportId, cancellationToken);

            if (report == null)
            {
                return Result.Failure<BugReportExportResult>(
                    Error.NotFound("BugReport.NotFound", $"Bug report with ID '{bugReportId}' was not found."));
            }

            var userName = await GetUserNameAsync(report.ReportedByUserId, cancellationToken);
            var resolverName = await GetUserNameAsync(report.ResolvedByUserId, cancellationToken);

            var wordBytes = GenerateWord(report, userName, resolverName);

            return Result.Success(new BugReportExportResult
            {
                FileData = wordBytes,
                FileName = $"{report.TicketNumber}.docx",
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileSizeBytes = wordBytes.Length,
                ExportedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex) when (IsBugReportTableMissing(ex))
        {
            _logger.LogWarning(ex, "BugReport tables not available.");
            return Result.Failure<BugReportExportResult>(BugReportTablesNotAvailableError);
        }
    }

    private byte[] GeneratePdf(BugReport report, string? reporterName, string? resolverName)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(c => ComposeHeader(c, report));
                page.Content().Element(c => ComposeContent(c, report, reporterName, resolverName));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, BugReport report)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Bug Report").FontSize(24).Bold().FontColor(Colors.Grey.Darken3);
                column.Item().Text(report.TicketNumber).FontSize(14).FontColor(Colors.Orange.Medium);
            });

            row.ConstantItem(100).Height(50).AlignRight().Column(column =>
            {
                var severityColor = report.Severity switch
                {
                    BugReportSeverity.Critical => Colors.Red.Medium,
                    BugReportSeverity.High => Colors.Orange.Medium,
                    BugReportSeverity.Medium => Colors.Yellow.Darken2,
                    _ => Colors.Blue.Medium
                };
                column.Item().Background(severityColor).Padding(5).Text(report.Severity.ToString()).FontColor(Colors.White).Bold();
            });
        });
    }

    private void ComposeContent(IContainer container, BugReport report, string? reporterName, string? resolverName)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Title
            column.Item().Text(report.Title).FontSize(18).Bold();

            // Meta info
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Page/Section: {report.PageSection}");
                row.RelativeItem().Text($"Status: {report.Status}");
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Reported: {report.Created:yyyy-MM-dd HH:mm}");
                row.RelativeItem().Text($"By: {reporterName ?? "Unknown"}");
            });

            // Description
            column.Item().Text("Description").FontSize(14).Bold();
            column.Item().Text(report.Description);

            // System Information
            column.Item().Text("System Information").FontSize(14).Bold();
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"App Version: {report.AppVersion ?? "N/A"}");
                row.RelativeItem().Text($"Browser: {report.Browser ?? "N/A"}");
                row.RelativeItem().Text($"OS: {report.OperatingSystem ?? "N/A"}");
            });

            // Resolution (if resolved)
            if (report.ResolvedAt.HasValue)
            {
                column.Item().Text("Resolution").FontSize(14).Bold();
                column.Item().Text($"Resolved: {report.ResolvedAt:yyyy-MM-dd HH:mm} by {resolverName ?? "Unknown"}");
                if (!string.IsNullOrWhiteSpace(report.ResolutionNotes))
                {
                    column.Item().Text(report.ResolutionNotes);
                }
            }

            // Attachments
            if (report.Attachments.Any(a => !a.IsDeleted))
            {
                column.Item().Text("Attachments").FontSize(14).Bold();
                foreach (var attachment in report.Attachments.Where(a => !a.IsDeleted))
                {
                    column.Item().Text($"- {attachment.FileName} ({FormatFileSize(attachment.FileSizeBytes)})");
                }
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Generated by Sqordia CMS - ");
            text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC"));
        });
    }

    private byte[] GenerateWord(BugReport report, string? reporterName, string? resolverName)
    {
        using var memoryStream = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new OpenXmlDocument();
            var body = mainPart.Document.AppendChild(new Body());

            // Title
            var title = new Paragraph(
                new Run(new Text($"Bug Report - {report.TicketNumber}"))
                {
                    RunProperties = new RunProperties(new Bold(), new FontSize { Val = "48" })
                });
            body.AppendChild(title);

            // Info row
            body.AppendChild(CreateParagraph($"Title: {report.Title}", true));
            body.AppendChild(CreateParagraph($"Severity: {report.Severity}"));
            body.AppendChild(CreateParagraph($"Status: {report.Status}"));
            body.AppendChild(CreateParagraph($"Page/Section: {report.PageSection}"));
            body.AppendChild(CreateParagraph($"Reported: {report.Created:yyyy-MM-dd HH:mm} by {reporterName ?? "Unknown"}"));

            body.AppendChild(CreateParagraph(""));

            // Description
            body.AppendChild(CreateParagraph("Description", true));
            body.AppendChild(CreateParagraph(report.Description));

            body.AppendChild(CreateParagraph(""));

            // System Information
            body.AppendChild(CreateParagraph("System Information", true));
            body.AppendChild(CreateParagraph($"App Version: {report.AppVersion ?? "N/A"}"));
            body.AppendChild(CreateParagraph($"Browser: {report.Browser ?? "N/A"}"));
            body.AppendChild(CreateParagraph($"OS: {report.OperatingSystem ?? "N/A"}"));

            // Resolution
            if (report.ResolvedAt.HasValue)
            {
                body.AppendChild(CreateParagraph(""));
                body.AppendChild(CreateParagraph("Resolution", true));
                body.AppendChild(CreateParagraph($"Resolved: {report.ResolvedAt:yyyy-MM-dd HH:mm} by {resolverName ?? "Unknown"}"));
                if (!string.IsNullOrWhiteSpace(report.ResolutionNotes))
                {
                    body.AppendChild(CreateParagraph(report.ResolutionNotes));
                }
            }

            // Attachments
            if (report.Attachments.Any(a => !a.IsDeleted))
            {
                body.AppendChild(CreateParagraph(""));
                body.AppendChild(CreateParagraph("Attachments", true));
                foreach (var attachment in report.Attachments.Where(a => !a.IsDeleted))
                {
                    body.AppendChild(CreateParagraph($"- {attachment.FileName} ({FormatFileSize(attachment.FileSizeBytes)})"));
                }
            }

            // Footer
            body.AppendChild(CreateParagraph(""));
            body.AppendChild(CreateParagraph($"Generated by Sqordia CMS - {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC"));

            mainPart.Document.Save();
        }

        return memoryStream.ToArray();
    }

    private static Paragraph CreateParagraph(string text, bool isBold = false)
    {
        var run = new Run(new Text(text));
        if (isBold)
        {
            run.RunProperties = new RunProperties(new Bold());
        }
        return new Paragraph(run);
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private async Task<string?> GetUserNameAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (!userId.HasValue) return null;

        var user = await _context.Users
            .Where(u => u.Id == userId.Value)
            .Select(u => u.FirstName + " " + u.LastName)
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }

    private async Task<BugReportResponse> MapToResponseAsync(BugReport report, CancellationToken cancellationToken)
    {
        var reporterName = await GetUserNameAsync(report.ReportedByUserId, cancellationToken);
        var resolverName = await GetUserNameAsync(report.ResolvedByUserId, cancellationToken);

        return new BugReportResponse
        {
            Id = report.Id,
            Title = report.Title,
            PageSection = report.PageSection,
            Severity = report.Severity.ToString(),
            Description = report.Description,
            Status = report.Status.ToString(),
            TicketNumber = report.TicketNumber,
            AppVersion = report.AppVersion,
            Browser = report.Browser,
            OperatingSystem = report.OperatingSystem,
            ReportedByUserId = report.ReportedByUserId,
            ReportedByUserName = reporterName,
            ResolutionNotes = report.ResolutionNotes,
            ResolvedAt = report.ResolvedAt,
            ResolvedByUserId = report.ResolvedByUserId,
            ResolvedByUserName = resolverName,
            CreatedAt = report.Created,
            LastModifiedAt = report.LastModified,
            Attachments = report.Attachments
                .Where(a => !a.IsDeleted)
                .Select(MapToAttachmentResponse)
                .ToList()
        };
    }

    private BugReportSummaryResponse MapToSummaryResponse(BugReport report, Dictionary<Guid, string> users)
    {
        string? userName = null;
        if (report.ReportedByUserId.HasValue && users.TryGetValue(report.ReportedByUserId.Value, out var name))
        {
            userName = name;
        }

        return new BugReportSummaryResponse
        {
            Id = report.Id,
            Title = report.Title,
            PageSection = report.PageSection,
            Severity = report.Severity.ToString(),
            Status = report.Status.ToString(),
            TicketNumber = report.TicketNumber,
            ReportedByUserName = userName,
            CreatedAt = report.Created,
            AttachmentCount = report.Attachments.Count(a => !a.IsDeleted)
        };
    }

    private static BugReportAttachmentResponse MapToAttachmentResponse(BugReportAttachment attachment)
    {
        return new BugReportAttachmentResponse
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSizeBytes = attachment.FileSizeBytes,
            StorageUrl = attachment.StorageUrl,
            CreatedAt = attachment.Created
        };
    }

    private static readonly Error BugReportTablesNotAvailableError = Error.Failure(
        "BugReport.TablesNotAvailable",
        "Bug reporting is not yet available. The database migration needs to be applied.");

    private static bool IsBugReportTableMissing(Exception ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            && message.Contains("relation", StringComparison.OrdinalIgnoreCase);
    }
}

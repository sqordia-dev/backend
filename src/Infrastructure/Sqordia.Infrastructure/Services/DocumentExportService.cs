using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Export;
using Sqordia.Contracts.Responses.Export;
using Sqordia.Domain.Entities.BusinessPlan;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Presentation;
using Drawing = DocumentFormat.OpenXml.Drawing;
using WordParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WordRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WordText = DocumentFormat.OpenXml.Wordprocessing.Text;
using WordTable = DocumentFormat.OpenXml.Wordprocessing.Table;
using WordTableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using WordTableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using WordBold = DocumentFormat.OpenXml.Wordprocessing.Bold;
using WordItalic = DocumentFormat.OpenXml.Wordprocessing.Italic;
using WordFontSize = DocumentFormat.OpenXml.Wordprocessing.FontSize;
using WordColor = DocumentFormat.OpenXml.Wordprocessing.Color;
using WordRunProperties = DocumentFormat.OpenXml.Wordprocessing.RunProperties;
using WordTableProperties = DocumentFormat.OpenXml.Wordprocessing.TableProperties;
using WordTableCellProperties = DocumentFormat.OpenXml.Wordprocessing.TableCellProperties;
using WordShading = DocumentFormat.OpenXml.Wordprocessing.Shading;
using WordTableBorders = DocumentFormat.OpenXml.Wordprocessing.TableBorders;
using WordTopBorder = DocumentFormat.OpenXml.Wordprocessing.TopBorder;
using WordBottomBorder = DocumentFormat.OpenXml.Wordprocessing.BottomBorder;
using WordLeftBorder = DocumentFormat.OpenXml.Wordprocessing.LeftBorder;
using WordRightBorder = DocumentFormat.OpenXml.Wordprocessing.RightBorder;
using WordInsideHorizontalBorder = DocumentFormat.OpenXml.Wordprocessing.InsideHorizontalBorder;
using WordInsideVerticalBorder = DocumentFormat.OpenXml.Wordprocessing.InsideVerticalBorder;
using SpreadsheetColors = DocumentFormat.OpenXml.Spreadsheet.Colors;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Service for exporting business plans to various document formats
/// </summary>
public class DocumentExportService : IDocumentExportService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DocumentExportService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DocumentExportService(
        IApplicationDbContext context,
        ILogger<DocumentExportService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<Result<ExportResult>> ExportToPdfAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting PDF export for business plan {BusinessPlanId} in {Language}", businessPlanId, language);

            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<ExportResult>("Business plan not found or access denied");
            }

            // Generate PDF using QuestPDF
            var pdfBytes = GeneratePdfWithQuestPDF(businessPlan, language);

            var result = new ExportResult
            {
                FileData = pdfBytes,
                FileName = $"{SanitizeFileName(businessPlan.Title)}_{language}_{DateTime.UtcNow:yyyyMMdd}.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = pdfBytes.Length,
                Language = language,
                Template = "default"
            };

            _logger.LogInformation("PDF export completed successfully. File size: {FileSize} bytes", pdfBytes.Length);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting business plan {BusinessPlanId} to PDF", businessPlanId);
            return Result.Failure<ExportResult>($"Export failed: {ex.Message}");
        }
    }

    public async Task<Result<ExportResult>> ExportToWordAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting Word export for business plan {BusinessPlanId} in {Language}", businessPlanId, language);

            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<ExportResult>("Business plan not found or access denied");
            }

            // Generate Word document
            var wordBytes = GenerateWordDocument(businessPlan, language);

            var result = new ExportResult
            {
                FileData = wordBytes,
                FileName = $"{SanitizeFileName(businessPlan.Title)}_{language}_{DateTime.UtcNow:yyyyMMdd}.docx",
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileSizeBytes = wordBytes.Length,
                Language = language,
                Template = "default"
            };

            _logger.LogInformation("Word export completed successfully. File size: {FileSize} bytes", wordBytes.Length);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting business plan {BusinessPlanId} to Word", businessPlanId);
            return Result.Failure<ExportResult>($"Export failed: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExportToHtmlAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default)
    {
        try
        {
            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<string>("Business plan not found or access denied");
            }

            var html = GenerateHtmlContent(businessPlan, language);
            return Result.Success(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting business plan {BusinessPlanId} to HTML", businessPlanId);
            return Result.Failure<string>($"Export failed: {ex.Message}");
        }
    }

    public async Task<Result<List<ExportTemplate>>> GetAvailableTemplatesAsync()
    {
        await Task.CompletedTask; // For future template expansion

        var templates = new List<ExportTemplate>
        {
            new()
            {
                Id = "default",
                Name = "Default Template",
                Description = "Clean, professional business plan template with standard formatting",
                IsDefault = true,
                SupportedFormats = new List<string> { "pdf", "docx", "html" },
                SupportedLanguages = new List<string> { "fr", "en" }
            },
            new()
            {
                Id = "executive",
                Name = "Executive Summary",
                Description = "Condensed template focusing on key business highlights",
                IsDefault = false,
                SupportedFormats = new List<string> { "pdf", "docx" },
                SupportedLanguages = new List<string> { "fr", "en" }
            }
        };

        return Result.Success(templates);
    }

    private async Task<BusinessPlan?> GetBusinessPlanWithValidationAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return null;
        }

        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
                .ThenInclude(o => o.Members)
            .Include(bp => bp.QuestionnaireResponses)
                .ThenInclude(qr => qr.QuestionTemplate)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null)
        {
            return null;
        }

        // Verify user has access to this business plan
        var hasAccess = businessPlan.Organization.Members
            .Any(m => m.UserId == currentUserId.Value && m.IsActive);

        return hasAccess ? businessPlan : null;
    }

    private byte[] GeneratePdfWithQuestPDF(BusinessPlan businessPlan, string language)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(QuestPDF.Helpers.Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text($"{GetLocalizedText("Business Plan", language)}: {businessPlan.Title}")
                    .SemiBold().FontSize(16).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);

                        // Executive Summary
                        if (!string.IsNullOrEmpty(businessPlan.ExecutiveSummary))
                        {
                            x.Item().Text(GetLocalizedText("Executive Summary", language)).SemiBold().FontSize(14);
                            x.Item().Text(businessPlan.ExecutiveSummary);
                        }

                        // Problem Statement
                        if (!string.IsNullOrEmpty(businessPlan.ProblemStatement))
                        {
                            x.Item().Text(GetLocalizedText("Problem Statement", language)).SemiBold().FontSize(14);
                            x.Item().Text(businessPlan.ProblemStatement);
                        }

                        // Solution
                        if (!string.IsNullOrEmpty(businessPlan.Solution))
                        {
                            x.Item().Text(GetLocalizedText("Solution", language)).SemiBold().FontSize(14);
                            x.Item().Text(businessPlan.Solution);
                        }

                        // Market Analysis
                        if (!string.IsNullOrEmpty(businessPlan.MarketAnalysis))
                        {
                            x.Item().Text(GetLocalizedText("Market Analysis", language)).SemiBold().FontSize(14);
                            x.Item().Text(businessPlan.MarketAnalysis);
                        }

                        // Competitive Analysis
                        if (!string.IsNullOrEmpty(businessPlan.CompetitiveAnalysis))
                        {
                            x.Item().Text(GetLocalizedText("Competitive Analysis", language)).SemiBold().FontSize(14);
                            x.Item().Text(businessPlan.CompetitiveAnalysis);
                        }

                        // Financial Projections
                        if (!string.IsNullOrEmpty(businessPlan.FinancialProjections))
                        {
                            x.Item().Text(GetLocalizedText("Financial Projections", language)).SemiBold().FontSize(14);
                            x.Item().Text(businessPlan.FinancialProjections);
                        }

                        // Marketing Strategy
                        if (!string.IsNullOrEmpty(businessPlan.MarketingStrategy))
                        {
                            x.Item().Text(GetLocalizedText("Marketing Strategy", language)).SemiBold().FontSize(14);
                            x.Item().Text(businessPlan.MarketingStrategy);
                        }

                        // Management Team
                        if (!string.IsNullOrEmpty(businessPlan.ManagementTeam))
                        {
                            x.Item().Text(GetLocalizedText("Management Team", language)).SemiBold().FontSize(14);
                            x.Item().Text(businessPlan.ManagementTeam);
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span($"{GetLocalizedText("Generated on", language)}: ");
                        x.Span(DateTime.Now.ToString("MMMM dd, yyyy"));
                        x.Span(" | ");
                        x.Span($"{GetLocalizedText("Page", language)} ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
            });
        })
        .GeneratePdf();
    }

    private byte[] GenerateWordDocument(BusinessPlan businessPlan, string language)
    {
        using var memoryStream = new MemoryStream();
        using var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document);

        var mainPart = wordDocument.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        var body = mainPart.Document.AppendChild(new Body());

        // Title
        var titleParagraph = new WordParagraph();
        var titleRun = new WordRun();
        var titleText = new WordText($"{GetLocalizedText("Business Plan", language)}: {businessPlan.Title}");
        titleRun.Append(titleText);
        titleParagraph.Append(titleRun);
        body.Append(titleParagraph);

        // Add sections
        AddWordSection(body, GetLocalizedText("Executive Summary", language), businessPlan.ExecutiveSummary);
        AddWordSection(body, GetLocalizedText("Problem Statement", language), businessPlan.ProblemStatement);
        AddWordSection(body, GetLocalizedText("Solution", language), businessPlan.Solution);
        AddWordSection(body, GetLocalizedText("Market Analysis", language), businessPlan.MarketAnalysis);
        AddWordSection(body, GetLocalizedText("Competitive Analysis", language), businessPlan.CompetitiveAnalysis);
        AddWordSection(body, GetLocalizedText("Financial Projections", language), businessPlan.FinancialProjections);
        AddWordSection(body, GetLocalizedText("Marketing Strategy", language), businessPlan.MarketingStrategy);
        AddWordSection(body, GetLocalizedText("Management Team", language), businessPlan.ManagementTeam);

        wordDocument.Save();
        return memoryStream.ToArray();
    }

    private static void AddWordSection(Body body, string heading, string? content)
    {
        if (string.IsNullOrEmpty(content)) return;

        // Add heading
        var headingParagraph = new WordParagraph();
        var headingRun = new WordRun();
        var headingText = new WordText(heading);
        headingRun.Append(headingText);
        headingParagraph.Append(headingRun);
        body.Append(headingParagraph);

        // Add content
        var contentParagraph = new WordParagraph();
        var contentRun = new WordRun();
        var contentText = new WordText(content);
        contentRun.Append(contentText);
        contentParagraph.Append(contentRun);
        body.Append(contentParagraph);

        // Add spacing
        body.Append(new WordParagraph());
    }

    private string GenerateHtmlContent(BusinessPlan businessPlan, string language)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='UTF-8'>");
        html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        html.AppendLine($"<title>{GetLocalizedText("Business Plan", language)}: {businessPlan.Title}</title>");
        html.AppendLine("<style>");
        html.AppendLine(GetDefaultCss());
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine("<div class='container'>");
        html.AppendLine($"<h1>{GetLocalizedText("Business Plan", language)}: {businessPlan.Title}</h1>");

        AddHtmlSection(html, GetLocalizedText("Executive Summary", language), businessPlan.ExecutiveSummary);
        AddHtmlSection(html, GetLocalizedText("Problem Statement", language), businessPlan.ProblemStatement);
        AddHtmlSection(html, GetLocalizedText("Solution", language), businessPlan.Solution);
        AddHtmlSection(html, GetLocalizedText("Market Analysis", language), businessPlan.MarketAnalysis);
        AddHtmlSection(html, GetLocalizedText("Competitive Analysis", language), businessPlan.CompetitiveAnalysis);
        AddHtmlSection(html, GetLocalizedText("Financial Projections", language), businessPlan.FinancialProjections);
        AddHtmlSection(html, GetLocalizedText("Marketing Strategy", language), businessPlan.MarketingStrategy);
        AddHtmlSection(html, GetLocalizedText("Management Team", language), businessPlan.ManagementTeam);

        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private static void AddHtmlSection(StringBuilder html, string heading, string? content)
    {
        if (string.IsNullOrEmpty(content)) return;

        html.AppendLine($"<section>");
        html.AppendLine($"<h2>{heading}</h2>");
        html.AppendLine($"<div class='content'>{content.Replace("\n", "<br>")}</div>");
        html.AppendLine("</section>");
    }

    private static string GetDefaultCss()
    {
        return @"
            body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f5f5f5; }
            .container { max-width: 800px; margin: 0 auto; background: white; padding: 40px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
            h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }
            h2 { color: #34495e; margin-top: 30px; margin-bottom: 15px; }
            section { margin-bottom: 25px; }
            .content { text-align: justify; color: #2c3e50; }
            @media print { body { background: white; } .container { box-shadow: none; } }
        ";
    }

    private static string GetLocalizedText(string key, string language)
    {
        var translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["Business Plan"] = new() { ["fr"] = "Plan d'Affaires", ["en"] = "Business Plan" },
            ["Executive Summary"] = new() { ["fr"] = "Résumé Exécutif", ["en"] = "Executive Summary" },
            ["Problem Statement"] = new() { ["fr"] = "Énoncé du Problème", ["en"] = "Problem Statement" },
            ["Solution"] = new() { ["fr"] = "Solution", ["en"] = "Solution" },
            ["Market Analysis"] = new() { ["fr"] = "Analyse de Marché", ["en"] = "Market Analysis" },
            ["Competitive Analysis"] = new() { ["fr"] = "Analyse Concurrentielle", ["en"] = "Competitive Analysis" },
            ["Financial Projections"] = new() { ["fr"] = "Projections Financières", ["en"] = "Financial Projections" },
            ["Marketing Strategy"] = new() { ["fr"] = "Stratégie Marketing", ["en"] = "Marketing Strategy" },
            ["Management Team"] = new() { ["fr"] = "Équipe de Direction", ["en"] = "Management Team" },
            ["Generated on"] = new() { ["fr"] = "Généré le", ["en"] = "Generated on" },
            ["Page"] = new() { ["fr"] = "Page", ["en"] = "Page" },
            ["Table of Contents"] = new() { ["fr"] = "Table des Matières", ["en"] = "Table of Contents" },
            ["Prepared for"] = new() { ["fr"] = "Préparé pour", ["en"] = "Prepared for" },
            ["Prepared by"] = new() { ["fr"] = "Préparé par", ["en"] = "Prepared by" }
        };

        if (translations.TryGetValue(key, out var translation) &&
            translation.TryGetValue(language, out var text))
        {
            return text;
        }

        return key; // Fallback to key if translation not found
    }

    private static string SanitizeFileName(string fileName)
    {
        // Get platform-specific invalid characters
        var invalidChars = System.IO.Path.GetInvalidFileNameChars();
        
        // Also sanitize additional characters that are problematic across platforms
        // even if not in the platform's invalid chars list (e.g., < > : on Linux)
        var additionalCharsToSanitize = new[] { '<', '>', '"', ':' };
        
        var sanitized = fileName;
        
        // Replace platform-specific invalid characters
        foreach (var c in invalidChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }
        
        // Replace additional problematic characters
        foreach (var c in additionalCharsToSanitize)
        {
            sanitized = sanitized.Replace(c, '_');
        }
        
        // Remove multiple consecutive underscores and trim
        while (sanitized.Contains("__"))
        {
            sanitized = sanitized.Replace("__", "_");
        }
        return sanitized.Trim('_');
    }

    public async Task<Result<ExportResult>> ExportToExcelAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting Excel export for business plan {BusinessPlanId} in {Language}", businessPlanId, language);

            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<ExportResult>("Business plan not found or access denied");
            }

            var excelBytes = GenerateExcelDocument(businessPlan, language);

            var result = new ExportResult
            {
                FileData = excelBytes,
                FileName = $"{SanitizeFileName(businessPlan.Title)}_{language}_{DateTime.UtcNow:yyyyMMdd}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileSizeBytes = excelBytes.Length,
                Language = language,
                Template = "default"
            };

            _logger.LogInformation("Excel export completed successfully. File size: {FileSize} bytes", excelBytes.Length);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting business plan {BusinessPlanId} to Excel", businessPlanId);
            return Result.Failure<ExportResult>($"Export failed: {ex.Message}");
        }
    }

    public async Task<Result<ExportResult>> ExportToPowerPointAsync(Guid businessPlanId, string language = "fr", CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting PowerPoint export for business plan {BusinessPlanId} in {Language}", businessPlanId, language);

            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<ExportResult>("Business plan not found or access denied");
            }

            var pptxBytes = GeneratePowerPointDocument(businessPlan, language);

            var result = new ExportResult
            {
                FileData = pptxBytes,
                FileName = $"{SanitizeFileName(businessPlan.Title)}_{language}_{DateTime.UtcNow:yyyyMMdd}.pptx",
                ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                FileSizeBytes = pptxBytes.Length,
                Language = language,
                Template = "default"
            };

            _logger.LogInformation("PowerPoint export completed successfully. File size: {FileSize} bytes", pptxBytes.Length);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting business plan {BusinessPlanId} to PowerPoint", businessPlanId);
            return Result.Failure<ExportResult>($"Export failed: {ex.Message}");
        }
    }

    private byte[] GenerateExcelDocument(BusinessPlan businessPlan, string language)
    {
        using var memoryStream = new MemoryStream();
        using var spreadsheet = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook);

        var workbookPart = spreadsheet.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();
        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());

        var sheets = spreadsheet.WorkbookPart!.Workbook.AppendChild(new Sheets());
        var sheet = new Sheet
        {
            Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = GetLocalizedText("Business Plan", language)
        };
        sheets.Append(sheet);

        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;
        
        // Add title row
        var titleRow = new Row { RowIndex = 1 };
        titleRow.AppendChild(new Cell { CellValue = new CellValue(businessPlan.Title), DataType = CellValues.String });
        sheetData.AppendChild(titleRow);

        // Add sections
        uint rowIndex = 2;
        AddExcelSection(sheetData, ref rowIndex, GetLocalizedText("Executive Summary", language), businessPlan.ExecutiveSummary);
        AddExcelSection(sheetData, ref rowIndex, GetLocalizedText("Market Analysis", language), businessPlan.MarketAnalysis);
        AddExcelSection(sheetData, ref rowIndex, GetLocalizedText("Financial Projections", language), businessPlan.FinancialProjections);

        worksheetPart.Worksheet.Save();
        spreadsheet.WorkbookPart.Workbook.Save();
        spreadsheet.Dispose();

        return memoryStream.ToArray();
    }

    private void AddExcelSection(SheetData sheetData, ref uint rowIndex, string heading, string? content)
    {
        if (string.IsNullOrEmpty(content)) return;

        var headingRow = new Row { RowIndex = rowIndex++ };
        headingRow.AppendChild(new Cell { CellValue = new CellValue(heading), DataType = CellValues.String });
        sheetData.AppendChild(headingRow);

        var contentRow = new Row { RowIndex = rowIndex++ };
        contentRow.AppendChild(new Cell { CellValue = new CellValue(content), DataType = CellValues.String });
        sheetData.AppendChild(contentRow);

        rowIndex++; // Empty row
    }

    private byte[] GeneratePowerPointDocument(BusinessPlan businessPlan, string language)
    {
        // Simplified PowerPoint generation - creates a basic presentation with title slide
        // Full implementation would require proper slide templates and layouts
        
        using var memoryStream = new MemoryStream();
        using (var presentation = PresentationDocument.Create(memoryStream, PresentationDocumentType.Presentation))
        {
            var presentationPart = presentation.AddPresentationPart();
            presentationPart.Presentation = new Presentation();

            var slideIdList = new SlideIdList();
            presentationPart.Presentation.AppendChild(slideIdList);

            // Create a simple title slide
            var titleSlidePart = presentationPart.AddNewPart<SlidePart>();
            var slide = new Slide();
            var commonSlideData = new CommonSlideData();
            var shapeTree = new ShapeTree();
            
            // Create a simple text shape for the title
            var shape = new DocumentFormat.OpenXml.Presentation.Shape();
            var nonVisualShapeProperties = new DocumentFormat.OpenXml.Presentation.NonVisualShapeProperties();
            var nonVisualDrawingProperties = new DocumentFormat.OpenXml.Presentation.NonVisualDrawingProperties 
            { 
                Id = 1U, 
                Name = "Title" 
            };
            nonVisualShapeProperties.Append(nonVisualDrawingProperties);
            nonVisualShapeProperties.Append(new DocumentFormat.OpenXml.Presentation.NonVisualShapeDrawingProperties());
            
            shape.Append(nonVisualShapeProperties);
            shape.Append(new DocumentFormat.OpenXml.Presentation.ShapeProperties());
            
            var textBody = new Drawing.TextBody();
            textBody.Append(new Drawing.BodyProperties());
            textBody.Append(new Drawing.ListStyle());
            
            var paragraph = new Drawing.Paragraph();
            var run = new Drawing.Run();
            run.Append(new Drawing.Text(businessPlan.Title));
            paragraph.Append(run);
            textBody.Append(paragraph);
            
            shape.Append(textBody);
            shapeTree.Append(shape);
            commonSlideData.Append(shapeTree);
            slide.Append(commonSlideData);
            titleSlidePart.Slide = slide;

            slideIdList.AppendChild(new SlideId 
            { 
                Id = 256U, 
                RelationshipId = presentationPart.GetIdOfPart(titleSlidePart) 
            });

            presentationPart.Presentation.Save();
            presentation.Save();
        }

        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    private Guid? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #region Export with Visual Elements

    /// <inheritdoc />
    public async Task<Result<ExportWithVisualsResponse>> ExportToPdfWithVisualsAsync(
        Guid businessPlanId,
        ExportWithVisualsRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Starting PDF export with visuals for business plan {BusinessPlanId} in {Language}",
                businessPlanId, request.Language);

            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<ExportWithVisualsResponse>("Business plan not found or access denied");
            }

            var statistics = new ExportStatistics();
            var pdfBytes = GeneratePdfWithVisualsQuestPDF(businessPlan, request, statistics);

            stopwatch.Stop();
            statistics.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;

            var response = new ExportWithVisualsResponse
            {
                FileData = pdfBytes,
                FileName = $"{SanitizeFileName(businessPlan.Title)}_{request.Language}_{DateTime.UtcNow:yyyyMMdd}.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = pdfBytes.Length,
                Format = "pdf",
                Language = request.Language,
                Template = request.TemplateId ?? "default",
                Statistics = statistics
            };

            _logger.LogInformation("PDF export with visuals completed. Size: {FileSize} bytes, Visuals: {VisualCount}",
                pdfBytes.Length, statistics.VisualElementCount);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting business plan {BusinessPlanId} to PDF with visuals", businessPlanId);
            return Result.Failure<ExportWithVisualsResponse>($"Export failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ExportWithVisualsResponse>> ExportToWordWithVisualsAsync(
        Guid businessPlanId,
        ExportWithVisualsRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Starting Word export with visuals for business plan {BusinessPlanId} in {Language}",
                businessPlanId, request.Language);

            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<ExportWithVisualsResponse>("Business plan not found or access denied");
            }

            var statistics = new ExportStatistics();
            var wordBytes = GenerateWordDocumentWithVisuals(businessPlan, request, statistics);

            stopwatch.Stop();
            statistics.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;

            var response = new ExportWithVisualsResponse
            {
                FileData = wordBytes,
                FileName = $"{SanitizeFileName(businessPlan.Title)}_{request.Language}_{DateTime.UtcNow:yyyyMMdd}.docx",
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileSizeBytes = wordBytes.Length,
                Format = "docx",
                Language = request.Language,
                Template = request.TemplateId ?? "default",
                Statistics = statistics
            };

            _logger.LogInformation("Word export with visuals completed. Size: {FileSize} bytes, Visuals: {VisualCount}",
                wordBytes.Length, statistics.VisualElementCount);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting business plan {BusinessPlanId} to Word with visuals", businessPlanId);
            return Result.Failure<ExportWithVisualsResponse>($"Export failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> ExportToHtmlWithVisualsAsync(
        Guid businessPlanId,
        ExportWithVisualsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting HTML export with visuals for business plan {BusinessPlanId} in {Language}",
                businessPlanId, request.Language);

            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<string>("Business plan not found or access denied");
            }

            var html = GenerateHtmlWithVisuals(businessPlan, request);
            return Result.Success(html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting business plan {BusinessPlanId} to HTML with visuals", businessPlanId);
            return Result.Failure<string>($"Export failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<ExportPreviewResponse>> GetExportPreviewAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<ExportPreviewResponse>("Business plan not found or access denied");
            }

            var sections = GetExportableSections(businessPlan);
            var totalVisuals = 0; // Would be calculated from actual visual elements in production

            var response = new ExportPreviewResponse
            {
                BusinessPlanId = businessPlanId,
                IsReadyForExport = sections.Any(s => s.HasContent),
                CompletedSections = sections.Count(s => s.HasContent),
                TotalSections = sections.Count,
                CompletionPercentage = sections.Count > 0
                    ? (double)sections.Count(s => s.HasContent) / sections.Count * 100
                    : 0,
                AvailableFormats = new List<string> { "pdf", "docx", "html" },
                SupportedLanguages = new List<string> { "fr", "en" },
                EstimatedPdfPages = EstimatePdfPages(businessPlan),
                TotalVisualElements = totalVisuals,
                LastUpdated = businessPlan.LastModified ?? businessPlan.Created,
                Sections = sections
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting export preview for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<ExportPreviewResponse>($"Failed to get export preview: {ex.Message}");
        }
    }

    private byte[] GeneratePdfWithVisualsQuestPDF(
        BusinessPlan businessPlan,
        ExportWithVisualsRequest request,
        ExportStatistics statistics)
    {
        var sections = request.Sections ?? GetDefaultSections(businessPlan, request.Language);
        statistics.SectionCount = sections.Count;

        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(QuestPDF.Helpers.Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Cover page
                if (request.CoverPageSettings != null)
                {
                    page.Header().Element(c => ComposeCoverPageHeader(c, request.CoverPageSettings, businessPlan, request.Language));
                }
                else
                {
                    page.Header()
                        .Text($"{GetLocalizedText("Business Plan", request.Language)}: {businessPlan.Title}")
                        .SemiBold().FontSize(16).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                }

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(15);

                        // Table of Contents
                        if (request.IncludeTableOfContents)
                        {
                            column.Item().Text(GetLocalizedText("Table of Contents", request.Language))
                                .Bold().FontSize(16);
                            column.Item().PaddingBottom(10);

                            var sectionNumber = 1;
                            foreach (var section in sections.Where(s => !string.IsNullOrWhiteSpace(s.Content)))
                            {
                                column.Item().Text($"{sectionNumber}. {section.Title}").FontSize(11);
                                sectionNumber++;
                            }
                            column.Item().PageBreak();
                        }

                        // Sections with visual elements
                        foreach (var section in sections)
                        {
                            if (string.IsNullOrWhiteSpace(section.Content) && (section.VisualElements == null || !section.VisualElements.Any()))
                                continue;

                            // Section title
                            column.Item().Text(section.Title).SemiBold().FontSize(14);
                            column.Item().PaddingBottom(5)
                                .LineHorizontal(1)
                                .LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                            // Section prose content
                            if (!string.IsNullOrWhiteSpace(section.Content))
                            {
                                var wordCount = section.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                                statistics.WordCount += wordCount;
                                column.Item().Text(StripHtml(section.Content)).FontSize(11).LineHeight(1.4f);
                            }

                            // Visual elements
                            if (request.IncludeVisuals && section.VisualElements != null)
                            {
                                foreach (var visual in section.VisualElements)
                                {
                                    column.Item().PaddingTop(10);
                                    RenderVisualElementToPdf(column, visual, statistics, request.VisualOptions);
                                }
                            }

                            column.Item().PaddingTop(15);
                        }
                    });

                // Footer with page numbers
                if (request.IncludePageNumbers)
                {
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span($"{GetLocalizedText("Page", request.Language)} ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                }
            });
        })
        .GeneratePdf();
    }

    private void ComposeCoverPageHeader(
        IContainer container,
        ExportCoverPageSettings settings,
        BusinessPlan businessPlan,
        string language)
    {
        container.Column(column =>
        {
            column.Item().PaddingBottom(20);

            // Company name
            column.Item().AlignCenter().Text(settings.CompanyName ?? businessPlan.Organization?.Name ?? "")
                .Bold().FontSize(28).FontColor(QuestPDF.Helpers.Colors.Blue.Darken3);

            column.Item().PaddingTop(10);

            // Document title
            column.Item().AlignCenter().Text(settings.DocumentTitle ?? GetLocalizedText("Business Plan", language))
                .FontSize(20);

            // Subtitle
            if (!string.IsNullOrWhiteSpace(settings.Subtitle))
            {
                column.Item().PaddingTop(5);
                column.Item().AlignCenter().Text(settings.Subtitle)
                    .FontSize(14).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
            }

            column.Item().PaddingTop(40);

            // Prepared for/by
            if (!string.IsNullOrWhiteSpace(settings.PreparedFor))
            {
                column.Item().AlignCenter().Text($"{GetLocalizedText("Prepared for", language)}: {settings.PreparedFor}")
                    .FontSize(12);
            }
            if (!string.IsNullOrWhiteSpace(settings.PreparedBy))
            {
                column.Item().PaddingTop(5);
                column.Item().AlignCenter().Text($"{GetLocalizedText("Prepared by", language)}: {settings.PreparedBy}")
                    .FontSize(12);
            }

            // Date
            var date = settings.PreparedDate ?? DateTime.UtcNow;
            column.Item().PaddingTop(20);
            column.Item().AlignCenter().Text(date.ToString("MMMM dd, yyyy"))
                .FontSize(11).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);

            column.Item().PageBreak();
        });
    }

    private void RenderVisualElementToPdf(
        ColumnDescriptor column,
        ExportVisualElement visual,
        ExportStatistics statistics,
        VisualElementOptions? options)
    {
        statistics.VisualElementCount++;

        // Visual element title
        if (!string.IsNullOrWhiteSpace(visual.Title))
        {
            column.Item().Text(visual.Title).SemiBold().FontSize(12);
            column.Item().PaddingBottom(5);
        }

        switch (visual.Type.ToLower())
        {
            case "table":
                statistics.TableCount++;
                RenderTableToPdf(column, visual.Data, options);
                break;
            case "chart":
                statistics.ChartCount++;
                RenderChartPlaceholderToPdf(column, visual, options);
                break;
            case "metric":
                statistics.MetricCount++;
                RenderMetricsToPdf(column, visual.Data, options);
                break;
            case "infographic":
                statistics.InfographicCount++;
                RenderInfographicToPdf(column, visual.Data, options);
                break;
        }
    }

    private void RenderTableToPdf(ColumnDescriptor column, object data, VisualElementOptions? options)
    {
        try
        {
            var tableData = DeserializeVisualData<ExportTableData>(data);
            if (tableData == null || tableData.Headers == null || tableData.Rows == null)
                return;

            column.Item().Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    foreach (var _ in tableData.Headers)
                    {
                        columns.RelativeColumn();
                    }
                });

                // Header row
                table.Header(header =>
                {
                    foreach (var headerText in tableData.Headers)
                    {
                        header.Cell()
                            .Background(QuestPDF.Helpers.Colors.Grey.Lighten3)
                            .Padding(5)
                            .Text(headerText)
                            .Bold()
                            .FontSize(10);
                    }
                });

                // Data rows
                var rowIndex = 0;
                foreach (var row in tableData.Rows)
                {
                    var bgColor = rowIndex % 2 == 0
                        ? QuestPDF.Helpers.Colors.White
                        : QuestPDF.Helpers.Colors.Grey.Lighten5;

                    foreach (var cell in row.Cells)
                    {
                        var cellContainer = table.Cell().Background(bgColor).Padding(5);

                        var valueText = cell.Value?.ToString() ?? "";
                        var textDescriptor = cellContainer.Text(valueText).FontSize(10);

                        if (cell.Format == "bold")
                            textDescriptor.Bold();
                    }
                    rowIndex++;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render table to PDF");
            column.Item().Text("[Table rendering error]").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Red.Medium);
        }
    }

    private void RenderChartPlaceholderToPdf(ColumnDescriptor column, ExportVisualElement visual, VisualElementOptions? options)
    {
        // In a full implementation, this would render actual chart images
        // For now, render a placeholder with chart data summary
        try
        {
            var chartData = DeserializeVisualData<ExportChartData>(visual.Data);
            if (chartData == null)
                return;

            var width = options?.ChartWidth ?? 500;
            var height = options?.ChartHeight ?? 200;

            column.Item()
                .Width(width)
                .Height(height)
                .Background(QuestPDF.Helpers.Colors.Grey.Lighten4)
                .Padding(10)
                .Column(chartColumn =>
                {
                    chartColumn.Item().AlignCenter()
                        .Text($"[{chartData.ChartType.ToUpper()} Chart]")
                        .Bold().FontSize(12);

                    chartColumn.Item().PaddingTop(10);

                    // Show legend/data summary
                    if (chartData.Datasets != null)
                    {
                        foreach (var dataset in chartData.Datasets.Take(3))
                        {
                            var sum = dataset.Data?.Sum() ?? 0;
                            chartColumn.Item().Text($"- {dataset.Label}: Total {sum:N0}")
                                .FontSize(10);
                        }
                    }
                });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render chart placeholder to PDF");
            column.Item().Text("[Chart rendering error]").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Red.Medium);
        }
    }

    private void RenderMetricsToPdf(ColumnDescriptor column, object data, VisualElementOptions? options)
    {
        try
        {
            var metricData = DeserializeVisualData<ExportMetricData>(data);
            if (metricData?.Metrics == null)
                return;

            column.Item().Row(row =>
            {
                foreach (var metric in metricData.Metrics.Take(4))
                {
                    row.RelativeItem()
                        .Background(QuestPDF.Helpers.Colors.Grey.Lighten4)
                        .Padding(10)
                        .Column(metricColumn =>
                        {
                            // Label
                            metricColumn.Item().Text(metric.Label)
                                .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken2);

                            // Value
                            var formattedValue = FormatMetricValue(metric);
                            metricColumn.Item().Text(formattedValue)
                                .Bold().FontSize(16);

                            // Trend
                            if (!string.IsNullOrWhiteSpace(metric.TrendValue))
                            {
                                var trendColor = metric.Trend == "up"
                                    ? QuestPDF.Helpers.Colors.Green.Darken1
                                    : metric.Trend == "down"
                                        ? QuestPDF.Helpers.Colors.Red.Darken1
                                        : QuestPDF.Helpers.Colors.Grey.Darken1;

                                metricColumn.Item().Text(metric.TrendValue)
                                    .FontSize(10).FontColor(trendColor);
                            }
                        });
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render metrics to PDF");
            column.Item().Text("[Metrics rendering error]").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Red.Medium);
        }
    }

    private void RenderInfographicToPdf(ColumnDescriptor column, object data, VisualElementOptions? options)
    {
        try
        {
            var infographicData = DeserializeVisualData<ExportInfographicData>(data);
            if (infographicData?.Items == null)
                return;

            var orderedItems = infographicData.Items.OrderBy(i => i.Order ?? 0);

            column.Item().Column(infoColumn =>
            {
                var itemNumber = 1;
                foreach (var item in orderedItems)
                {
                    infoColumn.Item().Row(row =>
                    {
                        // Number/icon placeholder
                        row.ConstantItem(30)
                            .Background(QuestPDF.Helpers.Colors.Blue.Lighten4)
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{itemNumber}")
                            .Bold().FontSize(12);

                        // Content
                        row.RelativeItem()
                            .PaddingLeft(10)
                            .Column(contentColumn =>
                            {
                                contentColumn.Item().Text(item.Title).Bold().FontSize(11);
                                if (!string.IsNullOrWhiteSpace(item.Description))
                                {
                                    contentColumn.Item().Text(item.Description).FontSize(10);
                                }
                            });
                    });
                    infoColumn.Item().PaddingBottom(5);
                    itemNumber++;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render infographic to PDF");
            column.Item().Text("[Infographic rendering error]").FontSize(10).FontColor(QuestPDF.Helpers.Colors.Red.Medium);
        }
    }

    private byte[] GenerateWordDocumentWithVisuals(
        BusinessPlan businessPlan,
        ExportWithVisualsRequest request,
        ExportStatistics statistics)
    {
        var sections = request.Sections ?? GetDefaultSections(businessPlan, request.Language);
        statistics.SectionCount = sections.Count;

        using var memoryStream = new MemoryStream();
        using var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document);

        var mainPart = wordDocument.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        var body = mainPart.Document.AppendChild(new Body());

        // Add styles
        AddWordStyles(mainPart);

        // Cover page
        if (request.CoverPageSettings != null)
        {
            AddWordCoverPage(body, request.CoverPageSettings, businessPlan, request.Language);
            body.Append(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Break { Type = BreakValues.Page })));
        }
        else
        {
            // Simple title
            AddWordTitle(body, $"{GetLocalizedText("Business Plan", request.Language)}: {businessPlan.Title}");
        }

        // Table of contents placeholder
        if (request.IncludeTableOfContents)
        {
            AddWordHeading(body, GetLocalizedText("Table of Contents", request.Language), 1);
            var sectionNumber = 1;
            foreach (var section in sections.Where(s => !string.IsNullOrWhiteSpace(s.Content)))
            {
                AddWordParagraph(body, $"{sectionNumber}. {section.Title}");
                sectionNumber++;
            }
            body.Append(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Break { Type = BreakValues.Page })));
        }

        // Sections with visual elements
        foreach (var section in sections)
        {
            if (string.IsNullOrWhiteSpace(section.Content) && (section.VisualElements == null || !section.VisualElements.Any()))
                continue;

            // Section title
            AddWordHeading(body, section.Title, 1);

            // Prose content
            if (!string.IsNullOrWhiteSpace(section.Content))
            {
                var wordCount = section.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                statistics.WordCount += wordCount;
                AddWordParagraph(body, StripHtml(section.Content));
            }

            // Visual elements
            if (request.IncludeVisuals && section.VisualElements != null)
            {
                foreach (var visual in section.VisualElements)
                {
                    body.Append(new WordParagraph()); // Spacing
                    RenderVisualElementToWord(body, visual, statistics);
                }
            }

            body.Append(new WordParagraph()); // Spacing between sections
        }

        wordDocument.Save();
        return memoryStream.ToArray();
    }

    private void AddWordStyles(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        stylesPart.Styles = new Styles();

        // Heading 1 style
        var heading1Style = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Heading1",
            CustomStyle = true
        };
        heading1Style.Append(new StyleName { Val = "Heading 1" });
        heading1Style.Append(new StyleRunProperties(
            new WordBold(),
            new WordFontSize { Val = "32" },
            new WordColor { Val = "2E75B6" }
        ));
        stylesPart.Styles.Append(heading1Style);

        stylesPart.Styles.Save();
    }

    private void AddWordCoverPage(Body body, ExportCoverPageSettings settings, BusinessPlan businessPlan, string language)
    {
        // Company name
        var companyPara = new WordParagraph();
        var companyProps = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
        companyPara.Append(companyProps);
        var companyRun = new WordRun();
        companyRun.Append(new WordRunProperties(new WordBold(), new WordFontSize { Val = "56" }, new WordColor { Val = "2E75B6" }));
        companyRun.Append(new WordText(settings.CompanyName ?? businessPlan.Organization?.Name ?? ""));
        companyPara.Append(companyRun);
        body.Append(companyPara);

        body.Append(new WordParagraph()); // Spacing

        // Document title
        var titlePara = new WordParagraph();
        var titleProps = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
        titlePara.Append(titleProps);
        var titleRun = new WordRun();
        titleRun.Append(new WordRunProperties(new WordFontSize { Val = "40" }));
        titleRun.Append(new WordText(settings.DocumentTitle ?? GetLocalizedText("Business Plan", language)));
        titlePara.Append(titleRun);
        body.Append(titlePara);

        // Subtitle
        if (!string.IsNullOrWhiteSpace(settings.Subtitle))
        {
            var subtitlePara = new WordParagraph();
            var subtitleProps = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
            subtitlePara.Append(subtitleProps);
            var subtitleRun = new WordRun();
            subtitleRun.Append(new WordRunProperties(new WordFontSize { Val = "28" }, new WordColor { Val = "666666" }));
            subtitleRun.Append(new WordText(settings.Subtitle));
            subtitlePara.Append(subtitleRun);
            body.Append(subtitlePara);
        }

        // Add spacing
        for (int i = 0; i < 5; i++)
            body.Append(new WordParagraph());

        // Prepared for/by
        if (!string.IsNullOrWhiteSpace(settings.PreparedFor))
        {
            AddWordCenteredText(body, $"{GetLocalizedText("Prepared for", language)}: {settings.PreparedFor}");
        }
        if (!string.IsNullOrWhiteSpace(settings.PreparedBy))
        {
            AddWordCenteredText(body, $"{GetLocalizedText("Prepared by", language)}: {settings.PreparedBy}");
        }

        // Date
        var date = settings.PreparedDate ?? DateTime.UtcNow;
        body.Append(new WordParagraph());
        AddWordCenteredText(body, date.ToString("MMMM dd, yyyy"));
    }

    private void AddWordCenteredText(Body body, string text)
    {
        var para = new WordParagraph();
        var props = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
        para.Append(props);
        var run = new WordRun();
        run.Append(new WordText(text));
        para.Append(run);
        body.Append(para);
    }

    private void AddWordTitle(Body body, string text)
    {
        var paragraph = new WordParagraph();
        var run = new WordRun();
        var runProperties = new WordRunProperties();
        runProperties.Append(new WordBold());
        runProperties.Append(new WordFontSize { Val = "48" });
        runProperties.Append(new WordColor { Val = "1F4E79" });
        run.Append(runProperties);
        run.Append(new WordText(text));
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private void AddWordHeading(Body body, string text, int level)
    {
        var paragraph = new WordParagraph();
        var paragraphProperties = new ParagraphProperties();
        paragraphProperties.Append(new SpacingBetweenLines { Before = "240", After = "120" });
        paragraph.Append(paragraphProperties);

        var run = new WordRun();
        var runProperties = new WordRunProperties();
        runProperties.Append(new WordBold());
        runProperties.Append(new WordFontSize { Val = level == 1 ? "32" : "28" });
        runProperties.Append(new WordColor { Val = level == 1 ? "2E75B6" : "5B9BD5" });
        run.Append(runProperties);
        run.Append(new WordText(text));
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private void AddWordParagraph(Body body, string text, bool isItalic = false)
    {
        var paragraph = new WordParagraph();
        var paragraphProperties = new ParagraphProperties();
        paragraphProperties.Append(new SpacingBetweenLines { After = "120", Line = "276", LineRule = LineSpacingRuleValues.Auto });
        paragraph.Append(paragraphProperties);

        var run = new WordRun();
        if (isItalic)
        {
            var runProperties = new WordRunProperties();
            runProperties.Append(new WordItalic());
            runProperties.Append(new WordColor { Val = "666666" });
            run.Append(runProperties);
        }
        run.Append(new WordText(text) { Space = SpaceProcessingModeValues.Preserve });
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private void RenderVisualElementToWord(Body body, ExportVisualElement visual, ExportStatistics statistics)
    {
        statistics.VisualElementCount++;

        // Visual element title
        if (!string.IsNullOrWhiteSpace(visual.Title))
        {
            AddWordHeading(body, visual.Title, 2);
        }

        switch (visual.Type.ToLower())
        {
            case "table":
                statistics.TableCount++;
                RenderTableToWord(body, visual.Data);
                break;
            case "chart":
                statistics.ChartCount++;
                RenderChartPlaceholderToWord(body, visual);
                break;
            case "metric":
                statistics.MetricCount++;
                RenderMetricsToWord(body, visual.Data);
                break;
            case "infographic":
                statistics.InfographicCount++;
                RenderInfographicToWord(body, visual.Data);
                break;
        }
    }

    private void RenderTableToWord(Body body, object data)
    {
        try
        {
            var tableData = DeserializeVisualData<ExportTableData>(data);
            if (tableData?.Headers == null || tableData.Rows == null)
                return;

            var table = new WordTable();

            // Table properties
            var tableProperties = new WordTableProperties(
                new WordTableBorders(
                    new WordTopBorder { Val = BorderValues.Single, Size = 4 },
                    new WordBottomBorder { Val = BorderValues.Single, Size = 4 },
                    new WordLeftBorder { Val = BorderValues.Single, Size = 4 },
                    new WordRightBorder { Val = BorderValues.Single, Size = 4 },
                    new WordInsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                    new WordInsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                ),
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
            );
            table.Append(tableProperties);

            // Header row
            var headerRow = new WordTableRow();
            foreach (var header in tableData.Headers)
            {
                var cell = new WordTableCell();
                cell.Append(new WordTableCellProperties(new WordShading { Fill = "E0E0E0" }));
                var para = new WordParagraph();
                var run = new WordRun();
                run.Append(new WordRunProperties(new WordBold()));
                run.Append(new WordText(header));
                para.Append(run);
                cell.Append(para);
                headerRow.Append(cell);
            }
            table.Append(headerRow);

            // Data rows
            foreach (var row in tableData.Rows)
            {
                var tableRow = new WordTableRow();
                foreach (var cellData in row.Cells)
                {
                    var cell = new WordTableCell();
                    var para = new WordParagraph();
                    var run = new WordRun();
                    if (cellData.Format == "bold")
                        run.Append(new WordRunProperties(new WordBold()));
                    run.Append(new WordText(cellData.Value?.ToString() ?? ""));
                    para.Append(run);
                    cell.Append(para);
                    tableRow.Append(cell);
                }
                table.Append(tableRow);
            }

            body.Append(table);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render table to Word");
            AddWordParagraph(body, "[Table rendering error]", true);
        }
    }

    private void RenderChartPlaceholderToWord(Body body, ExportVisualElement visual)
    {
        try
        {
            var chartData = DeserializeVisualData<ExportChartData>(visual.Data);

            var para = new WordParagraph();
            var props = new ParagraphProperties(new WordShading { Fill = "F0F0F0" });
            para.Append(props);

            var run = new WordRun();
            run.Append(new WordText($"[{chartData?.ChartType?.ToUpper() ?? "CHART"} Chart"));

            if (chartData?.Datasets != null && chartData.Datasets.Any())
            {
                run.Append(new WordText($" - {chartData.Datasets.Count} data series"));
            }

            run.Append(new WordText("]"));
            para.Append(run);
            body.Append(para);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render chart placeholder to Word");
            AddWordParagraph(body, "[Chart rendering error]", true);
        }
    }

    private void RenderMetricsToWord(Body body, object data)
    {
        try
        {
            var metricData = DeserializeVisualData<ExportMetricData>(data);
            if (metricData?.Metrics == null)
                return;

            // Render metrics as a simple table
            var table = new WordTable();
            var tableProperties = new WordTableProperties(
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
            );
            table.Append(tableProperties);

            var row = new WordTableRow();
            foreach (var metric in metricData.Metrics.Take(4))
            {
                var cell = new WordTableCell();
                cell.Append(new WordTableCellProperties(new WordShading { Fill = "F8F8F8" }));

                // Label
                var labelPara = new WordParagraph();
                var labelRun = new WordRun();
                labelRun.Append(new WordRunProperties(new WordFontSize { Val = "18" }, new WordColor { Val = "666666" }));
                labelRun.Append(new WordText(metric.Label));
                labelPara.Append(labelRun);
                cell.Append(labelPara);

                // Value
                var valuePara = new WordParagraph();
                var valueRun = new WordRun();
                valueRun.Append(new WordRunProperties(new WordBold(), new WordFontSize { Val = "28" }));
                valueRun.Append(new WordText(FormatMetricValue(metric)));
                valuePara.Append(valueRun);
                cell.Append(valuePara);

                row.Append(cell);
            }
            table.Append(row);
            body.Append(table);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render metrics to Word");
            AddWordParagraph(body, "[Metrics rendering error]", true);
        }
    }

    private void RenderInfographicToWord(Body body, object data)
    {
        try
        {
            var infographicData = DeserializeVisualData<ExportInfographicData>(data);
            if (infographicData?.Items == null)
                return;

            var orderedItems = infographicData.Items.OrderBy(i => i.Order ?? 0);

            var itemNumber = 1;
            foreach (var item in orderedItems)
            {
                var para = new WordParagraph();
                var run = new WordRun();
                run.Append(new WordRunProperties(new WordBold()));
                run.Append(new WordText($"{itemNumber}. {item.Title}"));
                para.Append(run);
                body.Append(para);

                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    AddWordParagraph(body, $"   {item.Description}");
                }

                itemNumber++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render infographic to Word");
            AddWordParagraph(body, "[Infographic rendering error]", true);
        }
    }

    private string GenerateHtmlWithVisuals(BusinessPlan businessPlan, ExportWithVisualsRequest request)
    {
        var sections = request.Sections ?? GetDefaultSections(businessPlan, request.Language);
        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"" + request.Language + "\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='UTF-8'>");
        html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        html.AppendLine($"<title>{GetLocalizedText("Business Plan", request.Language)}: {businessPlan.Title}</title>");
        html.AppendLine("<style>");
        html.AppendLine(GetEnhancedCss(request.CoverPageSettings?.PrimaryColor));
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Cover page
        if (request.CoverPageSettings != null)
        {
            html.AppendLine("<div class='cover-page'>");
            html.AppendLine($"<h1 class='company-name'>{System.Net.WebUtility.HtmlEncode(request.CoverPageSettings.CompanyName ?? businessPlan.Title)}</h1>");
            html.AppendLine($"<h2 class='document-title'>{System.Net.WebUtility.HtmlEncode(request.CoverPageSettings.DocumentTitle ?? GetLocalizedText("Business Plan", request.Language))}</h2>");
            if (!string.IsNullOrWhiteSpace(request.CoverPageSettings.Subtitle))
            {
                html.AppendLine($"<p class='subtitle'>{System.Net.WebUtility.HtmlEncode(request.CoverPageSettings.Subtitle)}</p>");
            }
            if (!string.IsNullOrWhiteSpace(request.CoverPageSettings.PreparedFor))
            {
                html.AppendLine($"<p class='prepared-info'>{GetLocalizedText("Prepared for", request.Language)}: {System.Net.WebUtility.HtmlEncode(request.CoverPageSettings.PreparedFor)}</p>");
            }
            if (!string.IsNullOrWhiteSpace(request.CoverPageSettings.PreparedBy))
            {
                html.AppendLine($"<p class='prepared-info'>{GetLocalizedText("Prepared by", request.Language)}: {System.Net.WebUtility.HtmlEncode(request.CoverPageSettings.PreparedBy)}</p>");
            }
            var date = request.CoverPageSettings.PreparedDate ?? DateTime.UtcNow;
            html.AppendLine($"<p class='date'>{date:MMMM dd, yyyy}</p>");
            html.AppendLine("</div>");
        }

        html.AppendLine("<div class='container'>");

        // Table of contents
        if (request.IncludeTableOfContents)
        {
            html.AppendLine("<nav class='toc'>");
            html.AppendLine($"<h2>{GetLocalizedText("Table of Contents", request.Language)}</h2>");
            html.AppendLine("<ol>");
            foreach (var section in sections.Where(s => !string.IsNullOrWhiteSpace(s.Content)))
            {
                var sectionId = section.SectionKey.ToLower().Replace(" ", "-");
                html.AppendLine($"<li><a href='#{sectionId}'>{System.Net.WebUtility.HtmlEncode(section.Title)}</a></li>");
            }
            html.AppendLine("</ol>");
            html.AppendLine("</nav>");
        }

        // Sections
        foreach (var section in sections)
        {
            if (string.IsNullOrWhiteSpace(section.Content) && (section.VisualElements == null || !section.VisualElements.Any()))
                continue;

            var sectionId = section.SectionKey.ToLower().Replace(" ", "-");
            html.AppendLine($"<section id='{sectionId}' class='section'>");
            html.AppendLine($"<h2>{System.Net.WebUtility.HtmlEncode(section.Title)}</h2>");

            // Prose content
            if (!string.IsNullOrWhiteSpace(section.Content))
            {
                html.AppendLine($"<div class='content'>{section.Content}</div>");
            }

            // Visual elements
            if (request.IncludeVisuals && section.VisualElements != null)
            {
                foreach (var visual in section.VisualElements)
                {
                    html.AppendLine(RenderVisualElementToHtml(visual));
                }
            }

            html.AppendLine("</section>");
        }

        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private string RenderVisualElementToHtml(ExportVisualElement visual)
    {
        var html = new StringBuilder();
        html.AppendLine("<div class='visual-element'>");

        if (!string.IsNullOrWhiteSpace(visual.Title))
        {
            html.AppendLine($"<h3>{System.Net.WebUtility.HtmlEncode(visual.Title)}</h3>");
        }

        switch (visual.Type.ToLower())
        {
            case "table":
                html.AppendLine(RenderTableToHtml(visual.Data));
                break;
            case "chart":
                html.AppendLine(RenderChartPlaceholderToHtml(visual));
                break;
            case "metric":
                html.AppendLine(RenderMetricsToHtml(visual.Data));
                break;
            case "infographic":
                html.AppendLine(RenderInfographicToHtml(visual.Data));
                break;
        }

        html.AppendLine("</div>");
        return html.ToString();
    }

    private string RenderTableToHtml(object data)
    {
        try
        {
            var tableData = DeserializeVisualData<ExportTableData>(data);
            if (tableData?.Headers == null || tableData.Rows == null)
                return "<p>[Invalid table data]</p>";

            var html = new StringBuilder();
            html.AppendLine("<table class='data-table'>");

            // Header
            html.AppendLine("<thead><tr>");
            foreach (var header in tableData.Headers)
            {
                html.AppendLine($"<th>{System.Net.WebUtility.HtmlEncode(header)}</th>");
            }
            html.AppendLine("</tr></thead>");

            // Body
            html.AppendLine("<tbody>");
            foreach (var row in tableData.Rows)
            {
                html.AppendLine(row.IsHighlighted ? "<tr class='highlighted'>" : "<tr>");
                foreach (var cell in row.Cells)
                {
                    var cellClass = cell.Format == "bold" ? " class='bold'" : "";
                    html.AppendLine($"<td{cellClass}>{System.Net.WebUtility.HtmlEncode(cell.Value?.ToString() ?? "")}</td>");
                }
                html.AppendLine("</tr>");
            }
            html.AppendLine("</tbody>");

            html.AppendLine("</table>");
            return html.ToString();
        }
        catch
        {
            return "<p>[Table rendering error]</p>";
        }
    }

    private string RenderChartPlaceholderToHtml(ExportVisualElement visual)
    {
        try
        {
            var chartData = DeserializeVisualData<ExportChartData>(visual.Data);
            return $"<div class='chart-placeholder'>[{chartData?.ChartType?.ToUpper() ?? "CHART"} Chart - Interactive charts require JavaScript rendering]</div>";
        }
        catch
        {
            return "<div class='chart-placeholder'>[Chart rendering error]</div>";
        }
    }

    private string RenderMetricsToHtml(object data)
    {
        try
        {
            var metricData = DeserializeVisualData<ExportMetricData>(data);
            if (metricData?.Metrics == null)
                return "<p>[Invalid metrics data]</p>";

            var html = new StringBuilder();
            html.AppendLine("<div class='metrics-container'>");

            foreach (var metric in metricData.Metrics.Take(4))
            {
                var trendClass = metric.Trend == "up" ? "trend-up" : metric.Trend == "down" ? "trend-down" : "";
                html.AppendLine("<div class='metric-card'>");
                html.AppendLine($"<div class='metric-label'>{System.Net.WebUtility.HtmlEncode(metric.Label)}</div>");
                html.AppendLine($"<div class='metric-value'>{System.Net.WebUtility.HtmlEncode(FormatMetricValue(metric))}</div>");
                if (!string.IsNullOrWhiteSpace(metric.TrendValue))
                {
                    html.AppendLine($"<div class='metric-trend {trendClass}'>{System.Net.WebUtility.HtmlEncode(metric.TrendValue)}</div>");
                }
                html.AppendLine("</div>");
            }

            html.AppendLine("</div>");
            return html.ToString();
        }
        catch
        {
            return "<p>[Metrics rendering error]</p>";
        }
    }

    private string RenderInfographicToHtml(object data)
    {
        try
        {
            var infographicData = DeserializeVisualData<ExportInfographicData>(data);
            if (infographicData?.Items == null)
                return "<p>[Invalid infographic data]</p>";

            var html = new StringBuilder();
            html.AppendLine("<div class='infographic'>");

            var orderedItems = infographicData.Items.OrderBy(i => i.Order ?? 0);
            var itemNumber = 1;

            foreach (var item in orderedItems)
            {
                html.AppendLine("<div class='infographic-item'>");
                html.AppendLine($"<div class='item-number'>{itemNumber}</div>");
                html.AppendLine("<div class='item-content'>");
                html.AppendLine($"<div class='item-title'>{System.Net.WebUtility.HtmlEncode(item.Title)}</div>");
                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    html.AppendLine($"<div class='item-description'>{System.Net.WebUtility.HtmlEncode(item.Description)}</div>");
                }
                html.AppendLine("</div></div>");
                itemNumber++;
            }

            html.AppendLine("</div>");
            return html.ToString();
        }
        catch
        {
            return "<p>[Infographic rendering error]</p>";
        }
    }

    private string GetEnhancedCss(string? primaryColor)
    {
        var color = primaryColor ?? "#2563EB";
        return $@"
            :root {{ --primary-color: {color}; }}
            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; margin: 0; padding: 0; background-color: #f5f5f5; }}
            .cover-page {{ min-height: 100vh; display: flex; flex-direction: column; justify-content: center; align-items: center; background: linear-gradient(135deg, var(--primary-color) 0%, #1e40af 100%); color: white; text-align: center; padding: 40px; }}
            .cover-page .company-name {{ font-size: 3rem; margin-bottom: 0.5rem; }}
            .cover-page .document-title {{ font-size: 2rem; font-weight: normal; margin-bottom: 1rem; }}
            .cover-page .subtitle {{ font-size: 1.2rem; opacity: 0.9; }}
            .cover-page .prepared-info {{ margin: 0.5rem 0; }}
            .cover-page .date {{ margin-top: 2rem; opacity: 0.8; }}
            .container {{ max-width: 900px; margin: 0 auto; background: white; padding: 40px; box-shadow: 0 0 20px rgba(0,0,0,0.1); }}
            .toc {{ background: #f8f9fa; padding: 20px; margin-bottom: 30px; border-radius: 8px; }}
            .toc h2 {{ margin-top: 0; color: var(--primary-color); }}
            .toc ol {{ padding-left: 20px; }}
            .toc a {{ color: #333; text-decoration: none; }}
            .toc a:hover {{ color: var(--primary-color); }}
            .section {{ margin-bottom: 40px; }}
            .section h2 {{ color: var(--primary-color); border-bottom: 2px solid var(--primary-color); padding-bottom: 10px; }}
            .content {{ text-align: justify; color: #2c3e50; }}
            .visual-element {{ margin: 20px 0; }}
            .visual-element h3 {{ color: #34495e; margin-bottom: 10px; }}
            .data-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
            .data-table th, .data-table td {{ padding: 12px; text-align: left; border: 1px solid #ddd; }}
            .data-table th {{ background: #f0f0f0; font-weight: bold; }}
            .data-table tr:nth-child(even) {{ background: #fafafa; }}
            .data-table tr.highlighted {{ background: #fff3cd; }}
            .data-table td.bold {{ font-weight: bold; }}
            .chart-placeholder {{ background: #f0f0f0; padding: 40px; text-align: center; color: #666; border-radius: 8px; }}
            .metrics-container {{ display: flex; gap: 15px; flex-wrap: wrap; }}
            .metric-card {{ flex: 1; min-width: 150px; background: #f8f9fa; padding: 20px; border-radius: 8px; text-align: center; }}
            .metric-label {{ color: #666; font-size: 0.9rem; }}
            .metric-value {{ font-size: 1.8rem; font-weight: bold; color: #333; }}
            .metric-trend {{ font-size: 0.9rem; margin-top: 5px; }}
            .trend-up {{ color: #22c55e; }}
            .trend-down {{ color: #ef4444; }}
            .infographic {{ margin: 15px 0; }}
            .infographic-item {{ display: flex; gap: 15px; margin-bottom: 15px; }}
            .item-number {{ width: 30px; height: 30px; background: var(--primary-color); color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; flex-shrink: 0; }}
            .item-content {{ flex: 1; }}
            .item-title {{ font-weight: bold; margin-bottom: 5px; }}
            .item-description {{ color: #666; }}
            @media print {{ body {{ background: white; }} .container {{ box-shadow: none; }} .cover-page {{ page-break-after: always; }} }}
        ";
    }

    private List<SectionExportContent> GetDefaultSections(BusinessPlan businessPlan, string language)
    {
        var sections = new List<SectionExportContent>();
        var order = 0;

        void AddSection(string key, string? content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                sections.Add(new SectionExportContent
                {
                    SectionKey = key,
                    Title = GetLocalizedText(key, language),
                    Content = content,
                    Order = order++
                });
            }
        }

        AddSection("Executive Summary", businessPlan.ExecutiveSummary);
        AddSection("Problem Statement", businessPlan.ProblemStatement);
        AddSection("Solution", businessPlan.Solution);
        AddSection("Market Analysis", businessPlan.MarketAnalysis);
        AddSection("Competitive Analysis", businessPlan.CompetitiveAnalysis);
        AddSection("Financial Projections", businessPlan.FinancialProjections);
        AddSection("Marketing Strategy", businessPlan.MarketingStrategy);
        AddSection("Management Team", businessPlan.ManagementTeam);

        return sections;
    }

    private List<ExportableSectionInfo> GetExportableSections(BusinessPlan businessPlan)
    {
        var sections = new List<ExportableSectionInfo>();

        void AddSectionInfo(string key, string? content)
        {
            var wordCount = content?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
            sections.Add(new ExportableSectionInfo
            {
                SectionKey = key,
                Title = key,
                HasContent = !string.IsNullOrWhiteSpace(content),
                WordCount = wordCount,
                VisualElementCount = 0, // Would be calculated from actual visual elements
                VisualElementTypes = new List<string>()
            });
        }

        AddSectionInfo("Executive Summary", businessPlan.ExecutiveSummary);
        AddSectionInfo("Problem Statement", businessPlan.ProblemStatement);
        AddSectionInfo("Solution", businessPlan.Solution);
        AddSectionInfo("Market Analysis", businessPlan.MarketAnalysis);
        AddSectionInfo("Competitive Analysis", businessPlan.CompetitiveAnalysis);
        AddSectionInfo("Financial Projections", businessPlan.FinancialProjections);
        AddSectionInfo("Marketing Strategy", businessPlan.MarketingStrategy);
        AddSectionInfo("Management Team", businessPlan.ManagementTeam);

        return sections;
    }

    private int EstimatePdfPages(BusinessPlan businessPlan)
    {
        var totalWords = 0;
        totalWords += businessPlan.ExecutiveSummary?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        totalWords += businessPlan.ProblemStatement?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        totalWords += businessPlan.Solution?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        totalWords += businessPlan.MarketAnalysis?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        totalWords += businessPlan.CompetitiveAnalysis?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        totalWords += businessPlan.FinancialProjections?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        totalWords += businessPlan.MarketingStrategy?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
        totalWords += businessPlan.ManagementTeam?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;

        // Estimate ~400 words per page
        return Math.Max(1, (totalWords / 400) + 1);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Simple HTML tag removal
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", " ");
        // Decode HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);
        // Normalize whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }

    private static string FormatMetricValue(ExportMetric metric)
    {
        if (metric.Value == null)
            return "";

        if (metric.Value is decimal decimalValue || decimal.TryParse(metric.Value.ToString(), out decimalValue))
        {
            return metric.Format?.ToLower() switch
            {
                "currency" => decimalValue.ToString("C0"),
                "percentage" => (decimalValue * 100).ToString("F1") + "%",
                "number" => decimalValue.ToString("N0"),
                _ => decimalValue.ToString("N0")
            };
        }

        return metric.Value.ToString() ?? "";
    }

    private T? DeserializeVisualData<T>(object data) where T : class
    {
        try
        {
            if (data is T typedData)
                return typedData;

            if (data is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            var json = JsonSerializer.Serialize(data);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize visual data to {Type}", typeof(T).Name);
            return null;
        }
    }

    #endregion
}
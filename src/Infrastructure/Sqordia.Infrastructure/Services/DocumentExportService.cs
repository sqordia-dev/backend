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
using System.Text.RegularExpressions;
using Sqordia.Application.Models.Export;
using Sqordia.Infrastructure.Services.Helpers;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Service for exporting business plans to various document formats
/// </summary>
public class DocumentExportService : IDocumentExportService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DocumentExportService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IExportThemeService _themeService;

    public DocumentExportService(
        IApplicationDbContext context,
        ILogger<DocumentExportService> logger,
        IHttpContextAccessor httpContextAccessor,
        IExportThemeService themeService)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _themeService = themeService;

        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Converts a hex color string (e.g. "#1E3A5F") to a QuestPDF Color.
    /// QuestPDF supports implicit string-to-Color conversion for hex values.
    /// </summary>
    private static QuestPDF.Infrastructure.Color HexToColor(string hex)
    {
        // Ensure the hex string starts with '#' for QuestPDF
        if (!hex.StartsWith('#'))
            hex = "#" + hex;
        return QuestPDF.Infrastructure.Color.FromHex(hex);
    }

    /// <summary>
    /// Strips '#' prefix from a hex color for OpenXml usage.
    /// </summary>
    private static string HexForOpenXml(string hex) => hex.TrimStart('#');

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

            // Resolve theme
            var themeResult = await _themeService.ResolveThemeAsync(null, null, cancellationToken);
            var theme = themeResult.IsSuccess ? themeResult.Value! : ExportThemeRegistry.GetTheme("classic");

            // Generate PDF using QuestPDF
            var pdfBytes = GeneratePdfWithQuestPDF(businessPlan, language, theme);

            var result = new ExportResult
            {
                FileData = pdfBytes,
                FileName = $"{SanitizeFileName(businessPlan.Title)}_{language}_{DateTime.UtcNow:yyyyMMdd}.pdf",
                ContentType = "application/pdf",
                FileSizeBytes = pdfBytes.Length,
                Language = language,
                Template = theme.Id
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

            // Resolve theme
            var themeResult = await _themeService.ResolveThemeAsync(null, null, cancellationToken);
            var theme = themeResult.IsSuccess ? themeResult.Value! : ExportThemeRegistry.GetTheme("classic");

            // Generate Word document
            var wordBytes = GenerateWordDocument(businessPlan, language, theme);

            var result = new ExportResult
            {
                FileData = wordBytes,
                FileName = $"{SanitizeFileName(businessPlan.Title)}_{language}_{DateTime.UtcNow:yyyyMMdd}.docx",
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileSizeBytes = wordBytes.Length,
                Language = language,
                Template = theme.Id
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
        return await _themeService.GetAvailableThemesAsync();
    }

    private async Task<BusinessPlan?> GetBusinessPlanWithValidationAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return null;
        }

        var businessPlan = await _context.BusinessPlans
            .AsNoTracking()
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

    private byte[] GeneratePdfWithQuestPDF(BusinessPlan businessPlan, string language, ExportTheme theme)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(HexToColor(theme.PageBackgroundColor));
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text($"{GetLocalizedText("Business Plan", language)}: {businessPlan.Title}")
                    .SemiBold().FontSize(16).FontColor(HexToColor(theme.HeadingColor));

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);

                        var sections = GetDefaultSections(businessPlan, language);
                        foreach (var section in sections.Where(s => !string.IsNullOrWhiteSpace(s.Content)))
                        {
                            x.Item().Text(section.Title)
                                .SemiBold().FontSize(14).FontColor(HexToColor(theme.HeadingColor));
                            RenderContentToPdf(x, section.Content, theme);
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

    private byte[] GenerateWordDocument(BusinessPlan businessPlan, string language, ExportTheme? theme = null)
    {
        theme ??= ExportThemeRegistry.GetTheme("classic");
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
        var sections = GetDefaultSections(businessPlan, language);
        foreach (var section in sections.Where(s => !string.IsNullOrWhiteSpace(s.Content)))
        {
            AddWordSection(body, section.Title, StripHtml(section.Content));
        }

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

        var sections = GetDefaultSections(businessPlan, language);
        foreach (var section in sections.Where(s => !string.IsNullOrWhiteSpace(s.Content)))
        {
            AddHtmlSection(html, section.Title, section.Content);
        }

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
            ["SWOT Analysis"] = new() { ["fr"] = "Analyse SWOT", ["en"] = "SWOT Analysis" },
            ["Business Model"] = new() { ["fr"] = "Modèle d'Affaires", ["en"] = "Business Model" },
            ["Branding Strategy"] = new() { ["fr"] = "Stratégie de Marque", ["en"] = "Branding Strategy" },
            ["Operations Plan"] = new() { ["fr"] = "Plan des Opérations", ["en"] = "Operations Plan" },
            ["Funding Requirements"] = new() { ["fr"] = "Besoins de Financement", ["en"] = "Funding Requirements" },
            ["Risk Analysis"] = new() { ["fr"] = "Analyse des Risques", ["en"] = "Risk Analysis" },
            ["Exit Strategy"] = new() { ["fr"] = "Stratégie de Sortie", ["en"] = "Exit Strategy" },
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
                FileName = $"{SanitizeFileName(businessPlan.Organization?.Name ?? businessPlan.Title)}_V{businessPlan.Version}.pptx",
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
        var isFr = language == "fr";
        var theme = ExportThemeRegistry.GetTheme("classic");
        var companyName = businessPlan.Organization?.Name ?? "";

        using var builder = new PowerPointBuilder(theme);

        // Title slide
        builder.AddTitleSlide(companyName, businessPlan.Title, null, DateTime.UtcNow.ToString("MMMM yyyy"));

        // Add section slides from available content
        var sections = GetPowerPointSections(businessPlan, isFr);
        if (sections.Count > 0)
        {
            var tocTitle = isFr ? "Table des matières" : "Table of Contents";
            builder.AddTableOfContentsSlide(tocTitle, sections.Select(s => s.Title).ToList());

            foreach (var (title, bullets) in sections)
            {
                builder.AddContentSlide(title, bullets);
            }
        }

        // Thank you slide
        var thankYouText = isFr ? "Merci" : "Thank You";
        builder.AddThankYouSlide(thankYouText, companyName);

        return builder.Build();
    }

    private static List<(string Title, List<string> Bullets)> GetPowerPointSections(BusinessPlan bp, bool isFr)
    {
        var sections = new List<(string Title, List<string> Bullets)>();

        void Add(string titleEn, string titleFr, string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            var title = isFr ? titleFr : titleEn;
            var bullets = ExtractBulletsFromContent(content);
            if (bullets.Count > 0)
                sections.Add((title, bullets));
        }

        Add("Executive Summary", "Sommaire exécutif", bp.ExecutiveSummary);
        Add("Problem Statement", "Énoncé du problème", bp.ProblemStatement);
        Add("Solution", "Solution", bp.Solution);
        Add("Market Analysis", "Analyse de marché", bp.MarketAnalysis);
        Add("Competitive Analysis", "Analyse concurrentielle", bp.CompetitiveAnalysis);
        Add("Business Model", "Modèle d'affaires", bp.BusinessModel);
        Add("Marketing Strategy", "Stratégie marketing", bp.MarketingStrategy);
        Add("Operations Plan", "Plan des opérations", bp.OperationsPlan);
        Add("Management Team", "Équipe de direction", bp.ManagementTeam);
        Add("Financial Projections", "Projections financières", bp.FinancialProjections);
        Add("Funding Requirements", "Besoins de financement", bp.FundingRequirements);
        Add("Risk Analysis", "Analyse des risques", bp.RiskAnalysis);

        return sections;
    }

    private static List<string> ExtractBulletsFromContent(string content)
    {
        const int maxLen = 100;
        const int maxBullets = 5;

        // 1. Try to extract existing list items (<li> tags)
        var listItems = Regex.Matches(content, @"<li[^>]*>(.*?)</li>", RegexOptions.Singleline)
            .Select(m => CleanHtmlText(m.Groups[1].Value))
            .Where(s => s.Length > 5)
            .Select(s => Truncate(s, maxLen))
            .Take(maxBullets)
            .ToList();
        if (listItems.Count >= 2) return listItems;

        // 2. Try bold/strong text as key points
        var boldItems = Regex.Matches(content, @"<(?:strong|b)>(.*?)</(?:strong|b)>", RegexOptions.Singleline)
            .Select(m => CleanHtmlText(m.Groups[1].Value))
            .Where(s => s.Length > 5 && s.Length < 200)
            .Select(s => Truncate(s, maxLen))
            .Distinct()
            .Take(maxBullets)
            .ToList();
        if (boldItems.Count >= 2) return boldItems;

        // 3. Fall back to key sentences, trimmed for slides
        var text = CleanHtmlText(content);
        return Regex.Split(text, @"(?<=[.!?])\s+")
            .Select(s => s.Trim())
            .Where(s => s.Length > 15)
            .Select(s => Truncate(s, maxLen))
            .Take(maxBullets)
            .ToList();
    }

    private static string CleanHtmlText(string html)
    {
        var text = Regex.Replace(html, "<[^>]*>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static string Truncate(string text, int maxLen)
    {
        if (text.Length <= maxLen) return text;
        var cut = text.LastIndexOf(' ', maxLen - 3);
        return (cut > maxLen / 2 ? text[..cut] : text[..(maxLen - 3)]) + "...";
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

            // Resolve theme from templateId and cover page primary color
            var themeResult = await _themeService.ResolveThemeAsync(
                request.TemplateId, request.CoverPageSettings?.PrimaryColor, cancellationToken);
            var theme = themeResult.IsSuccess ? themeResult.Value! : ExportThemeRegistry.GetTheme("classic");

            var statistics = new ExportStatistics();
            var pdfBytes = GeneratePdfWithVisualsQuestPDF(businessPlan, request, statistics, theme);

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
                Template = theme.Id,
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

            // Resolve theme
            var themeResult = await _themeService.ResolveThemeAsync(
                request.TemplateId, request.CoverPageSettings?.PrimaryColor, cancellationToken);
            var theme = themeResult.IsSuccess ? themeResult.Value! : ExportThemeRegistry.GetTheme("classic");

            var statistics = new ExportStatistics();
            var wordBytes = GenerateWordDocumentWithVisuals(businessPlan, request, statistics, theme);

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
                Template = theme.Id,
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

            // Resolve theme
            var themeResult = await _themeService.ResolveThemeAsync(
                request.TemplateId, request.CoverPageSettings?.PrimaryColor, cancellationToken);
            var theme = themeResult.IsSuccess ? themeResult.Value! : ExportThemeRegistry.GetTheme("classic");

            var html = GenerateHtmlWithVisuals(businessPlan, request, theme);
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
        ExportStatistics statistics,
        ExportTheme theme)
    {
        var sections = request.Sections ?? GetDefaultSections(businessPlan, request.Language);
        statistics.SectionCount = sections.Count;
        var contentSections = sections.Where(s => !string.IsNullOrWhiteSpace(s.Content) || (s.VisualElements != null && s.VisualElements.Any())).ToList();

        return QuestPDF.Fluent.Document.Create(container =>
        {
            // ── Cover Page ──
            container.Page(coverPage =>
            {
                coverPage.Size(PageSizes.A4);
                coverPage.Margin(0);

                coverPage.Content().Column(column =>
                {
                    // Gradient band (top 40% of page)
                    column.Item().Height(420).Background(HexToColor(theme.PrimaryColor)).Padding(50).Column(band =>
                    {
                        band.Spacing(8);
                        band.Item().PaddingTop(80);
                        band.Item().Text(request.CoverPageSettings?.DocumentTitle ?? businessPlan.Title ?? GetLocalizedText("Business Plan", request.Language))
                            .Bold().FontSize(32).FontColor(QuestPDF.Helpers.Colors.White);
                        band.Item().Text(request.CoverPageSettings?.CompanyName ?? businessPlan.Organization?.Name ?? "")
                            .FontSize(18).FontColor(QuestPDF.Helpers.Colors.White).Light();

                        if (!string.IsNullOrWhiteSpace(request.CoverPageSettings?.Subtitle))
                        {
                            band.Item().PaddingTop(4).Text(request.CoverPageSettings.Subtitle)
                                .FontSize(13).FontColor(HexToColor(theme.CoverSubtitleColor));
                        }

                        // Chart color palette dots (small colored squares)
                        band.Item().PaddingTop(16).Row(row =>
                        {
                            foreach (var color in theme.ChartColorPalette.Take(4))
                            {
                                row.ConstantItem(12).Height(12).Background(HexToColor(color));
                                row.ConstantItem(6); // spacing
                            }
                        });
                    });

                    // Lower section with prepared by/for
                    column.Item().ExtendVertical().Background(HexToColor(theme.PageBackgroundColor)).Padding(50).Column(lower =>
                    {
                        lower.Spacing(6);
                        lower.Item().PaddingTop(30);

                        if (!string.IsNullOrWhiteSpace(request.CoverPageSettings?.PreparedBy))
                        {
                            lower.Item().Text($"{GetLocalizedText("Prepared by", request.Language)}: {request.CoverPageSettings.PreparedBy}")
                                .FontSize(12).FontColor(HexToColor(theme.TextColor));
                        }
                        if (!string.IsNullOrWhiteSpace(request.CoverPageSettings?.PreparedFor))
                        {
                            lower.Item().Text($"{GetLocalizedText("Prepared for", request.Language)}: {request.CoverPageSettings.PreparedFor}")
                                .FontSize(12).FontColor(HexToColor(theme.TextColor));
                        }

                        var date = request.CoverPageSettings?.PreparedDate ?? DateTime.UtcNow;
                        lower.Item().PaddingTop(10).Text(date.ToString("MMMM dd, yyyy"))
                            .FontSize(11).FontColor(HexToColor(theme.MutedTextColor));
                    });
                });
            });

            // ── Table of Contents Page ──
            if (request.IncludeTableOfContents)
            {
                container.Page(tocPage =>
                {
                    tocPage.Size(PageSizes.A4);
                    tocPage.Margin(2, Unit.Centimetre);
                    tocPage.PageColor(HexToColor(theme.TocBackgroundColor));
                    tocPage.DefaultTextStyle(x => x.FontSize(11));

                    tocPage.Content().Column(column =>
                    {
                        column.Item().Text(GetLocalizedText("Table of Contents", request.Language))
                            .Bold().FontSize(20).FontColor(HexToColor(theme.HeadingColor));
                        column.Item().PaddingBottom(16);

                        var sectionNumber = 1;
                        foreach (var section in contentSections)
                        {
                            column.Item().BorderBottom(1).BorderColor(HexToColor(theme.SeparatorColor))
                                .PaddingVertical(8).Row(row =>
                                {
                                    row.RelativeItem().Text($"{sectionNumber}. {section.Title}")
                                        .FontSize(12).FontColor(HexToColor(theme.TextColor));
                                    row.ConstantItem(60).AlignRight().Text($"{GetLocalizedText("Page", request.Language)} {sectionNumber + 1}")
                                        .FontSize(10).FontColor(HexToColor(theme.MutedTextColor));
                                });
                            sectionNumber++;
                        }
                    });

                    tocPage.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(9).FontColor(HexToColor(theme.MutedTextColor)));
                        x.CurrentPageNumber();
                    });
                });
            }

            // ── Content Pages ──
            container.Page(contentPage =>
            {
                contentPage.Size(PageSizes.A4);
                contentPage.Margin(2, Unit.Centimetre);
                contentPage.PageColor(HexToColor(theme.PageBackgroundColor));
                contentPage.DefaultTextStyle(x => x.FontSize(11).FontColor(HexToColor(theme.TextColor)));

                contentPage.Header().PaddingBottom(10).Row(row =>
                {
                    row.RelativeItem().Text(businessPlan.Title ?? "")
                        .FontSize(9).FontColor(HexToColor(theme.MutedTextColor));
                    row.ConstantItem(100).AlignRight().Text(businessPlan.Organization?.Name ?? "")
                        .FontSize(9).FontColor(HexToColor(theme.MutedTextColor));
                });

                contentPage.Content()
                    .Column(column =>
                    {
                        column.Spacing(10);

                        foreach (var section in contentSections)
                        {
                            // Section title with accent border
                            column.Item().BorderBottom(2).BorderColor(HexToColor(theme.AccentColor))
                                .PaddingBottom(6).Text(section.Title)
                                .Bold().FontSize(16).FontColor(HexToColor(theme.HeadingColor));

                            // Section prose content — render markdown-aware paragraphs
                            if (!string.IsNullOrWhiteSpace(section.Content))
                            {
                                var wordCount = section.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                                statistics.WordCount += wordCount;
                                RenderContentToPdf(column, section.Content, theme);
                            }

                            // Visual elements
                            if (request.IncludeVisuals && section.VisualElements != null)
                            {
                                foreach (var visual in section.VisualElements)
                                {
                                    column.Item().PaddingTop(10);
                                    RenderVisualElementToPdf(column, visual, statistics, request.VisualOptions, theme);
                                }
                            }

                            column.Item().PaddingTop(12);
                        }
                    });

                // Footer with page numbers
                if (request.IncludePageNumbers)
                {
                    contentPage.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.DefaultTextStyle(s => s.FontSize(9).FontColor(HexToColor(theme.MutedTextColor)));
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

    private void RenderVisualElementToPdf(
        ColumnDescriptor column,
        ExportVisualElement visual,
        ExportStatistics statistics,
        VisualElementOptions? options,
        ExportTheme theme)
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
                RenderTableToPdf(column, visual.Data, options, theme);
                break;
            case "chart":
                statistics.ChartCount++;
                RenderChartPlaceholderToPdf(column, visual, options, theme);
                break;
            case "metric":
                statistics.MetricCount++;
                RenderMetricsToPdf(column, visual.Data, options, theme);
                break;
            case "infographic":
                statistics.InfographicCount++;
                RenderInfographicToPdf(column, visual.Data, options, theme);
                break;
        }
    }

    private void RenderTableToPdf(ColumnDescriptor column, object data, VisualElementOptions? options, ExportTheme theme)
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
                            .Background(HexToColor(theme.TableHeaderBg))
                            .Padding(5)
                            .Text(headerText)
                            .Bold()
                            .FontSize(10)
                            .FontColor(HexToColor(theme.TableHeaderFg));
                    }
                });

                // Data rows
                var rowIndex = 0;
                foreach (var row in tableData.Rows)
                {
                    var bgColor = rowIndex % 2 == 0
                        ? HexToColor(theme.PageBackgroundColor)
                        : HexToColor(theme.TableAlternateRowBg);

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
            column.Item().Text("[Table rendering error]").FontSize(10).FontColor(HexToColor(theme.ErrorColor));
        }
    }

    private void RenderChartPlaceholderToPdf(ColumnDescriptor column, ExportVisualElement visual, VisualElementOptions? options, ExportTheme theme)
    {
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
                .Background(HexToColor(theme.ChartPlaceholderBg))
                .Padding(10)
                .Column(chartColumn =>
                {
                    chartColumn.Item().AlignCenter()
                        .Text($"[{chartData.ChartType.ToUpper()} Chart]")
                        .Bold().FontSize(12);

                    chartColumn.Item().PaddingTop(10);

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
            column.Item().Text("[Chart rendering error]").FontSize(10).FontColor(HexToColor(theme.ErrorColor));
        }
    }

    private void RenderMetricsToPdf(ColumnDescriptor column, object data, VisualElementOptions? options, ExportTheme theme)
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
                        .Background(HexToColor(theme.MetricCardBg))
                        .Padding(10)
                        .Column(metricColumn =>
                        {
                            // Label
                            metricColumn.Item().Text(metric.Label)
                                .FontSize(9).FontColor(HexToColor(theme.MetricLabelColor));

                            // Value
                            var formattedValue = FormatMetricValue(metric);
                            metricColumn.Item().Text(formattedValue)
                                .Bold().FontSize(16).FontColor(HexToColor(theme.MetricValueColor));

                            // Trend
                            if (!string.IsNullOrWhiteSpace(metric.TrendValue))
                            {
                                var trendColor = metric.Trend == "up"
                                    ? HexToColor(theme.TrendUpColor)
                                    : metric.Trend == "down"
                                        ? HexToColor(theme.TrendDownColor)
                                        : HexToColor(theme.TrendNeutralColor);

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
            column.Item().Text("[Metrics rendering error]").FontSize(10).FontColor(HexToColor(theme.ErrorColor));
        }
    }

    private void RenderInfographicToPdf(ColumnDescriptor column, object data, VisualElementOptions? options, ExportTheme theme)
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
                            .Background(HexToColor(theme.InfographicNumberBg))
                            .AlignCenter()
                            .AlignMiddle()
                            .Text($"{itemNumber}")
                            .Bold().FontSize(12).FontColor(HexToColor(theme.InfographicNumberFg));

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
            column.Item().Text("[Infographic rendering error]").FontSize(10).FontColor(HexToColor(theme.ErrorColor));
        }
    }

    private byte[] GenerateWordDocumentWithVisuals(
        BusinessPlan businessPlan,
        ExportWithVisualsRequest request,
        ExportStatistics statistics,
        ExportTheme theme)
    {
        var sections = request.Sections ?? GetDefaultSections(businessPlan, request.Language);
        statistics.SectionCount = sections.Count;

        using var memoryStream = new MemoryStream();
        using var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document);

        var mainPart = wordDocument.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        var body = mainPart.Document.AppendChild(new Body());

        // Add styles
        AddWordStyles(mainPart, theme);

        // Cover page
        if (request.CoverPageSettings != null)
        {
            AddWordCoverPage(body, request.CoverPageSettings, businessPlan, request.Language, theme);
            body.Append(new WordParagraph(new DocumentFormat.OpenXml.Wordprocessing.Run(
                new DocumentFormat.OpenXml.Wordprocessing.Break { Type = BreakValues.Page })));
        }
        else
        {
            // Simple title
            AddWordTitle(body, $"{GetLocalizedText("Business Plan", request.Language)}: {businessPlan.Title}", theme);
        }

        // Table of contents placeholder
        if (request.IncludeTableOfContents)
        {
            AddWordHeading(body, GetLocalizedText("Table of Contents", request.Language), 1, theme);
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
            AddWordHeading(body, section.Title, 1, theme);

            // Prose content
            if (!string.IsNullOrWhiteSpace(section.Content))
            {
                var wordCount = section.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                statistics.WordCount += wordCount;
                RenderContentToWord(body, section.Content, theme);
            }

            // Visual elements
            if (request.IncludeVisuals && section.VisualElements != null)
            {
                foreach (var visual in section.VisualElements)
                {
                    body.Append(new WordParagraph()); // Spacing
                    RenderVisualElementToWord(body, visual, statistics, theme);
                }
            }

            body.Append(new WordParagraph()); // Spacing between sections
        }

        wordDocument.Save();
        return memoryStream.ToArray();
    }

    private void AddWordStyles(MainDocumentPart mainPart, ExportTheme theme)
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
            new WordColor { Val = HexForOpenXml(theme.HeadingColor) }
        ));
        stylesPart.Styles.Append(heading1Style);

        stylesPart.Styles.Save();
    }

    private void AddWordCoverPage(Body body, ExportCoverPageSettings settings, BusinessPlan businessPlan, string language, ExportTheme theme)
    {
        // Company name
        var companyPara = new WordParagraph();
        var companyProps = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
        companyPara.Append(companyProps);
        var companyRun = new WordRun();
        companyRun.Append(new WordRunProperties(new WordBold(), new WordFontSize { Val = "56" }, new WordColor { Val = HexForOpenXml(theme.CoverTitleColor) }));
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
            subtitleRun.Append(new WordRunProperties(new WordFontSize { Val = "28" }, new WordColor { Val = HexForOpenXml(theme.CoverSubtitleColor) }));
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

    private void AddWordTitle(Body body, string text, ExportTheme? theme = null)
    {
        theme ??= ExportThemeRegistry.GetTheme("classic");
        var paragraph = new WordParagraph();
        var run = new WordRun();
        var runProperties = new WordRunProperties();
        runProperties.Append(new WordBold());
        runProperties.Append(new WordFontSize { Val = "48" });
        runProperties.Append(new WordColor { Val = HexForOpenXml(theme.HeadingColor) });
        run.Append(runProperties);
        run.Append(new WordText(text));
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private void AddWordHeading(Body body, string text, int level, ExportTheme? theme = null)
    {
        theme ??= ExportThemeRegistry.GetTheme("classic");
        var paragraph = new WordParagraph();
        var paragraphProperties = new ParagraphProperties();
        paragraphProperties.Append(new SpacingBetweenLines { Before = "240", After = "120" });
        paragraph.Append(paragraphProperties);

        var run = new WordRun();
        var runProperties = new WordRunProperties();
        runProperties.Append(new WordBold());
        runProperties.Append(new WordFontSize { Val = level == 1 ? "32" : "28" });
        runProperties.Append(new WordColor { Val = HexForOpenXml(level == 1 ? theme.HeadingColor : theme.Heading2Color) });
        run.Append(runProperties);
        run.Append(new WordText(text));
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private void AddWordParagraph(Body body, string text, bool isItalic = false, ExportTheme? theme = null)
    {
        var paragraph = new WordParagraph();
        var paragraphProperties = new ParagraphProperties();
        paragraphProperties.Append(new SpacingBetweenLines { After = "120", Line = "276", LineRule = LineSpacingRuleValues.Auto });
        paragraph.Append(paragraphProperties);

        var run = new WordRun();
        if (isItalic)
        {
            theme ??= ExportThemeRegistry.GetTheme("classic");
            var runProperties = new WordRunProperties();
            runProperties.Append(new WordItalic());
            runProperties.Append(new WordColor { Val = HexForOpenXml(theme.MutedTextColor) });
            run.Append(runProperties);
        }
        run.Append(new WordText(text) { Space = SpaceProcessingModeValues.Preserve });
        paragraph.Append(run);
        body.Append(paragraph);
    }

    /// <summary>
    /// Renders section content to Word, converting markdown headings into styled sub-headings
    /// and splitting paragraphs properly.
    /// </summary>
    private void RenderContentToWord(Body body, string content, ExportTheme theme)
    {
        // Strip HTML tags, decode entities
        var text = System.Text.RegularExpressions.Regex.Replace(content, "<[^>]*>", "\n");
        text = System.Net.WebUtility.HtmlDecode(text);

        var blocks = System.Text.RegularExpressions.Regex.Split(text, @"\n\s*\n")
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrWhiteSpace(b));

        foreach (var block in blocks)
        {
            var headingMatch = System.Text.RegularExpressions.Regex.Match(block, @"^(#{1,4})\s+(.+)$", System.Text.RegularExpressions.RegexOptions.Multiline);
            if (headingMatch.Success)
            {
                var headingText = StripInlineMarkdown(headingMatch.Groups[2].Value);
                AddWordHeading(body, headingText, 2, theme);
                continue;
            }

            var paragraphText = StripInlineMarkdown(block);
            paragraphText = System.Text.RegularExpressions.Regex.Replace(paragraphText, @"\s*\n\s*", " ").Trim();

            if (!string.IsNullOrWhiteSpace(paragraphText))
            {
                AddWordParagraph(body, paragraphText);
            }
        }
    }

    private void RenderVisualElementToWord(Body body, ExportVisualElement visual, ExportStatistics statistics, ExportTheme theme)
    {
        statistics.VisualElementCount++;

        // Visual element title
        if (!string.IsNullOrWhiteSpace(visual.Title))
        {
            AddWordHeading(body, visual.Title, 2, theme);
        }

        switch (visual.Type.ToLower())
        {
            case "table":
                statistics.TableCount++;
                RenderTableToWord(body, visual.Data, theme);
                break;
            case "chart":
                statistics.ChartCount++;
                RenderChartPlaceholderToWord(body, visual, theme);
                break;
            case "metric":
                statistics.MetricCount++;
                RenderMetricsToWord(body, visual.Data, theme);
                break;
            case "infographic":
                statistics.InfographicCount++;
                RenderInfographicToWord(body, visual.Data);
                break;
        }
    }

    private void RenderTableToWord(Body body, object data, ExportTheme theme)
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
                cell.Append(new WordTableCellProperties(new WordShading { Fill = HexForOpenXml(theme.TableHeaderBg) }));
                var para = new WordParagraph();
                var run = new WordRun();
                run.Append(new WordRunProperties(new WordBold(), new WordColor { Val = HexForOpenXml(theme.TableHeaderFg) }));
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

    private void RenderChartPlaceholderToWord(Body body, ExportVisualElement visual, ExportTheme theme)
    {
        try
        {
            var chartData = DeserializeVisualData<ExportChartData>(visual.Data);

            var para = new WordParagraph();
            var props = new ParagraphProperties(new WordShading { Fill = HexForOpenXml(theme.ChartPlaceholderBg) });
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

    private void RenderMetricsToWord(Body body, object data, ExportTheme theme)
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
                cell.Append(new WordTableCellProperties(new WordShading { Fill = HexForOpenXml(theme.MetricCardBg) }));

                // Label
                var labelPara = new WordParagraph();
                var labelRun = new WordRun();
                labelRun.Append(new WordRunProperties(new WordFontSize { Val = "18" }, new WordColor { Val = HexForOpenXml(theme.MetricLabelColor) }));
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

    private string GenerateHtmlWithVisuals(BusinessPlan businessPlan, ExportWithVisualsRequest request, ExportTheme theme)
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
        html.AppendLine(GetEnhancedCss(theme));
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

    private string GetEnhancedCss(ExportTheme theme)
    {
        return $@"
            :root {{ --primary-color: {theme.PrimaryColor}; --secondary-color: {theme.SecondaryColor}; }}
            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; margin: 0; padding: 0; background-color: {theme.BodyBackgroundColor}; }}
            .cover-page {{ min-height: 100vh; display: flex; flex-direction: column; justify-content: center; align-items: center; background: linear-gradient(135deg, var(--primary-color) 0%, {theme.CoverGradientEnd} 100%); color: white; text-align: center; padding: 40px; }}
            .cover-page .company-name {{ font-size: 3rem; margin-bottom: 0.5rem; }}
            .cover-page .document-title {{ font-size: 2rem; font-weight: normal; margin-bottom: 1rem; }}
            .cover-page .subtitle {{ font-size: 1.2rem; opacity: 0.9; }}
            .cover-page .prepared-info {{ margin: 0.5rem 0; }}
            .cover-page .date {{ margin-top: 2rem; opacity: 0.8; }}
            .container {{ max-width: 900px; margin: 0 auto; background: white; padding: 40px; box-shadow: 0 0 20px rgba(0,0,0,0.1); }}
            .toc {{ background: {theme.TocBackgroundColor}; padding: 20px; margin-bottom: 30px; border-radius: 8px; }}
            .toc h2 {{ margin-top: 0; color: var(--primary-color); }}
            .toc ol {{ padding-left: 20px; }}
            .toc a {{ color: {theme.TextColor}; text-decoration: none; }}
            .toc a:hover {{ color: var(--primary-color); }}
            .section {{ margin-bottom: 40px; }}
            .section h2 {{ color: var(--primary-color); border-bottom: 2px solid {theme.SeparatorColor}; padding-bottom: 10px; }}
            .content {{ text-align: justify; color: {theme.TextColor}; }}
            .visual-element {{ margin: 20px 0; }}
            .visual-element h3 {{ color: {theme.Heading2Color}; margin-bottom: 10px; }}
            .data-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
            .data-table th, .data-table td {{ padding: 12px; text-align: left; border: 1px solid {theme.TableBorderColor}; }}
            .data-table th {{ background: {theme.TableHeaderBg}; color: {theme.TableHeaderFg}; font-weight: bold; }}
            .data-table tr:nth-child(even) {{ background: {theme.TableAlternateRowBg}; }}
            .data-table tr.highlighted {{ background: {theme.TableHighlightBg}; }}
            .data-table td.bold {{ font-weight: bold; }}
            .chart-placeholder {{ background: {theme.ChartPlaceholderBg}; padding: 40px; text-align: center; color: {theme.MutedTextColor}; border-radius: 8px; }}
            .metrics-container {{ display: flex; gap: 15px; flex-wrap: wrap; }}
            .metric-card {{ flex: 1; min-width: 150px; background: {theme.MetricCardBg}; padding: 20px; border-radius: 8px; text-align: center; }}
            .metric-label {{ color: {theme.MetricLabelColor}; font-size: 0.9rem; }}
            .metric-value {{ font-size: 1.8rem; font-weight: bold; color: {theme.MetricValueColor}; }}
            .metric-trend {{ font-size: 0.9rem; margin-top: 5px; }}
            .trend-up {{ color: {theme.TrendUpColor}; }}
            .trend-down {{ color: {theme.TrendDownColor}; }}
            .infographic {{ margin: 15px 0; }}
            .infographic-item {{ display: flex; gap: 15px; margin-bottom: 15px; }}
            .item-number {{ width: 30px; height: 30px; background: {theme.InfographicNumberBg}; color: {theme.InfographicNumberFg}; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; flex-shrink: 0; }}
            .item-content {{ flex: 1; }}
            .item-title {{ font-weight: bold; margin-bottom: 5px; }}
            .item-description {{ color: {theme.MutedTextColor}; }}
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
        AddSection("SWOT Analysis", businessPlan.SwotAnalysis);
        AddSection("Business Model", businessPlan.BusinessModel);
        AddSection("Marketing Strategy", businessPlan.MarketingStrategy);
        AddSection("Branding Strategy", businessPlan.BrandingStrategy);
        AddSection("Operations Plan", businessPlan.OperationsPlan);
        AddSection("Management Team", businessPlan.ManagementTeam);
        AddSection("Financial Projections", businessPlan.FinancialProjections);
        AddSection("Funding Requirements", businessPlan.FundingRequirements);
        AddSection("Risk Analysis", businessPlan.RiskAnalysis);
        AddSection("Exit Strategy", businessPlan.ExitStrategy);

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
        AddSectionInfo("SWOT Analysis", businessPlan.SwotAnalysis);
        AddSectionInfo("Business Model", businessPlan.BusinessModel);
        AddSectionInfo("Marketing Strategy", businessPlan.MarketingStrategy);
        AddSectionInfo("Branding Strategy", businessPlan.BrandingStrategy);
        AddSectionInfo("Operations Plan", businessPlan.OperationsPlan);
        AddSectionInfo("Management Team", businessPlan.ManagementTeam);
        AddSectionInfo("Financial Projections", businessPlan.FinancialProjections);
        AddSectionInfo("Funding Requirements", businessPlan.FundingRequirements);
        AddSectionInfo("Risk Analysis", businessPlan.RiskAnalysis);
        AddSectionInfo("Exit Strategy", businessPlan.ExitStrategy);

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

        // Remove HTML tags
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", " ");
        // Decode HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);
        // Strip markdown headings (## Title → Title)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^#{1,6}\s+", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        // Strip markdown bold/italic (**text** → text, *text* → text)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*{1,3}([^*]+)\*{1,3}", "$1");
        // Strip markdown bullet points (- item → item)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*[-*+]\s+", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        // Strip markdown numbered lists (1. item → item)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*\d+\.\s+", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        // Normalize whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }

    /// <summary>
    /// Renders section content to PDF, converting markdown headings into styled sub-headings
    /// and splitting on double-newlines into separate paragraphs.
    /// </summary>
    private static void RenderContentToPdf(QuestPDF.Fluent.ColumnDescriptor column, string content, ExportTheme theme)
    {
        // First strip HTML tags and decode entities
        var text = System.Text.RegularExpressions.Regex.Replace(content, "<[^>]*>", "\n");
        text = System.Net.WebUtility.HtmlDecode(text);

        // Split into blocks by double-newline
        var blocks = System.Text.RegularExpressions.Regex.Split(text, @"\n\s*\n")
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrWhiteSpace(b));

        foreach (var block in blocks)
        {
            var line = block;

            // Check if this block is a markdown heading
            var headingMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(#{1,4})\s+(.+)$", System.Text.RegularExpressions.RegexOptions.Multiline);
            if (headingMatch.Success)
            {
                var level = headingMatch.Groups[1].Value.Length;
                var headingText = StripInlineMarkdown(headingMatch.Groups[2].Value);
                var fontSize = level switch { 1 => 15f, 2 => 14f, 3 => 13f, _ => 12f };

                column.Item().PaddingTop(level <= 2 ? 10 : 6)
                    .Text(headingText)
                    .Bold().FontSize(fontSize).FontColor(HexToColor(theme.HeadingColor));
                continue;
            }

            // Regular paragraph — strip inline markdown and render
            var paragraphText = StripInlineMarkdown(line);
            // Collapse internal newlines to spaces for prose flow
            paragraphText = System.Text.RegularExpressions.Regex.Replace(paragraphText, @"\s*\n\s*", " ").Trim();

            if (!string.IsNullOrWhiteSpace(paragraphText))
            {
                column.Item().PaddingTop(4)
                    .Text(paragraphText)
                    .FontSize(11).FontColor(HexToColor(theme.TextColor)).LineHeight(1.5f);
            }
        }
    }

    /// <summary>Strips inline markdown formatting (bold, italic, bullets, numbered lists).</summary>
    private static string StripInlineMarkdown(string text)
    {
        // Bold/italic: **text**, *text*, ***text***
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*{1,3}([^*]+)\*{1,3}", "$1");
        // Bullet points: - item, * item, + item
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*[-*+]\s+", "• ", System.Text.RegularExpressions.RegexOptions.Multiline);
        // Numbered lists: 1. item
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^\s*(\d+)\.\s+", "$1. ", System.Text.RegularExpressions.RegexOptions.Multiline);
        return text.Trim();
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
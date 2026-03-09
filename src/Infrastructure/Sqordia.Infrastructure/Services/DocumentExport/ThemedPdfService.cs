using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Models.Export;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Export;
using System.Diagnostics;
using System.Security.Claims;

namespace Sqordia.Infrastructure.Services.DocumentExport;

/// <summary>
/// Generates themed PDFs by building HTML (identical to frontend preview)
/// and rendering it with PuppeteerSharp for selectable text and proper page breaks.
/// </summary>
public class ThemedPdfService : IThemedPdfService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IExportThemeService _themeService;
    private readonly IHtmlToPdfRenderer _renderer;
    private readonly ILogger<ThemedPdfService> _logger;

    public ThemedPdfService(
        IApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IExportThemeService themeService,
        IHtmlToPdfRenderer renderer,
        ILogger<ThemedPdfService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _themeService = themeService;
        _renderer = renderer;
        _logger = logger;
    }

    public async Task<Result<ExportResult>> GenerateThemedPdfAsync(
        Guid businessPlanId,
        string? themeId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation(
                "Starting themed PDF export for {BusinessPlanId} with theme {ThemeId} in {Language}",
                businessPlanId, themeId ?? "classic", language);

            // Validate user access
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return Result.Failure<ExportResult>("Authentication required");

            // Load business plan
            var bp = await _context.BusinessPlans
                .AsNoTracking()
                .Include(b => b.Organization)
                    .ThenInclude(o => o.Members)
                .FirstOrDefaultAsync(b => b.Id == businessPlanId && !b.IsDeleted, cancellationToken);

            if (bp == null)
                return Result.Failure<ExportResult>("Business plan not found");

            var hasAccess = bp.Organization.Members.Any(m => m.UserId == currentUserId.Value && m.IsActive);
            if (!hasAccess)
                return Result.Failure<ExportResult>("Access denied");

            // Resolve theme
            var themeResult = await _themeService.ResolveThemeAsync(themeId, null, cancellationToken);
            var theme = themeResult.IsSuccess ? themeResult.Value! : ExportThemeRegistry.GetTheme("classic");

            // Build sections list
            var sections = BuildSections(bp, language);
            var companyName = bp.Organization?.Name ?? "Company";
            var planTitle = bp.Title ?? (language == "fr" ? "Plan d'affaires" : "Business Plan");

            // Build themed HTML (mirrors frontend TemplatePreviewModal)
            var html = ThemedHtmlBuilder.Build(
                theme, sections, planTitle, companyName, language);

            // Render with Puppeteer
            var pdfBytes = await _renderer.RenderAsync(html, cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Themed PDF generated in {ElapsedMs}ms, size: {SizeKB} KB",
                stopwatch.ElapsedMilliseconds, pdfBytes.Length / 1024);

            var isFr = language == "fr";
            var sanitizedName = SanitizeFileName(companyName);
            var dateStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var label = isFr ? "Plan_d_affaires" : "BusinessPlan";
            var fileName = $"{sanitizedName}_{label}_{dateStr}.pdf";

            return Result.Success(new ExportResult
            {
                FileData = pdfBytes,
                FileName = fileName,
                ContentType = "application/pdf",
                FileSizeBytes = pdfBytes.Length,
                Language = language,
                Template = theme.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating themed PDF for {BusinessPlanId}", businessPlanId);
            return Result.Failure<ExportResult>("PDF generation failed. Please try again.");
        }
    }

    private List<SectionExportContent> BuildSections(
        Domain.Entities.BusinessPlan.BusinessPlan bp,
        string language)
    {
        var sections = new List<SectionExportContent>();
        var order = 0;
        var isFr = language == "fr";

        void Add(string key, string frTitle, string enTitle, string? content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                sections.Add(new SectionExportContent
                {
                    SectionKey = key,
                    Title = isFr ? frTitle : enTitle,
                    Content = content,
                    Order = order++
                });
            }
        }

        Add("ExecutiveSummary", "Résumé Exécutif", "Executive Summary", bp.ExecutiveSummary);
        Add("ProblemStatement", "Énoncé du Problème", "Problem Statement", bp.ProblemStatement);
        Add("Solution", "Solution", "Solution", bp.Solution);
        Add("MarketAnalysis", "Analyse de Marché", "Market Analysis", bp.MarketAnalysis);
        Add("CompetitiveAnalysis", "Analyse Concurrentielle", "Competitive Analysis", bp.CompetitiveAnalysis);
        Add("SwotAnalysis", "Analyse SWOT", "SWOT Analysis", bp.SwotAnalysis);
        Add("BusinessModel", "Modèle d'Affaires", "Business Model", bp.BusinessModel);
        Add("MarketingStrategy", "Stratégie Marketing", "Marketing Strategy", bp.MarketingStrategy);
        Add("BrandingStrategy", "Stratégie de Marque", "Branding Strategy", bp.BrandingStrategy);
        Add("OperationsPlan", "Plan des Opérations", "Operations Plan", bp.OperationsPlan);
        Add("ManagementTeam", "Équipe de Direction", "Management Team", bp.ManagementTeam);
        Add("FinancialProjections", "Projections Financières", "Financial Projections", bp.FinancialProjections);
        Add("FundingRequirements", "Besoins de Financement", "Funding Requirements", bp.FundingRequirements);
        Add("RiskAnalysis", "Analyse des Risques", "Risk Analysis", bp.RiskAnalysis);
        Add("ExitStrategy", "Stratégie de Sortie", "Exit Strategy", bp.ExitStrategy);

        return sections;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;
        foreach (var c in invalidChars)
            sanitized = sanitized.Replace(c, '_');
        foreach (var c in new[] { '<', '>', '"', ':' })
            sanitized = sanitized.Replace(c, '_');
        while (sanitized.Contains("__"))
            sanitized = sanitized.Replace("__", "_");
        return sanitized.Trim('_');
    }
}

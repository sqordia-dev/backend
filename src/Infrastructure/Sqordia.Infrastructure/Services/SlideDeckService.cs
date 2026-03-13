using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Models.Export;
using Sqordia.Application.Services;
using Sqordia.Contracts.Responses.Export;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Infrastructure.Services.Helpers;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Sqordia.Infrastructure.Services;

public class SlideDeckService : ISlideDeckService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IStructuredExtractionService _structuredExtraction;
    private readonly IContentAdaptationService _adaptationService;
    private readonly IExportThemeService _themeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SlideDeckService> _logger;

    public SlideDeckService(
        IApplicationDbContext context,
        IAIService aiService,
        IStructuredExtractionService structuredExtraction,
        IContentAdaptationService adaptationService,
        IExportThemeService themeService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SlideDeckService> logger)
    {
        _context = context;
        _aiService = aiService;
        _structuredExtraction = structuredExtraction;
        _adaptationService = adaptationService;
        _themeService = themeService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<ExportResult>> GenerateSlideDeckAsync(
        Guid businessPlanId, string? themeId, string language, CancellationToken ct = default)
    {
        try
        {
            // Load business plan with authorization
            var businessPlan = await GetBusinessPlanWithValidationAsync(businessPlanId, ct);
            if (businessPlan == null)
            {
                return Result.Failure<ExportResult>("Business plan not found or access denied");
            }

            // Resolve theme
            var themeResult = await _themeService.ResolveThemeAsync(themeId, null, ct);
            var theme = themeResult.IsSuccess ? themeResult.Value! : ExportThemeRegistry.GetTheme("classic");

            _logger.LogInformation("Generating slide deck for business plan {Id} with theme {Theme}",
                businessPlanId, theme.Id);

            var isFr = language == "fr";
            var companyName = businessPlan.Organization?.Name ?? "";
            var planTitle = businessPlan.Title;

            // Build sections
            var sections = GetSectionsWithContent(businessPlan, language);

            // Adapt content using AI for PowerPoint format
            var adaptationInput = sections
                .Select(s => (s.Key, s.Title, s.Content))
                .ToList();
            var adaptResult = await _adaptationService.AdaptAllSectionsAsync(
                adaptationInput, ExportFormatTarget.PowerPoint, language, ct);

            var adaptedSections = adaptResult.IsSuccess ? adaptResult.Value! : null;

            // Build bullet lists per section
            var slideSections = new List<(string Key, string Title, List<string> Bullets)>();
            for (int i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                List<string> bullets;

                if (adaptedSections != null && i < adaptedSections.Count && adaptedSections[i].WasAiAdapted)
                {
                    // Use adapted content — split lines into bullets
                    bullets = adaptedSections[i].AdaptedContent
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(line => line.TrimStart('-', '*', '\u2022', ' ').Trim())
                        .Where(line => line.Length > 5)
                        .Take(8)
                        .ToList();
                }
                else
                {
                    // Fallback to original summarization
                    bullets = await SummarizeToBulletPointsAsync(section.Title, section.Content, language, ct);
                }

                slideSections.Add((section.Key, section.Title, bullets));
            }

            // ── Build the PPTX ──────────────────────────────────────

            using var builder = new PowerPointBuilder(theme);

            // 1. Title slide (gradient)
            var dateStr = DateTime.UtcNow.ToString("MMMM yyyy");
            builder.AddTitleSlide(companyName, planTitle, null, dateStr);

            // 2. Table of contents
            var tocTitle = isFr ? "Table des matières" : "Table of Contents";
            builder.AddTableOfContentsSlide(tocTitle, slideSections.Select(s => s.Title).ToList());

            // 3. Section slides (with dividers + specialized slide types)
            for (int i = 0; i < slideSections.Count; i++)
            {
                var (key, title, bullets) = slideSections[i];
                var sectionNum = $"{i + 1:D2}";

                // Section divider
                builder.AddSectionDividerSlide(title, sectionNum);

                // Choose slide type based on section key
                switch (key)
                {
                    case "SwotAnalysis":
                    case "CompetitiveAnalysis" when !string.IsNullOrWhiteSpace(businessPlan.CompetitiveAnalysis):
                        if (key == "SwotAnalysis" || key == "CompetitiveAnalysis")
                        {
                            var swotContent = key == "SwotAnalysis"
                                ? businessPlan.SwotAnalysis
                                : businessPlan.CompetitiveAnalysis;
                            if (!string.IsNullOrWhiteSpace(swotContent))
                            {
                                var swotTitle = isFr ? "Analyse SWOT" : "SWOT Analysis";
                                var swot = await ExtractSwotAsync(swotContent, language, ct);
                                builder.AddSwotSlide(swotTitle, swot.S, swot.W, swot.O, swot.T);
                            }
                            else
                            {
                                builder.AddContentSlides(title, bullets, 5, language);
                            }
                        }
                        break;

                    case "FinancialProjections":
                        if (string.IsNullOrWhiteSpace(businessPlan.FinancialProjections))
                        {
                            builder.AddContentSlides(title, bullets, 5, language);
                            break;
                        }

                        // Table slide with financial data
                        var tableData = await ExtractTableDataAsync(
                            businessPlan.FinancialProjections, language, ct);
                        if (tableData.Headers.Count > 0 && tableData.Rows.Count > 0)
                        {
                            var tableTitle = isFr ? "Projections financières" : "Financial Projections";
                            builder.AddTableSlide(tableTitle, tableData.Headers, tableData.Rows);
                        }

                        // Metrics slide
                        var finTitle = isFr ? "Résumé financier" : "Financial Summary";
                        var metrics = await ExtractFinancialMetricsAsync(
                            businessPlan.FinancialProjections, language, ct);
                        if (metrics.Count > 0)
                        {
                            builder.AddMetricsSlide(finTitle, metrics);
                        }
                        else
                        {
                            builder.AddContentSlides(title, bullets, 5, language);
                        }
                        break;

                    case "RiskAnalysis":
                        if (!string.IsNullOrWhiteSpace(businessPlan.RiskAnalysis))
                        {
                            var riskPairs = await ExtractRiskMitigationAsync(
                                businessPlan.RiskAnalysis, language, ct);
                            if (riskPairs.Risks.Count > 0)
                            {
                                var riskTitle = isFr ? "Risques et Mitigations" : "Risks & Mitigations";
                                var risksLabel = isFr ? "Risques" : "Risks";
                                var mitigLabel = isFr ? "Mitigations" : "Mitigations";
                                builder.AddTwoColumnSlide(riskTitle,
                                    risksLabel, riskPairs.Risks,
                                    mitigLabel, riskPairs.Mitigations);
                            }
                            else
                            {
                                builder.AddContentSlides(title, bullets, 5, language);
                            }
                        }
                        else
                        {
                            builder.AddContentSlides(title, bullets, 5, language);
                        }
                        break;

                    default:
                        // Standard content slides (multi-slide if >5 bullets)
                        builder.AddContentSlides(title, bullets, 5, language);
                        break;
                }
            }

            // 4. Thank you slide (gradient)
            var thankYouText = isFr ? "Merci" : "Thank You";
            builder.AddThankYouSlide(thankYouText, companyName);

            var pptxBytes = builder.Build();

            var result = new ExportResult
            {
                FileData = pptxBytes,
                FileName = $"{SanitizeFileName(companyName)}_V{businessPlan.Version}.pptx",
                ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                FileSizeBytes = pptxBytes.Length,
                Language = language,
                Template = theme.Id
            };

            _logger.LogInformation("Slide deck generated successfully. {Size} bytes", pptxBytes.Length);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating slide deck for business plan {Id}", businessPlanId);
            return Result.Failure<ExportResult>("Slide deck generation failed. Please try again.");
        }
    }

    // ── AI Extraction Methods ────────────────────────────────────

    private async Task<List<string>> SummarizeToBulletPointsAsync(
        string sectionTitle, string content, string language, CancellationToken ct)
    {
        try
        {
            var isAvailable = await _aiService.IsAvailableAsync(ct);
            if (!isAvailable)
            {
                return FallbackBullets(content);
            }

            var langInstruction = language == "fr"
                ? "Réponds en français."
                : "Respond in English.";

            var systemPrompt = $"You are a presentation content summarizer. {langInstruction} Extract exactly 3-5 concise bullet points from the given business plan section. Each bullet should be one sentence. Return only the bullet points, one per line, without numbering or bullet characters.";
            var userPrompt = $"Section: {sectionTitle}\n\nContent:\n{content}";

            var aiResponse = await _aiService.GenerateContentAsync(systemPrompt, userPrompt, 500, 0.3f, ct);

            var bullets = aiResponse
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => line.TrimStart('-', '*', '\u2022', ' ').Trim())
                .Where(line => line.Length > 5)
                .Take(5)
                .ToList();

            return bullets.Count > 0 ? bullets : FallbackBullets(content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI summarization failed for section {Section}, using fallback", sectionTitle);
            return FallbackBullets(content);
        }
    }

    private async Task<(List<string> S, List<string> W, List<string> O, List<string> T)> ExtractSwotAsync(
        string competitiveAnalysis, string language, CancellationToken ct)
    {
        try
        {
            // Try structured extraction first (Claude tool_use — reliable JSON)
            var structuredResult = await _structuredExtraction.ExtractSwotAsync(competitiveAnalysis, language, ct);
            if (structuredResult.IsSuccess)
            {
                var swot = structuredResult.Value!;
                if (swot.Strengths.Count > 0 || swot.Weaknesses.Count > 0)
                {
                    _logger.LogInformation("SWOT extracted via structured tool_use");
                    return (swot.Strengths, swot.Weaknesses, swot.Opportunities, swot.Threats);
                }
            }

            // Fallback to text-based extraction
            var isAvailable = await _aiService.IsAvailableAsync(ct);
            if (!isAvailable)
            {
                return (new() { "N/A" }, new() { "N/A" }, new() { "N/A" }, new() { "N/A" });
            }

            var langInstruction = language == "fr" ? "Réponds en français." : "Respond in English.";
            var systemPrompt = $"You are a SWOT analysis expert. {langInstruction} Extract 2-3 items for each SWOT quadrant from the competitive analysis text. Format output as:\nS: item1 | item2\nW: item1 | item2\nO: item1 | item2\nT: item1 | item2";

            var aiResponse = await _aiService.GenerateContentAsync(systemPrompt, competitiveAnalysis, 400, 0.3f, ct);

            return ParseSwotResponse(aiResponse);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SWOT extraction failed, using fallback");
            return (new() { "N/A" }, new() { "N/A" }, new() { "N/A" }, new() { "N/A" });
        }
    }

    private async Task<(List<string> Headers, List<List<string>> Rows)> ExtractTableDataAsync(
        string content, string language, CancellationToken ct)
    {
        try
        {
            var isAvailable = await _aiService.IsAvailableAsync(ct);
            if (!isAvailable) return (new(), new());

            var langInstruction = language == "fr" ? "Réponds en français." : "Respond in English.";
            var systemPrompt = $"You are a financial data extractor. {langInstruction} " +
                "Extract key financial data as a simple table. Return ONLY pipe-separated values:\n" +
                "Line 1: headers (e.g., Year | Revenue | Expenses | Profit)\n" +
                "Line 2+: data rows (e.g., 2025 | $500K | $300K | $200K)\n" +
                "Maximum 6 rows. Return only the table lines, nothing else.";

            var aiResponse = await _aiService.GenerateContentAsync(systemPrompt, content, 400, 0.2f, ct);

            var lines = aiResponse
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(l => l.Contains('|'))
                .ToList();

            if (lines.Count < 2) return (new(), new());

            var headers = lines[0].Split('|').Select(h => h.Trim()).Where(h => h.Length > 0).ToList();
            var rows = lines.Skip(1).Take(6)
                .Select(l => l.Split('|').Select(c => c.Trim()).Where(c => c.Length > 0).ToList())
                .ToList();

            return (headers, rows);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Table data extraction failed, using fallback");
            return (new(), new());
        }
    }

    private async Task<(List<string> Risks, List<string> Mitigations)> ExtractRiskMitigationAsync(
        string riskContent, string language, CancellationToken ct)
    {
        try
        {
            // Try structured extraction first (Claude tool_use)
            var structuredResult = await _structuredExtraction.ExtractRiskPairsAsync(riskContent, language, ct);
            if (structuredResult.IsSuccess && structuredResult.Value!.Pairs.Count > 0)
            {
                _logger.LogInformation("Risks extracted via structured tool_use ({Count} pairs)", structuredResult.Value.Pairs.Count);
                var pairs = structuredResult.Value.Pairs.Take(4).ToList();
                return (
                    pairs.Select(p => p.Risk).ToList(),
                    pairs.Select(p => p.Mitigation).ToList()
                );
            }

            // Fallback to text-based extraction
            var isAvailable = await _aiService.IsAvailableAsync(ct);
            if (!isAvailable) return (new(), new());

            var langInstruction = language == "fr" ? "Réponds en français." : "Respond in English.";
            var systemPrompt = $"You are a risk analyst. {langInstruction} " +
                "Extract up to 4 risk-mitigation pairs from the risk analysis text.\n" +
                "Format each pair as: RISK: description | MITIGATION: description\n" +
                "One pair per line. Return only the pairs.";

            var aiResponse = await _aiService.GenerateContentAsync(systemPrompt, riskContent, 500, 0.3f, ct);

            var risks = new List<string>();
            var mitigations = new List<string>();

            foreach (var line in aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('|', 2);
                var riskPart = parts[0].Trim();
                var mitigPart = parts.Length > 1 ? parts[1].Trim() : "";

                riskPart = Regex.Replace(riskPart, @"^(RISK|RISQUE)\s*:\s*", "", RegexOptions.IgnoreCase).Trim();
                mitigPart = Regex.Replace(mitigPart, @"^(MITIGATION|ATTÉNUATION)\s*:\s*", "", RegexOptions.IgnoreCase).Trim();

                if (riskPart.Length > 5)
                {
                    risks.Add(riskPart);
                    mitigations.Add(mitigPart.Length > 5 ? mitigPart : "—");
                }
            }

            return (risks.Take(4).ToList(), mitigations.Take(4).ToList());
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Risk-mitigation extraction failed, using fallback");
            return (new(), new());
        }
    }

    private async Task<List<(string Label, string Value)>> ExtractFinancialMetricsAsync(
        string financialProjections, string language, CancellationToken ct)
    {
        try
        {
            // Try structured extraction first (Claude tool_use)
            var structuredResult = await _structuredExtraction.ExtractFinancialMetricsAsync(financialProjections, language, ct);
            if (structuredResult.IsSuccess)
            {
                var fin = structuredResult.Value!;
                var metrics = new List<(string Label, string Value)>();
                var revenueLabel = language == "fr" ? "Revenus" : "Revenue";
                var expensesLabel = language == "fr" ? "Dépenses" : "Expenses";
                var profitLabel = language == "fr" ? "Profit net" : "Net Profit";
                var marginLabel = language == "fr" ? "Marge brute" : "Gross Margin";

                if (!string.IsNullOrEmpty(fin.Revenue)) metrics.Add((revenueLabel, fin.Revenue));
                if (!string.IsNullOrEmpty(fin.Expenses)) metrics.Add((expensesLabel, fin.Expenses));
                if (!string.IsNullOrEmpty(fin.Profit)) metrics.Add((profitLabel, fin.Profit));
                if (!string.IsNullOrEmpty(fin.GrossMargin)) metrics.Add((marginLabel, fin.GrossMargin));

                if (metrics.Count > 0)
                {
                    _logger.LogInformation("Financial metrics extracted via structured tool_use ({Count} metrics)", metrics.Count);
                    return metrics.Take(4).ToList();
                }
            }

            // Fallback to text-based extraction
            var isAvailable = await _aiService.IsAvailableAsync(ct);
            if (!isAvailable) return new();

            var langInstruction = language == "fr" ? "Réponds en français." : "Respond in English.";
            var systemPrompt = $"You are a financial analyst. {langInstruction} Extract up to 4 key financial metrics from the text. Format output as one metric per line: Label: Value (e.g., Revenue Year 1: $500,000). Return only the metrics.";

            var aiResponse = await _aiService.GenerateContentAsync(systemPrompt, financialProjections, 300, 0.3f, ct);

            var fallbackMetrics = new List<(string Label, string Value)>();
            foreach (var line in aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0 && colonIndex < line.Length - 1)
                {
                    var label = line[..colonIndex].Trim().TrimStart('-', '*', '\u2022', ' ');
                    var value = line[(colonIndex + 1)..].Trim();
                    if (label.Length > 0 && value.Length > 0)
                    {
                        fallbackMetrics.Add((label, value));
                    }
                }
            }

            return fallbackMetrics.Take(4).ToList();
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Financial metrics extraction failed, using fallback");
            return new();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static (List<string> S, List<string> W, List<string> O, List<string> T) ParseSwotResponse(string response)
    {
        var s = new List<string>();
        var w = new List<string>();
        var o = new List<string>();
        var t = new List<string>();

        foreach (var line in response.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("S:", StringComparison.OrdinalIgnoreCase))
                s = trimmed[2..].Split('|').Select(x => x.Trim()).Where(x => x.Length > 0).Take(3).ToList();
            else if (trimmed.StartsWith("W:", StringComparison.OrdinalIgnoreCase))
                w = trimmed[2..].Split('|').Select(x => x.Trim()).Where(x => x.Length > 0).Take(3).ToList();
            else if (trimmed.StartsWith("O:", StringComparison.OrdinalIgnoreCase))
                o = trimmed[2..].Split('|').Select(x => x.Trim()).Where(x => x.Length > 0).Take(3).ToList();
            else if (trimmed.StartsWith("T:", StringComparison.OrdinalIgnoreCase))
                t = trimmed[2..].Split('|').Select(x => x.Trim()).Where(x => x.Length > 0).Take(3).ToList();
        }

        if (s.Count == 0) s.Add("N/A");
        if (w.Count == 0) w.Add("N/A");
        if (o.Count == 0) o.Add("N/A");
        if (t.Count == 0) t.Add("N/A");

        return (s, w, o, t);
    }

    private static List<string> FallbackBullets(string content)
    {
        const int maxBulletLen = 100;
        const int maxBullets = 5;

        // 1. Try to extract existing list items (<li> tags) — best source for bullets
        var listItems = Regex.Matches(content, @"<li[^>]*>(.*?)</li>", RegexOptions.Singleline)
            .Select(m => CleanHtml(m.Groups[1].Value))
            .Where(s => s.Length > 5)
            .Select(s => TruncateBullet(s, maxBulletLen))
            .Take(maxBullets)
            .ToList();
        if (listItems.Count >= 2) return listItems;

        // 2. Try to extract bold/strong text as key points
        var boldItems = Regex.Matches(content, @"<(?:strong|b)>(.*?)</(?:strong|b)>", RegexOptions.Singleline)
            .Select(m => CleanHtml(m.Groups[1].Value))
            .Where(s => s.Length > 5 && s.Length < 200)
            .Select(s => TruncateBullet(s, maxBulletLen))
            .Distinct()
            .Take(maxBullets)
            .ToList();
        if (boldItems.Count >= 2) return boldItems;

        // 3. Try headings as key topics
        var headings = Regex.Matches(content, @"<h[1-6][^>]*>(.*?)</h[1-6]>", RegexOptions.Singleline)
            .Select(m => CleanHtml(m.Groups[1].Value))
            .Where(s => s.Length > 3)
            .Take(maxBullets)
            .ToList();
        if (headings.Count >= 2) return headings;

        // 4. Fall back to extracting key sentences, trimmed for slides
        var text = CleanHtml(content);
        var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
            .Select(s => s.Trim())
            .Where(s => s.Length > 15)
            .Select(s => TruncateBullet(s, maxBulletLen))
            .Take(maxBullets)
            .ToList();

        return sentences.Count > 0 ? sentences : new List<string> { TruncateBullet(text, maxBulletLen) };
    }

    private static string CleanHtml(string html)
    {
        var text = Regex.Replace(html, "<[^>]*>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static string TruncateBullet(string text, int maxLen)
    {
        if (text.Length <= maxLen) return text;
        // Cut at last word boundary before maxLen
        var cut = text.LastIndexOf(' ', maxLen - 3);
        return (cut > maxLen / 2 ? text[..cut] : text[..(maxLen - 3)]) + "...";
    }

    private List<(string Key, string Title, string Content)> GetSectionsWithContent(BusinessPlan bp, string language)
    {
        var sections = new List<(string Key, string Title, string Content)>();

        void Add(string key, string titleEn, string titleFr, string? content)
        {
            if (!string.IsNullOrWhiteSpace(content))
                sections.Add((key, language == "fr" ? titleFr : titleEn, content));
        }

        Add("ExecutiveSummary", "Executive Summary", "Sommaire exécutif", bp.ExecutiveSummary);
        Add("ProblemStatement", "Problem Statement", "Énoncé du problème", bp.ProblemStatement);
        Add("Solution", "Solution", "Solution", bp.Solution);
        Add("MarketAnalysis", "Market Analysis", "Analyse de marché", bp.MarketAnalysis);
        Add("CompetitiveAnalysis", "Competitive Analysis", "Analyse concurrentielle", bp.CompetitiveAnalysis);
        Add("SwotAnalysis", "SWOT Analysis", "Analyse SWOT", bp.SwotAnalysis);
        Add("BusinessModel", "Business Model", "Modèle d'affaires", bp.BusinessModel);
        Add("MarketingStrategy", "Marketing Strategy", "Stratégie marketing", bp.MarketingStrategy);
        Add("BrandingStrategy", "Branding Strategy", "Stratégie de marque", bp.BrandingStrategy);
        Add("OperationsPlan", "Operations Plan", "Plan des opérations", bp.OperationsPlan);
        Add("ManagementTeam", "Management Team", "Équipe de direction", bp.ManagementTeam);
        Add("FinancialProjections", "Financial Projections", "Projections financières", bp.FinancialProjections);
        Add("FundingRequirements", "Funding Requirements", "Besoins de financement", bp.FundingRequirements);
        Add("RiskAnalysis", "Risk Analysis", "Analyse des risques", bp.RiskAnalysis);
        Add("ExitStrategy", "Exit Strategy", "Stratégie de sortie", bp.ExitStrategy);

        return sections;
    }

    private async Task<BusinessPlan?> GetBusinessPlanWithValidationAsync(Guid businessPlanId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (!currentUserId.HasValue) return null;

        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
                .ThenInclude(o => o.Members)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, ct);

        if (businessPlan == null) return null;

        var hasAccess = businessPlan.Organization.Members
            .Any(m => m.UserId == currentUserId.Value && m.IsActive);

        return hasAccess ? businessPlan : null;
    }

    private Guid? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true) return null;
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
    }
}

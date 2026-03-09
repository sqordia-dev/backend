using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Sqordia.Application.Models.Export;
using Sqordia.Contracts.Requests.Export;

namespace Sqordia.Infrastructure.Services.DocumentExport;

/// <summary>
/// Builds a themed HTML document that mirrors the frontend TemplatePreviewModal layout.
/// The resulting HTML is designed for Puppeteer rendering to PDF with:
/// - CSS @page rules for Letter-size pages
/// - page-break-before/after for clean section breaks
/// - Print-friendly styling (no background clipping)
/// </summary>
public static class ThemedHtmlBuilder
{
    /// <summary>
    /// Build a complete HTML document with cover page, TOC, and sections.
    /// </summary>
    public static string Build(
        ExportTheme theme,
        List<SectionExportContent> sections,
        string planTitle,
        string companyName,
        string language,
        ExportCoverPageSettings? coverSettings = null)
    {
        var isFr = language == "fr";
        var safePlanTitle = WebUtility.HtmlEncode(planTitle);
        var safeCompanyName = WebUtility.HtmlEncode(companyName);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"" + language + "\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine($"<title>{safePlanTitle}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(BuildCss(theme));
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Cover page
        sb.AppendLine(BuildCoverPage(theme, safePlanTitle, safeCompanyName, coverSettings));

        // Table of contents
        sb.AppendLine(BuildToc(theme, sections, isFr));

        // Sections
        for (var i = 0; i < sections.Count; i++)
        {
            sb.AppendLine(BuildSection(theme, sections[i], i + 1, isFr));
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    // ── CSS ──────────────────────────────────────────────────

    private static string BuildCss(ExportTheme theme) => $@"
        @page {{
            size: letter;
            margin: 0;
        }}
        * {{ box-sizing: border-box; margin: 0; padding: 0; }}
        body {{
            font-family: system-ui, -apple-system, 'Segoe UI', Roboto, sans-serif;
            font-size: 14px;
            line-height: 1.7;
            color: {theme.TextColor};
            -webkit-print-color-adjust: exact;
            print-color-adjust: exact;
        }}
        .page {{
            width: 8.5in;
            min-height: 11in;
            padding: 0;
            page-break-after: always;
            position: relative;
        }}
        .page:last-child {{ page-break-after: auto; }}

        /* Cover page */
        .cover-band {{
            background: {theme.PrimaryColor};
            padding: 64px 40px 48px;
            min-height: 50%;
        }}
        .cover-title {{
            font-size: 32px;
            font-weight: 700;
            color: #FFFFFF;
            margin: 0 0 12px;
        }}
        .cover-subtitle {{
            font-size: 18px;
            color: #FFFFFF;
            opacity: 0.8;
            margin: 0;
        }}
        .cover-dots {{
            margin-top: 24px;
            display: flex;
            gap: 8px;
        }}
        .cover-dot {{
            display: inline-block;
            width: 12px;
            height: 12px;
            border-radius: 50%;
            opacity: 0.8;
        }}

        /* TOC */
        .toc-page {{
            background: {theme.TocBackgroundColor};
            padding: 40px;
        }}
        .toc-title {{
            font-size: 20px;
            font-weight: 700;
            margin: 0 0 16px;
            color: {theme.HeadingColor};
        }}
        .toc-entry {{
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 6px 0;
            border-bottom: 1px solid {theme.SeparatorColor};
        }}
        .toc-entry-title {{
            font-size: 14px;
            color: {theme.TextColor};
        }}
        .toc-entry-page {{
            font-size: 12px;
            color: {theme.MutedTextColor};
        }}

        /* Sections */
        .section-page {{
            background: {theme.PageBackgroundColor};
            padding: 40px;
        }}
        .section-header {{
            padding-bottom: 12px;
            margin-bottom: 16px;
            border-bottom: 2px solid {theme.AccentColor};
        }}
        .section-title {{
            font-size: 20px;
            font-weight: 700;
            color: {theme.HeadingColor};
            margin: 0;
        }}
        .section-empty {{
            font-style: italic;
            font-size: 14px;
            color: {theme.MutedTextColor};
        }}

        /* Prose content */
        .export-prose {{
            font-size: 14px;
            line-height: 1.7;
            color: {theme.TextColor};
        }}
        .export-prose h1 {{ font-size: 18px; font-weight: 700; color: {theme.Heading2Color}; margin: 16px 0 8px; }}
        .export-prose h2 {{ font-size: 16px; font-weight: 700; color: {theme.Heading2Color}; margin: 16px 0 8px; }}
        .export-prose h3 {{ font-size: 14px; font-weight: 600; color: {theme.Heading2Color}; margin: 12px 0 6px; }}
        .export-prose h4 {{ font-size: 14px; font-weight: 600; color: {theme.Heading2Color}; margin: 12px 0 6px; }}
        .export-prose p  {{ margin-bottom: 8px; }}
        .export-prose ul {{ padding-left: 20px; margin-bottom: 8px; }}
        .export-prose ol {{ padding-left: 20px; margin-bottom: 8px; }}
        .export-prose li {{ margin-bottom: 2px; }}
        .export-prose strong {{ font-weight: 600; }}
        .export-prose a {{ text-decoration: underline; color: {theme.AccentColor}; }}
        .export-prose table {{ border-collapse: collapse; width: 100%; margin: 12px 0; }}
        .export-prose th,
        .export-prose td {{ border: 1px solid {theme.SeparatorColor}; padding: 6px 10px; font-size: 13px; text-align: left; }}
        .export-prose th {{ background: {theme.TocBackgroundColor}; font-weight: 600; }}
        .export-prose blockquote {{
            border-left: 3px solid {theme.AccentColor};
            padding-left: 16px;
            margin: 12px 0;
            font-style: italic;
            color: {theme.MutedTextColor};
        }}
    ";

    // ── Cover Page ───────────────────────────────────────────

    private static string BuildCoverPage(
        ExportTheme theme,
        string safePlanTitle,
        string safeCompanyName,
        ExportCoverPageSettings? cover)
    {
        var dots = new StringBuilder();
        foreach (var color in theme.ChartColorPalette.Take(4))
        {
            dots.Append($"<span class=\"cover-dot\" style=\"background:{color}\"></span>");
        }

        var subtitle = cover?.Subtitle;
        var preparedBy = cover?.PreparedBy;

        return $@"
        <div class=""page"">
            <div class=""cover-band"">
                <h1 class=""cover-title"">{safePlanTitle}</h1>
                <p class=""cover-subtitle"">{safeCompanyName}</p>
                {(subtitle != null ? $"<p class=\"cover-subtitle\" style=\"font-size:14px;margin-top:8px;\">{WebUtility.HtmlEncode(subtitle)}</p>" : "")}
                {(preparedBy != null ? $"<p class=\"cover-subtitle\" style=\"font-size:13px;margin-top:16px;opacity:0.7;\">{WebUtility.HtmlEncode(preparedBy)}</p>" : "")}
                <div class=""cover-dots"">{dots}</div>
            </div>
        </div>";
    }

    // ── Table of Contents ────────────────────────────────────

    private static string BuildToc(ExportTheme theme, List<SectionExportContent> sections, bool isFr)
    {
        var tocLabel = isFr ? "Table des matières" : "Table of Contents";
        var pageLabel = isFr ? "Page" : "Page";

        var entries = new StringBuilder();
        for (var i = 0; i < sections.Count; i++)
        {
            var safeTitle = WebUtility.HtmlEncode(sections[i].Title);
            entries.AppendLine($@"
            <div class=""toc-entry"">
                <span class=""toc-entry-title"">{i + 1}. {safeTitle}</span>
                <span class=""toc-entry-page"">{pageLabel} {i + 2}</span>
            </div>");
        }

        return $@"
        <div class=""page toc-page"">
            <h2 class=""toc-title"">{tocLabel}</h2>
            {entries}
        </div>";
    }

    // ── Section ──────────────────────────────────────────────

    private static string BuildSection(ExportTheme theme, SectionExportContent section, int index, bool isFr)
    {
        var safeTitle = WebUtility.HtmlEncode(section.Title);
        var emptyLabel = isFr ? "Section vide" : "Empty section";

        string body;
        if (!string.IsNullOrWhiteSpace(section.Content))
        {
            var htmlContent = MarkdownToHtml(section.Content);
            body = $"<div class=\"export-prose\">{htmlContent}</div>";
        }
        else
        {
            body = $"<p class=\"section-empty\">{emptyLabel}</p>";
        }

        return $@"
        <div class=""page section-page"">
            <div class=""section-header"">
                <h2 class=""section-title"">{index}. {safeTitle}</h2>
            </div>
            {body}
        </div>";
    }

    // ── Markdown → HTML (simple converter matching frontend's markdownToHtmlForEditor) ──

    private static string MarkdownToHtml(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return "";

        var trimmed = content.Trim();
        // Already HTML: pass through
        if (trimmed.StartsWith('<') && content.Contains("</"))
            return content;

        var lines = content.Split('\n');
        var output = new StringBuilder();
        var inList = false;
        var listType = "";
        var listItems = new List<string>();

        void FlushList()
        {
            if (listItems.Count > 0 && !string.IsNullOrEmpty(listType))
            {
                output.AppendLine($"<{listType}>");
                foreach (var item in listItems)
                    output.AppendLine(item);
                output.AppendLine($"</{listType}>");
                listItems.Clear();
            }
            inList = false;
            listType = "";
        }

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine))
            {
                FlushList();
                continue;
            }

            // Headings
            var headingMatch = Regex.Match(trimmedLine, @"^(#{1,6})\s+(.+)$");
            if (headingMatch.Success)
            {
                FlushList();
                var level = Math.Min(headingMatch.Groups[1].Value.Length, 6);
                var text = ProcessInline(headingMatch.Groups[2].Value.Trim());
                output.AppendLine($"<h{level}>{text}</h{level}>");
                continue;
            }

            // Bullet list
            if (Regex.IsMatch(trimmedLine, @"^[-*]\s"))
            {
                if (!inList || listType != "ul")
                {
                    FlushList();
                    inList = true;
                    listType = "ul";
                }
                var text = Regex.Replace(trimmedLine, @"^[-*]\s+", "");
                listItems.Add($"<li>{ProcessInline(text)}</li>");
                continue;
            }

            // Numbered list
            if (Regex.IsMatch(trimmedLine, @"^\d+\.\s"))
            {
                if (!inList || listType != "ol")
                {
                    FlushList();
                    inList = true;
                    listType = "ol";
                }
                var text = Regex.Replace(trimmedLine, @"^\d+\.\s+", "");
                listItems.Add($"<li>{ProcessInline(text)}</li>");
                continue;
            }

            FlushList();
            output.AppendLine($"<p>{ProcessInline(trimmedLine)}</p>");
        }

        FlushList();
        return output.ToString();
    }

    private static string ProcessInline(string text)
    {
        // HTML-encode first to prevent injection, then apply markdown substitutions
        // We need to preserve markdown syntax chars, so encode selectively:
        // encode &, <, > but leave *, _, [, ], (, ) for markdown parsing
        text = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        // Links [text](url) — validate URL scheme to prevent javascript: injection
        text = Regex.Replace(text, @"\[([^\]]+)\]\((https?://[^)]+)\)", "<a href=\"$2\">$1</a>");
        // Bold **text** or __text__
        text = Regex.Replace(text, @"\*\*([^*]+)\*\*", "<strong>$1</strong>");
        text = Regex.Replace(text, @"__([^_]+)__", "<strong>$1</strong>");
        // Italic *text* or _text_
        text = Regex.Replace(text, @"\*([^*\n]+?)\*", "<em>$1</em>");
        text = Regex.Replace(text, @"_([^_\n]+?)_", "<em>$1</em>");
        return text;
    }
}

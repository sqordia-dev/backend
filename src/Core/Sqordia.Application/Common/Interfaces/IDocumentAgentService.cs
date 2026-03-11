using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// AI document agent that uses Claude tool_use to produce structured document blueprints.
/// Each format has specialized tools that generate structured blocks (headings, tables,
/// bullet lists, SWOT grids, financial tables, charts) instead of raw text.
///
/// The pipeline: Content → DocumentAgent → DocumentBlueprint → Renderer (Word/PDF/PPTX/Excel)
/// </summary>
public interface IDocumentAgentService
{
    /// <summary>
    /// Generate a structured Word document blueprint from business plan sections
    /// </summary>
    Task<Result<WordDocumentBlueprint>> GenerateWordBlueprintAsync(
        DocumentAgentRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generate a structured PDF document blueprint from business plan sections.
    /// PDF-specific: cover page, table of contents, header/footer, chart placeholders.
    /// </summary>
    Task<Result<PdfDocumentBlueprint>> GeneratePdfBlueprintAsync(
        DocumentAgentRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generate a structured PowerPoint blueprint from business plan sections
    /// </summary>
    Task<Result<PresentationBlueprint>> GeneratePresentationBlueprintAsync(
        DocumentAgentRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generate a structured Excel blueprint from business plan financial data
    /// </summary>
    Task<Result<SpreadsheetBlueprint>> GenerateSpreadsheetBlueprintAsync(
        DocumentAgentRequest request, CancellationToken ct = default);
}

// ── Request ──────────────────────────────────────────────

public class DocumentAgentRequest
{
    public Dictionary<string, string> Sections { get; set; } = new(); // sectionKey → content
    public string CompanyName { get; set; } = string.Empty;
    public string PlanTitle { get; set; } = string.Empty;
    public string Language { get; set; } = "fr";
}

// ── Word Blueprint ───────────────────────────────────────

public class WordDocumentBlueprint
{
    public List<WordBlock> Blocks { get; set; } = new();
}

public class WordBlock
{
    public string Type { get; set; } = string.Empty; // "heading", "paragraph", "bullet_list", "table", "swot_grid", "page_break", "callout"
    public int Level { get; set; } = 1; // heading level (1-3)
    public string? Text { get; set; }
    public List<string>? Items { get; set; } // for bullet_list
    public WordTableData? Table { get; set; }
    public SwotGridData? SwotGrid { get; set; }
    public string? Style { get; set; } // "highlight", "warning", "success"
}

public class WordTableData
{
    public List<string> Headers { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
    public string? Caption { get; set; }
}

public class SwotGridData
{
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<string> Opportunities { get; set; } = new();
    public List<string> Threats { get; set; } = new();
}

// ── PDF Blueprint ───────────────────────────────────────

public class PdfDocumentBlueprint
{
    public PdfCoverPage? CoverPage { get; set; }
    public PdfHeaderFooter? HeaderFooter { get; set; }
    public bool IncludeTableOfContents { get; set; } = true;
    public List<PdfBlock> Blocks { get; set; } = new();
}

public class PdfCoverPage
{
    public string CompanyName { get; set; } = string.Empty;
    public string DocumentTitle { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Date { get; set; }
    public string? PreparedBy { get; set; }
}

public class PdfHeaderFooter
{
    public string? HeaderLeft { get; set; }
    public string? HeaderCenter { get; set; }
    public string? HeaderRight { get; set; }
    public string? FooterLeft { get; set; }
    public string? FooterCenter { get; set; } // e.g. "Page {page} of {pages}"
    public string? FooterRight { get; set; }
}

public class PdfBlock
{
    public string Type { get; set; } = string.Empty; // "heading", "paragraph", "bullet_list", "table", "swot_grid", "metrics_panel", "chart_placeholder", "callout", "page_break"
    public int Level { get; set; } = 1; // heading level (1-3)
    public string? Text { get; set; }
    public List<string>? Items { get; set; } // for bullet_list
    public WordTableData? Table { get; set; }
    public SwotGridData? SwotGrid { get; set; }
    public List<MetricItem>? Metrics { get; set; } // for metrics_panel
    public PdfChartPlaceholder? Chart { get; set; }
    public string? Style { get; set; } // "highlight", "warning", "success"
}

public class PdfChartPlaceholder
{
    public string ChartType { get; set; } = string.Empty; // "bar", "line", "pie", "donut"
    public string Title { get; set; } = string.Empty;
    public List<string> Labels { get; set; } = new();
    public List<List<string>> DataSeries { get; set; } = new(); // series name + values
    public string? DataSourceHint { get; set; } // description of what data to visualize
}

// ── Presentation Blueprint ───────────────────────────────

public class PresentationBlueprint
{
    public List<SlideBlueprint> Slides { get; set; } = new();
}

public class SlideBlueprint
{
    public string Type { get; set; } = string.Empty; // "title", "content", "two_column", "swot", "table", "metrics", "image_placeholder", "section_divider", "thank_you"
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public List<string>? Bullets { get; set; }
    public string? LeftColumnTitle { get; set; }
    public List<string>? LeftColumnBullets { get; set; }
    public string? RightColumnTitle { get; set; }
    public List<string>? RightColumnBullets { get; set; }
    public SwotGridData? SwotGrid { get; set; }
    public WordTableData? Table { get; set; }
    public List<MetricItem>? Metrics { get; set; }
    public string? SpeakerNotes { get; set; }
}

public class MetricItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Trend { get; set; } // "up", "down", "flat"
}

// ── Spreadsheet Blueprint ────────────────────────────────

public class SpreadsheetBlueprint
{
    public List<SheetBlueprint> Sheets { get; set; } = new();
}

public class SheetBlueprint
{
    public string Name { get; set; } = string.Empty;
    public List<string> Headers { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
    public List<string>? SummaryRow { get; set; }
    public string? ChartType { get; set; } // "bar", "line", "pie" — hint for renderer
    public string? ChartTitle { get; set; }
}

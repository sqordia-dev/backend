using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Export;

/// <summary>
/// Request to export a business plan with visual elements included
/// </summary>
public class ExportWithVisualsRequest
{
    /// <summary>
    /// Export format: pdf, docx, or html
    /// </summary>
    [Required]
    [RegularExpression(@"^(pdf|docx|html)$", ErrorMessage = "Format must be 'pdf', 'docx', or 'html'")]
    public required string Format { get; set; }

    /// <summary>
    /// Export language (fr or en)
    /// </summary>
    [Required]
    [RegularExpression(@"^(fr|en)$", ErrorMessage = "Language must be 'fr' or 'en'")]
    public string Language { get; set; } = "fr";

    /// <summary>
    /// Whether to include visual elements (charts, tables, metrics)
    /// </summary>
    public bool IncludeVisuals { get; set; } = true;

    /// <summary>
    /// Whether to include table of contents
    /// </summary>
    public bool IncludeTableOfContents { get; set; } = true;

    /// <summary>
    /// Whether to include page numbers
    /// </summary>
    public bool IncludePageNumbers { get; set; } = true;

    /// <summary>
    /// Whether to include header and footer
    /// </summary>
    public bool IncludeHeaderFooter { get; set; } = true;

    /// <summary>
    /// Optional template ID to use
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Optional cover page settings override
    /// </summary>
    public ExportCoverPageSettings? CoverPageSettings { get; set; }

    /// <summary>
    /// Optional sections with visual elements to include
    /// If null, all sections will be included
    /// </summary>
    public List<SectionExportContent>? Sections { get; set; }

    /// <summary>
    /// Visual element rendering options
    /// </summary>
    public VisualElementOptions? VisualOptions { get; set; }
}

/// <summary>
/// Cover page settings for export
/// </summary>
public class ExportCoverPageSettings
{
    /// <summary>
    /// Company name to display on cover
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Document title
    /// </summary>
    public string? DocumentTitle { get; set; }

    /// <summary>
    /// Subtitle text
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// Primary brand color (hex format)
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Primary color must be a valid hex color (e.g., #2563EB)")]
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// URL to company logo
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Prepared for (recipient name/company)
    /// </summary>
    public string? PreparedFor { get; set; }

    /// <summary>
    /// Prepared by (author name)
    /// </summary>
    public string? PreparedBy { get; set; }

    /// <summary>
    /// Document date
    /// </summary>
    public DateTime? PreparedDate { get; set; }
}

/// <summary>
/// Section content with visual elements for export
/// </summary>
public class SectionExportContent
{
    /// <summary>
    /// Section identifier/key
    /// </summary>
    [Required]
    public required string SectionKey { get; set; }

    /// <summary>
    /// Section title for display
    /// </summary>
    [Required]
    public required string Title { get; set; }

    /// <summary>
    /// Section prose content (can contain HTML)
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Display order for the section
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Visual elements to include in this section
    /// </summary>
    public List<ExportVisualElement>? VisualElements { get; set; }
}

/// <summary>
/// Visual element for export matching frontend VisualElement type
/// </summary>
public class ExportVisualElement
{
    /// <summary>
    /// Unique identifier for the visual element
    /// </summary>
    [Required]
    public required string Id { get; set; }

    /// <summary>
    /// Type of visual element: table, chart, metric, or infographic
    /// </summary>
    [Required]
    [RegularExpression(@"^(table|chart|metric|infographic)$", ErrorMessage = "Type must be 'table', 'chart', 'metric', or 'infographic'")]
    public required string Type { get; set; }

    /// <summary>
    /// Optional title for the visual element
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Position: inline, full-width, float-left, or float-right
    /// </summary>
    [RegularExpression(@"^(inline|full-width|float-left|float-right)$")]
    public string Position { get; set; } = "inline";

    /// <summary>
    /// The actual data for the visual element (depends on Type)
    /// </summary>
    [Required]
    public required object Data { get; set; }

    /// <summary>
    /// Optional styling options
    /// </summary>
    public ExportVisualStyling? Styling { get; set; }
}

/// <summary>
/// Table data for export
/// </summary>
public class ExportTableData
{
    /// <summary>
    /// Table type: financial, swot, comparison, timeline, pricing, or custom
    /// </summary>
    [Required]
    public required string TableType { get; set; }

    /// <summary>
    /// Column headers
    /// </summary>
    [Required]
    public required List<string> Headers { get; set; }

    /// <summary>
    /// Table rows
    /// </summary>
    [Required]
    public required List<ExportTableRow> Rows { get; set; }

    /// <summary>
    /// Optional footer row
    /// </summary>
    public ExportTableRow? Footer { get; set; }

    /// <summary>
    /// Column data types: text, number, currency, percentage, date
    /// </summary>
    public List<string>? ColumnTypes { get; set; }
}

/// <summary>
/// Table row for export
/// </summary>
public class ExportTableRow
{
    /// <summary>
    /// Row cells
    /// </summary>
    [Required]
    public required List<ExportTableCell> Cells { get; set; }

    /// <summary>
    /// Whether this row should be highlighted
    /// </summary>
    public bool IsHighlighted { get; set; }
}

/// <summary>
/// Table cell for export
/// </summary>
public class ExportTableCell
{
    /// <summary>
    /// Cell value (string or number)
    /// </summary>
    [Required]
    public required object Value { get; set; }

    /// <summary>
    /// Cell format: bold, italic, or highlight
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Column span
    /// </summary>
    public int? Colspan { get; set; }

    /// <summary>
    /// Row span
    /// </summary>
    public int? Rowspan { get; set; }
}

/// <summary>
/// Chart data for export
/// </summary>
public class ExportChartData
{
    /// <summary>
    /// Chart type: line, bar, pie, donut, area, stacked-bar
    /// </summary>
    [Required]
    public required string ChartType { get; set; }

    /// <summary>
    /// X-axis labels
    /// </summary>
    [Required]
    public required List<string> Labels { get; set; }

    /// <summary>
    /// Data series
    /// </summary>
    [Required]
    public required List<ExportChartDataset> Datasets { get; set; }

    /// <summary>
    /// Chart options
    /// </summary>
    public ExportChartOptions? Options { get; set; }
}

/// <summary>
/// Chart dataset for export
/// </summary>
public class ExportChartDataset
{
    /// <summary>
    /// Dataset label
    /// </summary>
    [Required]
    public required string Label { get; set; }

    /// <summary>
    /// Data values
    /// </summary>
    [Required]
    public required List<decimal> Data { get; set; }

    /// <summary>
    /// Optional color for this dataset
    /// </summary>
    public string? Color { get; set; }
}

/// <summary>
/// Chart display options
/// </summary>
public class ExportChartOptions
{
    /// <summary>
    /// Whether to show legend
    /// </summary>
    public bool ShowLegend { get; set; } = true;

    /// <summary>
    /// Whether to show grid lines
    /// </summary>
    public bool ShowGrid { get; set; } = true;

    /// <summary>
    /// Currency code for formatting (e.g., USD, CAD)
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Whether values are percentages
    /// </summary>
    public bool PercentageFormat { get; set; }

    /// <summary>
    /// Whether to stack bars (for bar charts)
    /// </summary>
    public bool Stacked { get; set; }
}

/// <summary>
/// Metric data for export (metric cards)
/// </summary>
public class ExportMetricData
{
    /// <summary>
    /// List of metrics to display
    /// </summary>
    [Required]
    public required List<ExportMetric> Metrics { get; set; }

    /// <summary>
    /// Layout: grid, row, or column
    /// </summary>
    public string Layout { get; set; } = "grid";
}

/// <summary>
/// Individual metric for export
/// </summary>
public class ExportMetric
{
    /// <summary>
    /// Metric label
    /// </summary>
    [Required]
    public required string Label { get; set; }

    /// <summary>
    /// Metric value (string or number)
    /// </summary>
    [Required]
    public required object Value { get; set; }

    /// <summary>
    /// Format: currency, percentage, number, or text
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Trend direction: up, down, or neutral
    /// </summary>
    public string? Trend { get; set; }

    /// <summary>
    /// Trend value (e.g., "+15%")
    /// </summary>
    public string? TrendValue { get; set; }

    /// <summary>
    /// Optional icon identifier
    /// </summary>
    public string? Icon { get; set; }
}

/// <summary>
/// Infographic data for export
/// </summary>
public class ExportInfographicData
{
    /// <summary>
    /// Infographic type: process-flow, icon-list, quote, callout, timeline
    /// </summary>
    [Required]
    public required string InfographicType { get; set; }

    /// <summary>
    /// Infographic items
    /// </summary>
    [Required]
    public required List<ExportInfographicItem> Items { get; set; }
}

/// <summary>
/// Infographic item for export
/// </summary>
public class ExportInfographicItem
{
    /// <summary>
    /// Optional icon identifier
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Item title
    /// </summary>
    [Required]
    public required string Title { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order
    /// </summary>
    public int? Order { get; set; }
}

/// <summary>
/// Styling options for visual elements
/// </summary>
public class ExportVisualStyling
{
    /// <summary>
    /// Theme: default, minimal, corporate, or modern
    /// </summary>
    public string Theme { get; set; } = "default";

    /// <summary>
    /// Primary color override
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$")]
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// Font size: small, medium, or large
    /// </summary>
    public string? FontSize { get; set; }

    /// <summary>
    /// Border style: none, light, medium, or heavy
    /// </summary>
    public string? BorderStyle { get; set; }
}

/// <summary>
/// Options for rendering visual elements in export
/// </summary>
public class VisualElementOptions
{
    /// <summary>
    /// Default chart width in points (PDF) or pixels (HTML)
    /// </summary>
    public int ChartWidth { get; set; } = 500;

    /// <summary>
    /// Default chart height in points (PDF) or pixels (HTML)
    /// </summary>
    public int ChartHeight { get; set; } = 300;

    /// <summary>
    /// Image quality for chart rendering (0.0 to 1.0)
    /// </summary>
    public double ChartQuality { get; set; } = 0.9;

    /// <summary>
    /// Whether to render charts as images (for PDF) or interactive (for HTML)
    /// </summary>
    public bool RenderChartsAsImages { get; set; } = true;

    /// <summary>
    /// Default theme for visual elements
    /// </summary>
    public string DefaultTheme { get; set; } = "default";

    /// <summary>
    /// Whether to include visual element captions
    /// </summary>
    public bool IncludeCaptions { get; set; } = true;
}

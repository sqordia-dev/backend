using System.Text.Json;

namespace Sqordia.Contracts.Responses.Content;

/// <summary>
/// Response containing enhanced section content with visual elements
/// </summary>
public class EnhancedSectionContentResponse
{
    /// <summary>
    /// The section type that was generated
    /// </summary>
    public required string SectionType { get; set; }

    /// <summary>
    /// The content version
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// The structured content with prose and visual elements
    /// </summary>
    public required ContentDataDto Content { get; set; }

    /// <summary>
    /// Metadata about the generation process
    /// </summary>
    public required GenerationMetadataDto Metadata { get; set; }
}

/// <summary>
/// Container for structured content with prose and visual elements
/// </summary>
public class ContentDataDto
{
    /// <summary>
    /// Prose sections with placement markers
    /// </summary>
    public List<ProsePartDto> Prose { get; set; } = new();

    /// <summary>
    /// Visual elements to render (tables, charts, metrics)
    /// </summary>
    public List<VisualElementDto> VisualElements { get; set; } = new();

    /// <summary>
    /// Key metrics extracted from the content
    /// </summary>
    public List<MetricDto> KeyMetrics { get; set; } = new();

    /// <summary>
    /// Suggested highlights from the content
    /// </summary>
    public List<string> Highlights { get; set; } = new();
}

/// <summary>
/// A prose section within generated content
/// </summary>
public class ProsePartDto
{
    /// <summary>
    /// Unique identifier for this prose part
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The prose content (HTML or Markdown)
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Order position within the section
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// ID of visual element to place after this prose part
    /// </summary>
    public string? VisualAfter { get; set; }
}

/// <summary>
/// Represents a visual element (table, chart, metric, infographic)
/// </summary>
public class VisualElementDto
{
    /// <summary>
    /// Unique identifier for this visual element
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Type of visual element: table, chart, metric, infographic
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Optional title for the visual element
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The data for the visual element (structure depends on type)
    /// </summary>
    public required JsonElement Data { get; set; }

    /// <summary>
    /// Optional styling configuration
    /// </summary>
    public JsonElement? Styling { get; set; }

    /// <summary>
    /// Position: inline, full-width, float-left, float-right
    /// </summary>
    public string? Position { get; set; }
}

/// <summary>
/// A metric value for display in metric cards
/// </summary>
public class MetricDto
{
    /// <summary>
    /// Label for the metric
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// The value (can be numeric or string)
    /// </summary>
    public required object Value { get; set; }

    /// <summary>
    /// Format: currency, percentage, number, text
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Trend direction: up, down, neutral
    /// </summary>
    public string? Trend { get; set; }

    /// <summary>
    /// Trend value description (e.g., "+12% YoY")
    /// </summary>
    public string? TrendValue { get; set; }

    /// <summary>
    /// Optional icon name
    /// </summary>
    public string? Icon { get; set; }
}

/// <summary>
/// Metadata about the content generation process
/// </summary>
public class GenerationMetadataDto
{
    /// <summary>
    /// Version of the prompt template used
    /// </summary>
    public int PromptVersion { get; set; }

    /// <summary>
    /// Alias of the prompt (production, staging, development, experimental)
    /// </summary>
    public string PromptAlias { get; set; } = string.Empty;

    /// <summary>
    /// The AI model used for generation
    /// </summary>
    public string ModelUsed { get; set; } = string.Empty;

    /// <summary>
    /// Time taken to generate content in milliseconds
    /// </summary>
    public int GenerationTimeMs { get; set; }

    /// <summary>
    /// Number of tokens used in the generation
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    /// Whether the content includes visual elements
    /// </summary>
    public bool IncludesVisuals { get; set; }

    /// <summary>
    /// Timestamp when the content was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Table data structure matching frontend types
/// </summary>
public class TableDataDto
{
    /// <summary>
    /// Type of table: financial, swot, comparison, timeline, pricing, custom
    /// </summary>
    public required string TableType { get; set; }

    /// <summary>
    /// Column headers
    /// </summary>
    public required List<string> Headers { get; set; }

    /// <summary>
    /// Table rows
    /// </summary>
    public required List<TableRowDto> Rows { get; set; }

    /// <summary>
    /// Optional footer row
    /// </summary>
    public TableRowDto? Footer { get; set; }

    /// <summary>
    /// Column types: text, number, currency, percentage, date
    /// </summary>
    public List<string>? ColumnTypes { get; set; }
}

/// <summary>
/// A row in a table
/// </summary>
public class TableRowDto
{
    /// <summary>
    /// Cells in the row
    /// </summary>
    public required List<TableCellDto> Cells { get; set; }

    /// <summary>
    /// Whether this row should be highlighted
    /// </summary>
    public bool IsHighlighted { get; set; }
}

/// <summary>
/// A cell in a table
/// </summary>
public class TableCellDto
{
    /// <summary>
    /// Cell value
    /// </summary>
    public required object Value { get; set; }

    /// <summary>
    /// Format: bold, italic, highlight
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
/// Chart data structure matching frontend types
/// </summary>
public class ChartDataDto
{
    /// <summary>
    /// Chart type: line, bar, pie, donut, area, stacked-bar
    /// </summary>
    public required string ChartType { get; set; }

    /// <summary>
    /// X-axis labels
    /// </summary>
    public required List<string> Labels { get; set; }

    /// <summary>
    /// Data series
    /// </summary>
    public required List<ChartDatasetDto> Datasets { get; set; }

    /// <summary>
    /// Chart display options
    /// </summary>
    public ChartOptionsDto? Options { get; set; }

    /// <summary>
    /// Chart title
    /// </summary>
    public string? Title { get; set; }
}

/// <summary>
/// A dataset in a chart
/// </summary>
public class ChartDatasetDto
{
    /// <summary>
    /// Dataset label
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Data values
    /// </summary>
    public required List<double> Data { get; set; }

    /// <summary>
    /// Optional color for this dataset
    /// </summary>
    public string? Color { get; set; }
}

/// <summary>
/// Chart display options
/// </summary>
public class ChartOptionsDto
{
    /// <summary>
    /// Whether to show the legend
    /// </summary>
    public bool? ShowLegend { get; set; }

    /// <summary>
    /// Whether to show grid lines
    /// </summary>
    public bool? ShowGrid { get; set; }

    /// <summary>
    /// Currency code for currency formatting
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Whether to format values as percentages
    /// </summary>
    public bool? PercentageFormat { get; set; }

    /// <summary>
    /// Whether to stack bars/areas
    /// </summary>
    public bool? Stacked { get; set; }
}

/// <summary>
/// Metric card data structure matching frontend types
/// </summary>
public class MetricCardDataDto
{
    /// <summary>
    /// List of metrics to display
    /// </summary>
    public required List<MetricDto> Metrics { get; set; }

    /// <summary>
    /// Layout: grid, row, column
    /// </summary>
    public string Layout { get; set; } = "row";
}

/// <summary>
/// Infographic data structure matching frontend types
/// </summary>
public class InfographicDataDto
{
    /// <summary>
    /// Type: process-flow, icon-list, quote, callout, timeline
    /// </summary>
    public required string InfographicType { get; set; }

    /// <summary>
    /// Items in the infographic
    /// </summary>
    public required List<InfographicItemDto> Items { get; set; }
}

/// <summary>
/// An item in an infographic
/// </summary>
public class InfographicItemDto
{
    /// <summary>
    /// Optional icon name
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Item title
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Order position
    /// </summary>
    public int? Order { get; set; }
}

/// <summary>
/// Visual styling configuration
/// </summary>
public class VisualStylingDto
{
    /// <summary>
    /// Theme: default, minimal, corporate, modern
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// Primary color (hex)
    /// </summary>
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// Font size: small, medium, large
    /// </summary>
    public string? FontSize { get; set; }

    /// <summary>
    /// Border style: none, light, medium, heavy
    /// </summary>
    public string? BorderStyle { get; set; }
}

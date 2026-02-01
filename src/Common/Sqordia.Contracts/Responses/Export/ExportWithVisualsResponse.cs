namespace Sqordia.Contracts.Responses.Export;

/// <summary>
/// Response for export with visual elements
/// </summary>
public class ExportWithVisualsResponse
{
    /// <summary>
    /// The export file data as base64 encoded string
    /// </summary>
    public required byte[] FileData { get; set; }

    /// <summary>
    /// Suggested file name
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// MIME content type
    /// </summary>
    public required string ContentType { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public required long FileSizeBytes { get; set; }

    /// <summary>
    /// Export format used
    /// </summary>
    public required string Format { get; set; }

    /// <summary>
    /// Language used for export
    /// </summary>
    public required string Language { get; set; }

    /// <summary>
    /// Template used for export
    /// </summary>
    public string Template { get; set; } = "default";

    /// <summary>
    /// When the export was generated
    /// </summary>
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Statistics about the export
    /// </summary>
    public ExportStatistics? Statistics { get; set; }
}

/// <summary>
/// Statistics about the export
/// </summary>
public class ExportStatistics
{
    /// <summary>
    /// Total number of pages (if applicable)
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// Number of sections included
    /// </summary>
    public int SectionCount { get; set; }

    /// <summary>
    /// Number of visual elements included
    /// </summary>
    public int VisualElementCount { get; set; }

    /// <summary>
    /// Number of tables rendered
    /// </summary>
    public int TableCount { get; set; }

    /// <summary>
    /// Number of charts rendered
    /// </summary>
    public int ChartCount { get; set; }

    /// <summary>
    /// Number of metric cards rendered
    /// </summary>
    public int MetricCount { get; set; }

    /// <summary>
    /// Number of infographics rendered
    /// </summary>
    public int InfographicCount { get; set; }

    /// <summary>
    /// Total word count of prose content
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}

/// <summary>
/// Response for export status/preview
/// </summary>
public class ExportPreviewResponse
{
    /// <summary>
    /// Business plan ID
    /// </summary>
    public required Guid BusinessPlanId { get; set; }

    /// <summary>
    /// Whether the plan is ready for export
    /// </summary>
    public bool IsReadyForExport { get; set; }

    /// <summary>
    /// Number of completed sections
    /// </summary>
    public int CompletedSections { get; set; }

    /// <summary>
    /// Total number of sections
    /// </summary>
    public int TotalSections { get; set; }

    /// <summary>
    /// Completion percentage
    /// </summary>
    public double CompletionPercentage { get; set; }

    /// <summary>
    /// Available export formats
    /// </summary>
    public required List<string> AvailableFormats { get; set; }

    /// <summary>
    /// Supported languages
    /// </summary>
    public required List<string> SupportedLanguages { get; set; }

    /// <summary>
    /// Estimated page count for PDF
    /// </summary>
    public int? EstimatedPdfPages { get; set; }

    /// <summary>
    /// Total visual elements in the plan
    /// </summary>
    public int TotalVisualElements { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Sections available for export
    /// </summary>
    public List<ExportableSectionInfo>? Sections { get; set; }
}

/// <summary>
/// Information about a section available for export
/// </summary>
public class ExportableSectionInfo
{
    /// <summary>
    /// Section key/identifier
    /// </summary>
    public required string SectionKey { get; set; }

    /// <summary>
    /// Section title
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Whether the section has content
    /// </summary>
    public bool HasContent { get; set; }

    /// <summary>
    /// Word count of the section content
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// Number of visual elements in this section
    /// </summary>
    public int VisualElementCount { get; set; }

    /// <summary>
    /// Types of visual elements in this section
    /// </summary>
    public List<string> VisualElementTypes { get; set; } = new();
}

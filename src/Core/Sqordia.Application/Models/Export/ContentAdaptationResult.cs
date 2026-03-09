namespace Sqordia.Application.Models.Export;

/// <summary>
/// Result of AI content adaptation for a single section.
/// </summary>
public class ContentAdaptationResult
{
    public string SectionKey { get; set; } = string.Empty;
    public string AdaptedContent { get; set; } = string.Empty;
    public ExportFormatTarget TargetFormat { get; set; }
    public string Language { get; set; } = "en";
    public bool WasAiAdapted { get; set; }
    public int OriginalWordCount { get; set; }
    public int AdaptedWordCount { get; set; }
}

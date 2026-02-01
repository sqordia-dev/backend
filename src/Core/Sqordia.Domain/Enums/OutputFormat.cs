namespace Sqordia.Domain.Enums;

/// <summary>
/// Output format for AI-generated content
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Plain text or markdown prose
    /// </summary>
    Prose = 1,

    /// <summary>
    /// Structured table data
    /// </summary>
    Table = 2,

    /// <summary>
    /// Chart specification (JSON format for rendering)
    /// </summary>
    Chart = 3,

    /// <summary>
    /// Combination of prose and visual elements
    /// </summary>
    Mixed = 4,

    /// <summary>
    /// JSON structured output for programmatic use
    /// </summary>
    Structured = 5
}

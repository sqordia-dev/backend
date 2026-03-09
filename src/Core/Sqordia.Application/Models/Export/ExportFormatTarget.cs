namespace Sqordia.Application.Models.Export;

/// <summary>
/// Target export format for AI content adaptation.
/// </summary>
public enum ExportFormatTarget
{
    /// <summary>Full prose, passthrough (no AI adaptation needed).</summary>
    Pdf,
    /// <summary>Structured with headings and bullet points.</summary>
    Word,
    /// <summary>Condensed key points, 3-5 bullets per section.</summary>
    PowerPoint
}

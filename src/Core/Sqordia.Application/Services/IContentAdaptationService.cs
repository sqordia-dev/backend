using Sqordia.Application.Common.Models;
using Sqordia.Application.Models.Export;

namespace Sqordia.Application.Services;

/// <summary>
/// Adapts business plan section content for different export formats using AI.
/// PDF = passthrough, Word = structured headings/bullets, PowerPoint = condensed key points.
/// </summary>
public interface IContentAdaptationService
{
    /// <summary>
    /// Adapt a single section's content for the target format.
    /// </summary>
    Task<Result<ContentAdaptationResult>> AdaptContentAsync(
        string sectionKey,
        string sectionTitle,
        string content,
        ExportFormatTarget targetFormat,
        string language,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adapt all sections in parallel (up to 3 concurrent AI calls).
    /// </summary>
    Task<Result<List<ContentAdaptationResult>>> AdaptAllSectionsAsync(
        List<(string Key, string Title, string Content)> sections,
        ExportFormatTarget targetFormat,
        string language,
        CancellationToken cancellationToken = default);
}

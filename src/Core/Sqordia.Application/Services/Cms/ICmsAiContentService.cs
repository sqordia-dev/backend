using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Services.Cms;

/// <summary>
/// AI-powered content generation for CMS
/// </summary>
public interface ICmsAiContentService
{
    /// <summary>
    /// Generate content (non-streaming)
    /// </summary>
    Task<Result<CmsAiGenerationResult>> GenerateContentAsync(
        GenerateCmsContentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stream content generation for real-time preview
    /// </summary>
    IAsyncEnumerable<string> StreamContentAsync(
        GenerateCmsContentRequest request,
        CancellationToken cancellationToken = default);
}

public class GenerateCmsContentRequest
{
    public string Brief { get; set; } = string.Empty;
    public string BlockType { get; set; } = "text"; // text, richtext, heading
    public string Language { get; set; } = "en";
    public string? SectionContext { get; set; }
}

public class CmsAiGenerationResult
{
    public string Content { get; set; } = string.Empty;
    public string ModelUsed { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
}

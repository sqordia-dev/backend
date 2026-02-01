using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Content;
using Sqordia.Contracts.Responses.Content;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services;

/// <summary>
/// Service interface for enhanced AI content generation with visual elements
/// Integrates with the Prompt Repository System for optimized prompts and A/B testing
/// </summary>
public interface IEnhancedContentGenerationService
{
    /// <summary>
    /// Generates enhanced section content with prose and visual elements
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionType">The section type to generate</param>
    /// <param name="options">Generation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Structured content with prose and visual elements</returns>
    Task<Result<EnhancedSectionContentResponse>> GenerateSectionAsync(
        Guid businessPlanId,
        SectionType sectionType,
        GenerationOptionsDto? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Regenerates a section with enhanced content
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionType">The section type to regenerate</param>
    /// <param name="feedback">Optional feedback to incorporate</param>
    /// <param name="options">Generation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Regenerated structured content</returns>
    Task<Result<EnhancedSectionContentResponse>> RegenerateSectionAsync(
        Guid businessPlanId,
        SectionType sectionType,
        string? feedback = null,
        GenerationOptionsDto? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Improves existing section content using AI
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionType">The section type to improve</param>
    /// <param name="currentContent">The current content to improve</param>
    /// <param name="improvementType">Type of improvement to apply</param>
    /// <param name="customPrompt">Optional custom improvement prompt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Improved structured content</returns>
    Task<Result<EnhancedSectionContentResponse>> ImproveSectionAsync(
        Guid businessPlanId,
        SectionType sectionType,
        string currentContent,
        ImprovementType improvementType,
        string? customPrompt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records user feedback for A/B testing
    /// </summary>
    /// <param name="promptId">The prompt template ID that was used</param>
    /// <param name="usageType">Type of usage event</param>
    /// <param name="rating">Optional user rating (1-5)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordUsageFeedbackAsync(
        Guid promptId,
        UsageType usageType,
        int? rating = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the generation capabilities for a section type
    /// </summary>
    /// <param name="sectionType">The section type</param>
    /// <param name="planType">The plan type</param>
    /// <returns>Available visual element types and other capabilities</returns>
    SectionCapabilities GetSectionCapabilities(SectionType sectionType, BusinessPlanType planType);
}

/// <summary>
/// Types of content improvement
/// </summary>
public enum ImprovementType
{
    /// <summary>
    /// General enhancement and quality improvement
    /// </summary>
    Enhance = 1,

    /// <summary>
    /// Expand with more detail and depth
    /// </summary>
    Expand = 2,

    /// <summary>
    /// Simplify language and reduce complexity
    /// </summary>
    Simplify = 3,

    /// <summary>
    /// Make more professional and business-oriented
    /// </summary>
    Professionalize = 4,

    /// <summary>
    /// Add more data and statistics
    /// </summary>
    AddData = 5,

    /// <summary>
    /// Improve visual elements specifically
    /// </summary>
    EnhanceVisuals = 6
}

/// <summary>
/// Capabilities and recommendations for a section type
/// </summary>
public class SectionCapabilities
{
    /// <summary>
    /// The section type
    /// </summary>
    public SectionType SectionType { get; set; }

    /// <summary>
    /// Recommended visual element types for this section
    /// </summary>
    public List<string> RecommendedVisuals { get; set; } = new();

    /// <summary>
    /// Required visual element types for this section
    /// </summary>
    public List<string> RequiredVisuals { get; set; } = new();

    /// <summary>
    /// Optional visual element types for this section
    /// </summary>
    public List<string> OptionalVisuals { get; set; } = new();

    /// <summary>
    /// Whether structured output is supported
    /// </summary>
    public bool SupportsStructuredOutput { get; set; }

    /// <summary>
    /// Recommended content length (word count)
    /// </summary>
    public int RecommendedWordCount { get; set; }

    /// <summary>
    /// Default tone for the section
    /// </summary>
    public string DefaultTone { get; set; } = "professional";
}

using Sqordia.Application.Common.Models;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services;

/// <summary>
/// Service interface for selecting and building prompts
/// </summary>
public interface IPromptSelectorService
{
    /// <summary>
    /// Selects the best prompt template based on context
    /// Uses priority order: industry-specific, then generic, then fallback
    /// </summary>
    Task<Result<PromptTemplate>> SelectPromptAsync(
        PromptSelectionContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the final prompt with variable substitution
    /// </summary>
    string BuildPrompt(PromptTemplate template, Dictionary<string, string> variables);

    /// <summary>
    /// Gets a prompt by alias for A/B testing or staged rollouts
    /// </summary>
    Task<Result<PromptTemplate>> GetPromptByAliasAsync(
        PromptSelectionContext context,
        PromptAlias alias,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Context for prompt selection
/// </summary>
public class PromptSelectionContext
{
    /// <summary>
    /// The section type to generate content for
    /// </summary>
    public SectionType SectionType { get; set; }

    /// <summary>
    /// The type of business plan
    /// </summary>
    public BusinessPlanType PlanType { get; set; }

    /// <summary>
    /// Industry category (NAICS code) for industry-specific prompts
    /// </summary>
    public string? IndustryCategory { get; set; }

    /// <summary>
    /// Preferred language for content generation
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Whether to prefer high-performance prompts based on metrics
    /// </summary>
    public bool PreferHighPerformance { get; set; } = true;
}

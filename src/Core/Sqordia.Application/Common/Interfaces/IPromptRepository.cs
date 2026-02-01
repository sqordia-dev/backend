using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Repository interface for prompt template management
/// </summary>
public interface IPromptRepository
{
    /// <summary>
    /// Gets the active prompt template for a section and plan type
    /// </summary>
    Task<PromptTemplate?> GetActivePromptAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        string? industryCategory = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prompt template by its alias
    /// </summary>
    Task<PromptTemplate?> GetByAliasAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        PromptAlias alias,
        string? industryCategory = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific version of a prompt template
    /// </summary>
    Task<PromptTemplate?> GetByVersionAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        int version,
        string? industryCategory = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a prompt template by its ID
    /// </summary>
    Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all prompt templates for a section
    /// </summary>
    Task<IEnumerable<PromptTemplate>> GetAllForSectionAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new prompt template
    /// </summary>
    Task<PromptTemplate> CreateAsync(PromptTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing prompt template
    /// </summary>
    Task<PromptTemplate> UpdateAsync(PromptTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a prompt template (and deactivates others for the same section/plan type)
    /// </summary>
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets an alias for a prompt template
    /// </summary>
    Task SetAliasAsync(Guid id, PromptAlias alias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records usage of a prompt template
    /// </summary>
    Task RecordUsageAsync(Guid promptId, UsageType usageType, int? rating = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets performance metrics for a prompt template
    /// </summary>
    Task<IEnumerable<PromptPerformance>> GetPerformanceMetricsAsync(
        Guid promptId,
        DateTime? startDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Types of usage events for prompt templates
/// </summary>
public enum UsageType
{
    /// <summary>
    /// Content was generated using the prompt
    /// </summary>
    Generated = 1,

    /// <summary>
    /// User edited the generated content
    /// </summary>
    Edited = 2,

    /// <summary>
    /// User regenerated the content
    /// </summary>
    Regenerated = 3,

    /// <summary>
    /// User accepted the content without changes
    /// </summary>
    Accepted = 4,

    /// <summary>
    /// User rated the generated content
    /// </summary>
    Rated = 5
}

using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.PromptRegistry;
using Sqordia.Contracts.Responses.Admin.PromptRegistry;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services;

/// <summary>
/// Service interface for managing prompt templates in the admin registry
/// </summary>
public interface IPromptRegistryService
{
    #region CRUD Operations

    /// <summary>
    /// Gets a prompt template by its ID
    /// </summary>
    Task<Result<PromptTemplateDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all prompt templates with filtering and pagination
    /// </summary>
    Task<Result<PaginatedList<PromptTemplateListDto>>> GetAllAsync(PromptRegistryFilter filter, CancellationToken ct = default);

    /// <summary>
    /// Creates a new prompt template
    /// </summary>
    Task<Result<Guid>> CreateAsync(CreatePromptTemplateRequest request, string userId, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing prompt template
    /// </summary>
    Task<Result> UpdateAsync(Guid id, UpdatePromptTemplateRequest request, string userId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a prompt template
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Versioning

    /// <summary>
    /// Creates a new version of an existing prompt template
    /// </summary>
    Task<Result<Guid>> CreateNewVersionAsync(Guid sourceId, string userId, CancellationToken ct = default);

    /// <summary>
    /// Gets the version history for a section/plan type combination
    /// </summary>
    Task<Result<List<PromptVersionHistoryDto>>> GetVersionHistoryAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        string? industryCategory = null,
        CancellationToken ct = default);

    /// <summary>
    /// Rolls back to a specific version (activates it and deactivates current)
    /// </summary>
    Task<Result> RollbackToVersionAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Activation & Deployment

    /// <summary>
    /// Activates a prompt template (deactivates others for same section/plan)
    /// </summary>
    Task<Result> ActivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a prompt template
    /// </summary>
    Task<Result> DeactivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Sets the deployment alias for a prompt template
    /// </summary>
    Task<Result> SetAliasAsync(Guid id, PromptAlias? alias, CancellationToken ct = default);

    #endregion

    #region Testing

    /// <summary>
    /// Tests an existing prompt with sample data
    /// </summary>
    Task<Result<PromptTestResultDto>> TestPromptAsync(Guid promptId, TestPromptRequest request, CancellationToken ct = default);

    /// <summary>
    /// Tests a draft prompt (before saving) with sample data
    /// </summary>
    Task<Result<PromptTestResultDto>> TestDraftPromptAsync(TestDraftPromptRequest request, CancellationToken ct = default);

    #endregion

    #region Performance Metrics

    /// <summary>
    /// Gets detailed performance metrics for a prompt template
    /// </summary>
    Task<Result<PromptPerformanceDto>> GetPerformanceAsync(Guid promptId, DateTime? startDate = null, CancellationToken ct = default);

    /// <summary>
    /// Gets a summary of performance across all prompts
    /// </summary>
    Task<Result<PromptPerformanceSummaryDto>> GetPerformanceSummaryAsync(CancellationToken ct = default);

    #endregion
}

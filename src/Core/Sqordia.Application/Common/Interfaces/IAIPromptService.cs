using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Responses.Admin;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Service for managing AI prompt templates
/// </summary>
public interface IAIPromptService
{
    /// <summary>
    /// Create a new AI prompt template
    /// </summary>
    Task<string> CreatePromptAsync(CreateAIPromptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing AI prompt template
    /// </summary>
    Task<bool> UpdatePromptAsync(string promptId, UpdateAIPromptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an AI prompt template by ID
    /// </summary>
    Task<AIPromptDto?> GetPromptAsync(string promptId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all AI prompt templates with optional filtering
    /// </summary>
    Task<List<AIPromptDto>> GetPromptsAsync(
        string? category = null,
        string? planType = null,
        string? language = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an AI prompt template
    /// </summary>
    Task<bool> DeletePromptAsync(string promptId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test an AI prompt with sample data
    /// </summary>
    Task<AIPromptTestResult> TestPromptAsync(TestAIPromptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get prompt usage statistics
    /// </summary>
    Task<List<AIPromptStats>> GetPromptStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate or deactivate a prompt
    /// </summary>
    Task<bool> TogglePromptStatusAsync(string promptId, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new version of an existing prompt
    /// </summary>
    Task<string> CreatePromptVersionAsync(string parentPromptId, CreateAIPromptRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get prompt versions for a parent prompt
    /// </summary>
    Task<List<AIPromptDto>> GetPromptVersionsAsync(string parentPromptId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a prompt by section name, plan type, and language
    /// </summary>
    Task<AIPromptDto?> GetPromptBySectionAsync(
        string sectionName,
        string planType,
        string language,
        string category = "ContentGeneration",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the system prompt for a plan type and language
    /// </summary>
    Task<AIPromptDto?> GetSystemPromptAsync(
        string planType,
        string language,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get version history for a prompt
    /// </summary>
    Task<List<AIPromptVersionDto>> GetVersionHistoryAsync(
        string promptId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback a prompt to a previous version
    /// </summary>
    Task<bool> RollbackToVersionAsync(
        string promptId,
        int targetVersion,
        string? notes = null,
        string? changedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Test a draft prompt without saving to database
    /// </summary>
    Task<AIPromptTestResult> TestDraftPromptAsync(
        TestDraftAIPromptRequest request,
        CancellationToken cancellationToken = default);
}

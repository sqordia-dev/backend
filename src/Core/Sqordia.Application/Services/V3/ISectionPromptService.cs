using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.SectionPrompt;
using Sqordia.Contracts.Responses.Admin.SectionPrompt;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.V3;

/// <summary>
/// Service for managing section prompts with master/override hierarchy
/// </summary>
public interface ISectionPromptService
{
    // Query
    Task<Result<List<SectionPromptListResponse>>> GetPromptsAsync(SectionPromptFilterRequest? filter = null, CancellationToken cancellationToken = default);
    Task<Result<SectionPromptResponse>> GetPromptByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the effective prompt for a sub-section, resolving master/override hierarchy
    /// </summary>
    Task<Result<SectionPromptResponse>> GetEffectivePromptAsync(
        Guid subSectionId,
        BusinessPlanType planType,
        string language,
        string? industryCategory = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the master prompt for a main section
    /// </summary>
    Task<Result<SectionPromptResponse>> GetMasterPromptAsync(
        Guid mainSectionId,
        BusinessPlanType planType,
        string language,
        CancellationToken cancellationToken = default);

    // Commands
    Task<Result<Guid>> CreatePromptAsync(CreateSectionPromptRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdatePromptAsync(Guid id, UpdateSectionPromptRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeletePromptAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> ActivatePromptAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> DeactivatePromptAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Test a prompt with provided variables
    /// </summary>
    Task<Result<SectionPromptTestResponse>> TestPromptAsync(Guid id, TestSectionPromptRequest request, CancellationToken cancellationToken = default);

    // Version management
    Task<Result<List<SectionPromptListResponse>>> GetPromptVersionsAsync(Guid mainSectionId, Guid? subSectionId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> ClonePromptAsync(Guid id, CancellationToken cancellationToken = default);
}

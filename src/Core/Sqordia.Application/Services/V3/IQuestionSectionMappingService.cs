using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.QuestionMapping;
using Sqordia.Contracts.Responses.Admin.QuestionMapping;

namespace Sqordia.Application.Services.V3;

/// <summary>
/// Service for managing question-to-section mappings
/// </summary>
public interface IQuestionSectionMappingService
{
    // Query
    Task<Result<List<QuestionMappingResponse>>> GetMappingsAsync(
        QuestionMappingFilterRequest? filter = null,
        CancellationToken cancellationToken = default);

    Task<Result<QuestionMappingResponse>> GetMappingByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<List<QuestionMappingResponse>>> GetMappingsForQuestionAsync(
        Guid questionId,
        CancellationToken cancellationToken = default);

    Task<Result<List<QuestionMappingResponse>>> GetMappingsForSubSectionAsync(
        Guid subSectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the mapping matrix for admin UI (questions × sub-sections)
    /// </summary>
    Task<Result<MappingMatrixResponse>> GetMappingMatrixAsync(
        MappingMatrixRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<Result<MappingStatsResponse>> GetMappingStatsAsync(CancellationToken cancellationToken = default);

    // Commands
    Task<Result<Guid>> CreateMappingAsync(CreateQuestionMappingRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateMappingAsync(Guid id, UpdateQuestionMappingRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteMappingAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update mappings from matrix UI
    /// </summary>
    Task<Result<BulkUpdateResult>> BulkUpdateMappingsAsync(BulkUpdateMappingsRequest request, CancellationToken cancellationToken = default);

    // Utilities
    Task<Result<bool>> MappingExistsAsync(Guid questionId, Guid subSectionId, CancellationToken cancellationToken = default);
    Task<Result> ToggleMappingAsync(Guid questionId, Guid subSectionId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of bulk update operation
/// </summary>
public record BulkUpdateResult
{
    public int Created { get; init; }
    public int Updated { get; init; }
    public int Deleted { get; init; }
    public int Failed { get; init; }
    public List<string> Errors { get; init; } = new();
}

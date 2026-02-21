using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Admin;
using Sqordia.Contracts.Responses.Questionnaire;

namespace Sqordia.Application.Services.Questionnaire;

/// <summary>
/// Service for managing questionnaire versioning with draft/publish workflow
/// </summary>
public interface IQuestionnaireVersionService
{
    #region Version Management

    /// <summary>
    /// Gets the active draft version, if one exists
    /// </summary>
    Task<Result<QuestionnaireVersionDetailResponse?>> GetActiveDraftAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the currently published version
    /// </summary>
    Task<Result<QuestionnaireVersionDetailResponse>> GetPublishedVersionAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a specific version by ID
    /// </summary>
    Task<Result<QuestionnaireVersionDetailResponse>> GetVersionByIdAsync(Guid versionId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new draft version by cloning the published version
    /// </summary>
    Task<Result<QuestionnaireVersionDetailResponse>> CreateDraftAsync(string? notes = null, CancellationToken ct = default);

    /// <summary>
    /// Publishes a draft version, making it the live questionnaire content
    /// </summary>
    Task<Result<QuestionnaireVersionResponse>> PublishDraftAsync(Guid versionId, CancellationToken ct = default);

    /// <summary>
    /// Gets the version history (all versions)
    /// </summary>
    Task<Result<List<QuestionnaireVersionResponse>>> GetVersionHistoryAsync(CancellationToken ct = default);

    /// <summary>
    /// Discards (deletes) a draft version
    /// </summary>
    Task<Result> DiscardDraftAsync(Guid versionId, CancellationToken ct = default);

    /// <summary>
    /// Restores an archived version as a new draft
    /// </summary>
    Task<Result<QuestionnaireVersionDetailResponse>> RestoreVersionAsync(Guid versionId, CancellationToken ct = default);

    #endregion

    #region Draft Question Editing

    /// <summary>
    /// Creates a new question in the draft version
    /// </summary>
    Task<Result<QuestionTemplateDto>> CreateQuestionInDraftAsync(
        Guid versionId,
        CreateQuestionTemplateRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Updates a question in the draft version
    /// </summary>
    Task<Result<QuestionTemplateDto>> UpdateQuestionInDraftAsync(
        Guid versionId,
        Guid questionId,
        UpdateQuestionTemplateRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes a question from the draft version
    /// </summary>
    Task<Result> DeleteQuestionFromDraftAsync(
        Guid versionId,
        Guid questionId,
        CancellationToken ct = default);

    /// <summary>
    /// Reorders questions in the draft version
    /// </summary>
    Task<Result> ReorderQuestionsInDraftAsync(
        Guid versionId,
        ReorderQuestionsRequest request,
        CancellationToken ct = default);

    #endregion

    #region Draft Step Editing

    /// <summary>
    /// Updates a step configuration in the draft version
    /// </summary>
    Task<Result<QuestionnaireStepDto>> UpdateStepInDraftAsync(
        Guid versionId,
        int stepNumber,
        UpdateQuestionnaireStepRequest request,
        CancellationToken ct = default);

    #endregion
}

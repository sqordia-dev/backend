using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Admin;
using Sqordia.Contracts.Responses.Questionnaire;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Questionnaire;

public class QuestionnaireVersionService : IQuestionnaireVersionService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<QuestionnaireVersionService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public QuestionnaireVersionService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<QuestionnaireVersionService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    #region Version Management

    public async Task<Result<QuestionnaireVersionDetailResponse?>> GetActiveDraftAsync(CancellationToken ct = default)
    {
        try
        {
            var draftVersion = await _context.QuestionnaireVersions
                .FirstOrDefaultAsync(v => v.Status == QuestionnaireVersionStatus.Draft, ct);

            if (draftVersion == null)
            {
                return Result.Success<QuestionnaireVersionDetailResponse?>(null);
            }

            var createdByUserName = await GetUserNameAsync(draftVersion.CreatedByUserId, ct);
            return Result.Success<QuestionnaireVersionDetailResponse?>(MapToVersionDetailResponse(draftVersion, createdByUserName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active draft version");
            return Result.Failure<QuestionnaireVersionDetailResponse?>(
                Error.Failure("QuestionnaireVersion.GetDraft.Failed", "Failed to get active draft version."));
        }
    }

    public async Task<Result<QuestionnaireVersionDetailResponse>> GetPublishedVersionAsync(CancellationToken ct = default)
    {
        try
        {
            var publishedVersion = await _context.QuestionnaireVersions
                .FirstOrDefaultAsync(v => v.Status == QuestionnaireVersionStatus.Published, ct);

            if (publishedVersion == null)
            {
                // No published version exists, create initial version from live data
                var initialVersion = await CreateInitialVersionFromLiveDataAsync(ct);
                if (initialVersion.IsFailure)
                {
                    return Result.Failure<QuestionnaireVersionDetailResponse>(initialVersion.Error);
                }
                publishedVersion = initialVersion.Value;
            }

            var createdByUserName = await GetUserNameAsync(publishedVersion.CreatedByUserId, ct);
            var publishedByUserName = publishedVersion.PublishedByUserId.HasValue
                ? await GetUserNameAsync(publishedVersion.PublishedByUserId.Value, ct)
                : null;

            return Result.Success(MapToVersionDetailResponse(publishedVersion, createdByUserName, publishedByUserName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting published version");
            return Result.Failure<QuestionnaireVersionDetailResponse>(
                Error.Failure("QuestionnaireVersion.GetPublished.Failed", "Failed to get published version."));
        }
    }

    public async Task<Result<QuestionnaireVersionDetailResponse>> GetVersionByIdAsync(Guid versionId, CancellationToken ct = default)
    {
        try
        {
            var version = await _context.QuestionnaireVersions
                .FirstOrDefaultAsync(v => v.Id == versionId, ct);

            if (version == null)
            {
                return Result.Failure<QuestionnaireVersionDetailResponse>(
                    Error.NotFound("QuestionnaireVersion.NotFound", $"Version with ID '{versionId}' was not found."));
            }

            var createdByUserName = await GetUserNameAsync(version.CreatedByUserId, ct);
            var publishedByUserName = version.PublishedByUserId.HasValue
                ? await GetUserNameAsync(version.PublishedByUserId.Value, ct)
                : null;

            return Result.Success(MapToVersionDetailResponse(version, createdByUserName, publishedByUserName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version by ID: {VersionId}", versionId);
            return Result.Failure<QuestionnaireVersionDetailResponse>(
                Error.Failure("QuestionnaireVersion.GetById.Failed", "Failed to get version."));
        }
    }

    public async Task<Result<QuestionnaireVersionDetailResponse>> CreateDraftAsync(string? notes = null, CancellationToken ct = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Failure<QuestionnaireVersionDetailResponse>(
                Error.Unauthorized("QuestionnaireVersion.Unauthorized", "You must be authenticated to create a draft."));
        }

        try
        {
            var userId = Guid.Parse(_currentUserService.UserId!);

            // Check for existing draft
            var existingDraft = await _context.QuestionnaireVersions
                .AnyAsync(v => v.Status == QuestionnaireVersionStatus.Draft, ct);

            if (existingDraft)
            {
                return Result.Failure<QuestionnaireVersionDetailResponse>(
                    Error.Conflict("QuestionnaireVersion.DraftExists", "A draft version already exists. Discard it before creating a new one."));
            }

            // Get the published version to clone
            var publishedVersion = await _context.QuestionnaireVersions
                .FirstOrDefaultAsync(v => v.Status == QuestionnaireVersionStatus.Published, ct);

            string questionsSnapshot;
            string stepsSnapshot;

            if (publishedVersion != null)
            {
                questionsSnapshot = publishedVersion.QuestionsSnapshot;
                stepsSnapshot = publishedVersion.StepsSnapshot;
            }
            else
            {
                // No published version, create from live data
                var liveData = await GetLiveDataSnapshotsAsync(ct);
                questionsSnapshot = liveData.questionsSnapshot;
                stepsSnapshot = liveData.stepsSnapshot;
            }

            // Get next version number
            var maxVersionNumber = await _context.QuestionnaireVersions
                .MaxAsync(v => (int?)v.VersionNumber, ct) ?? 0;

            var newVersion = new QuestionnaireVersion(
                maxVersionNumber + 1,
                userId,
                questionsSnapshot,
                stepsSnapshot,
                notes);

            _context.QuestionnaireVersions.Add(newVersion);
            await _context.SaveChangesAsync(ct);

            var createdByUserName = await GetUserNameAsync(userId, ct);
            return Result.Success(MapToVersionDetailResponse(newVersion, createdByUserName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating draft version");
            return Result.Failure<QuestionnaireVersionDetailResponse>(
                Error.Failure("QuestionnaireVersion.CreateDraft.Failed", "Failed to create draft version."));
        }
    }

    public async Task<Result<QuestionnaireVersionResponse>> PublishDraftAsync(Guid versionId, CancellationToken ct = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Failure<QuestionnaireVersionResponse>(
                Error.Unauthorized("QuestionnaireVersion.Unauthorized", "You must be authenticated to publish."));
        }

        try
        {
            var userId = Guid.Parse(_currentUserService.UserId!);

            var draftVersion = await _context.QuestionnaireVersions
                .FirstOrDefaultAsync(v => v.Id == versionId, ct);

            if (draftVersion == null)
            {
                return Result.Failure<QuestionnaireVersionResponse>(
                    Error.NotFound("QuestionnaireVersion.NotFound", "Draft version not found."));
            }

            if (draftVersion.Status != QuestionnaireVersionStatus.Draft)
            {
                return Result.Failure<QuestionnaireVersionResponse>(
                    Error.Validation("QuestionnaireVersion.NotDraft", "Only draft versions can be published."));
            }

            // Archive the current published version
            var currentPublished = await _context.QuestionnaireVersions
                .FirstOrDefaultAsync(v => v.Status == QuestionnaireVersionStatus.Published, ct);

            if (currentPublished != null)
            {
                currentPublished.Archive();
            }

            // Publish the draft
            draftVersion.Publish(userId);

            // Update live tables from snapshot
            await ApplySnapshotToLiveTablesAsync(draftVersion, ct);

            await _context.SaveChangesAsync(ct);

            var createdByUserName = await GetUserNameAsync(draftVersion.CreatedByUserId, ct);
            var publishedByUserName = await GetUserNameAsync(userId, ct);

            return Result.Success(MapToVersionResponse(draftVersion, createdByUserName, publishedByUserName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing draft version: {VersionId}", versionId);
            return Result.Failure<QuestionnaireVersionResponse>(
                Error.Failure("QuestionnaireVersion.Publish.Failed", "Failed to publish version."));
        }
    }

    public async Task<Result<List<QuestionnaireVersionResponse>>> GetVersionHistoryAsync(CancellationToken ct = default)
    {
        try
        {
            var versions = await _context.QuestionnaireVersions
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync(ct);

            var userIds = versions
                .Select(v => v.CreatedByUserId)
                .Union(versions.Where(v => v.PublishedByUserId.HasValue).Select(v => v.PublishedByUserId!.Value))
                .Distinct()
                .ToList();

            var userNames = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.GetFullName(), ct);

            var responses = versions.Select(v =>
            {
                userNames.TryGetValue(v.CreatedByUserId, out var createdByName);
                string? publishedByName = null;
                if (v.PublishedByUserId.HasValue)
                {
                    userNames.TryGetValue(v.PublishedByUserId.Value, out publishedByName);
                }
                return MapToVersionResponse(v, createdByName, publishedByName);
            }).ToList();

            return Result.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version history");
            return Result.Failure<List<QuestionnaireVersionResponse>>(
                Error.Failure("QuestionnaireVersion.GetHistory.Failed", "Failed to get version history."));
        }
    }

    public async Task<Result> DiscardDraftAsync(Guid versionId, CancellationToken ct = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Failure(Error.Unauthorized("QuestionnaireVersion.Unauthorized", "You must be authenticated."));
        }

        try
        {
            var draftVersion = await _context.QuestionnaireVersions
                .FirstOrDefaultAsync(v => v.Id == versionId, ct);

            if (draftVersion == null)
            {
                return Result.Failure(Error.NotFound("QuestionnaireVersion.NotFound", "Version not found."));
            }

            if (draftVersion.Status != QuestionnaireVersionStatus.Draft)
            {
                return Result.Failure(Error.Validation("QuestionnaireVersion.NotDraft", "Only draft versions can be discarded."));
            }

            draftVersion.SoftDelete();
            await _context.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discarding draft: {VersionId}", versionId);
            return Result.Failure(Error.Failure("QuestionnaireVersion.Discard.Failed", "Failed to discard draft."));
        }
    }

    public async Task<Result<QuestionnaireVersionDetailResponse>> RestoreVersionAsync(Guid versionId, CancellationToken ct = default)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Failure<QuestionnaireVersionDetailResponse>(
                Error.Unauthorized("QuestionnaireVersion.Unauthorized", "You must be authenticated."));
        }

        try
        {
            var userId = Guid.Parse(_currentUserService.UserId!);

            // Check for existing draft
            var existingDraft = await _context.QuestionnaireVersions
                .AnyAsync(v => v.Status == QuestionnaireVersionStatus.Draft, ct);

            if (existingDraft)
            {
                return Result.Failure<QuestionnaireVersionDetailResponse>(
                    Error.Conflict("QuestionnaireVersion.DraftExists", "A draft already exists. Discard it before restoring."));
            }

            var archivedVersion = await _context.QuestionnaireVersions
                .FirstOrDefaultAsync(v => v.Id == versionId, ct);

            if (archivedVersion == null)
            {
                return Result.Failure<QuestionnaireVersionDetailResponse>(
                    Error.NotFound("QuestionnaireVersion.NotFound", "Version not found."));
            }

            // Get next version number
            var maxVersionNumber = await _context.QuestionnaireVersions
                .MaxAsync(v => (int?)v.VersionNumber, ct) ?? 0;

            // Create new draft from archived version
            var newDraft = new QuestionnaireVersion(
                maxVersionNumber + 1,
                userId,
                archivedVersion.QuestionsSnapshot,
                archivedVersion.StepsSnapshot,
                $"Restored from version {archivedVersion.VersionNumber}");

            _context.QuestionnaireVersions.Add(newDraft);
            await _context.SaveChangesAsync(ct);

            var createdByUserName = await GetUserNameAsync(userId, ct);
            return Result.Success(MapToVersionDetailResponse(newDraft, createdByUserName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring version: {VersionId}", versionId);
            return Result.Failure<QuestionnaireVersionDetailResponse>(
                Error.Failure("QuestionnaireVersion.Restore.Failed", "Failed to restore version."));
        }
    }

    #endregion

    #region Draft Question Editing

    public async Task<Result<QuestionTemplateDto>> CreateQuestionInDraftAsync(
        Guid versionId,
        CreateQuestionTemplateRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var draftVersion = await GetAndValidateDraftAsync(versionId, ct);
            if (draftVersion == null)
            {
                return Result.Failure<QuestionTemplateDto>(
                    Error.NotFound("QuestionnaireVersion.NotFound", "Draft version not found."));
            }

            var questions = DeserializeQuestions(draftVersion.QuestionsSnapshot);

            var newQuestion = new QuestionTemplateDto
            {
                Id = Guid.NewGuid(),
                QuestionText = request.QuestionText,
                QuestionTextEN = request.QuestionTextEN,
                HelpText = request.HelpText,
                HelpTextEN = request.HelpTextEN,
                QuestionType = request.QuestionType,
                StepNumber = request.StepNumber,
                PersonaType = request.PersonaType,
                Order = request.Order,
                IsRequired = request.IsRequired,
                Section = request.Section,
                Icon = request.Icon,
                Options = request.Options,
                OptionsEN = request.OptionsEN,
                ValidationRules = request.ValidationRules,
                ConditionalLogic = request.ConditionalLogic,
                IsActive = true,
                Created = DateTime.UtcNow
            };

            questions.Add(newQuestion);
            draftVersion.UpdateQuestionsSnapshot(SerializeQuestions(questions));
            await _context.SaveChangesAsync(ct);

            return Result.Success(newQuestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question in draft: {VersionId}", versionId);
            return Result.Failure<QuestionTemplateDto>(
                Error.Failure("QuestionnaireVersion.CreateQuestion.Failed", "Failed to create question."));
        }
    }

    public async Task<Result<QuestionTemplateDto>> UpdateQuestionInDraftAsync(
        Guid versionId,
        Guid questionId,
        UpdateQuestionTemplateRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var draftVersion = await GetAndValidateDraftAsync(versionId, ct);
            if (draftVersion == null)
            {
                return Result.Failure<QuestionTemplateDto>(
                    Error.NotFound("QuestionnaireVersion.NotFound", "Draft version not found."));
            }

            var questions = DeserializeQuestions(draftVersion.QuestionsSnapshot);
            var question = questions.FirstOrDefault(q => q.Id == questionId);

            if (question == null)
            {
                return Result.Failure<QuestionTemplateDto>(
                    Error.NotFound("QuestionnaireVersion.QuestionNotFound", "Question not found in draft."));
            }

            // Update fields if provided
            if (request.QuestionText != null) question.QuestionText = request.QuestionText;
            if (request.QuestionTextEN != null) question.QuestionTextEN = request.QuestionTextEN;
            if (request.HelpText != null) question.HelpText = request.HelpText;
            if (request.HelpTextEN != null) question.HelpTextEN = request.HelpTextEN;
            if (request.QuestionType != null) question.QuestionType = request.QuestionType;
            if (request.StepNumber.HasValue) question.StepNumber = request.StepNumber.Value;
            if (request.PersonaType != null) question.PersonaType = request.PersonaType;
            if (request.Order.HasValue) question.Order = request.Order.Value;
            if (request.IsRequired.HasValue) question.IsRequired = request.IsRequired.Value;
            if (request.Section != null) question.Section = request.Section;
            if (request.Icon != null) question.Icon = request.Icon;
            if (request.Options != null) question.Options = request.Options;
            if (request.OptionsEN != null) question.OptionsEN = request.OptionsEN;
            if (request.ValidationRules != null) question.ValidationRules = request.ValidationRules;
            if (request.ConditionalLogic != null) question.ConditionalLogic = request.ConditionalLogic;
            if (request.IsActive.HasValue) question.IsActive = request.IsActive.Value;

            question.LastModified = DateTime.UtcNow;

            draftVersion.UpdateQuestionsSnapshot(SerializeQuestions(questions));
            await _context.SaveChangesAsync(ct);

            return Result.Success(question);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question in draft: {VersionId}, {QuestionId}", versionId, questionId);
            return Result.Failure<QuestionTemplateDto>(
                Error.Failure("QuestionnaireVersion.UpdateQuestion.Failed", "Failed to update question."));
        }
    }

    public async Task<Result> DeleteQuestionFromDraftAsync(
        Guid versionId,
        Guid questionId,
        CancellationToken ct = default)
    {
        try
        {
            var draftVersion = await GetAndValidateDraftAsync(versionId, ct);
            if (draftVersion == null)
            {
                return Result.Failure(Error.NotFound("QuestionnaireVersion.NotFound", "Draft version not found."));
            }

            var questions = DeserializeQuestions(draftVersion.QuestionsSnapshot);
            var question = questions.FirstOrDefault(q => q.Id == questionId);

            if (question == null)
            {
                return Result.Failure(Error.NotFound("QuestionnaireVersion.QuestionNotFound", "Question not found."));
            }

            questions.Remove(question);
            draftVersion.UpdateQuestionsSnapshot(SerializeQuestions(questions));
            await _context.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question from draft: {VersionId}, {QuestionId}", versionId, questionId);
            return Result.Failure(Error.Failure("QuestionnaireVersion.DeleteQuestion.Failed", "Failed to delete question."));
        }
    }

    public async Task<Result> ReorderQuestionsInDraftAsync(
        Guid versionId,
        ReorderQuestionsRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var draftVersion = await GetAndValidateDraftAsync(versionId, ct);
            if (draftVersion == null)
            {
                return Result.Failure(Error.NotFound("QuestionnaireVersion.NotFound", "Draft version not found."));
            }

            var questions = DeserializeQuestions(draftVersion.QuestionsSnapshot);

            foreach (var item in request.Items)
            {
                var question = questions.FirstOrDefault(q => q.Id == item.QuestionId);
                if (question != null)
                {
                    question.Order = item.Order;
                }
            }

            draftVersion.UpdateQuestionsSnapshot(SerializeQuestions(questions));
            await _context.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering questions in draft: {VersionId}", versionId);
            return Result.Failure(Error.Failure("QuestionnaireVersion.ReorderQuestions.Failed", "Failed to reorder questions."));
        }
    }

    #endregion

    #region Draft Step Editing

    public async Task<Result<QuestionnaireStepDto>> UpdateStepInDraftAsync(
        Guid versionId,
        int stepNumber,
        UpdateQuestionnaireStepRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var draftVersion = await GetAndValidateDraftAsync(versionId, ct);
            if (draftVersion == null)
            {
                return Result.Failure<QuestionnaireStepDto>(
                    Error.NotFound("QuestionnaireVersion.NotFound", "Draft version not found."));
            }

            var steps = DeserializeSteps(draftVersion.StepsSnapshot);
            var step = steps.FirstOrDefault(s => s.StepNumber == stepNumber);

            if (step == null)
            {
                return Result.Failure<QuestionnaireStepDto>(
                    Error.NotFound("QuestionnaireVersion.StepNotFound", $"Step {stepNumber} not found in draft."));
            }

            // Update fields if provided
            if (request.TitleFR != null) step.TitleFR = request.TitleFR;
            if (request.TitleEN != null) step.TitleEN = request.TitleEN;
            if (request.DescriptionFR != null) step.DescriptionFR = request.DescriptionFR;
            if (request.DescriptionEN != null) step.DescriptionEN = request.DescriptionEN;

            draftVersion.UpdateStepsSnapshot(SerializeSteps(steps));
            await _context.SaveChangesAsync(ct);

            // Calculate question count for this step
            var questions = DeserializeQuestions(draftVersion.QuestionsSnapshot);
            step.QuestionCount = questions.Count(q => q.StepNumber == stepNumber);

            return Result.Success(step);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating step in draft: {VersionId}, Step {StepNumber}", versionId, stepNumber);
            return Result.Failure<QuestionnaireStepDto>(
                Error.Failure("QuestionnaireVersion.UpdateStep.Failed", "Failed to update step."));
        }
    }

    #endregion

    #region Private Helpers

    private async Task<QuestionnaireVersion?> GetAndValidateDraftAsync(Guid versionId, CancellationToken ct)
    {
        var version = await _context.QuestionnaireVersions
            .FirstOrDefaultAsync(v => v.Id == versionId && v.Status == QuestionnaireVersionStatus.Draft, ct);

        return version;
    }

    private async Task<string?> GetUserNameAsync(Guid userId, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user?.GetFullName();
    }

    private async Task<(string questionsSnapshot, string stepsSnapshot)> GetLiveDataSnapshotsAsync(CancellationToken ct)
    {
        // Get all active questions from QuestionTemplatesV2
        var questions = await _context.QuestionTemplatesV2
            .Where(q => q.IsActive)
            .OrderBy(q => q.StepNumber)
            .ThenBy(q => q.Order)
            .ToListAsync(ct);

        var questionDtos = questions.Select(q => new QuestionTemplateDto
        {
            Id = q.Id,
            QuestionText = q.QuestionText,
            QuestionTextEN = q.QuestionTextEN,
            HelpText = q.HelpText,
            HelpTextEN = q.HelpTextEN,
            QuestionType = q.QuestionType.ToString(),
            StepNumber = q.StepNumber,
            PersonaType = q.PersonaType?.ToString(),
            Order = q.Order,
            IsRequired = q.IsRequired,
            Section = q.Section,
            Icon = q.Icon,
            Options = q.Options,
            OptionsEN = q.OptionsEN,
            ValidationRules = q.ValidationRules,
            ConditionalLogic = q.ConditionalLogic,
            IsActive = q.IsActive,
            Created = q.Created,
            LastModified = q.LastModified
        }).ToList();

        // Get step configurations
        var steps = await _context.QuestionnaireSteps
            .Where(s => s.IsActive)
            .OrderBy(s => s.StepNumber)
            .ToListAsync(ct);

        var stepDtos = steps.Select(s => new QuestionnaireStepDto
        {
            Id = s.Id,
            StepNumber = s.StepNumber,
            TitleFR = s.TitleFR,
            TitleEN = s.TitleEN,
            DescriptionFR = s.DescriptionFR,
            DescriptionEN = s.DescriptionEN,
            Icon = s.Icon,
            IsActive = s.IsActive,
            QuestionCount = questionDtos.Count(q => q.StepNumber == s.StepNumber)
        }).ToList();

        return (SerializeQuestions(questionDtos), SerializeSteps(stepDtos));
    }

    private async Task<Result<QuestionnaireVersion>> CreateInitialVersionFromLiveDataAsync(CancellationToken ct)
    {
        try
        {
            var (questionsSnapshot, stepsSnapshot) = await GetLiveDataSnapshotsAsync(ct);

            // Use system user ID or a default admin ID
            var systemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var initialVersion = new QuestionnaireVersion(
                1,
                systemUserId,
                questionsSnapshot,
                stepsSnapshot,
                "Initial version created from existing data");

            initialVersion.Publish(systemUserId);

            _context.QuestionnaireVersions.Add(initialVersion);
            await _context.SaveChangesAsync(ct);

            return Result.Success(initialVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating initial version from live data");
            return Result.Failure<QuestionnaireVersion>(
                Error.Failure("QuestionnaireVersion.CreateInitial.Failed", "Failed to create initial version."));
        }
    }

    private async Task ApplySnapshotToLiveTablesAsync(QuestionnaireVersion version, CancellationToken ct)
    {
        var questions = DeserializeQuestions(version.QuestionsSnapshot);
        var steps = DeserializeSteps(version.StepsSnapshot);

        // Update QuestionnaireSteps table
        foreach (var stepDto in steps)
        {
            var existingStep = await _context.QuestionnaireSteps
                .FirstOrDefaultAsync(s => s.StepNumber == stepDto.StepNumber, ct);

            if (existingStep != null)
            {
                existingStep.Update(
                    stepDto.TitleFR,
                    stepDto.TitleEN,
                    stepDto.DescriptionFR,
                    stepDto.DescriptionEN,
                    stepDto.Icon);
            }
        }

        // Sync questions to QuestionTemplatesV2 table
        var existingQuestions = await _context.QuestionTemplatesV2.ToListAsync(ct);
        var snapshotQuestionIds = questions.Select(q => q.Id).ToHashSet();

        // Update or create questions from snapshot
        foreach (var questionDto in questions)
        {
            var existingQuestion = existingQuestions.FirstOrDefault(q => q.Id == questionDto.Id);

            if (existingQuestion != null)
            {
                // Update existing question
                var questionType = Enum.Parse<QuestionType>(questionDto.QuestionType);
                var personaType = string.IsNullOrEmpty(questionDto.PersonaType)
                    ? (PersonaType?)null
                    : Enum.Parse<PersonaType>(questionDto.PersonaType);

                existingQuestion.UpdateCoreFields(
                    questionDto.QuestionText,
                    questionType,
                    questionDto.StepNumber,
                    personaType,
                    questionDto.IsRequired);

                existingQuestion.SetEnglishText(questionDto.QuestionTextEN, questionDto.HelpTextEN, questionDto.OptionsEN);
                existingQuestion.SetHelpText(questionDto.HelpText, questionDto.HelpTextEN);
                existingQuestion.SetOptions(questionDto.Options, questionDto.OptionsEN);
                existingQuestion.SetValidationRules(questionDto.ValidationRules);
                existingQuestion.SetConditionalLogic(questionDto.ConditionalLogic);
                existingQuestion.SetIcon(questionDto.Icon);
                existingQuestion.UpdateOrder(questionDto.Order);
                existingQuestion.UpdateSection(questionDto.Section);

                if (questionDto.IsActive)
                    existingQuestion.Activate();
                else
                    existingQuestion.Deactivate();
            }
            else
            {
                // Create new question
                var questionType = Enum.Parse<QuestionType>(questionDto.QuestionType);
                var personaType = string.IsNullOrEmpty(questionDto.PersonaType)
                    ? (PersonaType?)null
                    : Enum.Parse<PersonaType>(questionDto.PersonaType);

                var newQuestion = new QuestionTemplateV2(
                    questionDto.StepNumber,
                    questionDto.QuestionText,
                    questionType,
                    questionDto.Order,
                    questionDto.IsRequired,
                    questionDto.Section,
                    personaType);

                newQuestion.SetEnglishText(questionDto.QuestionTextEN, questionDto.HelpTextEN, questionDto.OptionsEN);
                newQuestion.SetHelpText(questionDto.HelpText, questionDto.HelpTextEN);
                newQuestion.SetOptions(questionDto.Options, questionDto.OptionsEN);
                newQuestion.SetValidationRules(questionDto.ValidationRules);
                newQuestion.SetConditionalLogic(questionDto.ConditionalLogic);
                newQuestion.SetIcon(questionDto.Icon);

                if (!questionDto.IsActive)
                    newQuestion.Deactivate();

                _context.QuestionTemplatesV2.Add(newQuestion);
            }
        }

        // Deactivate questions that are not in the snapshot
        foreach (var existingQuestion in existingQuestions)
        {
            if (!snapshotQuestionIds.Contains(existingQuestion.Id))
            {
                existingQuestion.Deactivate();
            }
        }
    }

    private List<QuestionTemplateDto> DeserializeQuestions(string json)
    {
        return JsonSerializer.Deserialize<List<QuestionTemplateDto>>(json, JsonOptions) ?? new List<QuestionTemplateDto>();
    }

    private string SerializeQuestions(List<QuestionTemplateDto> questions)
    {
        return JsonSerializer.Serialize(questions, JsonOptions);
    }

    private List<QuestionnaireStepDto> DeserializeSteps(string json)
    {
        return JsonSerializer.Deserialize<List<QuestionnaireStepDto>>(json, JsonOptions) ?? new List<QuestionnaireStepDto>();
    }

    private string SerializeSteps(List<QuestionnaireStepDto> steps)
    {
        return JsonSerializer.Serialize(steps, JsonOptions);
    }

    private QuestionnaireVersionResponse MapToVersionResponse(
        QuestionnaireVersion version,
        string? createdByUserName,
        string? publishedByUserName = null)
    {
        var questions = DeserializeQuestions(version.QuestionsSnapshot);

        return new QuestionnaireVersionResponse
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            Status = version.Status.ToString(),
            Notes = version.Notes,
            CreatedByUserId = version.CreatedByUserId,
            CreatedByUserName = createdByUserName,
            CreatedAt = version.Created,
            PublishedAt = version.PublishedAt,
            PublishedByUserName = publishedByUserName,
            QuestionCount = questions.Count
        };
    }

    private QuestionnaireVersionDetailResponse MapToVersionDetailResponse(
        QuestionnaireVersion version,
        string? createdByUserName,
        string? publishedByUserName = null)
    {
        var questions = DeserializeQuestions(version.QuestionsSnapshot);
        var steps = DeserializeSteps(version.StepsSnapshot);

        // Calculate question counts per step
        foreach (var step in steps)
        {
            step.QuestionCount = questions.Count(q => q.StepNumber == step.StepNumber);
        }

        return new QuestionnaireVersionDetailResponse
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            Status = version.Status.ToString(),
            Notes = version.Notes,
            CreatedByUserId = version.CreatedByUserId,
            CreatedByUserName = createdByUserName,
            CreatedAt = version.Created,
            PublishedAt = version.PublishedAt,
            PublishedByUserName = publishedByUserName,
            QuestionCount = questions.Count,
            Questions = questions,
            Steps = steps
        };
    }

    #endregion
}

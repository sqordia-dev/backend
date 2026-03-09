using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.QuestionnaireV3;
using Sqordia.Contracts.Responses.Admin.QuestionnaireV3;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.V3.Implementations;

/// <summary>
/// Service implementation for V3 questionnaire with coach prompts
/// </summary>
public class QuestionnaireServiceV3 : IQuestionnaireServiceV3
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly ILogger<QuestionnaireServiceV3> _logger;

    public QuestionnaireServiceV3(
        IApplicationDbContext context,
        IAIService aiService,
        ILogger<QuestionnaireServiceV3> logger)
    {
        _context = context;
        _aiService = aiService;
        _logger = logger;
    }

    #region Query

    public async Task<Result<List<QuestionTemplateV3ListResponse>>> GetQuestionsAsync(
        QuestionTemplateV3FilterRequest? filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.QuestionTemplatesV3
                .Include(q => q.SectionMappings)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.PersonaType))
                {
                    if (Enum.TryParse<PersonaType>(filter.PersonaType, true, out var persona))
                    {
                        query = query.Where(q => q.PersonaType == null || q.PersonaType == persona);
                    }
                }

                if (filter.StepNumber.HasValue)
                {
                    query = query.Where(q => q.StepNumber == filter.StepNumber.Value);
                }

                if (!string.IsNullOrEmpty(filter.QuestionType))
                {
                    if (Enum.TryParse<QuestionType>(filter.QuestionType, true, out var questionType))
                    {
                        query = query.Where(q => q.QuestionType == questionType);
                    }
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(q => q.IsActive == filter.IsActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(q =>
                        q.QuestionTextFR.ToLower().Contains(search) ||
                        q.QuestionTextEN.ToLower().Contains(search));
                }
            }

            var questions = await query
                .OrderBy(q => q.StepNumber)
                .ThenBy(q => q.DisplayOrder)
                .Skip((filter?.Page - 1 ?? 0) * (filter?.PageSize ?? 50))
                .Take(filter?.PageSize ?? 50)
                .ToListAsync(cancellationToken);

            var response = questions.Select(MapToListResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting V3 questions");
            return Result.Failure<List<QuestionTemplateV3ListResponse>>(
                Error.InternalServerError("QuestionnaireV3.GetError", "Failed to retrieve questions"));
        }
    }

    public async Task<Result<QuestionTemplateV3Response>> GetQuestionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var question = await _context.QuestionTemplatesV3
                .Include(q => q.SectionMappings)
                    .ThenInclude(m => m.SubSection)
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

            if (question == null)
            {
                return Result.Failure<QuestionTemplateV3Response>(
                    Error.NotFound("QuestionnaireV3.NotFound", $"Question with ID {id} not found"));
            }

            return Result.Success(MapToResponse(question));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting V3 question {Id}", id);
            return Result.Failure<QuestionTemplateV3Response>(
                Error.InternalServerError("QuestionnaireV3.GetError", "Failed to retrieve question"));
        }
    }

    public async Task<Result<QuestionTemplateV3Response>> GetQuestionByNumberAsync(int questionNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var question = await _context.QuestionTemplatesV3
                .Include(q => q.SectionMappings)
                    .ThenInclude(m => m.SubSection)
                .FirstOrDefaultAsync(q => q.QuestionNumber == questionNumber, cancellationToken);

            if (question == null)
            {
                return Result.Failure<QuestionTemplateV3Response>(
                    Error.NotFound("QuestionnaireV3.NotFound", $"Question #{questionNumber} not found"));
            }

            return Result.Success(MapToResponse(question));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting V3 question #{Number}", questionNumber);
            return Result.Failure<QuestionTemplateV3Response>(
                Error.InternalServerError("QuestionnaireV3.GetError", "Failed to retrieve question"));
        }
    }

    public async Task<Result<List<QuestionTemplateV3ListResponse>>> GetQuestionsByStepAsync(
        int stepNumber,
        PersonaType? personaType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.QuestionTemplatesV3
                .Include(q => q.SectionMappings)
                .Where(q => q.StepNumber == stepNumber && q.IsActive);

            if (personaType.HasValue)
            {
                query = query.Where(q => q.PersonaType == null || q.PersonaType == personaType.Value);
            }

            var questions = await query
                .OrderBy(q => q.DisplayOrder)
                .ToListAsync(cancellationToken);

            var response = questions.Select(MapToListResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting V3 questions for step {StepNumber}", stepNumber);
            return Result.Failure<List<QuestionTemplateV3ListResponse>>(
                Error.InternalServerError("QuestionnaireV3.GetError", "Failed to retrieve questions"));
        }
    }

    public async Task<Result<List<QuestionTemplateV3ListResponse>>> GetQuestionsByPersonaAsync(
        PersonaType personaType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var questions = await _context.QuestionTemplatesV3
                .Include(q => q.SectionMappings)
                .Where(q => (q.PersonaType == null || q.PersonaType == personaType) && q.IsActive)
                .OrderBy(q => q.StepNumber)
                .ThenBy(q => q.DisplayOrder)
                .ToListAsync(cancellationToken);

            var response = questions.Select(MapToListResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting V3 questions for persona {Persona}", personaType);
            return Result.Failure<List<QuestionTemplateV3ListResponse>>(
                Error.InternalServerError("QuestionnaireV3.GetError", "Failed to retrieve questions"));
        }
    }

    #endregion

    #region Commands

    public async Task<Result<Guid>> CreateQuestionAsync(CreateQuestionTemplateV3Request request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for duplicate question number
            var exists = await _context.QuestionTemplatesV3.AnyAsync(
                q => q.QuestionNumber == request.QuestionNumber, cancellationToken);
            if (exists)
            {
                return Result.Failure<Guid>(
                    Error.Conflict("QuestionnaireV3.DuplicateNumber",
                        $"Question #{request.QuestionNumber} already exists"));
            }

            PersonaType? personaType = null;
            if (!string.IsNullOrEmpty(request.PersonaType) &&
                Enum.TryParse<PersonaType>(request.PersonaType, true, out var persona))
            {
                personaType = persona;
            }

            if (!Enum.TryParse<QuestionType>(request.QuestionType, true, out var questionType))
            {
                return Result.Failure<Guid>(
                    Error.Validation("QuestionnaireV3.InvalidType", "Invalid question type"));
            }

            var question = QuestionTemplateV3.Create(
                request.QuestionNumber,
                personaType,
                request.StepNumber,
                request.QuestionTextFR,
                request.QuestionTextEN,
                request.HelpTextFR,
                request.HelpTextEN,
                questionType,
                request.OptionsFR,
                request.OptionsEN,
                request.ValidationRules,
                request.ConditionalLogic,
                request.CoachPromptFR,
                request.CoachPromptEN,
                request.ExpertAdviceFR,
                request.ExpertAdviceEN,
                request.DisplayOrder,
                request.IsRequired,
                request.Icon);

            _context.QuestionTemplatesV3.Add(question);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created V3 question #{Number} with ID {Id}", request.QuestionNumber, question.Id);
            return Result.Success(question.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating V3 question #{Number}", request.QuestionNumber);
            return Result.Failure<Guid>(
                Error.InternalServerError("QuestionnaireV3.CreateError", "Failed to create question"));
        }
    }

    public async Task<Result> UpdateQuestionAsync(Guid id, UpdateQuestionTemplateV3Request request, CancellationToken cancellationToken = default)
    {
        try
        {
            var question = await _context.QuestionTemplatesV3.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
            if (question == null)
            {
                return Result.Failure(
                    Error.NotFound("QuestionnaireV3.NotFound", $"Question with ID {id} not found"));
            }

            if (!Enum.TryParse<QuestionType>(request.QuestionType, true, out var questionType))
            {
                return Result.Failure(
                    Error.Validation("QuestionnaireV3.InvalidType", "Invalid question type"));
            }

            question.Update(
                request.QuestionTextFR,
                request.QuestionTextEN,
                request.HelpTextFR,
                request.HelpTextEN,
                questionType,
                request.OptionsFR,
                request.OptionsEN,
                request.ValidationRules,
                request.ConditionalLogic,
                request.ExpertAdviceFR,
                request.ExpertAdviceEN,
                request.DisplayOrder,
                request.IsRequired,
                request.Icon);

            if (!request.IsActive && question.IsActive)
            {
                question.Deactivate();
            }
            else if (request.IsActive && !question.IsActive)
            {
                question.Activate();
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated V3 question {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating V3 question {Id}", id);
            return Result.Failure(
                Error.InternalServerError("QuestionnaireV3.UpdateError", "Failed to update question"));
        }
    }

    public async Task<Result> DeleteQuestionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var question = await _context.QuestionTemplatesV3.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
            if (question == null)
            {
                return Result.Failure(
                    Error.NotFound("QuestionnaireV3.NotFound", $"Question with ID {id} not found"));
            }

            // Soft delete
            question.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted (deactivated) V3 question {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting V3 question {Id}", id);
            return Result.Failure(
                Error.InternalServerError("QuestionnaireV3.DeleteError", "Failed to delete question"));
        }
    }

    public async Task<Result> ActivateQuestionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var question = await _context.QuestionTemplatesV3.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
            if (question == null)
            {
                return Result.Failure(
                    Error.NotFound("QuestionnaireV3.NotFound", $"Question with ID {id} not found"));
            }

            question.Activate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Activated V3 question {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating V3 question {Id}", id);
            return Result.Failure(
                Error.InternalServerError("QuestionnaireV3.ActivateError", "Failed to activate question"));
        }
    }

    public async Task<Result> DeactivateQuestionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var question = await _context.QuestionTemplatesV3.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
            if (question == null)
            {
                return Result.Failure(
                    Error.NotFound("QuestionnaireV3.NotFound", $"Question with ID {id} not found"));
            }

            question.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deactivated V3 question {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating V3 question {Id}", id);
            return Result.Failure(
                Error.InternalServerError("QuestionnaireV3.DeactivateError", "Failed to deactivate question"));
        }
    }

    #endregion

    #region Coach Prompts

    public async Task<Result> UpdateCoachPromptAsync(Guid id, UpdateCoachPromptRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var question = await _context.QuestionTemplatesV3.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
            if (question == null)
            {
                return Result.Failure(
                    Error.NotFound("QuestionnaireV3.NotFound", $"Question with ID {id} not found"));
            }

            question.UpdateCoachPrompt(request.CoachPromptFR, request.CoachPromptEN);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated coach prompt for V3 question {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coach prompt for V3 question {Id}", id);
            return Result.Failure(
                Error.InternalServerError("QuestionnaireV3.UpdateCoachPromptError", "Failed to update coach prompt"));
        }
    }

    public async Task<Result<CoachSuggestionResponse>> GetCoachSuggestionAsync(
        Guid questionId,
        GetCoachSuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var question = await _context.QuestionTemplatesV3.FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
            if (question == null)
            {
                return Result.Failure<CoachSuggestionResponse>(
                    Error.NotFound("QuestionnaireV3.NotFound", $"Question with ID {questionId} not found"));
            }

            var isEnglish = request.Language.Equals("en", StringComparison.OrdinalIgnoreCase);
            var coachPrompt = isEnglish ? question.CoachPromptEN : question.CoachPromptFR;
            var questionText = isEnglish ? question.QuestionTextEN : question.QuestionTextFR;

            if (string.IsNullOrEmpty(coachPrompt))
            {
                return Result.Success(new CoachSuggestionResponse
                {
                    Success = false,
                    ErrorMessage = "No coach prompt configured for this question"
                });
            }

            var systemPrompt = isEnglish
                ? "You are a helpful business plan coach. Help the user improve their answer."
                : "Vous êtes un coach en plan d'affaires. Aidez l'utilisateur à améliorer sa réponse.";

            var userPrompt = $"{coachPrompt}\n\nQuestion: {questionText}\n\nCurrent Answer: {request.CurrentAnswer}";

            var suggestion = await _aiService.GenerateContentAsync(
                systemPrompt,
                userPrompt,
                500,
                0.7f,
                cancellationToken);

            return Result.Success(new CoachSuggestionResponse
            {
                Success = true,
                Suggestion = suggestion,
                TokensUsed = 0 // Token count not available from this interface
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coach suggestion for V3 question {Id}", questionId);

            return Result.Success(new CoachSuggestionResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }

    #endregion

    #region Utilities

    public async Task<Result<int>> GetNextQuestionNumberAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var maxNumber = await _context.QuestionTemplatesV3
                .MaxAsync(q => (int?)q.QuestionNumber, cancellationToken) ?? 0;
            return Result.Success(maxNumber + 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next question number");
            return Result.Failure<int>(
                Error.InternalServerError("QuestionnaireV3.GetError", "Failed to get next question number"));
        }
    }

    public async Task<Result> ReorderQuestionsAsync(int stepNumber, ReorderQuestionsV3Request request, CancellationToken cancellationToken = default)
    {
        try
        {
            var questionIds = request.Items.Select(i => i.Id).ToList();
            var questions = await _context.QuestionTemplatesV3
                .Where(q => q.StepNumber == stepNumber && questionIds.Contains(q.Id))
                .ToListAsync(cancellationToken);

            foreach (var item in request.Items)
            {
                var question = questions.FirstOrDefault(q => q.Id == item.Id);
                question?.SetDisplayOrder(item.DisplayOrder);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reordered {Count} V3 questions in step {StepNumber}",
                request.Items.Count, stepNumber);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering V3 questions in step {StepNumber}", stepNumber);
            return Result.Failure(
                Error.InternalServerError("QuestionnaireV3.ReorderError", "Failed to reorder questions"));
        }
    }

    #endregion

    #region Mapping Helpers

    private static QuestionTemplateV3Response MapToResponse(QuestionTemplateV3 question)
    {
        return new QuestionTemplateV3Response
        {
            Id = question.Id,
            QuestionNumber = question.QuestionNumber,
            PersonaType = question.PersonaType?.ToString(),
            StepNumber = question.StepNumber,
            QuestionTextFR = question.QuestionTextFR,
            QuestionTextEN = question.QuestionTextEN,
            HelpTextFR = question.HelpTextFR,
            HelpTextEN = question.HelpTextEN,
            QuestionType = question.QuestionType.ToString(),
            OptionsFR = question.OptionsFR,
            OptionsEN = question.OptionsEN,
            ValidationRules = question.ValidationRules,
            ConditionalLogic = question.ConditionalLogic,
            CoachPromptFR = question.CoachPromptFR,
            CoachPromptEN = question.CoachPromptEN,
            ExpertAdviceFR = question.ExpertAdviceFR,
            ExpertAdviceEN = question.ExpertAdviceEN,
            DisplayOrder = question.DisplayOrder,
            IsRequired = question.IsRequired,
            IsActive = question.IsActive,
            Icon = question.Icon,
            Created = question.Created,
            LastModified = question.LastModified,
            SectionMappings = question.SectionMappings?
                .Where(m => m.IsActive)
                .Select(m => new QuestionMappingSummaryResponse
                {
                    MappingId = m.Id,
                    SubSectionId = m.SubSectionId,
                    SubSectionCode = m.SubSection?.Code ?? "",
                    SubSectionTitleFR = m.SubSection?.TitleFR ?? "",
                    SubSectionTitleEN = m.SubSection?.TitleEN ?? "",
                    MappingContext = m.MappingContext,
                    Weight = m.Weight
                })
                .ToList() ?? new List<QuestionMappingSummaryResponse>()
        };
    }

    private static QuestionTemplateV3ListResponse MapToListResponse(QuestionTemplateV3 question)
    {
        return new QuestionTemplateV3ListResponse
        {
            Id = question.Id,
            QuestionNumber = question.QuestionNumber,
            PersonaType = question.PersonaType?.ToString(),
            StepNumber = question.StepNumber,
            QuestionTextFR = question.QuestionTextFR,
            QuestionTextEN = question.QuestionTextEN,
            QuestionType = question.QuestionType.ToString(),
            IsRequired = question.IsRequired,
            IsActive = question.IsActive,
            DisplayOrder = question.DisplayOrder,
            MappingsCount = question.SectionMappings?.Count(m => m.IsActive) ?? 0
        };
    }

    #endregion
}

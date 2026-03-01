using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Responses.Admin;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

public class AdminQuestionTemplateService : IAdminQuestionTemplateService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AdminQuestionTemplateService> _logger;

    public AdminQuestionTemplateService(
        IApplicationDbContext context,
        ILogger<AdminQuestionTemplateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<QuestionTemplateDto>> GetAllQuestionsAsync(
        int? stepNumber = null,
        string? personaType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        // Use V3 questions (STRUCTURE FINALE with expert advice)
        var query = _context.QuestionTemplatesV3.AsQueryable();

        if (stepNumber.HasValue)
            query = query.Where(q => q.StepNumber == stepNumber.Value);

        if (!string.IsNullOrEmpty(personaType))
        {
            if (Enum.TryParse<PersonaType>(personaType, out var parsed))
                query = query.Where(q => q.PersonaType == parsed);
        }

        if (isActive.HasValue)
            query = query.Where(q => q.IsActive == isActive.Value);

        var questions = await query
            .OrderBy(q => q.StepNumber)
            .ThenBy(q => q.DisplayOrder)
            .ToListAsync(cancellationToken);

        return questions.Select(MapToDto).ToList();
    }

    public async Task<QuestionTemplateDto?> GetQuestionByIdAsync(
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var question = await _context.QuestionTemplatesV3
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        return question == null ? null : MapToDto(question);
    }

    public async Task<Guid> CreateQuestionAsync(
        CreateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<QuestionType>(request.QuestionType, out var questionType))
            throw new ArgumentException($"Invalid question type: {request.QuestionType}");

        PersonaType? persona = null;
        if (!string.IsNullOrEmpty(request.PersonaType))
        {
            if (!Enum.TryParse<PersonaType>(request.PersonaType, out var parsed))
                throw new ArgumentException($"Invalid persona type: {request.PersonaType}");
            persona = parsed;
        }

        // Calculate next question number
        var maxQuestionNumber = await _context.QuestionTemplatesV3
            .MaxAsync(q => (int?)q.QuestionNumber, cancellationToken) ?? 0;

        var entity = QuestionTemplateV3.Create(
            questionNumber: maxQuestionNumber + 1,
            personaType: persona,
            stepNumber: request.StepNumber,
            questionTextFR: request.QuestionText,
            questionTextEN: request.QuestionTextEN ?? request.QuestionText,
            helpTextFR: request.HelpText,
            helpTextEN: request.HelpTextEN,
            questionType: questionType,
            optionsFR: request.Options,
            optionsEN: request.OptionsEN,
            validationRules: request.ValidationRules,
            conditionalLogic: request.ConditionalLogic,
            coachPromptFR: request.CoachPromptFR,
            coachPromptEN: request.CoachPromptEN,
            expertAdviceFR: request.ExpertAdviceFR,
            expertAdviceEN: request.ExpertAdviceEN,
            displayOrder: request.Order,
            isRequired: request.IsRequired,
            icon: request.Icon);

        _context.QuestionTemplatesV3.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created question template {QuestionId} for step {StepNumber}", entity.Id, entity.StepNumber);
        return entity.Id;
    }

    public async Task<bool> UpdateQuestionAsync(
        Guid questionId,
        UpdateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuestionTemplatesV3
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (entity == null) return false;

        // Parse question type if provided
        var questionType = entity.QuestionType;
        if (request.QuestionType != null)
        {
            if (!Enum.TryParse<QuestionType>(request.QuestionType, out var parsedType))
                throw new ArgumentException($"Invalid question type: {request.QuestionType}");
            questionType = parsedType;
        }

        // Parse persona type if provided
        PersonaType? persona = entity.PersonaType;
        if (request.PersonaType != null)
        {
            if (request.PersonaType == "")
            {
                persona = null; // Clear persona (applicable to all)
            }
            else if (Enum.TryParse<PersonaType>(request.PersonaType, out var parsedPersona))
            {
                persona = parsedPersona;
            }
            else
            {
                throw new ArgumentException($"Invalid persona type: {request.PersonaType}");
            }
        }

        // Update core fields using V3 entity methods
        entity.Update(
            questionTextFR: request.QuestionText ?? entity.QuestionTextFR,
            questionTextEN: request.QuestionTextEN ?? entity.QuestionTextEN,
            helpTextFR: request.HelpText ?? entity.HelpTextFR,
            helpTextEN: request.HelpTextEN ?? entity.HelpTextEN,
            questionType: questionType,
            optionsFR: request.Options ?? entity.OptionsFR,
            optionsEN: request.OptionsEN ?? entity.OptionsEN,
            validationRules: request.ValidationRules ?? entity.ValidationRules,
            conditionalLogic: request.ConditionalLogic ?? entity.ConditionalLogic,
            expertAdviceFR: request.ExpertAdviceFR ?? entity.ExpertAdviceFR,
            expertAdviceEN: request.ExpertAdviceEN ?? entity.ExpertAdviceEN,
            displayOrder: request.Order ?? entity.DisplayOrder,
            isRequired: request.IsRequired ?? entity.IsRequired,
            icon: request.Icon ?? entity.Icon);

        // Update coach prompts if provided
        if (request.CoachPromptFR != null || request.CoachPromptEN != null)
        {
            entity.SetCoachPrompts(
                request.CoachPromptFR ?? entity.CoachPromptFR,
                request.CoachPromptEN ?? entity.CoachPromptEN);
        }

        // Update step number if provided
        if (request.StepNumber.HasValue)
            entity.UpdateStepNumber(request.StepNumber.Value);

        // Update persona type
        if (request.PersonaType != null)
            entity.SetPersonaType(persona);

        // Update active status
        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) entity.Activate();
            else entity.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Updated question template {QuestionId}", questionId);
        return true;
    }

    public async Task<bool> DeleteQuestionAsync(
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuestionTemplatesV3
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (entity == null) return false;

        entity.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated question template {QuestionId}", questionId);
        return true;
    }

    public async Task<bool> ReorderQuestionsAsync(
        ReorderQuestionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var ids = request.Items.Select(i => i.QuestionId).ToList();
        var entities = await _context.QuestionTemplatesV3
            .Where(q => ids.Contains(q.Id))
            .ToListAsync(cancellationToken);

        foreach (var item in request.Items)
        {
            var entity = entities.FirstOrDefault(e => e.Id == item.QuestionId);
            entity?.SetDisplayOrder(item.Order);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Reordered {Count} question templates", request.Items.Count);
        return true;
    }

    public async Task<bool> ToggleQuestionStatusAsync(
        Guid questionId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuestionTemplatesV3
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (entity == null) return false;

        if (isActive) entity.Activate();
        else entity.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Toggled question template {QuestionId} active={IsActive}", questionId, isActive);
        return true;
    }

    private static QuestionTemplateDto MapToDto(QuestionTemplateV3 entity)
    {
        return new QuestionTemplateDto
        {
            Id = entity.Id,
            PersonaType = entity.PersonaType?.ToString(),
            StepNumber = entity.StepNumber,
            QuestionText = entity.QuestionTextFR,
            QuestionTextEN = entity.QuestionTextEN,
            HelpText = entity.HelpTextFR,
            HelpTextEN = entity.HelpTextEN,
            QuestionType = entity.QuestionType.ToString(),
            Order = entity.DisplayOrder,
            IsRequired = entity.IsRequired,
            Section = entity.SectionGroup,
            Options = entity.OptionsFR,
            OptionsEN = entity.OptionsEN,
            ValidationRules = entity.ValidationRules,
            ConditionalLogic = entity.ConditionalLogic,
            Icon = entity.Icon,
            ExpertAdviceFR = entity.ExpertAdviceFR,
            ExpertAdviceEN = entity.ExpertAdviceEN,
            CoachPromptFR = entity.CoachPromptFR,
            CoachPromptEN = entity.CoachPromptEN,
            IsActive = entity.IsActive,
            Created = entity.Created,
            LastModified = entity.LastModified,
        };
    }
}

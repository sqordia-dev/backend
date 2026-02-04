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
        var query = _context.QuestionTemplatesV2.AsQueryable();

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
            .ThenBy(q => q.Order)
            .ToListAsync(cancellationToken);

        return questions.Select(MapToDto).ToList();
    }

    public async Task<QuestionTemplateDto?> GetQuestionByIdAsync(
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var question = await _context.QuestionTemplatesV2
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

        var entity = new QuestionTemplateV2(
            stepNumber: request.StepNumber,
            questionText: request.QuestionText,
            questionType: questionType,
            order: request.Order,
            isRequired: request.IsRequired,
            section: request.Section,
            personaType: persona);

        entity.SetEnglishText(request.QuestionTextEN, request.HelpTextEN, request.OptionsEN);
        entity.SetHelpText(request.HelpText, request.HelpTextEN);
        entity.SetOptions(request.Options, request.OptionsEN);
        entity.SetValidationRules(request.ValidationRules);
        entity.SetConditionalLogic(request.ConditionalLogic);
        entity.SetIcon(request.Icon);

        _context.QuestionTemplatesV2.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created question template {QuestionId} for step {StepNumber}", entity.Id, entity.StepNumber);
        return entity.Id;
    }

    public async Task<bool> UpdateQuestionAsync(
        Guid questionId,
        UpdateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuestionTemplatesV2
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (entity == null) return false;

        // Update core fields if any are provided
        var questionText = request.QuestionText ?? entity.QuestionText;
        var questionType = entity.QuestionType;
        var stepNumber = request.StepNumber ?? entity.StepNumber;
        var isRequired = request.IsRequired ?? entity.IsRequired;

        if (request.QuestionType != null)
        {
            if (!Enum.TryParse<QuestionType>(request.QuestionType, out var parsedType))
                throw new ArgumentException($"Invalid question type: {request.QuestionType}");
            questionType = parsedType;
        }

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

        entity.UpdateCoreFields(questionText, questionType, stepNumber, persona, isRequired);

        // Update optional fields
        if (request.QuestionTextEN != null || request.HelpTextEN != null || request.OptionsEN != null)
            entity.SetEnglishText(
                request.QuestionTextEN ?? entity.QuestionTextEN,
                request.HelpTextEN ?? entity.HelpTextEN,
                request.OptionsEN ?? entity.OptionsEN);

        if (request.HelpText != null)
            entity.SetHelpText(request.HelpText, request.HelpTextEN);

        if (request.Options != null || request.OptionsEN != null)
            entity.SetOptions(
                request.Options ?? entity.Options,
                request.OptionsEN ?? entity.OptionsEN);

        if (request.ValidationRules != null)
            entity.SetValidationRules(request.ValidationRules);

        if (request.ConditionalLogic != null)
            entity.SetConditionalLogic(request.ConditionalLogic);

        if (request.Icon != null)
            entity.SetIcon(request.Icon);

        if (request.Section != null)
            entity.UpdateSection(request.Section);

        if (request.Order.HasValue)
            entity.UpdateOrder(request.Order.Value);

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
        var entity = await _context.QuestionTemplatesV2
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
        var entities = await _context.QuestionTemplatesV2
            .Where(q => ids.Contains(q.Id))
            .ToListAsync(cancellationToken);

        foreach (var item in request.Items)
        {
            var entity = entities.FirstOrDefault(e => e.Id == item.QuestionId);
            entity?.UpdateOrder(item.Order);
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
        var entity = await _context.QuestionTemplatesV2
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (entity == null) return false;

        if (isActive) entity.Activate();
        else entity.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Toggled question template {QuestionId} active={IsActive}", questionId, isActive);
        return true;
    }

    private static QuestionTemplateDto MapToDto(QuestionTemplateV2 entity)
    {
        return new QuestionTemplateDto
        {
            Id = entity.Id,
            PersonaType = entity.PersonaType?.ToString(),
            StepNumber = entity.StepNumber,
            QuestionText = entity.QuestionText,
            QuestionTextEN = entity.QuestionTextEN,
            HelpText = entity.HelpText,
            HelpTextEN = entity.HelpTextEN,
            QuestionType = entity.QuestionType.ToString(),
            Order = entity.Order,
            IsRequired = entity.IsRequired,
            Section = entity.Section,
            Options = entity.Options,
            OptionsEN = entity.OptionsEN,
            ValidationRules = entity.ValidationRules,
            ConditionalLogic = entity.ConditionalLogic,
            Icon = entity.Icon,
            IsActive = entity.IsActive,
            Created = entity.Created,
            LastModified = entity.LastModified,
        };
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.V2.Questionnaire;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;
using System.Text.Json;

namespace Sqordia.Application.Services.V2.Implementations;

/// <summary>
/// V2 Questionnaire service with persona support
/// </summary>
public class QuestionnaireServiceV2 : IQuestionnaireServiceV2
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<QuestionnaireServiceV2> _logger;

    public QuestionnaireServiceV2(
        IApplicationDbContext context,
        ILocalizationService localizationService,
        ILogger<QuestionnaireServiceV2> logger)
    {
        _context = context;
        _localizationService = localizationService;
        _logger = logger;
    }

    public async Task<Result<QuestionnaireTemplateV2Response>> GetQuestionsByPersonaAsync(
        PersonaType? personaType,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting V2 questions for persona {Persona} in language {Language}",
                personaType?.ToString() ?? "All", language);

            var query = _context.QuestionTemplatesV2.AsQueryable();

            // Filter by persona: include universal questions (NULL PersonaType) plus persona-specific ones
            if (personaType.HasValue)
            {
                query = query.Where(q => (q.PersonaType == null || q.PersonaType == personaType.Value) && q.IsActive);
            }
            else
            {
                query = query.Where(q => q.IsActive);
            }

            var questions = await query
                .OrderBy(q => q.StepNumber)
                .ThenBy(q => q.Order)
                .ToListAsync(cancellationToken);

            if (!questions.Any())
            {
                return Result.Failure<QuestionnaireTemplateV2Response>(
                    Error.NotFound("Questionnaire.NoQuestions", "No questions found for the specified persona."));
            }

            var isEnglish = language.Equals("en", StringComparison.OrdinalIgnoreCase);

            // Group by step
            var steps = questions
                .GroupBy(q => q.StepNumber)
                .OrderBy(g => g.Key)
                .Select(g => new QuestionnaireStepResponse
                {
                    StepNumber = g.Key,
                    StepTitle = GetStepTitle(g.Key, isEnglish),
                    StepDescription = GetStepDescription(g.Key, isEnglish),
                    Questions = g.Select(q => MapToResponse(q, isEnglish)).ToList(),
                    TotalQuestions = g.Count()
                })
                .ToList();

            var response = new QuestionnaireTemplateV2Response
            {
                PersonaType = personaType?.ToString(),
                Steps = steps,
                TotalSteps = steps.Count,
                TotalQuestions = questions.Count
            };

            _logger.LogInformation("Retrieved {QuestionCount} questions in {StepCount} steps",
                questions.Count, steps.Count);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting V2 questions for persona {Persona}", personaType);
            return Result.Failure<QuestionnaireTemplateV2Response>(
                Error.InternalServerError("Questionnaire.GetError", "An error occurred while retrieving questions."));
        }
    }

    public async Task<Result<QuestionnaireStepResponse>> GetStepQuestionsAsync(
        int stepNumber,
        PersonaType? personaType = null,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting V2 questions for step {Step}, persona {Persona}",
                stepNumber, personaType?.ToString() ?? "All");

            var query = _context.QuestionTemplatesV2
                .Where(q => q.StepNumber == stepNumber && q.IsActive);

            // Include universal questions (NULL PersonaType) plus persona-specific ones
            if (personaType.HasValue)
            {
                query = query.Where(q => q.PersonaType == null || q.PersonaType == personaType.Value);
            }

            var questions = await query
                .OrderBy(q => q.Order)
                .ToListAsync(cancellationToken);

            if (!questions.Any())
            {
                return Result.Failure<QuestionnaireStepResponse>(
                    Error.NotFound("Step.NotFound", $"No questions found for step {stepNumber}."));
            }

            var isEnglish = language.Equals("en", StringComparison.OrdinalIgnoreCase);

            var response = new QuestionnaireStepResponse
            {
                StepNumber = stepNumber,
                StepTitle = GetStepTitle(stepNumber, isEnglish),
                StepDescription = GetStepDescription(stepNumber, isEnglish),
                Questions = questions.Select(q => MapToResponse(q, isEnglish)).ToList(),
                TotalQuestions = questions.Count
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting V2 questions for step {Step}", stepNumber);
            return Result.Failure<QuestionnaireStepResponse>(
                Error.InternalServerError("Step.GetError", "An error occurred while retrieving step questions."));
        }
    }

    public async Task<Result<PersonaQuestionResponse>> GetQuestionByIdAsync(
        Guid questionId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting V2 question {QuestionId}", questionId);

            var question = await _context.QuestionTemplatesV2
                .FirstOrDefaultAsync(q => q.Id == questionId && q.IsActive, cancellationToken);

            if (question == null)
            {
                return Result.Failure<PersonaQuestionResponse>(
                    Error.NotFound("Question.NotFound", "Question not found."));
            }

            var isEnglish = language.Equals("en", StringComparison.OrdinalIgnoreCase);
            var response = MapToResponse(question, isEnglish);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting V2 question {QuestionId}", questionId);
            return Result.Failure<PersonaQuestionResponse>(
                Error.InternalServerError("Question.GetError", "An error occurred while retrieving the question."));
        }
    }

    private static PersonaQuestionResponse MapToResponse(QuestionTemplateV2 question, bool isEnglish)
    {
        var questionText = isEnglish && !string.IsNullOrWhiteSpace(question.QuestionTextEN)
            ? question.QuestionTextEN
            : question.QuestionText;

        var helpText = isEnglish && !string.IsNullOrWhiteSpace(question.HelpTextEN)
            ? question.HelpTextEN
            : question.HelpText;

        List<string>? options = null;
        List<string>? optionsEN = null;

        if (!string.IsNullOrWhiteSpace(question.Options))
        {
            try
            {
                options = JsonSerializer.Deserialize<List<string>>(question.Options);
            }
            catch { }
        }

        if (!string.IsNullOrWhiteSpace(question.OptionsEN))
        {
            try
            {
                optionsEN = JsonSerializer.Deserialize<List<string>>(question.OptionsEN);
            }
            catch { }
        }

        return new PersonaQuestionResponse
        {
            Id = question.Id,
            PersonaType = question.PersonaType.ToString(),
            StepNumber = question.StepNumber,
            QuestionText = questionText,
            QuestionTextEN = question.QuestionTextEN,
            HelpText = helpText,
            HelpTextEN = question.HelpTextEN,
            QuestionType = question.QuestionType.ToString(),
            Order = question.Order,
            IsRequired = question.IsRequired,
            Section = question.Section,
            Options = isEnglish && optionsEN != null ? optionsEN : options,
            OptionsEN = optionsEN,
            Icon = question.Icon
        };
    }

    private static string GetStepTitle(int stepNumber, bool isEnglish)
    {
        return stepNumber switch
        {
            1 => isEnglish ? "Vision & Mission" : "Vision et mission",
            2 => isEnglish ? "Market & Customers" : "Marché et clients",
            3 => isEnglish ? "Products & Services" : "Produits et services",
            4 => isEnglish ? "Strategy & Operations" : "Stratégie et opérations",
            5 => isEnglish ? "Financials & Growth" : "Finances et croissance",
            _ => isEnglish ? $"Step {stepNumber}" : $"Étape {stepNumber}"
        };
    }

    private static string? GetStepDescription(int stepNumber, bool isEnglish)
    {
        return stepNumber switch
        {
            1 => isEnglish
                ? "Define your business purpose and long-term goals"
                : "Définissez votre raison d'être et vos objectifs à long terme",
            2 => isEnglish
                ? "Understand your market and target customers"
                : "Comprenez votre marché et vos clients cibles",
            3 => isEnglish
                ? "Describe what you offer and your value proposition"
                : "Décrivez ce que vous offrez et votre proposition de valeur",
            4 => isEnglish
                ? "Outline your competitive strategy and operations"
                : "Décrivez votre stratégie concurrentielle et vos opérations",
            5 => isEnglish
                ? "Plan your financial future and growth trajectory"
                : "Planifiez votre avenir financier et votre trajectoire de croissance",
            _ => null
        };
    }
}

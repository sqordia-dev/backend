using System.Diagnostics;
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
    private readonly IAIProviderFactory _aiProviderFactory;

    public AdminQuestionTemplateService(
        IApplicationDbContext context,
        ILogger<AdminQuestionTemplateService> logger,
        IAIProviderFactory aiProviderFactory)
    {
        _context = context;
        _logger = logger;
        _aiProviderFactory = aiProviderFactory;
    }

    public async Task<List<QuestionTemplateDto>> GetAllQuestionsAsync(
        int? stepNumber = null,
        string? personaType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        // Use V3 questions (STRUCTURE FINALE with expert advice)
        var query = _context.QuestionTemplates.AsQueryable();

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
        var question = await _context.QuestionTemplates
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
        var maxQuestionNumber = await _context.QuestionTemplates
            .MaxAsync(q => (int?)q.QuestionNumber, cancellationToken) ?? 0;

        var entity = QuestionTemplate.Create(
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

        _context.QuestionTemplates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created question template {QuestionId} for step {StepNumber}", entity.Id, entity.StepNumber);
        return entity.Id;
    }

    public async Task<bool> UpdateQuestionAsync(
        Guid questionId,
        UpdateQuestionTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuestionTemplates
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
        var entity = await _context.QuestionTemplates
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
        var entities = await _context.QuestionTemplates
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
        var entity = await _context.QuestionTemplates
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (entity == null) return false;

        if (isActive) entity.Activate();
        else entity.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Toggled question template {QuestionId} active={IsActive}", questionId, isActive);
        return true;
    }

    public async Task<TestCoachPromptResponse?> TestCoachPromptAsync(
        Guid questionId,
        TestCoachPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.QuestionTemplates
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Question template {QuestionId} not found for coach prompt test", questionId);
            return null;
        }

        // Get the appropriate coach prompt based on language
        var coachPrompt = request.Language == "en"
            ? entity.CoachPromptEN ?? entity.CoachPromptFR
            : entity.CoachPromptFR ?? entity.CoachPromptEN;

        if (string.IsNullOrWhiteSpace(coachPrompt))
        {
            _logger.LogWarning("Question template {QuestionId} has no coach prompt for language {Language}",
                questionId, request.Language);
            return new TestCoachPromptResponse
            {
                Output = request.Language == "en"
                    ? "This question has no coach prompt configured."
                    : "Cette question n'a pas de prompt de coach configuré.",
                TokensUsed = 0,
                ResponseTimeMs = 0,
                Provider = "None",
                Model = "N/A"
            };
        }

        // Get the question text for context
        var questionText = request.Language == "en"
            ? entity.QuestionTextEN ?? entity.QuestionTextFR
            : entity.QuestionTextFR ?? entity.QuestionTextEN;

        // Get the AI provider
        IAIService? aiService = null;
        string providerName = "Default";

        if (!string.IsNullOrEmpty(request.Provider) &&
            Enum.TryParse<AIProviderType>(request.Provider, true, out var providerType))
        {
            aiService = _aiProviderFactory.GetProvider(providerType);
            providerName = request.Provider;
        }

        aiService ??= await _aiProviderFactory.GetActiveProviderAsync();

        if (aiService == null)
        {
            _logger.LogError("No AI provider available for coach prompt test");
            return new TestCoachPromptResponse
            {
                Output = request.Language == "en"
                    ? "No AI provider is currently available. Please check your AI configuration."
                    : "Aucun fournisseur d'IA n'est disponible actuellement. Veuillez vérifier votre configuration IA.",
                TokensUsed = 0,
                ResponseTimeMs = 0,
                Provider = "None",
                Model = "N/A"
            };
        }

        // Build the system prompt for coaching
        var systemPrompt = request.Language == "en"
            ? """
              You are an expert business coach helping entrepreneurs improve their business plan responses.
              Your role is to provide constructive feedback, suggestions, and guidance based on the user's answer.
              Be encouraging but honest. Focus on practical improvements.
              Keep your response concise and actionable.
              """
            : """
              Vous êtes un coach d'affaires expert aidant les entrepreneurs à améliorer leurs réponses de plan d'affaires.
              Votre rôle est de fournir des commentaires constructifs, des suggestions et des conseils basés sur la réponse de l'utilisateur.
              Soyez encourageant mais honnête. Concentrez-vous sur des améliorations pratiques.
              Gardez votre réponse concise et actionnable.
              """;

        // Build the user prompt with context
        var userPrompt = request.Language == "en"
            ? $"""
              Question: {questionText}

              Coaching Instructions: {coachPrompt}

              User's Answer: {request.Answer}

              Please provide your coaching feedback based on the instructions above.
              """
            : $"""
              Question: {questionText}

              Instructions de coaching: {coachPrompt}

              Réponse de l'utilisateur: {request.Answer}

              Veuillez fournir vos commentaires de coaching basés sur les instructions ci-dessus.
              """;

        // Generate the response
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var output = await aiService.GenerateContentAsync(
                systemPrompt,
                userPrompt,
                request.MaxTokens,
                (float)request.Temperature,
                cancellationToken);

            stopwatch.Stop();

            // Estimate tokens (rough approximation: ~4 chars per token)
            var estimatedTokens = (systemPrompt.Length + userPrompt.Length + output.Length) / 4;

            _logger.LogInformation(
                "Coach prompt test completed for question {QuestionId} using {Provider} in {ResponseTimeMs}ms",
                questionId, providerName, stopwatch.ElapsedMilliseconds);

            return new TestCoachPromptResponse
            {
                Output = output,
                TokensUsed = estimatedTokens,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Provider = providerName,
                Model = GetModelName(aiService)
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error testing coach prompt for question {QuestionId}", questionId);

            return new TestCoachPromptResponse
            {
                Output = request.Language == "en"
                    ? $"Error generating response: {ex.Message}"
                    : $"Erreur lors de la génération de la réponse: {ex.Message}",
                TokensUsed = 0,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Provider = providerName,
                Model = "Error"
            };
        }
    }

    private static string GetModelName(IAIService aiService)
    {
        var typeName = aiService.GetType().Name;
        return typeName switch
        {
            "OpenAIService" => "GPT-4",
            "ClaudeService" or "AnthropicService" => "Claude-3",
            "GeminiService" or "GoogleService" => "Gemini-Pro",
            _ => typeName.Replace("Service", "")
        };
    }

    private static QuestionTemplateDto MapToDto(QuestionTemplate entity)
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

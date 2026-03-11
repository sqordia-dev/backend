using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services.AI;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Single source of truth for building questionnaire answer dictionaries
/// and enriching AI context with DB-driven question-to-section mappings.
/// </summary>
public class QuestionnaireContextService : IQuestionnaireContextService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<QuestionnaireContextService> _logger;

    public QuestionnaireContextService(
        IApplicationDbContext context,
        ILogger<QuestionnaireContextService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public Dictionary<int, string> BuildAnswersDictionary(
        ICollection<QuestionnaireResponse> responses)
    {
        var answers = new Dictionary<int, string>();
        if (responses == null || !responses.Any()) return answers;

        // V3 (STRUCTURE FINALE) — uses QuestionNumber directly
        var v3Responses = responses
            .Where(r => r.QuestionTemplate != null)
            .ToList();

        foreach (var response in v3Responses)
        {
            var questionNumber = response.QuestionTemplate!.QuestionNumber;
            if (questionNumber <= 0) continue;

            var answer = ExtractAnswer(response, response.QuestionTemplate.QuestionType);
            if (!string.IsNullOrWhiteSpace(answer))
                answers[questionNumber] = answer;
        }

        // Fallback: unlinked responses (legacy data without template references)
        if (!answers.Any())
        {
            var ordered = responses
                .Where(r => !string.IsNullOrWhiteSpace(r.ResponseText))
                .OrderBy(r => r.Created)
                .ToList();

            for (var i = 0; i < ordered.Count; i++)
            {
                answers[i + 1] = ordered[i].ResponseText;
            }

            if (ordered.Any())
            {
                _logger.LogWarning(
                    "Used fallback answer ordering (no template links) — {Count} responses",
                    answers.Count);
            }
        }

        return answers;
    }

    /// <inheritdoc />
    public async Task<SectionMappingContext?> GetSectionMappingContextAsync(
        string sectionName,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        // Resolve which STRUCTURE FINALE sub-sections map to this generated section
        var structureFinaleKeys = QuestionContextMapper.GeneratedSectionToStructureFinale
            .GetValueOrDefault(QuestionContextMapper.ToPascalCase(sectionName));

        if (structureFinaleKeys == null || structureFinaleKeys.Length == 0)
            return null;

        // Extract the numeric prefix codes (e.g., "1.1_Description" → "1.1")
        var subSectionCodes = structureFinaleKeys
            .Select(k => k.Split('_')[0])
            .ToList();

        // Load DB-driven mappings for matching sub-sections
        var mappings = await _context.QuestionSectionMappings
            .Include(m => m.QuestionTemplate)
            .Include(m => m.SubSection)
            .Where(m => m.IsActive && subSectionCodes.Contains(m.SubSection.Code))
            .OrderByDescending(m => m.Weight)
            .ThenBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        if (!mappings.Any())
            return null;

        var questionNumbers = mappings
            .Select(m => m.QuestionTemplate.QuestionNumber)
            .Distinct()
            .OrderBy(n => n)
            .ToArray();

        var transformationHints = mappings
            .Where(m => !string.IsNullOrWhiteSpace(m.TransformationHint))
            .GroupBy(m => m.QuestionTemplate.QuestionNumber)
            .ToDictionary(
                g => g.Key,
                g => g.First().TransformationHint!);

        var weights = mappings
            .GroupBy(m => m.QuestionTemplate.QuestionNumber)
            .ToDictionary(
                g => g.Key,
                g => g.Max(m => m.Weight));

        _logger.LogDebug(
            "Loaded {Count} DB-driven mappings for section {Section} ({Questions} questions)",
            mappings.Count, sectionName, questionNumbers.Length);

        return new SectionMappingContext(questionNumbers, transformationHints, weights);
    }

    /// <summary>
    /// Extracts a string answer from a questionnaire response based on question type.
    /// </summary>
    private static string ExtractAnswer(QuestionnaireResponse response, QuestionType questionType)
    {
        return questionType switch
        {
            QuestionType.ShortText or QuestionType.LongText
                => response.ResponseText ?? "",
            QuestionType.Number
                => response.NumericValue?.ToString() ?? "",
            QuestionType.Currency
                => response.NumericValue.HasValue ? $"${response.NumericValue:N2}" : "",
            QuestionType.Percentage
                => response.NumericValue.HasValue ? $"{response.NumericValue}%" : "",
            QuestionType.Date
                => response.DateValue?.ToString("yyyy-MM-dd") ?? "",
            QuestionType.YesNo
                => response.BooleanValue?.ToString() ?? "",
            QuestionType.SingleChoice or QuestionType.MultipleChoice
                => response.SelectedOptions ?? "",
            QuestionType.Scale
                => response.NumericValue?.ToString() ?? "",
            _ => response.ResponseText ?? ""
        };
    }
}

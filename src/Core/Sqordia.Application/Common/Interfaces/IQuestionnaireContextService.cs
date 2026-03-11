using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Single source of truth for building questionnaire answer dictionaries
/// and enriching AI context with question-to-section mappings.
/// Replaces duplicated BuildAnswersDictionary logic across services.
/// </summary>
public interface IQuestionnaireContextService
{
    /// <summary>
    /// Builds a Dictionary mapping question number (1-22) to answer text
    /// from a business plan's questionnaire responses.
    /// Supports V1, V2, and V3 (STRUCTURE FINALE) question templates.
    /// V3 QuestionNumber is preferred when available.
    /// </summary>
    Dictionary<int, string> BuildAnswersDictionary(
        ICollection<QuestionnaireResponse> responses);

    /// <summary>
    /// Loads DB-driven question-to-section mappings with weights and transformation hints.
    /// Returns enrichment metadata for the specified section that can be injected into AI prompts.
    /// </summary>
    Task<SectionMappingContext?> GetSectionMappingContextAsync(
        string sectionName,
        string language = "fr",
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Enrichment metadata from DB-driven QuestionSectionMappings for a specific section.
/// </summary>
public record SectionMappingContext(
    /// <summary>Question numbers relevant to this section, ordered by weight.</summary>
    int[] QuestionNumbers,
    /// <summary>Transformation hints from QuestionSectionMapping, keyed by question number.</summary>
    Dictionary<int, string> TransformationHints,
    /// <summary>Weights from QuestionSectionMapping, keyed by question number.</summary>
    Dictionary<int, decimal> Weights);

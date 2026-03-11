namespace Sqordia.Application.Services.AI;

/// <summary>
/// Heuristic language detection for post-generation validation.
/// Detects when AI generates content in the wrong language.
/// </summary>
public static class LanguageDetector
{
    public record LanguageCheckResult(bool IsCorrectLanguage, string DetectedLanguage, double Confidence);

    private static readonly HashSet<string> FrenchIndicators = new(StringComparer.OrdinalIgnoreCase)
    {
        "le", "la", "les", "de", "du", "des", "un", "une",
        "est", "sont", "dans", "pour", "avec", "qui", "que",
        "ce", "cette", "par", "plus", "nous", "vous", "notre",
        "votre", "ses", "aux", "sur", "pas", "mais", "ou",
        "donc", "comme", "aussi", "très", "tout", "tous",
        "être", "avoir", "faire", "peut", "entre", "sans",
        "même", "autre", "après", "selon", "vers", "chez"
    };

    private static readonly HashSet<string> EnglishIndicators = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "is", "are", "in", "for", "with", "this", "that",
        "from", "have", "has", "not", "our", "your", "will",
        "can", "been", "would", "their", "which", "when", "more",
        "about", "than", "into", "could", "other", "also", "its",
        "over", "such", "after", "most", "some", "these", "being",
        "each", "between", "should", "through", "where", "does",
        "while", "both", "those", "very", "during"
    };

    /// <summary>
    /// Checks if the content matches the expected language.
    /// Uses a word-frequency heuristic on the first 500 chars.
    /// </summary>
    public static LanguageCheckResult DetectLanguageMismatch(string content, string expectedLanguage)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new LanguageCheckResult(true, expectedLanguage, 1.0);

        // Sample first 500 chars for efficiency
        var sample = content.Length > 500 ? content[..500] : content;
        var words = sample.Split(new[] { ' ', '\n', '\r', '\t', ',', '.', ';', ':', '!', '?', '(', ')', '"', '\'', '«', '»' },
            StringSplitOptions.RemoveEmptyEntries);

        var frenchCount = 0;
        var englishCount = 0;

        foreach (var word in words)
        {
            var clean = word.Trim().ToLowerInvariant();
            if (FrenchIndicators.Contains(clean)) frenchCount++;
            if (EnglishIndicators.Contains(clean)) englishCount++;
        }

        var totalIndicators = frenchCount + englishCount;
        if (totalIndicators < 5)
            return new LanguageCheckResult(true, expectedLanguage, 0.5); // Too few indicators to judge

        var frenchRatio = (double)frenchCount / totalIndicators;

        var detectedLanguage = frenchRatio > 0.5 ? "fr" : "en";
        var confidence = Math.Abs(frenchRatio - 0.5) * 2; // 0 = uncertain, 1 = very confident

        var isCorrect = expectedLanguage.Equals("fr", StringComparison.OrdinalIgnoreCase)
            ? frenchRatio >= 0.4  // Allow some English technical terms in French text
            : frenchRatio <= 0.6; // Allow some French terms in English text

        return new LanguageCheckResult(isCorrect, detectedLanguage, confidence);
    }
}

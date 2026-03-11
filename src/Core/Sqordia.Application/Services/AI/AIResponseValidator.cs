using System.Text.Json;

namespace Sqordia.Application.Services.AI;

/// <summary>
/// Validates AI responses before they are stored or returned.
/// Catches malformed JSON, out-of-range scores, AI refusals, and empty content.
/// </summary>
public static class AIResponseValidator
{
    public record ValidationResult(bool IsValid, string[] Errors);

    private static readonly string[] RefusalIndicators =
    {
        "I cannot", "I can't", "I'm unable", "I am unable",
        "Je ne peux pas", "Il m'est impossible", "Je suis dans l'impossibilité",
        "As an AI", "En tant qu'IA"
    };

    /// <summary>
    /// Validates a QualityReport JSON from Pass 3.
    /// Checks required fields, score ranges, and non-empty summary.
    /// </summary>
    public static ValidationResult ValidateQualityReport(string json)
    {
        var errors = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check coherenceScore
            if (!root.TryGetProperty("coherenceScore", out var coherence) ||
                coherence.ValueKind != JsonValueKind.Number)
            {
                errors.Add("Missing or invalid 'coherenceScore'");
            }
            else
            {
                var val = coherence.GetDecimal();
                if (val < 0 || val > 100)
                    errors.Add($"'coherenceScore' out of range: {val} (expected 0-100)");
            }

            // Check bankReadinessScore
            if (!root.TryGetProperty("bankReadinessScore", out var bankReadiness) ||
                bankReadiness.ValueKind != JsonValueKind.Number)
            {
                errors.Add("Missing or invalid 'bankReadinessScore'");
            }
            else
            {
                var val = bankReadiness.GetDecimal();
                if (val < 0 || val > 100)
                    errors.Add($"'bankReadinessScore' out of range: {val} (expected 0-100)");
            }

            // Check synthesizedExecutiveSummary
            if (!root.TryGetProperty("synthesizedExecutiveSummary", out var summary) ||
                summary.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(summary.GetString()))
            {
                errors.Add("Missing or empty 'synthesizedExecutiveSummary'");
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Invalid JSON: {ex.Message}");
        }

        return new ValidationResult(errors.Count == 0, errors.ToArray());
    }

    /// <summary>
    /// Validates a generation plan JSON from Pass 1.
    /// </summary>
    public static ValidationResult ValidateGenerationPlan(string json)
    {
        var errors = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("overallTheme", out _))
                errors.Add("Missing 'overallTheme'");

            if (!root.TryGetProperty("narrativeArc", out _))
                errors.Add("Missing 'narrativeArc'");

            if (!root.TryGetProperty("sectionGuidance", out var guidance) ||
                guidance.ValueKind != JsonValueKind.Object)
            {
                errors.Add("Missing or invalid 'sectionGuidance'");
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"Invalid JSON: {ex.Message}");
        }

        return new ValidationResult(errors.Count == 0, errors.ToArray());
    }

    /// <summary>
    /// Validates generated section content.
    /// Checks for minimum length, AI refusals, and JSON artifacts.
    /// </summary>
    public static ValidationResult ValidateSectionContent(string content, string sectionName)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(content))
        {
            errors.Add("Content is empty");
            return new ValidationResult(false, errors.ToArray());
        }

        // Check minimum length (at least 100 chars for meaningful content)
        if (content.Length < 100)
            errors.Add($"Content too short ({content.Length} chars, minimum 100)");

        // Check for AI refusal
        foreach (var indicator in RefusalIndicators)
        {
            if (content.Contains(indicator, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"AI refusal detected: contains '{indicator}'");
                break;
            }
        }

        // Check for JSON artifacts in prose content (broken structured output)
        if (content.TrimStart().StartsWith("{") && content.Contains("\"prose\""))
            errors.Add("Content appears to be raw JSON instead of prose");

        return new ValidationResult(errors.Count == 0, errors.ToArray());
    }
}

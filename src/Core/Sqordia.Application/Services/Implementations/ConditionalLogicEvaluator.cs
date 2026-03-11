using System.Text.Json;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Constants;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Evaluates QuestionTemplate.ConditionalLogic JSON to determine question visibility.
///
/// Supports two JSON formats:
///
/// Simple format:
///   { "showIf": "question5", "equals": "Yes" }
///
/// Complex format:
///   {
///     "operator": "AND",
///     "conditions": [
///       { "field": "question5", "operator": "equals", "value": "Yes" },
///       { "field": "question7", "operator": "greaterThan", "value": "100000" }
///     ]
///   }
/// </summary>
public class ConditionalLogicEvaluator : IConditionalLogicEvaluator
{
    private readonly ILogger<ConditionalLogicEvaluator> _logger;

    public ConditionalLogicEvaluator(ILogger<ConditionalLogicEvaluator> logger)
    {
        _logger = logger;
    }

    public bool ShouldShow(
        string? conditionalLogicJson,
        IReadOnlyDictionary<int, string> answersByQuestionNumber,
        IReadOnlyDictionary<int, decimal>? numericAnswers = null)
    {
        if (string.IsNullOrWhiteSpace(conditionalLogicJson))
            return true; // No conditions = always visible

        try
        {
            using var doc = JsonDocument.Parse(conditionalLogicJson);
            var root = doc.RootElement;

            // Simple format: { "showIf": "question5", "equals": "Yes" }
            if (root.TryGetProperty(ConditionalOperators.ShowIfProperty, out var showIfProp))
            {
                return EvaluateSimpleCondition(root, showIfProp.GetString()!, answersByQuestionNumber);
            }

            // Complex format: { "operator": "AND|OR", "conditions": [...] }
            if (root.TryGetProperty(ConditionalOperators.ConditionsProperty, out var conditionsArray))
            {
                var logicalOp = root.TryGetProperty(ConditionalOperators.OperatorProperty, out var opProp)
                    ? opProp.GetString() ?? ConditionalOperators.And
                    : ConditionalOperators.And;

                return EvaluateCompoundCondition(logicalOp, conditionsArray, answersByQuestionNumber, numericAnswers);
            }

            // Single condition object: { "field": "question5", "operator": "equals", "value": "Yes" }
            if (root.TryGetProperty(ConditionalOperators.FieldProperty, out _))
            {
                return EvaluateSingleCondition(root, answersByQuestionNumber, numericAnswers);
            }

            _logger.LogWarning("Unrecognized conditional logic format: {Json}", conditionalLogicJson);
            return true; // Default: show if format is unrecognized
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse conditional logic JSON: {Json}", conditionalLogicJson);
            return true; // Show on parse failure
        }
    }

    private static bool EvaluateSimpleCondition(
        JsonElement root,
        string showIfField,
        IReadOnlyDictionary<int, string> answers)
    {
        var questionNumber = ExtractQuestionNumber(showIfField);
        if (questionNumber == null)
            return true;

        var hasAnswer = answers.TryGetValue(questionNumber.Value, out var answerText);
        if (!hasAnswer || string.IsNullOrWhiteSpace(answerText))
            return false; // Dependent question not answered yet → hide

        // Check equals match
        if (root.TryGetProperty(ConditionalOperators.EqualsProperty, out var equalsProp))
        {
            return string.Equals(answerText.Trim(), equalsProp.GetString()?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // Just "showIf" with no comparator means "show if answered"
        return true;
    }

    private bool EvaluateCompoundCondition(
        string logicalOperator,
        JsonElement conditionsArray,
        IReadOnlyDictionary<int, string> answers,
        IReadOnlyDictionary<int, decimal>? numericAnswers)
    {
        var results = new List<bool>();

        foreach (var condition in conditionsArray.EnumerateArray())
        {
            results.Add(EvaluateSingleCondition(condition, answers, numericAnswers));
        }

        if (results.Count == 0)
            return true;

        return logicalOperator.Equals(ConditionalOperators.Or, StringComparison.OrdinalIgnoreCase)
            ? results.Any(r => r)
            : results.All(r => r);
    }

    private bool EvaluateSingleCondition(
        JsonElement condition,
        IReadOnlyDictionary<int, string> answers,
        IReadOnlyDictionary<int, decimal>? numericAnswers)
    {
        if (!condition.TryGetProperty(ConditionalOperators.FieldProperty, out var fieldProp))
            return true;

        var field = fieldProp.GetString();
        if (string.IsNullOrEmpty(field))
            return true;

        var questionNumber = ExtractQuestionNumber(field);
        if (questionNumber == null)
            return true;

        var op = condition.TryGetProperty(ConditionalOperators.OperatorProperty, out var opProp)
            ? opProp.GetString() ?? ConditionalOperators.Equals
            : ConditionalOperators.Equals;

        var expectedValue = condition.TryGetProperty(ConditionalOperators.ValueProperty, out var valProp)
            ? valProp.GetString()
            : null;

        var hasTextAnswer = answers.TryGetValue(questionNumber.Value, out var textAnswer);

        return op switch
        {
            ConditionalOperators.IsEmpty => !hasTextAnswer || string.IsNullOrWhiteSpace(textAnswer),
            ConditionalOperators.IsNotEmpty => hasTextAnswer && !string.IsNullOrWhiteSpace(textAnswer),
            ConditionalOperators.Equals => hasTextAnswer &&
                string.Equals(textAnswer?.Trim(), expectedValue?.Trim(), StringComparison.OrdinalIgnoreCase),
            ConditionalOperators.NotEquals => !hasTextAnswer ||
                !string.Equals(textAnswer?.Trim(), expectedValue?.Trim(), StringComparison.OrdinalIgnoreCase),
            ConditionalOperators.Contains => hasTextAnswer &&
                (textAnswer?.Contains(expectedValue ?? "", StringComparison.OrdinalIgnoreCase) ?? false),
            ConditionalOperators.NotContains => !hasTextAnswer ||
                !(textAnswer?.Contains(expectedValue ?? "", StringComparison.OrdinalIgnoreCase) ?? false),
            ConditionalOperators.GreaterThan => EvaluateNumericComparison(
                questionNumber.Value, expectedValue, numericAnswers, textAnswer, (a, b) => a > b),
            ConditionalOperators.LessThan => EvaluateNumericComparison(
                questionNumber.Value, expectedValue, numericAnswers, textAnswer, (a, b) => a < b),
            ConditionalOperators.GreaterThanOrEqual => EvaluateNumericComparison(
                questionNumber.Value, expectedValue, numericAnswers, textAnswer, (a, b) => a >= b),
            ConditionalOperators.LessThanOrEqual => EvaluateNumericComparison(
                questionNumber.Value, expectedValue, numericAnswers, textAnswer, (a, b) => a <= b),
            _ => LogUnknownOperator(op)
        };
    }

    private bool LogUnknownOperator(string op)
    {
        _logger.LogWarning("Unknown conditional operator: {Operator}", op);
        return true;
    }

    private static bool EvaluateNumericComparison(
        int questionNumber,
        string? expectedValue,
        IReadOnlyDictionary<int, decimal>? numericAnswers,
        string? textAnswer,
        Func<decimal, decimal, bool> comparison)
    {
        if (!decimal.TryParse(expectedValue, out var expected))
            return false;

        // Try numeric answers first (from NumericValue field)
        if (numericAnswers != null && numericAnswers.TryGetValue(questionNumber, out var numericValue))
            return comparison(numericValue, expected);

        // Fallback: parse text answer as number
        if (decimal.TryParse(textAnswer, out var parsed))
            return comparison(parsed, expected);

        return false;
    }

    /// <summary>
    /// Extracts question number from field references like "question5", "q5", or just "5".
    /// </summary>
    private static int? ExtractQuestionNumber(string field)
    {
        if (int.TryParse(field, out var direct))
            return direct;

        // Strip "question" or "q" prefix
        var numericPart = field
            .Replace("question", "", StringComparison.OrdinalIgnoreCase)
            .Replace("q", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        return int.TryParse(numericPart, out var parsed) ? parsed : null;
    }
}

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Evaluates conditional logic on QuestionTemplate to determine whether
/// a question should be shown based on the user's prior answers.
/// </summary>
public interface IConditionalLogicEvaluator
{
    /// <summary>
    /// Evaluates the conditional logic JSON against the user's current answers.
    /// Returns true if the question should be visible, false if it should be hidden.
    /// </summary>
    /// <param name="conditionalLogicJson">JSON conditional logic from QuestionTemplate</param>
    /// <param name="answersByQuestionNumber">Map of questionNumber → responseText for all answered questions</param>
    /// <param name="numericAnswers">Map of questionNumber → numeric value for numeric/currency questions</param>
    bool ShouldShow(
        string? conditionalLogicJson,
        IReadOnlyDictionary<int, string> answersByQuestionNumber,
        IReadOnlyDictionary<int, decimal>? numericAnswers = null);
}

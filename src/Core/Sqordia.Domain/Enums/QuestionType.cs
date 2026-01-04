namespace Sqordia.Domain.Enums;

/// <summary>
/// Type of questionnaire question
/// </summary>
public enum QuestionType
{
    /// <summary>
    /// Free-text short answer
    /// </summary>
    ShortText = 0,
    
    /// <summary>
    /// Free-text long answer (paragraph/essay)
    /// </summary>
    LongText = 1,
    
    /// <summary>
    /// Single choice from multiple options
    /// </summary>
    SingleChoice = 2,
    
    /// <summary>
    /// Multiple choices from options
    /// </summary>
    MultipleChoice = 3,
    
    /// <summary>
    /// Numeric value
    /// </summary>
    Number = 4,
    
    /// <summary>
    /// Currency/money value
    /// </summary>
    Currency = 5,
    
    /// <summary>
    /// Percentage value (0-100)
    /// </summary>
    Percentage = 6,
    
    /// <summary>
    /// Date selection
    /// </summary>
    Date = 7,
    
    /// <summary>
    /// Yes/No boolean
    /// </summary>
    YesNo = 8,
    
    /// <summary>
    /// Rating scale (1-5, 1-10, etc.)
    /// </summary>
    Scale = 9
}


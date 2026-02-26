namespace Sqordia.Domain.Enums;

/// <summary>
/// Represents the current status of a business plan.
/// Primary statuses for user journey: Draft, Generating, Complete, Exported
/// </summary>
public enum BusinessPlanStatus
{
    /// <summary>
    /// Plan is being drafted, questionnaire incomplete
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Questionnaire completed, awaiting AI generation
    /// </summary>
    QuestionnaireComplete = 1,

    /// <summary>
    /// AI is generating content (shows orange badge)
    /// </summary>
    Generating = 2,

    /// <summary>
    /// AI generation complete, ready for review
    /// </summary>
    Generated = 3,

    /// <summary>
    /// User is reviewing and editing
    /// </summary>
    InReview = 4,

    /// <summary>
    /// Plan is finalized and ready for use
    /// </summary>
    Finalized = 5,

    /// <summary>
    /// Plan has been archived
    /// </summary>
    Archived = 6,

    /// <summary>
    /// Plan is complete and ready for export (shows green badge)
    /// </summary>
    Complete = 7,

    /// <summary>
    /// Plan has been exported as PDF/DOCX (shows navy badge)
    /// </summary>
    Exported = 8
}


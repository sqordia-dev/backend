namespace Sqordia.Domain.Enums;

/// <summary>
/// Represents the status of a questionnaire version
/// </summary>
public enum QuestionnaireVersionStatus
{
    /// <summary>
    /// Version is being drafted, content can be modified
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Version has been published and is live
    /// </summary>
    Published = 1,

    /// <summary>
    /// Version has been archived and is no longer active
    /// </summary>
    Archived = 2
}

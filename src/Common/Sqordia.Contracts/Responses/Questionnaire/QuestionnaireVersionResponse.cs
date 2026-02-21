using Sqordia.Contracts.Responses.Admin;

namespace Sqordia.Contracts.Responses.Questionnaire;

/// <summary>
/// Represents a questionnaire version summary
/// </summary>
public class QuestionnaireVersionResponse
{
    /// <summary>
    /// The version ID
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Sequential version number
    /// </summary>
    public required int VersionNumber { get; set; }

    /// <summary>
    /// Current status (Draft, Published, Archived)
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// Optional notes describing this version
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User who created this version
    /// </summary>
    public required Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Name of user who created this version
    /// </summary>
    public string? CreatedByUserName { get; set; }

    /// <summary>
    /// When this version was created
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this version was published (null if not published)
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Name of user who published this version
    /// </summary>
    public string? PublishedByUserName { get; set; }

    /// <summary>
    /// Total number of questions in this version
    /// </summary>
    public int QuestionCount { get; set; }
}

/// <summary>
/// Represents a questionnaire version with full details
/// </summary>
public class QuestionnaireVersionDetailResponse : QuestionnaireVersionResponse
{
    /// <summary>
    /// All questions in this version
    /// </summary>
    public List<QuestionTemplateDto> Questions { get; set; } = new();

    /// <summary>
    /// All step configurations in this version
    /// </summary>
    public List<QuestionnaireStepDto> Steps { get; set; } = new();
}

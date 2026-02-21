using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Represents a versioned snapshot of questionnaire content (questions and step configurations).
/// Each version contains JSON snapshots and goes through a Draft -> Published -> Archived lifecycle.
/// </summary>
public class QuestionnaireVersion : BaseAuditableEntity
{
    /// <summary>
    /// Sequential version number
    /// </summary>
    public int VersionNumber { get; private set; }

    /// <summary>
    /// Current status of this version
    /// </summary>
    public QuestionnaireVersionStatus Status { get; private set; }

    /// <summary>
    /// User who created this version
    /// </summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>
    /// When this version was published (null if not published)
    /// </summary>
    public DateTime? PublishedAt { get; private set; }

    /// <summary>
    /// User who published this version
    /// </summary>
    public Guid? PublishedByUserId { get; private set; }

    /// <summary>
    /// Optional notes describing this version
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// JSON snapshot of all questions at this version
    /// Contains serialized list of QuestionTemplateV2 data
    /// </summary>
    public string QuestionsSnapshot { get; private set; } = null!;

    /// <summary>
    /// JSON snapshot of step configurations at this version
    /// Contains serialized list of QuestionnaireStep data
    /// </summary>
    public string StepsSnapshot { get; private set; } = null!;

    private QuestionnaireVersion() { } // EF Core constructor

    public QuestionnaireVersion(
        int versionNumber,
        Guid createdByUserId,
        string questionsSnapshot,
        string stepsSnapshot,
        string? notes = null)
    {
        if (versionNumber <= 0)
            throw new ArgumentException("Version number must be greater than zero.", nameof(versionNumber));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Created by user ID cannot be empty.", nameof(createdByUserId));

        VersionNumber = versionNumber;
        CreatedByUserId = createdByUserId;
        Status = QuestionnaireVersionStatus.Draft;
        QuestionsSnapshot = questionsSnapshot ?? throw new ArgumentNullException(nameof(questionsSnapshot));
        StepsSnapshot = stepsSnapshot ?? throw new ArgumentNullException(nameof(stepsSnapshot));
        Notes = notes;
    }

    /// <summary>
    /// Publishes this version, making it the live questionnaire content.
    /// Only draft versions can be published.
    /// </summary>
    public void Publish(Guid userId)
    {
        if (Status != QuestionnaireVersionStatus.Draft)
            throw new InvalidOperationException($"Cannot publish a version with status '{Status}'. Only draft versions can be published.");

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        Status = QuestionnaireVersionStatus.Published;
        PublishedAt = DateTime.UtcNow;
        PublishedByUserId = userId;
    }

    /// <summary>
    /// Archives this version. Published or draft versions can be archived.
    /// </summary>
    public void Archive()
    {
        if (Status == QuestionnaireVersionStatus.Archived)
            throw new InvalidOperationException("Version is already archived.");

        Status = QuestionnaireVersionStatus.Archived;
    }

    /// <summary>
    /// Updates the questions snapshot. Only draft versions can be modified.
    /// </summary>
    public void UpdateQuestionsSnapshot(string questionsSnapshot)
    {
        if (Status != QuestionnaireVersionStatus.Draft)
            throw new InvalidOperationException($"Cannot modify a version with status '{Status}'. Only draft versions can be modified.");

        QuestionsSnapshot = questionsSnapshot ?? throw new ArgumentNullException(nameof(questionsSnapshot));
    }

    /// <summary>
    /// Updates the steps snapshot. Only draft versions can be modified.
    /// </summary>
    public void UpdateStepsSnapshot(string stepsSnapshot)
    {
        if (Status != QuestionnaireVersionStatus.Draft)
            throw new InvalidOperationException($"Cannot modify a version with status '{Status}'. Only draft versions can be modified.");

        StepsSnapshot = stepsSnapshot ?? throw new ArgumentNullException(nameof(stepsSnapshot));
    }

    /// <summary>
    /// Updates the notes for this version.
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }
}

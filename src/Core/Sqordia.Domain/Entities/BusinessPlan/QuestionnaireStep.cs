using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Represents a step in the questionnaire wizard.
/// Stores editable step titles and descriptions for the 5-step questionnaire flow.
/// </summary>
public class QuestionnaireStep : BaseAuditableEntity
{
    /// <summary>
    /// The step number (1-5)
    /// </summary>
    public int StepNumber { get; private set; }

    /// <summary>
    /// Step title in French (primary)
    /// </summary>
    public string TitleFR { get; private set; } = null!;

    /// <summary>
    /// Step title in English
    /// </summary>
    public string? TitleEN { get; private set; }

    /// <summary>
    /// Step description in French
    /// </summary>
    public string? DescriptionFR { get; private set; }

    /// <summary>
    /// Step description in English
    /// </summary>
    public string? DescriptionEN { get; private set; }

    /// <summary>
    /// Emoji icon for visual display
    /// </summary>
    public string? Icon { get; private set; }

    /// <summary>
    /// Whether this step is active
    /// </summary>
    public bool IsActive { get; private set; }

    private QuestionnaireStep() { } // EF Core constructor

    public QuestionnaireStep(
        int stepNumber,
        string titleFR,
        string? titleEN = null,
        string? descriptionFR = null,
        string? descriptionEN = null,
        string? icon = null)
    {
        if (stepNumber < 1 || stepNumber > 5)
            throw new ArgumentException("Step number must be between 1 and 5", nameof(stepNumber));

        StepNumber = stepNumber;
        TitleFR = titleFR ?? throw new ArgumentNullException(nameof(titleFR));
        TitleEN = titleEN;
        DescriptionFR = descriptionFR;
        DescriptionEN = descriptionEN;
        Icon = icon;
        IsActive = true;
    }

    /// <summary>
    /// Updates the step titles
    /// </summary>
    public void UpdateTitles(string titleFR, string? titleEN = null)
    {
        TitleFR = titleFR ?? throw new ArgumentNullException(nameof(titleFR));
        TitleEN = titleEN;
    }

    /// <summary>
    /// Updates the step descriptions
    /// </summary>
    public void UpdateDescriptions(string? descriptionFR, string? descriptionEN = null)
    {
        DescriptionFR = descriptionFR;
        DescriptionEN = descriptionEN;
    }

    /// <summary>
    /// Updates the step icon
    /// </summary>
    public void SetIcon(string? icon)
    {
        Icon = icon;
    }

    /// <summary>
    /// Updates all editable fields at once
    /// </summary>
    public void Update(
        string? titleFR = null,
        string? titleEN = null,
        string? descriptionFR = null,
        string? descriptionEN = null,
        string? icon = null)
    {
        if (titleFR != null)
            TitleFR = titleFR;
        if (titleEN != null)
            TitleEN = titleEN;
        DescriptionFR = descriptionFR ?? DescriptionFR;
        DescriptionEN = descriptionEN ?? DescriptionEN;
        if (icon != null)
            Icon = icon;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

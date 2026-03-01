using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Represents a sub-section within a main section (e.g., 1.1, 1.2, 3.7)
/// Based on STRUCTURE FINALE specification
/// </summary>
public class SubSection : BaseAuditableEntity
{
    /// <summary>
    /// Reference to the parent main section
    /// </summary>
    public Guid MainSectionId { get; private set; }

    /// <summary>
    /// Sub-section code (e.g., "1.1", "1.2", "3.7")
    /// </summary>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Sub-section title in French
    /// </summary>
    public string TitleFR { get; private set; } = null!;

    /// <summary>
    /// Sub-section title in English
    /// </summary>
    public string TitleEN { get; private set; } = null!;

    /// <summary>
    /// Sub-section description in French
    /// </summary>
    public string? DescriptionFR { get; private set; }

    /// <summary>
    /// Sub-section description in English
    /// </summary>
    public string? DescriptionEN { get; private set; }

    /// <summary>
    /// Display order within the main section
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Whether this sub-section is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Icon for visual display
    /// </summary>
    public string? Icon { get; private set; }

    /// <summary>
    /// Educational note in French - explanatory content for users
    /// </summary>
    public string? NoteFR { get; private set; }

    /// <summary>
    /// Educational note in English
    /// </summary>
    public string? NoteEN { get; private set; }

    // Navigation properties
    public virtual MainSection MainSection { get; private set; } = null!;
    public virtual ICollection<SectionPrompt> Prompts { get; private set; } = new List<SectionPrompt>();
    public virtual ICollection<QuestionSectionMapping> QuestionMappings { get; private set; } = new List<QuestionSectionMapping>();

    private SubSection() { } // EF Core constructor

    public SubSection(
        Guid mainSectionId,
        string code,
        string titleFR,
        string titleEN,
        int displayOrder,
        string? descriptionFR = null,
        string? descriptionEN = null,
        string? icon = null,
        string? noteFR = null,
        string? noteEN = null)
    {
        MainSectionId = mainSectionId;
        Code = code ?? throw new ArgumentNullException(nameof(code));
        TitleFR = titleFR ?? throw new ArgumentNullException(nameof(titleFR));
        TitleEN = titleEN ?? throw new ArgumentNullException(nameof(titleEN));
        DisplayOrder = displayOrder;
        DescriptionFR = descriptionFR;
        DescriptionEN = descriptionEN;
        Icon = icon;
        NoteFR = noteFR;
        NoteEN = noteEN;
        IsActive = true;
    }

    public void UpdateTitles(string titleFR, string titleEN)
    {
        TitleFR = titleFR ?? throw new ArgumentNullException(nameof(titleFR));
        TitleEN = titleEN ?? throw new ArgumentNullException(nameof(titleEN));
    }

    public void UpdateDescriptions(string? descriptionFR, string? descriptionEN)
    {
        DescriptionFR = descriptionFR;
        DescriptionEN = descriptionEN;
    }

    public void UpdateNotes(string? noteFR, string? noteEN)
    {
        NoteFR = noteFR;
        NoteEN = noteEN;
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void SetIcon(string? icon)
    {
        Icon = icon;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetDisplayOrder(int order) => DisplayOrder = order;

    /// <summary>
    /// Factory method to create a new SubSection
    /// </summary>
    public static SubSection Create(
        Guid mainSectionId,
        string code,
        string titleFR,
        string titleEN,
        string? descriptionFR,
        string? descriptionEN,
        string? noteFR,
        string? noteEN,
        int displayOrder,
        string? icon)
    {
        return new SubSection(mainSectionId, code, titleFR, titleEN, displayOrder, descriptionFR, descriptionEN, icon, noteFR, noteEN);
    }

    /// <summary>
    /// Update method for modifying sub-section properties
    /// </summary>
    public void Update(
        string titleFR,
        string titleEN,
        string? descriptionFR,
        string? descriptionEN,
        string? noteFR,
        string? noteEN,
        int displayOrder,
        string? icon)
    {
        UpdateTitles(titleFR, titleEN);
        UpdateDescriptions(descriptionFR, descriptionEN);
        UpdateNotes(noteFR, noteEN);
        UpdateDisplayOrder(displayOrder);
        SetIcon(icon);
    }
}

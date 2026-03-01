using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Represents a main section in the business plan structure (0-7)
/// Based on STRUCTURE FINALE specification
/// </summary>
public class MainSection : BaseAuditableEntity
{
    /// <summary>
    /// Section number (0-7): 0=Executive Summary, 1=Le Projet, etc.
    /// </summary>
    public int Number { get; private set; }

    /// <summary>
    /// Unique code identifier (e.g., "executive_summary", "le_projet")
    /// </summary>
    public string Code { get; private set; } = null!;

    /// <summary>
    /// Section title in French
    /// </summary>
    public string TitleFR { get; private set; } = null!;

    /// <summary>
    /// Section title in English
    /// </summary>
    public string TitleEN { get; private set; } = null!;

    /// <summary>
    /// Section description in French
    /// </summary>
    public string? DescriptionFR { get; private set; }

    /// <summary>
    /// Section description in English
    /// </summary>
    public string? DescriptionEN { get; private set; }

    /// <summary>
    /// Display order for UI rendering
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Whether this section is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// True for Executive Summary - indicates this section should be generated last
    /// after all other sections are complete
    /// </summary>
    public bool GeneratedLast { get; private set; }

    /// <summary>
    /// Icon for visual display (emoji or icon name)
    /// </summary>
    public string? Icon { get; private set; }

    // Navigation properties
    public virtual ICollection<SubSection> SubSections { get; private set; } = new List<SubSection>();
    public virtual ICollection<SectionPrompt> Prompts { get; private set; } = new List<SectionPrompt>();

    private MainSection() { } // EF Core constructor

    public MainSection(
        int number,
        string code,
        string titleFR,
        string titleEN,
        int displayOrder,
        bool generatedLast = false,
        string? descriptionFR = null,
        string? descriptionEN = null,
        string? icon = null)
    {
        if (number < 0 || number > 7)
            throw new ArgumentException("Section number must be between 0 and 7", nameof(number));

        Number = number;
        Code = code ?? throw new ArgumentNullException(nameof(code));
        TitleFR = titleFR ?? throw new ArgumentNullException(nameof(titleFR));
        TitleEN = titleEN ?? throw new ArgumentNullException(nameof(titleEN));
        DisplayOrder = displayOrder;
        GeneratedLast = generatedLast;
        DescriptionFR = descriptionFR;
        DescriptionEN = descriptionEN;
        Icon = icon;
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

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public void SetIcon(string? icon)
    {
        Icon = icon;
    }

    public void SetGeneratedLast(bool generatedLast)
    {
        GeneratedLast = generatedLast;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetDisplayOrder(int order) => DisplayOrder = order;

    /// <summary>
    /// Factory method to create a new MainSection
    /// </summary>
    public static MainSection Create(
        int number,
        string code,
        string titleFR,
        string titleEN,
        string? descriptionFR,
        string? descriptionEN,
        int displayOrder,
        bool generatedLast,
        string? icon)
    {
        return new MainSection(number, code, titleFR, titleEN, displayOrder, generatedLast, descriptionFR, descriptionEN, icon);
    }

    /// <summary>
    /// Update method for modifying section properties
    /// </summary>
    public void Update(
        string titleFR,
        string titleEN,
        string? descriptionFR,
        string? descriptionEN,
        int displayOrder,
        bool generatedLast,
        string? icon)
    {
        UpdateTitles(titleFR, titleEN);
        UpdateDescriptions(descriptionFR, descriptionEN);
        UpdateDisplayOrder(displayOrder);
        SetGeneratedLast(generatedLast);
        SetIcon(icon);
    }
}

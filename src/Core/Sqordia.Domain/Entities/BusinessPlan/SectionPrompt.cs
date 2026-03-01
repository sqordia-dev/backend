using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// AI prompt template for generating business plan section content
/// Supports master prompts (for main sections) and override prompts (for sub-sections)
/// </summary>
public class SectionPrompt : BaseAuditableEntity
{
    /// <summary>
    /// Reference to main section (for master prompts)
    /// </summary>
    public Guid? MainSectionId { get; private set; }

    /// <summary>
    /// Reference to sub-section (for override prompts)
    /// </summary>
    public Guid? SubSectionId { get; private set; }

    /// <summary>
    /// Prompt hierarchy level: Master or Override
    /// </summary>
    public PromptLevel Level { get; private set; }

    /// <summary>
    /// Business plan type this prompt applies to
    /// </summary>
    public BusinessPlanType PlanType { get; private set; }

    /// <summary>
    /// Language code: "fr" or "en"
    /// </summary>
    public string Language { get; private set; } = null!;

    /// <summary>
    /// Industry category (NAICS code) for industry-specific prompts
    /// Null means generic/applicable to all industries
    /// </summary>
    public string? IndustryCategory { get; private set; }

    /// <summary>
    /// Name/identifier for this prompt
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Description of what this prompt does
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// System prompt for the AI (role, context, instructions)
    /// </summary>
    public string SystemPrompt { get; private set; } = null!;

    /// <summary>
    /// User prompt template with {{variable}} placeholders
    /// </summary>
    public string UserPromptTemplate { get; private set; } = null!;

    /// <summary>
    /// JSON object defining expected variables and their descriptions
    /// </summary>
    public string? VariablesJson { get; private set; }

    /// <summary>
    /// Expected output format
    /// </summary>
    public OutputFormat OutputFormat { get; private set; }

    /// <summary>
    /// JSON specification for visual elements (charts, tables)
    /// </summary>
    public string? VisualElementsJson { get; private set; }

    /// <summary>
    /// Example output to guide the AI
    /// </summary>
    public string? ExampleOutput { get; private set; }

    /// <summary>
    /// Version number for tracking changes
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Whether this prompt is the active one for its section/plan/language combination
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    public virtual MainSection? MainSection { get; private set; }
    public virtual SubSection? SubSection { get; private set; }

    private SectionPrompt() { } // EF Core constructor

    /// <summary>
    /// Creates a master prompt for a main section
    /// </summary>
    public static SectionPrompt CreateMasterPrompt(
        Guid mainSectionId,
        BusinessPlanType planType,
        string language,
        string name,
        string systemPrompt,
        string userPromptTemplate,
        OutputFormat outputFormat,
        string? industryCategory = null,
        string? description = null,
        string? variablesJson = null,
        string? visualElementsJson = null,
        string? exampleOutput = null)
    {
        return new SectionPrompt
        {
            MainSectionId = mainSectionId,
            SubSectionId = null,
            Level = PromptLevel.Master,
            PlanType = planType,
            Language = language ?? throw new ArgumentNullException(nameof(language)),
            IndustryCategory = industryCategory,
            Name = name ?? throw new ArgumentNullException(nameof(name)),
            Description = description,
            SystemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt)),
            UserPromptTemplate = userPromptTemplate ?? throw new ArgumentNullException(nameof(userPromptTemplate)),
            VariablesJson = variablesJson,
            OutputFormat = outputFormat,
            VisualElementsJson = visualElementsJson,
            ExampleOutput = exampleOutput,
            Version = 1,
            IsActive = false
        };
    }

    /// <summary>
    /// Creates an override prompt for a sub-section
    /// </summary>
    public static SectionPrompt CreateOverridePrompt(
        Guid subSectionId,
        BusinessPlanType planType,
        string language,
        string name,
        string systemPrompt,
        string userPromptTemplate,
        OutputFormat outputFormat,
        string? industryCategory = null,
        string? description = null,
        string? variablesJson = null,
        string? visualElementsJson = null,
        string? exampleOutput = null)
    {
        return new SectionPrompt
        {
            MainSectionId = null,
            SubSectionId = subSectionId,
            Level = PromptLevel.Override,
            PlanType = planType,
            Language = language ?? throw new ArgumentNullException(nameof(language)),
            IndustryCategory = industryCategory,
            Name = name ?? throw new ArgumentNullException(nameof(name)),
            Description = description,
            SystemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt)),
            UserPromptTemplate = userPromptTemplate ?? throw new ArgumentNullException(nameof(userPromptTemplate)),
            VariablesJson = variablesJson,
            OutputFormat = outputFormat,
            VisualElementsJson = visualElementsJson,
            ExampleOutput = exampleOutput,
            Version = 1,
            IsActive = false
        };
    }

    public void UpdateContent(
        string systemPrompt,
        string userPromptTemplate,
        string? description = null,
        string? variablesJson = null,
        string? visualElementsJson = null,
        string? exampleOutput = null)
    {
        SystemPrompt = systemPrompt ?? throw new ArgumentNullException(nameof(systemPrompt));
        UserPromptTemplate = userPromptTemplate ?? throw new ArgumentNullException(nameof(userPromptTemplate));
        Description = description;
        VariablesJson = variablesJson;
        VisualElementsJson = visualElementsJson;
        ExampleOutput = exampleOutput;
    }

    public void UpdateName(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void UpdateOutputFormat(OutputFormat outputFormat)
    {
        OutputFormat = outputFormat;
    }

    public void SetIndustryCategory(string? industryCategory)
    {
        IndustryCategory = industryCategory;
    }

    public void IncrementVersion()
    {
        Version++;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Update all editable properties
    /// </summary>
    public void Update(
        string name,
        string? description,
        string systemPrompt,
        string userPromptTemplate,
        string? variablesJson,
        string outputFormat,
        string? visualElementsJson,
        string? exampleOutput,
        string? industryCategory)
    {
        UpdateName(name);
        if (Enum.TryParse<OutputFormat>(outputFormat, true, out var format))
        {
            UpdateOutputFormat(format);
        }
        UpdateContent(systemPrompt, userPromptTemplate, description, variablesJson, visualElementsJson, exampleOutput);
        SetIndustryCategory(industryCategory);
        IncrementVersion();
    }

    /// <summary>
    /// Creates a clone of this prompt with a new version
    /// </summary>
    public SectionPrompt Clone()
    {
        return new SectionPrompt
        {
            MainSectionId = MainSectionId,
            SubSectionId = SubSectionId,
            Level = Level,
            PlanType = PlanType,
            Language = Language,
            IndustryCategory = IndustryCategory,
            Name = $"{Name} (Copy)",
            Description = Description,
            SystemPrompt = SystemPrompt,
            UserPromptTemplate = UserPromptTemplate,
            VariablesJson = VariablesJson,
            OutputFormat = OutputFormat,
            VisualElementsJson = VisualElementsJson,
            ExampleOutput = ExampleOutput,
            Version = 1,
            IsActive = false
        };
    }
}

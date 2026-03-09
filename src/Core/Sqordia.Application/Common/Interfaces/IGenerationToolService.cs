namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Provides tools that can be invoked during AI section generation to enrich content
/// with real data (industry benchmarks, market sizing, regulatory info, cross-section references).
/// </summary>
public interface IGenerationToolService
{
    /// <summary>
    /// Gets the tool definitions available for a specific section.
    /// Returns tool specs in Claude/OpenAI tool format.
    /// </summary>
    List<GenerationTool> GetToolsForSection(string sectionName);

    /// <summary>
    /// Executes a tool by name with the given arguments.
    /// </summary>
    Task<string> ExecuteToolAsync(
        string toolName,
        Dictionary<string, object> arguments,
        Guid businessPlanId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// A tool definition for AI generation augmentation.
/// </summary>
public class GenerationTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, ToolParameter> Parameters { get; set; } = new();
}

public class ToolParameter
{
    public string Type { get; set; } = "string";
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; }
    public List<string>? Enum { get; set; }
}

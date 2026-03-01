namespace Sqordia.Domain.Enums;

/// <summary>
/// Defines the hierarchy level of a section prompt
/// </summary>
public enum PromptLevel
{
    /// <summary>
    /// Master prompt that applies to an entire main section
    /// Used as default when no sub-section override exists
    /// </summary>
    Master = 1,

    /// <summary>
    /// Override prompt specific to a sub-section
    /// Takes precedence over the master prompt when present
    /// </summary>
    Override = 2
}

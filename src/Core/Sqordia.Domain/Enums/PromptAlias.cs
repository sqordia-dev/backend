namespace Sqordia.Domain.Enums;

/// <summary>
/// Alias for prompt template deployment environments
/// Used for A/B testing and staged rollouts
/// </summary>
public enum PromptAlias
{
    /// <summary>
    /// Production-ready prompt for all users
    /// </summary>
    Production = 1,

    /// <summary>
    /// Staging environment for testing before production
    /// </summary>
    Staging = 2,

    /// <summary>
    /// Development environment for internal testing
    /// </summary>
    Development = 3,

    /// <summary>
    /// Experimental prompts for A/B testing
    /// </summary>
    Experimental = 4
}

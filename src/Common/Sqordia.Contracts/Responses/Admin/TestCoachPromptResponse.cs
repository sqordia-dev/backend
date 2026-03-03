namespace Sqordia.Contracts.Responses.Admin;

/// <summary>
/// Response from testing a coach prompt
/// </summary>
public class TestCoachPromptResponse
{
    /// <summary>
    /// The AI-generated coaching output
    /// </summary>
    public required string Output { get; set; }

    /// <summary>
    /// Number of tokens used in the response
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    /// Time taken to generate the response in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// The AI provider that was used
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// The specific model used by the provider
    /// </summary>
    public required string Model { get; set; }
}

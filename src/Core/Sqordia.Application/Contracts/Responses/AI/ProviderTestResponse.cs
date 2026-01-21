namespace Sqordia.Contracts.Responses.AI;

/// <summary>
/// Response from testing an AI provider connection
/// </summary>
public class ProviderTestResponse
{
    /// <summary>
    /// Whether the test was successful
    /// </summary>
    public required bool Success { get; set; }

    /// <summary>
    /// Message describing the test result
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public required int ResponseTimeMs { get; set; }

    /// <summary>
    /// Model that was tested
    /// </summary>
    public required string ModelUsed { get; set; }

    /// <summary>
    /// Error details if the test failed
    /// </summary>
    public string? ErrorDetails { get; set; }
}

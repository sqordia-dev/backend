namespace Sqordia.Contracts.Requests.AI;

/// <summary>
/// Request to test an AI provider connection
/// </summary>
public class ProviderTestRequest
{
    /// <summary>
    /// API key to test
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Model to test with
    /// </summary>
    public required string Model { get; set; }
}

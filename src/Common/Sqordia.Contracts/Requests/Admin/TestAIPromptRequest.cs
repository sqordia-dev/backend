using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Admin;

/// <summary>
/// Request to test an AI prompt with sample data
/// </summary>
public class TestAIPromptRequest
{
    /// <summary>
    /// The prompt ID to test
    /// </summary>
    [Required]
    public required string PromptId { get; set; }
    
    /// <summary>
    /// Sample variables to use in the prompt (JSON format)
    /// </summary>
    [Required]
    [StringLength(2000)]
    public required string SampleVariables { get; set; }
    
    /// <summary>
    /// Additional context for the test
    /// </summary>
    [StringLength(1000)]
    public string? TestContext { get; set; }
    
    /// <summary>
    /// Maximum tokens for the test response
    /// </summary>
    [Range(100, 2000)]
    public int MaxTokens { get; set; } = 1000;
    
    /// <summary>
    /// Temperature for the test (0.0 to 1.0)
    /// </summary>
    [Range(0.0, 1.0)]
    public double Temperature { get; set; } = 0.7;
}

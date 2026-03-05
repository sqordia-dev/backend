namespace Sqordia.Application.Services;

/// <summary>
/// Admin AI Assistant service for natural language queries about system data
/// </summary>
public interface IAdminAIAssistantService
{
    /// <summary>
    /// Stream admin query responses with tool-use support
    /// </summary>
    IAsyncEnumerable<AdminAIStreamEvent> StreamAdminQueryAsync(
        List<AdminAIMessage> conversationHistory,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Stream event types for Admin AI Assistant
/// </summary>
public record AdminAIStreamEvent
{
    public required string Type { get; init; } // token, tool_start, tool_end, done, error
    public string? Content { get; init; }
    public string? ToolName { get; init; }
    public string? Error { get; init; }
}

/// <summary>
/// Message in Admin AI conversation
/// </summary>
public class AdminAIMessage
{
    public string Role { get; set; } = "user"; // user or assistant
    public string Content { get; set; } = string.Empty;
}

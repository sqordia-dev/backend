using System.Text.Json;

namespace WebAPI.Helpers;

/// <summary>
/// Helper for Server-Sent Events (SSE) responses
/// </summary>
public static class SseHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Configures the HTTP response for SSE streaming
    /// </summary>
    public static void ConfigureForSse(HttpResponse response)
    {
        response.ContentType = "text/event-stream";
        response.Headers["Cache-Control"] = "no-cache";
        response.Headers["Connection"] = "keep-alive";
        response.Headers["X-Accel-Buffering"] = "no";
    }

    /// <summary>
    /// Writes a JSON-serialized SSE event to the response
    /// </summary>
    public static async Task WriteJsonEventAsync<T>(HttpResponse response, T data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    /// <summary>
    /// Writes the [DONE] sentinel to signal end of stream
    /// </summary>
    public static async Task WriteDoneAsync(HttpResponse response, CancellationToken cancellationToken = default)
    {
        await response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}

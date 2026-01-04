using System.Text.Json.Serialization;

namespace Sqordia.Functions.ExportHandler.Models;

/// <summary>
/// Document export job message from Pub/Sub topic
/// </summary>
public class ExportJobMessage
{
    [JsonPropertyName("jobId")]
    public string JobId { get; set; } = string.Empty;

    [JsonPropertyName("businessPlanId")]
    public string BusinessPlanId { get; set; } = string.Empty;

    [JsonPropertyName("exportType")]
    public string ExportType { get; set; } = "pdf"; // pdf, word, excel

    [JsonPropertyName("language")]
    public string Language { get; set; } = "fr";

    [JsonPropertyName("template")]
    public string Template { get; set; } = "default";

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
}


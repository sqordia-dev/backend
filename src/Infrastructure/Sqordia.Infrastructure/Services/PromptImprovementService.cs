using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Admin.PromptRegistry;
using Sqordia.Contracts.Responses.Admin.PromptRegistry;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Implementation of prompt improvement service using Anthropic's experimental API
/// </summary>
public class PromptImprovementService : IPromptImprovementService
{
    private readonly ILogger<PromptImprovementService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ClaudeSettings _settings;
    private const string ImprovePromptEndpoint = "https://api.anthropic.com/v1/experimental/improve_prompt";
    private const string AnthropicVersion = "2023-06-01";

    public PromptImprovementService(
        IHttpClientFactory httpClientFactory,
        IOptions<ClaudeSettings> settings,
        ILogger<PromptImprovementService> logger)
    {
        _logger = logger;
        _settings = settings.Value;
        _httpClient = httpClientFactory.CreateClient("Anthropic");

        // Configure headers for Anthropic API
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);
        _httpClient.DefaultRequestHeaders.Add("anthropic-beta", "prompt-improvement-2025-01-24");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Result<PromptImprovementResultDto>> ImprovePromptAsync(
        PromptImprovementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            _logger.LogWarning("Claude API key not configured");
            return Result.Failure<PromptImprovementResultDto>(
                Error.Validation("PromptImprovement.NotConfigured", "AI service is not configured"));
        }

        try
        {
            _logger.LogInformation("Improving prompt with focus area: {FocusArea}", request.FocusArea);

            // Build the prompt to improve (combine system + user template)
            var promptToImprove = BuildPromptForImprovement(request);

            // Build the improvement instructions based on focus area
            var improvementInstructions = BuildImprovementInstructions(request);

            // Create the request payload for Anthropic's experimental API
            var payload = new AnthropicImprovePromptRequest
            {
                Prompt = promptToImprove,
                TargetModel = _settings.Model,
                CustomInstructions = improvementInstructions
            };

            var response = await _httpClient.PostAsJsonAsync(
                ImprovePromptEndpoint,
                payload,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Anthropic API error: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);

                // Fall back to standard Claude API for improvement
                return await ImprovePromptWithStandardApiAsync(request, cancellationToken);
            }

            var result = await response.Content.ReadFromJsonAsync<AnthropicImprovePromptResponse>(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower },
                cancellationToken);

            if (result == null)
            {
                return Result.Failure<PromptImprovementResultDto>(
                    Error.Failure("PromptImprovement.InvalidResponse", "Failed to parse improvement response"));
            }

            // Parse the improved prompt back into system and user template
            var (improvedSystem, improvedUser) = ParseImprovedPrompt(result.ImprovedPrompt, request);

            var dto = new PromptImprovementResultDto
            {
                ImprovedSystemPrompt = improvedSystem,
                ImprovedUserPromptTemplate = improvedUser,
                Summary = result.Summary ?? "Prompt improved successfully",
                Model = result.Model ?? _settings.Model,
                TokensUsed = result.Usage?.TotalTokens ?? 0,
                Improvements = ParseImprovements(result.Changes ?? new List<AnthropicPromptChange>())
            };

            _logger.LogInformation("Prompt improved successfully with {Count} improvements",
                dto.Improvements.Count);

            return Result.Success(dto);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Anthropic API");
            // Fall back to standard API
            return await ImprovePromptWithStandardApiAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error improving prompt");
            return Result.Failure<PromptImprovementResultDto>(
                Error.Failure("PromptImprovement.Error", $"Failed to improve prompt: {ex.Message}"));
        }
    }

    /// <summary>
    /// Fallback method using standard Claude API when experimental endpoint is unavailable
    /// </summary>
    private async Task<Result<PromptImprovementResultDto>> ImprovePromptWithStandardApiAsync(
        PromptImprovementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Using standard Claude API for prompt improvement");

            var systemPrompt = GetPromptImprovementSystemPrompt(request.FocusArea, request.TargetLanguage);
            var userPrompt = BuildUserPromptForImprovement(request);

            // Use the standard messages API
            var messagesPayload = new
            {
                model = _settings.Model,
                max_tokens = 4096,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = userPrompt }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://api.anthropic.com/v1/messages",
                messagesPayload,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Claude API error: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);
                return Result.Failure<PromptImprovementResultDto>(
                    Error.Failure("PromptImprovement.ApiError", "Failed to call AI service"));
            }

            var result = await response.Content.ReadFromJsonAsync<ClaudeMessagesResponse>(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower },
                cancellationToken);

            if (result?.Content == null || result.Content.Count == 0)
            {
                return Result.Failure<PromptImprovementResultDto>(
                    Error.Failure("PromptImprovement.EmptyResponse", "AI returned empty response"));
            }

            var textContent = result.Content.FirstOrDefault(c => c.Type == "text")?.Text ?? "";

            // Parse the structured response
            var parsedResult = ParseStandardApiResponse(textContent, request);

            parsedResult.Model = result.Model ?? _settings.Model;
            parsedResult.TokensUsed = (result.Usage?.InputTokens ?? 0) + (result.Usage?.OutputTokens ?? 0);

            return Result.Success(parsedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in fallback prompt improvement");
            return Result.Failure<PromptImprovementResultDto>(
                Error.Failure("PromptImprovement.FallbackError", $"Failed to improve prompt: {ex.Message}"));
        }
    }

    private string BuildPromptForImprovement(PromptImprovementRequest request)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            parts.Add($"[SYSTEM PROMPT]\n{request.SystemPrompt}");
        }

        if (!string.IsNullOrWhiteSpace(request.UserPromptTemplate))
        {
            parts.Add($"[USER PROMPT TEMPLATE]\n{request.UserPromptTemplate}");
        }

        return string.Join("\n\n", parts);
    }

    private string BuildImprovementInstructions(PromptImprovementRequest request)
    {
        var instructions = new List<string>();

        switch (request.FocusArea.ToLowerInvariant())
        {
            case "clarity":
                instructions.Add("Focus on improving clarity and readability");
                instructions.Add("Simplify complex sentences");
                instructions.Add("Remove ambiguous language");
                break;
            case "specificity":
                instructions.Add("Add specific details and examples");
                instructions.Add("Define expected output formats clearly");
                instructions.Add("Include constraints and boundaries");
                break;
            case "format":
                instructions.Add("Improve structure and formatting");
                instructions.Add("Add clear sections and organization");
                instructions.Add("Use consistent formatting patterns");
                break;
            default: // "all"
                instructions.Add("Comprehensively improve clarity, specificity, and format");
                instructions.Add("Enhance overall prompt effectiveness");
                break;
        }

        if (!string.IsNullOrWhiteSpace(request.CustomInstructions))
        {
            instructions.Add(request.CustomInstructions);
        }

        if (!string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            instructions.Add($"Ensure the improved prompt is in {request.TargetLanguage}");
        }

        return string.Join(". ", instructions);
    }

    private (string SystemPrompt, string UserTemplate) ParseImprovedPrompt(
        string improvedPrompt,
        PromptImprovementRequest originalRequest)
    {
        // Try to extract system and user parts from the improved prompt
        var systemMatch = System.Text.RegularExpressions.Regex.Match(
            improvedPrompt,
            @"\[SYSTEM PROMPT\]\s*([\s\S]*?)(?=\[USER PROMPT TEMPLATE\]|$)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var userMatch = System.Text.RegularExpressions.Regex.Match(
            improvedPrompt,
            @"\[USER PROMPT TEMPLATE\]\s*([\s\S]*?)$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var improvedSystem = systemMatch.Success
            ? systemMatch.Groups[1].Value.Trim()
            : improvedPrompt.Trim();

        var improvedUser = userMatch.Success
            ? userMatch.Groups[1].Value.Trim()
            : originalRequest.UserPromptTemplate;

        // If we couldn't parse sections and original had both, try to split intelligently
        if (!systemMatch.Success && !userMatch.Success &&
            !string.IsNullOrWhiteSpace(originalRequest.SystemPrompt) &&
            !string.IsNullOrWhiteSpace(originalRequest.UserPromptTemplate))
        {
            // Keep the improved prompt as the system prompt if it's the primary content
            improvedSystem = improvedPrompt.Trim();
            improvedUser = originalRequest.UserPromptTemplate; // Keep original user template
        }

        return (improvedSystem, improvedUser);
    }

    private List<PromptImprovementExplanation> ParseImprovements(List<AnthropicPromptChange> changes)
    {
        return changes.Select(c => new PromptImprovementExplanation
        {
            Area = c.Category ?? "General",
            Description = c.Description ?? "Improvement applied",
            Before = c.Original,
            After = c.Improved,
            Reason = c.Reasoning ?? "Enhances prompt effectiveness"
        }).ToList();
    }

    private string GetPromptImprovementSystemPrompt(string focusArea, string? targetLanguage)
    {
        var languageInstruction = string.IsNullOrWhiteSpace(targetLanguage)
            ? ""
            : $"The improved prompts must be in {targetLanguage}. ";

        return $@"You are an expert prompt engineer. Your task is to improve AI prompts to make them more effective.
{languageInstruction}
Analyze the provided prompts and improve them based on the focus area.

You MUST respond in the following JSON format:
{{
  ""improved_system_prompt"": ""the improved system prompt here"",
  ""improved_user_prompt_template"": ""the improved user prompt template here"",
  ""summary"": ""brief summary of improvements made"",
  ""improvements"": [
    {{
      ""area"": ""clarity|specificity|format|structure"",
      ""description"": ""what was changed"",
      ""before"": ""original text snippet (optional)"",
      ""after"": ""improved text snippet (optional)"",
      ""reason"": ""why this change improves the prompt""
    }}
  ]
}}

Focus area: {focusArea}

Guidelines:
- Preserve all {{{{variable}}}} placeholders exactly as they appear
- Maintain the original intent and purpose of the prompts
- Make prompts clearer, more specific, and better structured
- Add explicit output format instructions where helpful
- Remove ambiguity and vague language";
    }

    private string BuildUserPromptForImprovement(PromptImprovementRequest request)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("Please improve the following prompts:");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            sb.AppendLine("=== SYSTEM PROMPT ===");
            sb.AppendLine(request.SystemPrompt);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(request.UserPromptTemplate))
        {
            sb.AppendLine("=== USER PROMPT TEMPLATE ===");
            sb.AppendLine(request.UserPromptTemplate);
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(request.CustomInstructions))
        {
            sb.AppendLine("=== ADDITIONAL INSTRUCTIONS ===");
            sb.AppendLine(request.CustomInstructions);
        }

        return sb.ToString();
    }

    private PromptImprovementResultDto ParseStandardApiResponse(
        string responseText,
        PromptImprovementRequest request)
    {
        try
        {
            // Try to parse as JSON first
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var parsed = JsonSerializer.Deserialize<PromptImprovementJsonResponse>(
                    jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed != null)
                {
                    return new PromptImprovementResultDto
                    {
                        ImprovedSystemPrompt = parsed.ImprovedSystemPrompt ?? request.SystemPrompt,
                        ImprovedUserPromptTemplate = parsed.ImprovedUserPromptTemplate ?? request.UserPromptTemplate,
                        Summary = parsed.Summary ?? "Prompt improved successfully",
                        Improvements = parsed.Improvements?.Select(i => new PromptImprovementExplanation
                        {
                            Area = i.Area ?? "General",
                            Description = i.Description ?? "",
                            Before = i.Before,
                            After = i.After,
                            Reason = i.Reason ?? ""
                        }).ToList() ?? new List<PromptImprovementExplanation>()
                    };
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON response, using text extraction");
        }

        // Fallback: return the raw response as the improved system prompt
        return new PromptImprovementResultDto
        {
            ImprovedSystemPrompt = responseText.Trim(),
            ImprovedUserPromptTemplate = request.UserPromptTemplate,
            Summary = "Prompt improved (raw response)",
            Improvements = new List<PromptImprovementExplanation>
            {
                new PromptImprovementExplanation
                {
                    Area = "Overall",
                    Description = "Comprehensive prompt improvement",
                    Reason = "Enhanced for clarity and effectiveness"
                }
            }
        };
    }

    #region API Request/Response DTOs

    private class AnthropicImprovePromptRequest
    {
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("target_model")]
        public string TargetModel { get; set; } = string.Empty;

        [JsonPropertyName("custom_instructions")]
        public string? CustomInstructions { get; set; }
    }

    private class AnthropicImprovePromptResponse
    {
        [JsonPropertyName("improved_prompt")]
        public string ImprovedPrompt { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("changes")]
        public List<AnthropicPromptChange>? Changes { get; set; }

        [JsonPropertyName("usage")]
        public AnthropicUsage? Usage { get; set; }
    }

    private class AnthropicPromptChange
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("original")]
        public string? Original { get; set; }

        [JsonPropertyName("improved")]
        public string? Improved { get; set; }

        [JsonPropertyName("reasoning")]
        public string? Reasoning { get; set; }
    }

    private class AnthropicUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    private class ClaudeMessagesResponse
    {
        [JsonPropertyName("content")]
        public List<ClaudeContentBlock> Content { get; set; } = new();

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("usage")]
        public ClaudeUsage? Usage { get; set; }
    }

    private class ClaudeContentBlock
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private class ClaudeUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }

    private class PromptImprovementJsonResponse
    {
        [JsonPropertyName("improved_system_prompt")]
        public string? ImprovedSystemPrompt { get; set; }

        [JsonPropertyName("improved_user_prompt_template")]
        public string? ImprovedUserPromptTemplate { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("improvements")]
        public List<ImprovementItem>? Improvements { get; set; }
    }

    private class ImprovementItem
    {
        [JsonPropertyName("area")]
        public string? Area { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("before")]
        public string? Before { get; set; }

        [JsonPropertyName("after")]
        public string? After { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }

    #endregion
}

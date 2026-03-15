using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Uses Claude tool_use with forced tool choice to extract structured data reliably.
/// Instead of asking the LLM to output JSON (fragile), we define a tool with the exact schema
/// and force Claude to call it, guaranteeing well-formed structured output.
/// </summary>
public class StructuredExtractionService : IStructuredExtractionService
{
    private AnthropicClient? _client;
    private readonly ClaudeSettings _settings;
    private readonly IAIKeyResolver _keyResolver;
    private string _lastApiKey = string.Empty;
    private readonly ILogger<StructuredExtractionService> _logger;

    private const int ExtractionMaxTokens = 4000;
    private const float ExtractionTemperature = 0.2f;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public StructuredExtractionService(
        IOptions<ClaudeSettings> settings,
        IAIKeyResolver keyResolver,
        ILogger<StructuredExtractionService> logger)
    {
        _settings = settings.Value;
        _keyResolver = keyResolver;
        _logger = logger;

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _client = new AnthropicClient(new Anthropic.Core.ClientOptions
            {
                ApiKey = _settings.ApiKey
            });
            _lastApiKey = _settings.ApiKey;
        }
    }

    private async Task<AnthropicClient?> GetClientAsync(CancellationToken ct = default)
    {
        var config = await _keyResolver.ResolveClaudeAsync(ct);
        if (!string.IsNullOrEmpty(config.ApiKey) && config.ApiKey != _lastApiKey)
        {
            _client = new AnthropicClient(new Anthropic.Core.ClientOptions { ApiKey = config.ApiKey });
            _lastApiKey = config.ApiKey;
            _settings.Model = config.Model;
        }
        return _client;
    }

    public async Task<Result<SwotData>> ExtractSwotAsync(string content, string language, CancellationToken ct = default)
    {
        var client = await GetClientAsync(ct);
        if (client == null)
            return Result.Failure<SwotData>(Error.Failure("StructuredExtraction.NotConfigured", "Claude API key not configured"));

        var tool = new Tool
        {
            Name = "extract_swot",
            Description = "Extract SWOT analysis data from the provided business plan content.",
            InputSchema = BuildSchema(new
            {
                strengths = new { type = "array", items = new { type = "string" }, description = "List of strengths (3-6 items)" },
                weaknesses = new { type = "array", items = new { type = "string" }, description = "List of weaknesses (3-6 items)" },
                opportunities = new { type = "array", items = new { type = "string" }, description = "List of opportunities (3-6 items)" },
                threats = new { type = "array", items = new { type = "string" }, description = "List of threats (3-6 items)" }
            })
        };

        var langNote = language == "fr" ? "Extract items in French." : "Extract items in English.";
        var systemPrompt = $"You are a business analyst. Extract a SWOT analysis from the business plan content. {langNote} Each item should be a concise, actionable statement (1-2 sentences max).";

        var result = await CallToolAsync(tool, systemPrompt, $"Extract the SWOT analysis from this content:\n\n{content}", ct);
        if (result == null)
            return Result.Failure<SwotData>(Error.Failure("StructuredExtraction.Failed", "Failed to extract SWOT data"));

        return Result.Success(new SwotData
        {
            Strengths = GetStringList(result, "strengths"),
            Weaknesses = GetStringList(result, "weaknesses"),
            Opportunities = GetStringList(result, "opportunities"),
            Threats = GetStringList(result, "threats")
        });
    }

    public async Task<Result<FinancialMetricsData>> ExtractFinancialMetricsAsync(string content, string language, CancellationToken ct = default)
    {
        var client = await GetClientAsync(ct);
        if (client == null)
            return Result.Failure<FinancialMetricsData>(Error.Failure("StructuredExtraction.NotConfigured", "Claude API key not configured"));

        var tool = new Tool
        {
            Name = "extract_financials",
            Description = "Extract financial metrics and key figures from the provided business plan content.",
            InputSchema = BuildSchema(new
            {
                revenue = new { type = "string", description = "Projected revenue (e.g., '$500,000 Year 1')" },
                expenses = new { type = "string", description = "Total projected expenses" },
                profit = new { type = "string", description = "Net profit or loss" },
                break_even_point = new { type = "string", description = "Break-even point (time or units)" },
                gross_margin = new { type = "string", description = "Gross margin percentage" },
                net_margin = new { type = "string", description = "Net margin percentage" },
                line_items = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            label = new { type = "string" },
                            value = new { type = "string" },
                            period = new { type = "string" }
                        }
                    },
                    description = "Key financial line items"
                }
            })
        };

        var langNote = language == "fr" ? "Extract values in French format." : "Extract values in English format.";
        var systemPrompt = $"You are a financial analyst. Extract key financial metrics from the business plan content. {langNote} Use the exact numbers mentioned in the text.";

        var result = await CallToolAsync(tool, systemPrompt, $"Extract financial metrics from:\n\n{content}", ct);
        if (result == null)
            return Result.Failure<FinancialMetricsData>(Error.Failure("StructuredExtraction.Failed", "Failed to extract financial data"));

        var data = new FinancialMetricsData
        {
            Revenue = GetString(result, "revenue"),
            Expenses = GetString(result, "expenses"),
            Profit = GetString(result, "profit"),
            BreakEvenPoint = GetString(result, "break_even_point"),
            GrossMargin = GetString(result, "gross_margin"),
            NetMargin = GetString(result, "net_margin"),
            LineItems = new()
        };

        if (result.TryGetValue("line_items", out var lineItemsEl) && lineItemsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in lineItemsEl.EnumerateArray())
            {
                data.LineItems.Add(new FinancialLineItem
                {
                    Label = item.GetProperty("label").GetString() ?? "",
                    Value = item.GetProperty("value").GetString() ?? "",
                    Period = item.TryGetProperty("period", out var p) ? p.GetString() : null
                });
            }
        }

        return Result.Success(data);
    }

    public async Task<Result<RiskMitigationData>> ExtractRiskPairsAsync(string content, string language, CancellationToken ct = default)
    {
        var client = await GetClientAsync(ct);
        if (client == null)
            return Result.Failure<RiskMitigationData>(Error.Failure("StructuredExtraction.NotConfigured", "Claude API key not configured"));

        var tool = new Tool
        {
            Name = "extract_risks",
            Description = "Extract risk-mitigation pairs from the business plan content.",
            InputSchema = BuildSchema(new
            {
                pairs = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            risk = new { type = "string", description = "Risk description" },
                            severity = new { type = "string", description = "high, medium, or low" },
                            mitigation = new { type = "string", description = "Mitigation strategy" }
                        }
                    },
                    description = "List of risk-mitigation pairs (4-8 items)"
                }
            })
        };

        var langNote = language == "fr" ? "Extract in French." : "Extract in English.";
        var systemPrompt = $"You are a risk analyst. Extract risk-mitigation pairs from the business plan. {langNote} Each pair should have the risk, its severity, and a concrete mitigation strategy.";

        var result = await CallToolAsync(tool, systemPrompt, $"Extract risk-mitigation pairs from:\n\n{content}", ct);
        if (result == null)
            return Result.Failure<RiskMitigationData>(Error.Failure("StructuredExtraction.Failed", "Failed to extract risk data"));

        var data = new RiskMitigationData();
        if (result.TryGetValue("pairs", out var pairsEl) && pairsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var pair in pairsEl.EnumerateArray())
            {
                data.Pairs.Add(new RiskMitigationPair
                {
                    Risk = pair.GetProperty("risk").GetString() ?? "",
                    Severity = pair.TryGetProperty("severity", out var s) ? s.GetString() ?? "medium" : "medium",
                    Mitigation = pair.GetProperty("mitigation").GetString() ?? ""
                });
            }
        }

        return Result.Success(data);
    }

    public async Task<Result<SectionHighlightsData>> ExtractHighlightsAsync(string sectionTitle, string content, string language, CancellationToken ct = default)
    {
        var client = await GetClientAsync(ct);
        if (client == null)
            return Result.Failure<SectionHighlightsData>(Error.Failure("StructuredExtraction.NotConfigured", "Claude API key not configured"));

        var tool = new Tool
        {
            Name = "extract_highlights",
            Description = "Extract key highlights and a one-liner summary from the business plan section.",
            InputSchema = BuildSchema(new
            {
                key_points = new { type = "array", items = new { type = "string" }, description = "3-6 key bullet points" },
                one_liner_summary = new { type = "string", description = "A single sentence summary of the section" }
            })
        };

        var langNote = language == "fr" ? "Extract in French." : "Extract in English.";
        var systemPrompt = $"You are a business plan reviewer. Extract the key highlights from the section. {langNote} Focus on the most impactful and investor-relevant points.";

        var result = await CallToolAsync(tool, systemPrompt, $"Section: {sectionTitle}\n\nContent:\n{content}", ct);
        if (result == null)
            return Result.Failure<SectionHighlightsData>(Error.Failure("StructuredExtraction.Failed", "Failed to extract highlights"));

        return Result.Success(new SectionHighlightsData
        {
            SectionTitle = sectionTitle,
            KeyPoints = GetStringList(result, "key_points"),
            OneLinerSummary = GetString(result, "one_liner_summary")
        });
    }

    // ── Core tool_use call ───────────────────────────────────

    private async Task<IReadOnlyDictionary<string, JsonElement>?> CallToolAsync(
        Tool tool, string systemPrompt, string userPrompt, CancellationToken ct)
    {
        try
        {
            var parameters = new MessageCreateParams
            {
                Model = _settings.Model,
                MaxTokens = ExtractionMaxTokens,
                Temperature = ExtractionTemperature,
                System = systemPrompt,
                Messages = new List<MessageParam>
                {
                    new() { Role = Role.User, Content = userPrompt }
                },
                Tools = new List<ToolUnion> { tool },
                ToolChoice = new ToolChoiceTool { Name = tool.Name } // Force the specific tool
            };

            var response = await _client!.Messages.Create(parameters); // _client guaranteed non-null by caller's GetClientAsync check

            if (response.Content != null)
            {
                foreach (var block in response.Content)
                {
                    if (block.TryPickToolUse(out var toolUseBlock))
                    {
                        _logger.LogInformation("Structured extraction via tool_use: {ToolName} ({OutputTokens} tokens)",
                            toolUseBlock.Name, response.Usage?.OutputTokens);
                        return toolUseBlock.Input;
                    }
                }
            }

            _logger.LogWarning("No tool_use block in response for {Tool}", tool.Name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Structured extraction failed for tool {Tool}", tool.Name);
            return null;
        }
    }

    // ── Helpers ──────────────────────────────────────────────

    private static InputSchema BuildSchema(object properties)
    {
        var schemaDict = new Dictionary<string, JsonElement>
        {
            { "type", JsonSerializer.SerializeToElement("object") },
            { "properties", JsonSerializer.SerializeToElement(properties) }
        };
        return InputSchema.FromRawUnchecked(schemaDict);
    }

    private static string? GetString(IReadOnlyDictionary<string, JsonElement> data, string key)
    {
        return data.TryGetValue(key, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;
    }

    private static List<string> GetStringList(IReadOnlyDictionary<string, JsonElement> data, string key)
    {
        if (!data.TryGetValue(key, out var el) || el.ValueKind != JsonValueKind.Array)
            return new List<string>();

        return el.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.String)
            .Select(e => e.GetString() ?? "")
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }
}

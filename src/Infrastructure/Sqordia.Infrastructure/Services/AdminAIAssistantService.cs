using System.Runtime.CompilerServices;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Services;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Admin AI Assistant with tool-use loop for querying system data
/// </summary>
public class AdminAIAssistantService : IAdminAIAssistantService
{
    private readonly ILogger<AdminAIAssistantService> _logger;
    private readonly ClaudeSettings _settings;
    private readonly AnthropicClient? _client;
    private readonly IAdminDashboardService _dashboardService;
    private const int MaxToolIterations = 5;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public AdminAIAssistantService(
        IOptions<ClaudeSettings> settings,
        ILogger<AdminAIAssistantService> logger,
        IAdminDashboardService dashboardService)
    {
        _logger = logger;
        _settings = settings.Value;
        _dashboardService = dashboardService;

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _client = new AnthropicClient(new Anthropic.Core.ClientOptions { ApiKey = _settings.ApiKey });
        }
    }

    public async IAsyncEnumerable<AdminAIStreamEvent> StreamAdminQueryAsync(
        List<AdminAIMessage> conversationHistory,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            yield return new AdminAIStreamEvent { Type = "error", Error = "AI service not configured" };
            yield break;
        }

        var systemPrompt = @"You are Sqordia's Admin AI Assistant. You help administrators understand system data by querying the platform's analytics.
When asked about users, organizations, business plans, AI usage, or system health, use the available tools to fetch real data.
Present data clearly with specific numbers. Be concise but thorough.";

        var messages = conversationHistory.Select(m => new MessageParam
        {
            Role = m.Role == "user" ? Role.User : Role.Assistant,
            Content = m.Content
        }).ToList();

        var tools = BuildToolDefinitions();
        var iteration = 0;

        while (iteration < MaxToolIterations)
        {
            iteration++;

            var parameters = new MessageCreateParams
            {
                Model = _settings.Model,
                MaxTokens = 4000,
                System = systemPrompt,
                Messages = messages,
                Tools = tools,
                ToolChoice = new ToolChoiceAuto()
            };

            var response = await _client.Messages.Create(parameters);

            // Check if response has tool use blocks
            var hasToolUse = false;
            var toolResults = new List<ContentBlockParam>();

            if (response.Content != null)
            {
                foreach (var block in response.Content)
                {
                    if (block.TryPickToolUse(out var toolUseBlock))
                    {
                        hasToolUse = true;

                        yield return new AdminAIStreamEvent
                        {
                            Type = "tool_start",
                            ToolName = toolUseBlock.Name
                        };

                        var toolResult = await ExecuteToolAsync(toolUseBlock.Name, toolUseBlock.Input, cancellationToken);

                        yield return new AdminAIStreamEvent
                        {
                            Type = "tool_end",
                            ToolName = toolUseBlock.Name
                        };

                        toolResults.Add(new ToolResultBlockParam
                        {
                            ToolUseID = toolUseBlock.ID,
                            Content = toolResult
                        });
                    }
                    else if (block.TryPickText(out var textBlock))
                    {
                        // If there's text alongside tool calls, yield it
                        if (!string.IsNullOrEmpty(textBlock.Text))
                        {
                            yield return new AdminAIStreamEvent
                            {
                                Type = "token",
                                Content = textBlock.Text
                            };
                        }
                    }
                }
            }

            if (hasToolUse && toolResults.Count > 0)
            {
                // Add assistant response to messages (convert ContentBlock → ContentBlockParam)
                var assistantBlocks = new List<ContentBlockParam>();
                foreach (var b in response.Content!)
                {
                    if (b.TryPickText(out var tb))
                        assistantBlocks.Add(new TextBlockParam { Text = tb.Text });
                    else if (b.TryPickToolUse(out var tu))
                        assistantBlocks.Add(new ToolUseBlockParam { ID = tu.ID, Name = tu.Name, Input = tu.Input });
                }
                messages.Add(new MessageParam
                {
                    Role = Role.Assistant,
                    Content = assistantBlocks
                });

                // Add tool results
                messages.Add(new MessageParam
                {
                    Role = Role.User,
                    Content = toolResults
                });

                continue; // Loop to process tool results
            }

            // Final text response - stream it
            if (response.Content != null)
            {
                foreach (var block in response.Content)
                {
                    if (block.TryPickText(out var textBlock))
                    {
                        // Yield in chunks to simulate streaming
                        var text = textBlock.Text;
                        const int chunkSize = 20;
                        for (int i = 0; i < text.Length; i += chunkSize)
                        {
                            var chunk = text.Substring(i, Math.Min(chunkSize, text.Length - i));
                            yield return new AdminAIStreamEvent { Type = "token", Content = chunk };
                        }
                    }
                }
            }

            break; // Done
        }

        yield return new AdminAIStreamEvent { Type = "done" };
    }

    private static InputSchema BuildSchema(object properties)
    {
        var schemaDict = new Dictionary<string, JsonElement>
        {
            { "type", JsonSerializer.SerializeToElement("object") },
            { "properties", JsonSerializer.SerializeToElement(properties) }
        };
        return InputSchema.FromRawUnchecked(schemaDict);
    }

    private List<ToolUnion> BuildToolDefinitions()
    {
        return new List<ToolUnion>
        {
            new Tool
            {
                Name = "query_system_overview",
                Description = "Get comprehensive system overview including user counts, organization counts, business plan counts, AI usage stats, and system performance metrics.",
                InputSchema = BuildSchema(new { })
            },
            new Tool
            {
                Name = "query_users",
                Description = "Search and filter users. Returns paginated user data with email, status, login count, and business plan count.",
                InputSchema = BuildSchema(new
                {
                    searchTerm = new { type = "string", description = "Search by name or email" },
                    status = new { type = "string", description = "Filter by status: Active, Inactive, Suspended" },
                    pageSize = new { type = "integer", description = "Number of results (default 10)" }
                })
            },
            new Tool
            {
                Name = "query_organizations",
                Description = "Search and filter organizations. Returns organization data with member counts and business plan counts.",
                InputSchema = BuildSchema(new
                {
                    searchTerm = new { type = "string", description = "Search by name" },
                    pageSize = new { type = "integer", description = "Number of results (default 10)" }
                })
            },
            new Tool
            {
                Name = "query_business_plans",
                Description = "Search business plans. Returns plan data with type, status, completion progress.",
                InputSchema = BuildSchema(new
                {
                    searchTerm = new { type = "string", description = "Search by title" },
                    status = new { type = "string", description = "Filter by status" },
                    pageSize = new { type = "integer", description = "Number of results (default 10)" }
                })
            },
            new Tool
            {
                Name = "query_ai_usage",
                Description = "Get AI usage statistics for a date range including request counts, costs, and feature breakdown.",
                InputSchema = BuildSchema(new
                {
                    daysBack = new { type = "integer", description = "Number of days to look back (default 30)" }
                })
            },
            new Tool
            {
                Name = "query_system_health",
                Description = "Get system health metrics including CPU, memory, database status, and alerts.",
                InputSchema = BuildSchema(new { })
            }
        };
    }

    private async Task<string> ExecuteToolAsync(
        string toolName,
        IReadOnlyDictionary<string, JsonElement> input,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing admin AI tool: {ToolName}", toolName);

            return toolName switch
            {
                "query_system_overview" => await ExecuteSystemOverviewAsync(cancellationToken),
                "query_users" => await ExecuteQueryUsersAsync(input, cancellationToken),
                "query_organizations" => await ExecuteQueryOrganizationsAsync(input, cancellationToken),
                "query_business_plans" => await ExecuteQueryBusinessPlansAsync(input, cancellationToken),
                "query_ai_usage" => await ExecuteQueryAIUsageAsync(input, cancellationToken),
                "query_system_health" => await ExecuteSystemHealthAsync(cancellationToken),
                _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {toolName}" })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool: {ToolName}", toolName);
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private async Task<string> ExecuteSystemOverviewAsync(CancellationToken ct)
    {
        var result = await _dashboardService.GetSystemOverviewAsync(ct);
        if (!result.IsSuccess) return JsonSerializer.Serialize(new { error = "Failed to get overview" });
        return JsonSerializer.Serialize(result.Value, JsonOpts);
    }

    private async Task<string> ExecuteQueryUsersAsync(IReadOnlyDictionary<string, JsonElement> input, CancellationToken ct)
    {
        var request = new AdminUserRequest
        {
            SearchTerm = input.TryGetValue("searchTerm", out var st) ? st.GetString() : null,
            PageSize = input.TryGetValue("pageSize", out var ps) ? ps.GetInt32() : 10
        };

        if (input.TryGetValue("status", out var status))
        {
            if (Enum.TryParse<UserStatus>(status.GetString(), true, out var userStatus))
                request.Status = userStatus;
        }

        var result = await _dashboardService.GetUsersAsync(request, ct);
        if (!result.IsSuccess) return JsonSerializer.Serialize(new { error = "Failed to query users" });
        return JsonSerializer.Serialize(new
        {
            totalCount = result.Value!.TotalCount,
            users = result.Value.Items.Select(u => new
            {
                u.Email, u.FirstName, u.LastName, Status = u.Status.ToString(),
                u.CreatedAt, u.LastLoginAt, u.BusinessPlanCount, u.LoginCount
            })
        }, JsonOpts);
    }

    private async Task<string> ExecuteQueryOrganizationsAsync(IReadOnlyDictionary<string, JsonElement> input, CancellationToken ct)
    {
        var request = new AdminOrganizationRequest
        {
            SearchTerm = input.TryGetValue("searchTerm", out var st) ? st.GetString() : null,
            PageSize = input.TryGetValue("pageSize", out var ps) ? ps.GetInt32() : 10
        };

        var result = await _dashboardService.GetOrganizationsAsync(request, ct);
        if (!result.IsSuccess) return JsonSerializer.Serialize(new { error = "Failed to query organizations" });
        return JsonSerializer.Serialize(new
        {
            totalCount = result.Value!.TotalCount,
            organizations = result.Value.Items.Select(o => new
            {
                o.Name, o.OrganizationType, o.IsActive, o.MemberCount, o.BusinessPlanCount, o.CreatedAt
            })
        }, JsonOpts);
    }

    private async Task<string> ExecuteQueryBusinessPlansAsync(IReadOnlyDictionary<string, JsonElement> input, CancellationToken ct)
    {
        var request = new AdminBusinessPlanRequest
        {
            SearchTerm = input.TryGetValue("searchTerm", out var st) ? st.GetString() : null,
            Status = input.TryGetValue("status", out var s) ? s.GetString() : null,
            PageSize = input.TryGetValue("pageSize", out var ps) ? ps.GetInt32() : 10
        };

        var result = await _dashboardService.GetBusinessPlansAsync(request, ct);
        if (!result.IsSuccess) return JsonSerializer.Serialize(new { error = "Failed to query business plans" });
        return JsonSerializer.Serialize(new
        {
            totalCount = result.Value!.TotalCount,
            plans = result.Value.Items.Select(p => new
            {
                p.Title, p.PlanType, p.Status, p.CreatedAt, p.OrganizationName, p.SectionCount, p.CompletedSectionCount
            })
        }, JsonOpts);
    }

    private async Task<string> ExecuteQueryAIUsageAsync(IReadOnlyDictionary<string, JsonElement> input, CancellationToken ct)
    {
        var daysBack = input.TryGetValue("daysBack", out var db) ? db.GetInt32() : 30;
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-daysBack);

        var result = await _dashboardService.GetAIUsageStatsAsync(startDate, endDate, ct);
        if (!result.IsSuccess) return JsonSerializer.Serialize(new { error = "Failed to get AI usage" });
        return JsonSerializer.Serialize(result.Value, JsonOpts);
    }

    private async Task<string> ExecuteSystemHealthAsync(CancellationToken ct)
    {
        var result = await _dashboardService.GetSystemHealthAsync(ct);
        if (!result.IsSuccess) return JsonSerializer.Serialize(new { error = "Failed to get health" });
        return JsonSerializer.Serialize(result.Value, JsonOpts);
    }
}

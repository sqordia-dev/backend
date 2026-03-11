using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Claude-powered document agent that uses tool_use to produce structured document blueprints.
/// Instead of asking the LLM to output formatted text, we define tools for each document element
/// (heading, table, bullet_list, swot_grid, etc.) and let Claude decide how to best structure
/// the business plan content for each format.
/// </summary>
public class DocumentAgentService : IDocumentAgentService
{
    private readonly AnthropicClient? _client;
    private readonly ClaudeSettings _settings;
    private readonly ILogger<DocumentAgentService> _logger;

    private const int MaxAgentTokens = 8000;
    private const float AgentTemperature = 0.4f;
    private const int MaxToolIterations = 15;
    private const int MaxContentChars = 4000;

    public DocumentAgentService(
        IOptions<ClaudeSettings> settings,
        ILogger<DocumentAgentService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _client = new AnthropicClient(new Anthropic.Core.ClientOptions
            {
                ApiKey = _settings.ApiKey
            });
        }
    }

    // ── Word Blueprint ───────────────────────────────────────

    public async Task<Result<WordDocumentBlueprint>> GenerateWordBlueprintAsync(
        DocumentAgentRequest request, CancellationToken ct = default)
    {
        if (_client == null)
            return Result.Failure<WordDocumentBlueprint>(Error.Failure("DocumentAgent.NotConfigured", "Claude API key not configured"));

        var blocks = new List<WordBlock>();
        var tools = BuildWordTools();

        var langNote = request.Language == "fr" ? "Produis tout le contenu en français." : "Produce all content in English.";

        var systemPrompt =
            $"You are a professional document architect. {langNote}\n\n" +
            "You are structuring a business plan for a professional Word document.\n" +
            "Use the tools to build the document block by block.\n" +
            "Rules:\n" +
            "- Start each section with a heading (level 1 for section titles, level 2 for subsections)\n" +
            "- Convert lists and enumerations into bullet_list blocks\n" +
            "- Use table blocks for financial data, comparisons, and structured data\n" +
            "- Use swot_grid for SWOT analysis sections\n" +
            "- Use callout blocks to highlight key insights or warnings\n" +
            "- Add page_break between major sections\n" +
            "- Preserve ALL data, numbers, and specific details from the source\n" +
            "- Write professionally — this is for bank presentations";

        var sectionsSummary = string.Join("\n\n", request.Sections.Select(
            s => $"=== {s.Key} ===\n{Truncate(s.Value, MaxContentChars)}"
        ));

        var userPrompt =
            $"Company: {request.CompanyName}\nPlan: {request.PlanTitle}\n\n" +
            $"Structure the following business plan sections into a professional Word document using the available tools:\n\n{sectionsSummary}";

        try
        {
            blocks = await RunAgentLoopAsync<WordBlock>(tools, systemPrompt, userPrompt, ParseWordBlock, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Word blueprint agent failed");
            return Result.Failure<WordDocumentBlueprint>(Error.Failure("DocumentAgent.Failed", ex.Message));
        }

        _logger.LogInformation("Word blueprint generated with {BlockCount} blocks", blocks.Count);
        return Result.Success(new WordDocumentBlueprint { Blocks = blocks });
    }

    // ── PDF Blueprint ─────────────────────────────────────────

    public async Task<Result<PdfDocumentBlueprint>> GeneratePdfBlueprintAsync(
        DocumentAgentRequest request, CancellationToken ct = default)
    {
        if (_client == null)
            return Result.Failure<PdfDocumentBlueprint>(Error.Failure("DocumentAgent.NotConfigured", "Claude API key not configured"));

        var tools = BuildPdfTools();

        var langNote = request.Language == "fr" ? "Produis tout le contenu en français." : "Produce all content in English.";
        var dateStr = DateTime.UtcNow.ToString("MMMM yyyy");

        var systemPrompt =
            $"You are a professional PDF document architect. {langNote}\n\n" +
            "You are structuring a business plan for a polished, print-ready PDF document.\n" +
            "Use the tools to build the document block by block.\n\n" +
            "Rules:\n" +
            "- FIRST call set_cover_page with the company name, title, and date\n" +
            "- THEN call set_header_footer for consistent page headers/footers\n" +
            "- Start each major section with a heading (level 1) and add page_break before it\n" +
            "- Use level 2 for subsections, level 3 for sub-subsections\n" +
            "- Convert lists into bullet_list blocks\n" +
            "- Use table blocks for financial data, comparisons, projections\n" +
            "- Use swot_grid for SWOT analysis\n" +
            "- Use metrics_panel for key KPIs (max 4 metrics per panel)\n" +
            "- Use chart_placeholder for data that should be visualized (revenue trends, market share, etc.)\n" +
            "- Use callout blocks to highlight key insights or warnings\n" +
            "- Preserve ALL data, numbers, and specific details from the source\n" +
            "- This is for bank presentations — be professional and data-driven\n" +
            "- A table of contents will be auto-generated from level-1 headings";

        var sectionsSummary = string.Join("\n\n", request.Sections.Select(
            s => $"=== {s.Key} ===\n{Truncate(s.Value, MaxContentChars)}"
        ));

        var userPrompt =
            $"Company: {request.CompanyName}\nPlan: {request.PlanTitle}\nDate: {dateStr}\n\n" +
            $"Structure the following business plan sections into a professional PDF document using the available tools:\n\n{sectionsSummary}";

        var blueprint = new PdfDocumentBlueprint();

        try
        {
            var blocks = await RunPdfAgentLoopAsync(tools, systemPrompt, userPrompt, blueprint, ct);
            blueprint.Blocks = blocks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF blueprint agent failed");
            return Result.Failure<PdfDocumentBlueprint>(Error.Failure("DocumentAgent.Failed", ex.Message));
        }

        _logger.LogInformation("PDF blueprint generated with {BlockCount} blocks, cover={HasCover}, header={HasHeader}",
            blueprint.Blocks.Count, blueprint.CoverPage != null, blueprint.HeaderFooter != null);
        return Result.Success(blueprint);
    }

    private async Task<List<PdfBlock>> RunPdfAgentLoopAsync(
        List<ToolUnion> tools,
        string systemPrompt,
        string userPrompt,
        PdfDocumentBlueprint blueprint,
        CancellationToken ct)
    {
        var blocks = new List<PdfBlock>();
        var messages = new List<MessageParam>
        {
            new() { Role = Role.User, Content = userPrompt }
        };

        for (var iteration = 0; iteration < MaxToolIterations; iteration++)
        {
            var parameters = new MessageCreateParams
            {
                Model = _settings.Model,
                MaxTokens = MaxAgentTokens,
                Temperature = AgentTemperature,
                System = systemPrompt,
                Messages = messages,
                Tools = tools,
                ToolChoice = new ToolChoiceAuto()
            };

            var response = await _client!.Messages.Create(parameters);

            var hasToolUse = false;
            var assistantBlocks = new List<ContentBlockParam>();
            var toolResults = new List<ContentBlockParam>();

            if (response.Content != null)
            {
                foreach (var block in response.Content)
                {
                    if (block.TryPickToolUse(out var toolUseBlock))
                    {
                        hasToolUse = true;
                        assistantBlocks.Add(new ToolUseBlockParam
                        {
                            ID = toolUseBlock.ID,
                            Name = toolUseBlock.Name,
                            Input = toolUseBlock.Input
                        });

                        // Handle special PDF-level tools
                        if (toolUseBlock.Name == "set_cover_page")
                        {
                            blueprint.CoverPage = ParseCoverPage(toolUseBlock.Input);
                        }
                        else if (toolUseBlock.Name == "set_header_footer")
                        {
                            blueprint.HeaderFooter = ParseHeaderFooter(toolUseBlock.Input);
                        }
                        else
                        {
                            var parsed = ParsePdfBlock(toolUseBlock.Name, toolUseBlock.Input);
                            if (parsed != null)
                                blocks.Add(parsed);
                        }

                        toolResults.Add(new ToolResultBlockParam
                        {
                            ToolUseID = toolUseBlock.ID,
                            Content = "Block added successfully. Continue with the next block."
                        });
                    }
                    else if (block.TryPickText(out var textBlock))
                    {
                        assistantBlocks.Add(new TextBlockParam { Text = textBlock.Text });
                    }
                }
            }

            if (!hasToolUse || response.StopReason == "end_turn")
                break;

            messages.Add(new MessageParam { Role = Role.Assistant, Content = assistantBlocks });
            messages.Add(new MessageParam { Role = Role.User, Content = toolResults });
        }

        return blocks;
    }

    // ── Presentation Blueprint ───────────────────────────────

    public async Task<Result<PresentationBlueprint>> GeneratePresentationBlueprintAsync(
        DocumentAgentRequest request, CancellationToken ct = default)
    {
        if (_client == null)
            return Result.Failure<PresentationBlueprint>(Error.Failure("DocumentAgent.NotConfigured", "Claude API key not configured"));

        var tools = BuildPresentationTools();

        var langNote = request.Language == "fr" ? "Produis tout le contenu en français." : "Produce all content in English.";

        var systemPrompt =
            $"You are a presentation designer. {langNote}\n\n" +
            "Design a compelling investor presentation from a business plan.\n" +
            "Rules:\n" +
            "- Start with a title slide (company name, plan title, date)\n" +
            "- Use section_divider slides between major topics\n" +
            "- Max 5 bullets per content slide (concise — 10-15 words each)\n" +
            "- Use two_column slides for comparisons (risks vs mitigations, current vs projected)\n" +
            "- Use swot slides for SWOT analysis\n" +
            "- Use table slides for financial data\n" +
            "- Use metrics slides for key KPIs (max 4 metrics)\n" +
            "- End with a thank_you slide\n" +
            "- Add speaker_notes to each slide\n" +
            "- Target 12-18 slides total\n" +
            "- Be specific — use actual numbers and data from the content";

        var sectionsSummary = string.Join("\n\n", request.Sections.Select(
            s => $"=== {s.Key} ===\n{Truncate(s.Value, MaxContentChars)}"
        ));

        var userPrompt =
            $"Company: {request.CompanyName}\nPlan: {request.PlanTitle}\n\n" +
            $"Design a presentation from:\n\n{sectionsSummary}";

        try
        {
            var slides = await RunAgentLoopAsync<SlideBlueprint>(tools, systemPrompt, userPrompt, ParseSlideBlock, ct);
            _logger.LogInformation("Presentation blueprint generated with {SlideCount} slides", slides.Count);
            return Result.Success(new PresentationBlueprint { Slides = slides });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Presentation blueprint agent failed");
            return Result.Failure<PresentationBlueprint>(Error.Failure("DocumentAgent.Failed", ex.Message));
        }
    }

    // ── Spreadsheet Blueprint ────────────────────────────────

    public async Task<Result<SpreadsheetBlueprint>> GenerateSpreadsheetBlueprintAsync(
        DocumentAgentRequest request, CancellationToken ct = default)
    {
        if (_client == null)
            return Result.Failure<SpreadsheetBlueprint>(Error.Failure("DocumentAgent.NotConfigured", "Claude API key not configured"));

        var tool = new Tool
        {
            Name = "create_spreadsheet",
            Description = "Create a multi-sheet Excel workbook from business plan financial data.",
            InputSchema = BuildSchema(new
            {
                sheets = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new { type = "string", description = "Sheet name" },
                            headers = new { type = "array", items = new { type = "string" } },
                            rows = new { type = "array", items = new { type = "array", items = new { type = "string" } } },
                            summary_row = new { type = "array", items = new { type = "string" }, description = "Optional totals row" },
                            chart_type = new { type = "string", description = "Optional: bar, line, or pie" },
                            chart_title = new { type = "string", description = "Optional chart title" }
                        }
                    },
                    description = "List of sheets (3-6 sheets)"
                }
            })
        };

        var langNote = request.Language == "fr" ? "Produis tout en français." : "Produce all in English.";
        var systemPrompt =
            $"You are a financial analyst creating an Excel workbook. {langNote}\n" +
            "Create sheets for: Financial Projections (3-year P&L), Revenue Breakdown, Expenses, Cash Flow, Key Metrics.\n" +
            "Use actual numbers from the content. Fill in reasonable estimates where data is missing.";

        var financialContent = request.Sections
            .Where(s => s.Key.Contains("Financial", StringComparison.OrdinalIgnoreCase)
                     || s.Key.Contains("Funding", StringComparison.OrdinalIgnoreCase)
                     || s.Key.Contains("Revenue", StringComparison.OrdinalIgnoreCase))
            .Select(s => $"{s.Key}:\n{s.Value}")
            .ToList();

        if (financialContent.Count == 0)
            financialContent = request.Sections.Select(s => $"{s.Key}:\n{Truncate(s.Value, 1000)}").ToList();

        var userPrompt = $"Company: {request.CompanyName}\n\n{string.Join("\n\n", financialContent)}";

        try
        {
            var parameters = new MessageCreateParams
            {
                Model = _settings.Model,
                MaxTokens = MaxAgentTokens,
                Temperature = AgentTemperature,
                System = systemPrompt,
                Messages = new List<MessageParam>
                {
                    new() { Role = Role.User, Content = userPrompt }
                },
                Tools = new List<ToolUnion> { tool },
                ToolChoice = new ToolChoiceTool { Name = tool.Name }
            };

            var response = await _client!.Messages.Create(parameters);

            if (response.Content != null)
            {
                foreach (var block in response.Content)
                {
                    if (block.TryPickToolUse(out var toolUseBlock))
                    {
                        var blueprint = ParseSpreadsheetFromTool(toolUseBlock.Input);
                        _logger.LogInformation("Spreadsheet blueprint generated with {SheetCount} sheets", blueprint.Sheets.Count);
                        return Result.Success(blueprint);
                    }
                }
            }

            return Result.Failure<SpreadsheetBlueprint>(Error.Failure("DocumentAgent.NoOutput", "Agent produced no spreadsheet"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Spreadsheet blueprint agent failed");
            return Result.Failure<SpreadsheetBlueprint>(Error.Failure("DocumentAgent.Failed", ex.Message));
        }
    }

    // ── Agent Loop ───────────────────────────────────────────

    private async Task<List<T>> RunAgentLoopAsync<T>(
        List<ToolUnion> tools,
        string systemPrompt,
        string userPrompt,
        Func<string, IReadOnlyDictionary<string, JsonElement>, T?> parser,
        CancellationToken ct)
    {
        var results = new List<T>();
        var messages = new List<MessageParam>
        {
            new() { Role = Role.User, Content = userPrompt }
        };

        for (var iteration = 0; iteration < MaxToolIterations; iteration++)
        {
            var parameters = new MessageCreateParams
            {
                Model = _settings.Model,
                MaxTokens = MaxAgentTokens,
                Temperature = AgentTemperature,
                System = systemPrompt,
                Messages = messages,
                Tools = tools,
                ToolChoice = new ToolChoiceAuto()
            };

            var response = await _client!.Messages.Create(parameters);

            var hasToolUse = false;
            var assistantBlocks = new List<ContentBlockParam>();
            var toolResults = new List<ContentBlockParam>();

            if (response.Content != null)
            {
                foreach (var block in response.Content)
                {
                    if (block.TryPickToolUse(out var toolUseBlock))
                    {
                        hasToolUse = true;
                        assistantBlocks.Add(new ToolUseBlockParam
                        {
                            ID = toolUseBlock.ID,
                            Name = toolUseBlock.Name,
                            Input = toolUseBlock.Input
                        });

                        var parsed = parser(toolUseBlock.Name, toolUseBlock.Input);
                        if (parsed != null)
                            results.Add(parsed);

                        toolResults.Add(new ToolResultBlockParam
                        {
                            ToolUseID = toolUseBlock.ID,
                            Content = "Block added successfully. Continue with the next block."
                        });
                    }
                    else if (block.TryPickText(out var textBlock))
                    {
                        assistantBlocks.Add(new TextBlockParam { Text = textBlock.Text });
                    }
                }
            }

            if (!hasToolUse || response.StopReason == "end_turn")
                break;

            // Continue the conversation
            messages.Add(new MessageParam { Role = Role.Assistant, Content = assistantBlocks });
            messages.Add(new MessageParam { Role = Role.User, Content = toolResults });
        }

        return results;
    }

    // ── Word Tools ───────────────────────────────────────────

    private List<ToolUnion> BuildWordTools()
    {
        return new List<ToolUnion>
        {
            new Tool
            {
                Name = "add_heading",
                Description = "Add a heading to the document.",
                InputSchema = BuildSchema(new
                {
                    text = new { type = "string", description = "Heading text" },
                    level = new { type = "integer", description = "Heading level: 1 (section), 2 (subsection), 3 (sub-subsection)" }
                })
            },
            new Tool
            {
                Name = "add_paragraph",
                Description = "Add a paragraph of professional prose text.",
                InputSchema = BuildSchema(new
                {
                    text = new { type = "string", description = "Paragraph text (preserve all data and numbers)" }
                })
            },
            new Tool
            {
                Name = "add_bullet_list",
                Description = "Add a bullet list for key points, features, or enumerations.",
                InputSchema = BuildSchema(new
                {
                    items = new { type = "array", items = new { type = "string" }, description = "List items (3-8 items)" }
                })
            },
            new Tool
            {
                Name = "add_table",
                Description = "Add a data table for financial data, comparisons, or structured information.",
                InputSchema = BuildSchema(new
                {
                    caption = new { type = "string", description = "Table caption/title" },
                    headers = new { type = "array", items = new { type = "string" }, description = "Column headers" },
                    rows = new { type = "array", items = new { type = "array", items = new { type = "string" } }, description = "Data rows" }
                })
            },
            new Tool
            {
                Name = "add_swot_grid",
                Description = "Add a SWOT analysis grid with four quadrants.",
                InputSchema = BuildSchema(new
                {
                    strengths = new { type = "array", items = new { type = "string" } },
                    weaknesses = new { type = "array", items = new { type = "string" } },
                    opportunities = new { type = "array", items = new { type = "string" } },
                    threats = new { type = "array", items = new { type = "string" } }
                })
            },
            new Tool
            {
                Name = "add_callout",
                Description = "Add a highlighted callout box for key insights, warnings, or important notes.",
                InputSchema = BuildSchema(new
                {
                    text = new { type = "string", description = "Callout text" },
                    style = new { type = "string", description = "highlight, warning, or success" }
                })
            },
            new Tool
            {
                Name = "add_page_break",
                Description = "Add a page break between major sections.",
                InputSchema = BuildSchema(new { })
            }
        };
    }

    // ── Presentation Tools ───────────────────────────────────

    private List<ToolUnion> BuildPresentationTools()
    {
        return new List<ToolUnion>
        {
            new Tool
            {
                Name = "add_title_slide",
                Description = "Add the title slide (first slide of the presentation).",
                InputSchema = BuildSchema(new
                {
                    title = new { type = "string", description = "Company name" },
                    subtitle = new { type = "string", description = "Plan title or tagline" },
                    speaker_notes = new { type = "string", description = "Notes for the presenter" }
                })
            },
            new Tool
            {
                Name = "add_section_divider",
                Description = "Add a section divider slide to separate major topics.",
                InputSchema = BuildSchema(new
                {
                    title = new { type = "string", description = "Section title" },
                    subtitle = new { type = "string", description = "Brief section description" }
                })
            },
            new Tool
            {
                Name = "add_content_slide",
                Description = "Add a slide with bullet points (3-5 bullets, 10-15 words each).",
                InputSchema = BuildSchema(new
                {
                    title = new { type = "string", description = "Slide title" },
                    bullets = new { type = "array", items = new { type = "string" }, description = "Bullet points" },
                    speaker_notes = new { type = "string", description = "Notes for the presenter" }
                })
            },
            new Tool
            {
                Name = "add_two_column_slide",
                Description = "Add a slide with two columns for comparisons (e.g., risks vs mitigations, before vs after).",
                InputSchema = BuildSchema(new
                {
                    title = new { type = "string" },
                    left_column_title = new { type = "string" },
                    left_column_bullets = new { type = "array", items = new { type = "string" } },
                    right_column_title = new { type = "string" },
                    right_column_bullets = new { type = "array", items = new { type = "string" } },
                    speaker_notes = new { type = "string" }
                })
            },
            new Tool
            {
                Name = "add_swot_slide",
                Description = "Add a SWOT analysis slide with four quadrants.",
                InputSchema = BuildSchema(new
                {
                    title = new { type = "string" },
                    strengths = new { type = "array", items = new { type = "string" } },
                    weaknesses = new { type = "array", items = new { type = "string" } },
                    opportunities = new { type = "array", items = new { type = "string" } },
                    threats = new { type = "array", items = new { type = "string" } },
                    speaker_notes = new { type = "string" }
                })
            },
            new Tool
            {
                Name = "add_table_slide",
                Description = "Add a slide with a data table (e.g., financial projections).",
                InputSchema = BuildSchema(new
                {
                    title = new { type = "string" },
                    headers = new { type = "array", items = new { type = "string" } },
                    rows = new { type = "array", items = new { type = "array", items = new { type = "string" } } },
                    speaker_notes = new { type = "string" }
                })
            },
            new Tool
            {
                Name = "add_metrics_slide",
                Description = "Add a slide with 2-4 key metrics/KPIs displayed prominently.",
                InputSchema = BuildSchema(new
                {
                    title = new { type = "string" },
                    metrics = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                label = new { type = "string" },
                                value = new { type = "string" },
                                trend = new { type = "string", description = "up, down, or flat" }
                            }
                        }
                    },
                    speaker_notes = new { type = "string" }
                })
            },
            new Tool
            {
                Name = "add_thank_you_slide",
                Description = "Add the closing/thank you slide.",
                InputSchema = BuildSchema(new
                {
                    title = new { type = "string", description = "e.g., 'Merci' or 'Thank You'" },
                    subtitle = new { type = "string", description = "Company name or contact info" }
                })
            }
        };
    }

    // ── PDF Tools ─────────────────────────────────────────────

    private List<ToolUnion> BuildPdfTools()
    {
        return new List<ToolUnion>
        {
            new Tool
            {
                Name = "set_cover_page",
                Description = "Set the PDF cover page (call this first). Creates a professional title page.",
                InputSchema = BuildSchema(new
                {
                    company_name = new { type = "string", description = "Company name" },
                    document_title = new { type = "string", description = "Document title (e.g. 'Business Plan 2026')" },
                    subtitle = new { type = "string", description = "Optional subtitle or tagline" },
                    date = new { type = "string", description = "Date (e.g. 'March 2026')" },
                    prepared_by = new { type = "string", description = "Optional: who prepared the document" }
                })
            },
            new Tool
            {
                Name = "set_header_footer",
                Description = "Configure page headers and footers for the entire document.",
                InputSchema = BuildSchema(new
                {
                    header_left = new { type = "string", description = "Left header text (e.g. company name)" },
                    header_center = new { type = "string", description = "Center header text" },
                    header_right = new { type = "string", description = "Right header text (e.g. 'Confidential')" },
                    footer_left = new { type = "string", description = "Left footer text" },
                    footer_center = new { type = "string", description = "Center footer (e.g. 'Page {page} of {pages}')" },
                    footer_right = new { type = "string", description = "Right footer text (e.g. date)" }
                })
            },
            new Tool
            {
                Name = "add_heading",
                Description = "Add a heading. Level 1 headings appear in the table of contents.",
                InputSchema = BuildSchema(new
                {
                    text = new { type = "string", description = "Heading text" },
                    level = new { type = "integer", description = "Heading level: 1 (section), 2 (subsection), 3 (sub-subsection)" }
                })
            },
            new Tool
            {
                Name = "add_paragraph",
                Description = "Add a paragraph of professional prose text.",
                InputSchema = BuildSchema(new
                {
                    text = new { type = "string", description = "Paragraph text (preserve all data and numbers)" }
                })
            },
            new Tool
            {
                Name = "add_bullet_list",
                Description = "Add a bullet list for key points, features, or enumerations.",
                InputSchema = BuildSchema(new
                {
                    items = new { type = "array", items = new { type = "string" }, description = "List items (3-8 items)" }
                })
            },
            new Tool
            {
                Name = "add_table",
                Description = "Add a data table for financial data, comparisons, or structured information.",
                InputSchema = BuildSchema(new
                {
                    caption = new { type = "string", description = "Table caption/title" },
                    headers = new { type = "array", items = new { type = "string" }, description = "Column headers" },
                    rows = new { type = "array", items = new { type = "array", items = new { type = "string" } }, description = "Data rows" }
                })
            },
            new Tool
            {
                Name = "add_swot_grid",
                Description = "Add a SWOT analysis grid with four quadrants.",
                InputSchema = BuildSchema(new
                {
                    strengths = new { type = "array", items = new { type = "string" } },
                    weaknesses = new { type = "array", items = new { type = "string" } },
                    opportunities = new { type = "array", items = new { type = "string" } },
                    threats = new { type = "array", items = new { type = "string" } }
                })
            },
            new Tool
            {
                Name = "add_metrics_panel",
                Description = "Add a panel displaying 2-4 key metrics/KPIs prominently with values and trends.",
                InputSchema = BuildSchema(new
                {
                    metrics = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                label = new { type = "string" },
                                value = new { type = "string" },
                                trend = new { type = "string", description = "up, down, or flat" }
                            }
                        },
                        description = "2-4 key metrics"
                    }
                })
            },
            new Tool
            {
                Name = "add_chart_placeholder",
                Description = "Add a chart area (rendered by the PDF engine). Use for revenue projections, market share, growth trends.",
                InputSchema = BuildSchema(new
                {
                    chart_type = new { type = "string", description = "bar, line, pie, or donut" },
                    title = new { type = "string", description = "Chart title" },
                    labels = new { type = "array", items = new { type = "string" }, description = "X-axis labels or categories" },
                    data_series = new
                    {
                        type = "array",
                        items = new { type = "array", items = new { type = "string" } },
                        description = "Each sub-array is [series_name, value1, value2, ...]. Values as strings."
                    },
                    data_source_hint = new { type = "string", description = "Description of what data is visualized" }
                })
            },
            new Tool
            {
                Name = "add_callout",
                Description = "Add a highlighted callout box for key insights, warnings, or important notes.",
                InputSchema = BuildSchema(new
                {
                    text = new { type = "string", description = "Callout text" },
                    style = new { type = "string", description = "highlight, warning, or success" }
                })
            },
            new Tool
            {
                Name = "add_page_break",
                Description = "Add a page break before a new major section.",
                InputSchema = BuildSchema(new { })
            }
        };
    }

    // ── PDF Parsers ─────────────────────────────────────────

    private static PdfCoverPage ParseCoverPage(IReadOnlyDictionary<string, JsonElement> input)
    {
        return new PdfCoverPage
        {
            CompanyName = GetStr(input, "company_name") ?? "",
            DocumentTitle = GetStr(input, "document_title") ?? "",
            Subtitle = GetStr(input, "subtitle"),
            Date = GetStr(input, "date"),
            PreparedBy = GetStr(input, "prepared_by")
        };
    }

    private static PdfHeaderFooter ParseHeaderFooter(IReadOnlyDictionary<string, JsonElement> input)
    {
        return new PdfHeaderFooter
        {
            HeaderLeft = GetStr(input, "header_left"),
            HeaderCenter = GetStr(input, "header_center"),
            HeaderRight = GetStr(input, "header_right"),
            FooterLeft = GetStr(input, "footer_left"),
            FooterCenter = GetStr(input, "footer_center"),
            FooterRight = GetStr(input, "footer_right")
        };
    }

    private static PdfBlock? ParsePdfBlock(string toolName, IReadOnlyDictionary<string, JsonElement> input)
    {
        return toolName switch
        {
            "add_heading" => new PdfBlock
            {
                Type = "heading",
                Level = GetInt(input, "level", 1),
                Text = GetStr(input, "text")
            },
            "add_paragraph" => new PdfBlock
            {
                Type = "paragraph",
                Text = GetStr(input, "text")
            },
            "add_bullet_list" => new PdfBlock
            {
                Type = "bullet_list",
                Items = GetStrList(input, "items")
            },
            "add_table" => new PdfBlock
            {
                Type = "table",
                Table = new WordTableData
                {
                    Caption = GetStr(input, "caption"),
                    Headers = GetStrList(input, "headers"),
                    Rows = GetStrGrid(input, "rows")
                }
            },
            "add_swot_grid" => new PdfBlock
            {
                Type = "swot_grid",
                SwotGrid = new SwotGridData
                {
                    Strengths = GetStrList(input, "strengths"),
                    Weaknesses = GetStrList(input, "weaknesses"),
                    Opportunities = GetStrList(input, "opportunities"),
                    Threats = GetStrList(input, "threats")
                }
            },
            "add_metrics_panel" => new PdfBlock
            {
                Type = "metrics_panel",
                Metrics = ParseMetrics(input)
            },
            "add_chart_placeholder" => new PdfBlock
            {
                Type = "chart_placeholder",
                Chart = ParseChartPlaceholder(input)
            },
            "add_callout" => new PdfBlock
            {
                Type = "callout",
                Text = GetStr(input, "text"),
                Style = GetStr(input, "style") ?? "highlight"
            },
            "add_page_break" => new PdfBlock { Type = "page_break" },
            _ => null
        };
    }

    private static PdfChartPlaceholder ParseChartPlaceholder(IReadOnlyDictionary<string, JsonElement> input)
    {
        return new PdfChartPlaceholder
        {
            ChartType = GetStr(input, "chart_type") ?? "bar",
            Title = GetStr(input, "title") ?? "",
            Labels = GetStrList(input, "labels"),
            DataSeries = GetStrGrid(input, "data_series"),
            DataSourceHint = GetStr(input, "data_source_hint")
        };
    }

    // ── Parsers ──────────────────────────────────────────────

    private static WordBlock? ParseWordBlock(string toolName, IReadOnlyDictionary<string, JsonElement> input)
    {
        return toolName switch
        {
            "add_heading" => new WordBlock
            {
                Type = "heading",
                Level = GetInt(input, "level", 1),
                Text = GetStr(input, "text")
            },
            "add_paragraph" => new WordBlock
            {
                Type = "paragraph",
                Text = GetStr(input, "text")
            },
            "add_bullet_list" => new WordBlock
            {
                Type = "bullet_list",
                Items = GetStrList(input, "items")
            },
            "add_table" => new WordBlock
            {
                Type = "table",
                Table = new WordTableData
                {
                    Caption = GetStr(input, "caption"),
                    Headers = GetStrList(input, "headers"),
                    Rows = GetStrGrid(input, "rows")
                }
            },
            "add_swot_grid" => new WordBlock
            {
                Type = "swot_grid",
                SwotGrid = new SwotGridData
                {
                    Strengths = GetStrList(input, "strengths"),
                    Weaknesses = GetStrList(input, "weaknesses"),
                    Opportunities = GetStrList(input, "opportunities"),
                    Threats = GetStrList(input, "threats")
                }
            },
            "add_callout" => new WordBlock
            {
                Type = "callout",
                Text = GetStr(input, "text"),
                Style = GetStr(input, "style") ?? "highlight"
            },
            "add_page_break" => new WordBlock { Type = "page_break" },
            _ => null
        };
    }

    private static SlideBlueprint? ParseSlideBlock(string toolName, IReadOnlyDictionary<string, JsonElement> input)
    {
        return toolName switch
        {
            "add_title_slide" => new SlideBlueprint
            {
                Type = "title",
                Title = GetStr(input, "title") ?? "",
                Subtitle = GetStr(input, "subtitle"),
                SpeakerNotes = GetStr(input, "speaker_notes")
            },
            "add_section_divider" => new SlideBlueprint
            {
                Type = "section_divider",
                Title = GetStr(input, "title") ?? "",
                Subtitle = GetStr(input, "subtitle")
            },
            "add_content_slide" => new SlideBlueprint
            {
                Type = "content",
                Title = GetStr(input, "title") ?? "",
                Bullets = GetStrList(input, "bullets"),
                SpeakerNotes = GetStr(input, "speaker_notes")
            },
            "add_two_column_slide" => new SlideBlueprint
            {
                Type = "two_column",
                Title = GetStr(input, "title") ?? "",
                LeftColumnTitle = GetStr(input, "left_column_title"),
                LeftColumnBullets = GetStrList(input, "left_column_bullets"),
                RightColumnTitle = GetStr(input, "right_column_title"),
                RightColumnBullets = GetStrList(input, "right_column_bullets"),
                SpeakerNotes = GetStr(input, "speaker_notes")
            },
            "add_swot_slide" => new SlideBlueprint
            {
                Type = "swot",
                Title = GetStr(input, "title") ?? "",
                SwotGrid = new SwotGridData
                {
                    Strengths = GetStrList(input, "strengths"),
                    Weaknesses = GetStrList(input, "weaknesses"),
                    Opportunities = GetStrList(input, "opportunities"),
                    Threats = GetStrList(input, "threats")
                },
                SpeakerNotes = GetStr(input, "speaker_notes")
            },
            "add_table_slide" => new SlideBlueprint
            {
                Type = "table",
                Title = GetStr(input, "title") ?? "",
                Table = new WordTableData
                {
                    Headers = GetStrList(input, "headers"),
                    Rows = GetStrGrid(input, "rows")
                },
                SpeakerNotes = GetStr(input, "speaker_notes")
            },
            "add_metrics_slide" => new SlideBlueprint
            {
                Type = "metrics",
                Title = GetStr(input, "title") ?? "",
                Metrics = ParseMetrics(input),
                SpeakerNotes = GetStr(input, "speaker_notes")
            },
            "add_thank_you_slide" => new SlideBlueprint
            {
                Type = "thank_you",
                Title = GetStr(input, "title") ?? "",
                Subtitle = GetStr(input, "subtitle")
            },
            _ => null
        };
    }

    private static SpreadsheetBlueprint ParseSpreadsheetFromTool(IReadOnlyDictionary<string, JsonElement> input)
    {
        var blueprint = new SpreadsheetBlueprint();
        if (input.TryGetValue("sheets", out var sheetsEl) && sheetsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var sheet in sheetsEl.EnumerateArray())
            {
                var sb = new SheetBlueprint
                {
                    Name = sheet.TryGetProperty("name", out var n) ? n.GetString() ?? "Sheet" : "Sheet",
                    Headers = new(),
                    Rows = new()
                };

                if (sheet.TryGetProperty("headers", out var h) && h.ValueKind == JsonValueKind.Array)
                    sb.Headers = h.EnumerateArray().Select(e => e.GetString() ?? "").ToList();

                if (sheet.TryGetProperty("rows", out var r) && r.ValueKind == JsonValueKind.Array)
                    sb.Rows = r.EnumerateArray()
                        .Select(row => row.ValueKind == JsonValueKind.Array
                            ? row.EnumerateArray().Select(c => c.GetString() ?? "").ToList()
                            : new List<string>())
                        .ToList();

                if (sheet.TryGetProperty("summary_row", out var sr) && sr.ValueKind == JsonValueKind.Array)
                    sb.SummaryRow = sr.EnumerateArray().Select(e => e.GetString() ?? "").ToList();

                if (sheet.TryGetProperty("chart_type", out var ct))
                    sb.ChartType = ct.GetString();
                if (sheet.TryGetProperty("chart_title", out var ctt))
                    sb.ChartTitle = ctt.GetString();

                blueprint.Sheets.Add(sb);
            }
        }
        return blueprint;
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

    private static string? GetStr(IReadOnlyDictionary<string, JsonElement> d, string key)
        => d.TryGetValue(key, out var e) && e.ValueKind == JsonValueKind.String ? e.GetString() : null;

    private static int GetInt(IReadOnlyDictionary<string, JsonElement> d, string key, int def)
        => d.TryGetValue(key, out var e) && e.ValueKind == JsonValueKind.Number ? e.GetInt32() : def;

    private static List<string> GetStrList(IReadOnlyDictionary<string, JsonElement> d, string key)
    {
        if (!d.TryGetValue(key, out var e) || e.ValueKind != JsonValueKind.Array)
            return new();
        return e.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString() ?? "")
            .Where(s => s.Length > 0)
            .ToList();
    }

    private static List<List<string>> GetStrGrid(IReadOnlyDictionary<string, JsonElement> d, string key)
    {
        if (!d.TryGetValue(key, out var e) || e.ValueKind != JsonValueKind.Array)
            return new();
        return e.EnumerateArray()
            .Where(row => row.ValueKind == JsonValueKind.Array)
            .Select(row => row.EnumerateArray()
                .Select(c => c.GetString() ?? c.ToString())
                .ToList())
            .ToList();
    }

    private static List<MetricItem> ParseMetrics(IReadOnlyDictionary<string, JsonElement> d)
    {
        if (!d.TryGetValue("metrics", out var e) || e.ValueKind != JsonValueKind.Array)
            return new();
        return e.EnumerateArray()
            .Select(m => new MetricItem
            {
                Label = m.TryGetProperty("label", out var l) ? l.GetString() ?? "" : "",
                Value = m.TryGetProperty("value", out var v) ? v.GetString() ?? "" : "",
                Trend = m.TryGetProperty("trend", out var t) ? t.GetString() : null
            })
            .Where(m => m.Label.Length > 0)
            .ToList();
    }

    private static string Truncate(string text, int maxChars)
        => text.Length <= maxChars ? text : text[..maxChars] + "...";
}

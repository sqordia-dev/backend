using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.Cms;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// AI-powered content generation for CMS blocks
/// </summary>
public class CmsAiContentService : ICmsAiContentService
{
    private readonly IAIService _aiService;
    private readonly ILogger<CmsAiContentService> _logger;

    public CmsAiContentService(
        IAIService aiService,
        ILogger<CmsAiContentService> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<Result<CmsAiGenerationResult>> GenerateContentAsync(
        GenerateCmsContentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var systemPrompt = BuildSystemPrompt(request);
            var userPrompt = BuildUserPrompt(request);

            var content = await _aiService.GenerateContentAsync(
                systemPrompt, userPrompt, 2000, 0.7f, cancellationToken);

            return Result.Success(new CmsAiGenerationResult
            {
                Content = content,
                ModelUsed = "claude-sonnet-4-6",
                TokensUsed = content.Length / 4
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CMS content");
            return Result.Failure<CmsAiGenerationResult>(
                Error.Failure("CmsAI.Error", $"Content generation failed: {ex.Message}"));
        }
    }

    public async IAsyncEnumerable<string> StreamContentAsync(
        GenerateCmsContentRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var systemPrompt = BuildSystemPrompt(request);
        var userPrompt = BuildUserPrompt(request);

        var messages = new List<AIChatMessage>
        {
            new() { Role = "user", Content = userPrompt }
        };

        await foreach (var chunk in _aiService.StreamChatResponseAsync(
            systemPrompt, messages, 2000, cancellationToken))
        {
            yield return chunk;
        }
    }

    private static string BuildSystemPrompt(GenerateCmsContentRequest request)
    {
        var languageInstruction = request.Language == "fr"
            ? "Write all content in French."
            : "Write all content in English.";

        var formatInstruction = request.BlockType switch
        {
            "richtext" => "Format the output as clean HTML suitable for a rich text editor. Use <h2>, <h3>, <p>, <ul>, <li> tags.",
            "heading" => "Generate only a heading/title. Keep it concise (under 80 characters).",
            _ => "Generate plain text content. Use paragraphs for structure."
        };

        return $@"You are a professional content writer for a business platform called Sqordia.
{languageInstruction}
{formatInstruction}
Write clear, engaging, professional content based on the user's brief.
Do not include any preamble or explanation - just output the content directly.";
    }

    private static string BuildUserPrompt(GenerateCmsContentRequest request)
    {
        var prompt = $"Brief: {request.Brief}";

        if (!string.IsNullOrEmpty(request.SectionContext))
        {
            prompt += $"\n\nContext (existing section content):\n{request.SectionContext}";
        }

        return prompt;
    }
}

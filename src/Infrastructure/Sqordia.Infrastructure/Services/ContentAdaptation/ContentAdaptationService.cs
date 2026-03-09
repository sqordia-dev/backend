using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Models.Export;
using Sqordia.Application.Services;
using System.Text.RegularExpressions;

namespace Sqordia.Infrastructure.Services.ContentAdaptation;

/// <summary>
/// Adapts business plan content for different export formats using AI.
/// Falls back to rule-based adaptation when AI is unavailable.
/// </summary>
public class ContentAdaptationService : IContentAdaptationService
{
    private readonly IAIService _aiService;
    private readonly ILogger<ContentAdaptationService> _logger;

    private const int MaxInputWords = 3000;
    private const int MaxConcurrentAiCalls = 3;

    public ContentAdaptationService(IAIService aiService, ILogger<ContentAdaptationService> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<Result<ContentAdaptationResult>> AdaptContentAsync(
        string sectionKey, string sectionTitle, string content,
        ExportFormatTarget targetFormat, string language,
        CancellationToken cancellationToken = default)
    {
        var originalWordCount = CountWords(content);

        // PDF = passthrough
        if (targetFormat == ExportFormatTarget.Pdf)
        {
            return Result.Success(new ContentAdaptationResult
            {
                SectionKey = sectionKey,
                AdaptedContent = content,
                TargetFormat = targetFormat,
                Language = language,
                WasAiAdapted = false,
                OriginalWordCount = originalWordCount,
                AdaptedWordCount = originalWordCount
            });
        }

        // Clean content for AI processing
        var cleanContent = StripHtml(content);
        cleanContent = TruncateToWords(cleanContent, MaxInputWords);

        try
        {
            var isAvailable = await _aiService.IsAvailableAsync(cancellationToken);
            if (!isAvailable)
            {
                _logger.LogWarning("AI unavailable, using rule-based adaptation for {Section}", sectionKey);
                return Result.Success(RuleBasedAdaptation(sectionKey, sectionTitle, cleanContent, targetFormat, language, originalWordCount));
            }

            var langInstruction = ContentAdaptationPrompts.LangInstruction(language);
            string systemPrompt, userPrompt;
            int maxTokens;
            float temperature;

            if (targetFormat == ExportFormatTarget.PowerPoint)
            {
                systemPrompt = ContentAdaptationPrompts.PowerPointSystem(langInstruction);
                userPrompt = ContentAdaptationPrompts.PowerPointUser(sectionTitle, cleanContent);
                maxTokens = ContentAdaptationPrompts.PowerPointMaxTokens;
                temperature = ContentAdaptationPrompts.PowerPointTemperature;
            }
            else // Word
            {
                systemPrompt = ContentAdaptationPrompts.WordSystem(langInstruction);
                userPrompt = ContentAdaptationPrompts.WordUser(sectionTitle, cleanContent);
                maxTokens = ContentAdaptationPrompts.WordMaxTokens;
                temperature = ContentAdaptationPrompts.WordTemperature;
            }

            var aiResponse = await _aiService.GenerateContentAsync(
                systemPrompt, userPrompt, maxTokens, temperature, cancellationToken);

            if (string.IsNullOrWhiteSpace(aiResponse))
            {
                return Result.Success(RuleBasedAdaptation(sectionKey, sectionTitle, cleanContent, targetFormat, language, originalWordCount));
            }

            return Result.Success(new ContentAdaptationResult
            {
                SectionKey = sectionKey,
                AdaptedContent = aiResponse.Trim(),
                TargetFormat = targetFormat,
                Language = language,
                WasAiAdapted = true,
                OriginalWordCount = originalWordCount,
                AdaptedWordCount = CountWords(aiResponse)
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI adaptation failed for {Section}, using fallback", sectionKey);
            return Result.Success(RuleBasedAdaptation(sectionKey, sectionTitle, cleanContent, targetFormat, language, originalWordCount));
        }
    }

    public async Task<Result<List<ContentAdaptationResult>>> AdaptAllSectionsAsync(
        List<(string Key, string Title, string Content)> sections,
        ExportFormatTarget targetFormat, string language,
        CancellationToken cancellationToken = default)
    {
        if (targetFormat == ExportFormatTarget.Pdf)
        {
            // Passthrough — no AI calls
            var results = sections.Select(s => new ContentAdaptationResult
            {
                SectionKey = s.Key,
                AdaptedContent = s.Content,
                TargetFormat = targetFormat,
                Language = language,
                WasAiAdapted = false,
                OriginalWordCount = CountWords(s.Content),
                AdaptedWordCount = CountWords(s.Content)
            }).ToList();
            return Result.Success(results);
        }

        var semaphore = new SemaphoreSlim(MaxConcurrentAiCalls);
        var tasks = sections.Select(async section =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await AdaptContentAsync(
                    section.Key, section.Title, section.Content,
                    targetFormat, language, cancellationToken);

                if (result.IsSuccess && result.Value != null)
                    return result.Value;

                // Fallback if adaptation failed
                return RuleBasedAdaptation(
                    section.Key, section.Title, StripHtml(section.Content),
                    targetFormat, language, CountWords(section.Content));
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        var adapted = await Task.WhenAll(tasks);
        return Result.Success(adapted.ToList());
    }

    // ── Fallback: rule-based adaptation ───────────────────────

    private static ContentAdaptationResult RuleBasedAdaptation(
        string sectionKey, string sectionTitle, string content,
        ExportFormatTarget targetFormat, string language, int originalWordCount)
    {
        var adapted = targetFormat == ExportFormatTarget.PowerPoint
            ? FallbackBullets(content)
            : content; // Word: pass through as-is

        return new ContentAdaptationResult
        {
            SectionKey = sectionKey,
            AdaptedContent = adapted,
            TargetFormat = targetFormat,
            Language = language,
            WasAiAdapted = false,
            OriginalWordCount = originalWordCount,
            AdaptedWordCount = CountWords(adapted)
        };
    }

    private static string FallbackBullets(string text)
    {
        var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
            .Where(s => s.Length > 10)
            .Take(5)
            .ToList();

        return sentences.Count > 0
            ? string.Join("\n", sentences)
            : (text.Length > 200 ? text[..200] + "..." : text);
    }

    // ── Helpers ───────────────────────────────────────────────

    private static string StripHtml(string content)
    {
        var text = Regex.Replace(content, "<[^>]*>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        return Regex.Replace(text, @"\s+", " ").Trim();
    }

    private static string TruncateToWords(string text, int maxWords)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length <= maxWords ? text : string.Join(' ', words.Take(maxWords));
    }

    private static int CountWords(string text) =>
        string.IsNullOrWhiteSpace(text) ? 0 : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
}

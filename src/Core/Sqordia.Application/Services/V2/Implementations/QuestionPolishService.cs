using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.V2.Questionnaire;
using Sqordia.Contracts.Responses.V2.Questionnaire;
using System.Text.Json;

namespace Sqordia.Application.Services.V2.Implementations;

/// <summary>
/// AI text enhancement service for questionnaire responses
/// Transforms raw notes into professional, BDC-standard prose
/// </summary>
public class QuestionPolishService : IQuestionPolishService
{
    private readonly IAIService _aiService;
    private readonly ILogger<QuestionPolishService> _logger;

    public QuestionPolishService(
        IAIService aiService,
        ILogger<QuestionPolishService> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<Result<PolishedTextResponse>> PolishTextAsync(
        PolishTextRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Polishing text with {Length} characters in {Language}",
                request.Text.Length, request.Language);

            var systemPrompt = BuildPolishPrompt(request.Language, request.Tone, request.TargetAudience);
            var userPrompt = BuildUserPrompt(request);

            var aiResponse = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt,
                userPrompt,
                maxTokens: 2000,
                temperature: 0.5f,
                cancellationToken: cancellationToken);

            var result = ParsePolishResponse(aiResponse, request.Text);

            _logger.LogInformation("Text polished successfully with {ImprovementCount} improvements",
                result.Improvements.Count);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polishing text");
            return Result.Failure<PolishedTextResponse>(
                Error.InternalServerError("Polish.Error", "An error occurred while polishing the text."));
        }
    }

    private static string BuildPolishPrompt(string language, string tone, string? targetAudience)
    {
        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var audienceNote = string.IsNullOrWhiteSpace(targetAudience)
            ? (isFrench ? "investisseurs et banquiers" : "investors and bankers")
            : targetAudience;

        return isFrench
            ? $@"Tu es un expert en rédaction de plans d'affaires BDC. Ta tâche est de transformer des notes brutes en prose professionnelle.

Règles:
1. Conserve TOUTES les informations factuelles originales
2. Améliore la clarté, la structure et le professionnalisme
3. Utilise un ton {GetToneFrench(tone)}
4. L'audience cible est: {audienceNote}
5. Corrige la grammaire et l'orthographe
6. Ajoute des transitions fluides entre les idées
7. Ne jamais inventer de données ou de chiffres

Réponds UNIQUEMENT en JSON valide:
{{
  ""polishedText"": ""Le texte amélioré ici"",
  ""improvements"": [""Amélioration 1"", ""Amélioration 2""],
  ""confidence"": 0.95,
  ""alternatives"": [""Version alternative si pertinente""]
}}"
            : $@"You are an expert business plan writer for BDC standards. Your task is to transform raw notes into professional prose.

Rules:
1. Preserve ALL original factual information
2. Improve clarity, structure, and professionalism
3. Use a {GetToneEnglish(tone)} tone
4. Target audience is: {audienceNote}
5. Fix grammar and spelling
6. Add smooth transitions between ideas
7. Never invent data or figures

Respond ONLY with valid JSON:
{{
  ""polishedText"": ""The improved text here"",
  ""improvements"": [""Improvement 1"", ""Improvement 2""],
  ""confidence"": 0.95,
  ""alternatives"": [""Alternative version if relevant""]
}}";
    }

    private static string BuildUserPrompt(PolishTextRequest request)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Original text to polish:");
        sb.AppendLine("---");
        sb.AppendLine(request.Text);
        sb.AppendLine("---");

        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            sb.AppendLine();
            sb.AppendLine($"Context: {request.Context}");
        }

        return sb.ToString();
    }

    private static PolishedTextResponse ParsePolishResponse(string aiResponse, string originalText)
    {
        try
        {
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                var polishedText = parsed.TryGetProperty("polishedText", out var pt)
                    ? pt.GetString() ?? aiResponse
                    : aiResponse;

                var improvements = new List<string>();
                if (parsed.TryGetProperty("improvements", out var imp) && imp.ValueKind == JsonValueKind.Array)
                {
                    improvements = imp.EnumerateArray()
                        .Select(i => i.GetString() ?? "")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();
                }

                var confidence = parsed.TryGetProperty("confidence", out var conf)
                    ? conf.GetDecimal()
                    : 0.8m;

                var alternatives = new List<string>();
                if (parsed.TryGetProperty("alternatives", out var alt) && alt.ValueKind == JsonValueKind.Array)
                {
                    alternatives = alt.EnumerateArray()
                        .Select(a => a.GetString() ?? "")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();
                }

                return new PolishedTextResponse
                {
                    OriginalText = originalText,
                    PolishedText = polishedText,
                    Confidence = confidence,
                    Improvements = improvements,
                    Alternatives = alternatives.Any() ? alternatives : null,
                    GeneratedAt = DateTime.UtcNow
                };
            }
        }
        catch (Exception)
        {
            // Fallback if JSON parsing fails
        }

        // Return AI response as-is if parsing fails
        return new PolishedTextResponse
        {
            OriginalText = originalText,
            PolishedText = aiResponse.Trim(),
            Confidence = 0.7m,
            Improvements = new List<string> { "Text has been professionally reformatted" },
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static string GetToneFrench(string tone)
    {
        return tone.ToLower() switch
        {
            "casual" => "décontracté mais professionnel",
            "formal" => "formel et rigoureux",
            "persuasive" => "persuasif et convaincant",
            _ => "professionnel et clair"
        };
    }

    private static string GetToneEnglish(string tone)
    {
        return tone.ToLower() switch
        {
            "casual" => "casual yet professional",
            "formal" => "formal and rigorous",
            "persuasive" => "persuasive and compelling",
            _ => "professional and clear"
        };
    }
}

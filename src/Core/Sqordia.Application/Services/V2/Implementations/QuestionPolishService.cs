using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.AI;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Requests.V2.Questionnaire;
using Sqordia.Contracts.Responses.V2.Questionnaire;
using System.Text.Json;
// Feature flag + Python service support

namespace Sqordia.Application.Services.V2.Implementations;

/// <summary>
/// AI text enhancement service for questionnaire responses
/// Transforms raw notes into professional, BDC-standard prose
/// </summary>
public class QuestionPolishService : IQuestionPolishService
{
    private readonly IAIService _aiService;
    private readonly IAIPythonService _pythonService;
    private readonly IFeatureFlagsService _featureFlags;
    private readonly ILogger<QuestionPolishService> _logger;

    public QuestionPolishService(
        IAIService aiService,
        IAIPythonService pythonService,
        IFeatureFlagsService featureFlags,
        ILogger<QuestionPolishService> logger)
    {
        _aiService = aiService;
        _pythonService = pythonService;
        _featureFlags = featureFlags;
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

    public async Task<Result<PolishedTextResponse>> TransformTextAsync(
        TransformAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!TransformActionTypes.IsValid(request.Action))
            {
                return Result.Failure<PolishedTextResponse>(
                    Error.Validation("Transform.InvalidAction", $"Invalid action type: {request.Action}. Valid actions are: {string.Join(", ", TransformActionTypes.AllActions)}"));
            }

            // Feature flag: delegate to Python LangChain pipeline if enabled
            var useLangChain = await _featureFlags.IsEnabledAsync("AI.UseLangChainPipeline", cancellationToken);
            if (useLangChain.IsSuccess && useLangChain.Value)
            {
                return await TransformViaPythonAsync(request, cancellationToken);
            }

            // Build business context from previous answers
            var businessContext = BuildBusinessContext(request);
            var hasContext = !string.IsNullOrWhiteSpace(businessContext);

            _logger.LogInformation(
                "Transforming text with action '{Action}' for question {QuestionId} (Q{QuestionNumber}), {Length} characters in {Language}, context: {HasContext}",
                request.Action, request.QuestionId, request.QuestionNumber, request.Answer.Length, request.Language, hasContext);

            var systemPrompt = BuildTransformPrompt(request.Action, request.Language, request.Persona, businessContext);
            var userPrompt = BuildTransformUserPrompt(request, businessContext);

            var aiResponse = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt,
                userPrompt,
                maxTokens: 2500,
                temperature: GetTemperatureForAction(request.Action),
                cancellationToken: cancellationToken);

            var result = ParsePolishResponse(aiResponse, request.Answer);

            _logger.LogInformation("Text transformed successfully with action '{Action}', {ImprovementCount} improvements",
                request.Action, result.Improvements.Count);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming text with action '{Action}'", request.Action);
            return Result.Failure<PolishedTextResponse>(
                Error.InternalServerError("Transform.Error", "An error occurred while transforming the text."));
        }
    }

    /// <summary>
    /// Delegates text transformation to the Python LangChain service.
    /// Falls back to the .NET pipeline on failure.
    /// </summary>
    private async Task<Result<PolishedTextResponse>> TransformViaPythonAsync(
        TransformAnswerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await _pythonService.IsAvailableAsync(cancellationToken))
            {
                _logger.LogWarning("Python AI service unavailable, falling back to .NET pipeline");
                return await TransformViaLegacyAsync(request, cancellationToken);
            }

            var pythonRequest = new TransformAnswerPythonRequest(
                Action: request.Action,
                QuestionNumber: request.QuestionNumber ?? 0,
                QuestionText: request.Context ?? "",
                CurrentAnswer: request.Answer,
                PreviousAnswers: request.PreviousAnswers,
                OrganizationContext: request.OrganizationContext,
                Language: request.Language
            );

            var pythonResponse = await _pythonService.TransformAnswerAsync(pythonRequest, cancellationToken);

            return Result.Success(new PolishedTextResponse
            {
                OriginalText = request.Answer,
                PolishedText = pythonResponse.TransformedAnswer,
                Confidence = 0.9m,
                Improvements = new List<string> { $"Transformed via LangChain ({pythonResponse.Action})" },
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Python AI service call failed, falling back to .NET pipeline");
            return await TransformViaLegacyAsync(request, cancellationToken);
        }
    }

    private async Task<Result<PolishedTextResponse>> TransformViaLegacyAsync(
        TransformAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var businessContext = BuildBusinessContext(request);
        var systemPrompt = BuildTransformPrompt(request.Action, request.Language, request.Persona, businessContext);
        var userPrompt = BuildTransformUserPrompt(request, businessContext);

        var aiResponse = await _aiService.GenerateContentWithRetryAsync(
            systemPrompt, userPrompt,
            maxTokens: 2500,
            temperature: GetTemperatureForAction(request.Action),
            cancellationToken: cancellationToken);

        return Result.Success(ParsePolishResponse(aiResponse, request.Answer));
    }

    /// <summary>
    /// Builds business context from organization profile + previous answers.
    /// Organization profile provides structured metadata from onboarding;
    /// previous answers provide questionnaire-level detail.
    /// </summary>
    private static string BuildBusinessContext(TransformAnswerRequest request)
    {
        var parts = new List<string>();
        var isFrench = request.Language.Equals("fr", StringComparison.OrdinalIgnoreCase);

        // 1. Organization profile context (from onboarding)
        var orgContext = BuildOrganizationProfileContext(request.OrganizationContext, isFrench);
        if (!string.IsNullOrWhiteSpace(orgContext))
            parts.Add(orgContext);

        // 2. Questionnaire answers context
        if (request.PreviousAnswers != null && request.PreviousAnswers.Count > 0)
        {
            var questionNumber = request.QuestionNumber ?? 0;
            var answersContext = QuestionContextMapper.BuildBusinessContextSummary(
                request.PreviousAnswers,
                questionNumber,
                request.Language);
            if (!string.IsNullOrWhiteSpace(answersContext))
                parts.Add(answersContext);
        }
        else
        {
            // Fallback: use business name and sector if provided directly
            var fallbackParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(request.BusinessName))
                fallbackParts.Add($"Business: {request.BusinessName}");
            if (!string.IsNullOrWhiteSpace(request.BusinessSector))
                fallbackParts.Add($"Sector: {request.BusinessSector}");
            var fallback = string.Join(". ", fallbackParts);
            if (!string.IsNullOrWhiteSpace(fallback))
                parts.Add(fallback);
        }

        return string.Join("\n\n", parts);
    }

    /// <summary>
    /// Builds a structured context block from the organization profile collected during onboarding.
    /// </summary>
    private static string BuildOrganizationProfileContext(OrganizationContextDto? org, bool isFrench)
    {
        if (org == null)
            return string.Empty;

        var header = isFrench
            ? "PROFIL DE L'ORGANISATION (collecté lors de l'inscription):"
            : "ORGANIZATION PROFILE (collected during onboarding):";

        var lines = new List<string> { header };

        void Add(string labelFr, string labelEn, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                lines.Add($"  - {(isFrench ? labelFr : labelEn)}: {value}");
        }

        Add("Entreprise", "Company", org.CompanyName);
        Add("Industrie", "Industry", org.Industry);
        Add("Secteur", "Sector", org.Sector);
        Add("Stade de l'entreprise", "Business Stage", org.BusinessStage);
        Add("Taille de l'équipe", "Team Size", org.TeamSize);
        Add("Statut de financement", "Funding Status", org.FundingStatus);
        Add("Marché cible", "Target Market", org.TargetMarket);

        // Location
        var locationParts = new[] { org.City, org.Province, org.Country }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var location = string.Join(", ", locationParts);
        if (!string.IsNullOrWhiteSpace(location))
            Add("Localisation", "Location", location);

        // Goals
        if (!string.IsNullOrWhiteSpace(org.Goals))
            Add("Objectifs", "Goals", org.Goals);

        return lines.Count <= 1 ? string.Empty : string.Join("\n", lines);
    }

    private static float GetTemperatureForAction(string action)
    {
        return action.ToLower() switch
        {
            TransformActionTypes.Generate => 0.7f, // Creative for generation
            TransformActionTypes.Examples => 0.7f,  // More creative for examples
            TransformActionTypes.Expand => 0.6f,   // Somewhat creative for expansion
            TransformActionTypes.Simplify => 0.3f, // More deterministic for simplification
            _ => 0.5f  // Default for polish, shorten, professional
        };
    }

    private static string BuildTransformPrompt(string action, string language, string persona, string? businessContext = null)
    {
        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var personaContext = GetPersonaContext(persona, isFrench);

        var actionPrompt = action.ToLower() switch
        {
            TransformActionTypes.Generate => GetGenerateActionPrompt(isFrench, personaContext),
            TransformActionTypes.Polish => GetPolishActionPrompt(isFrench, personaContext),
            TransformActionTypes.Shorten => GetShortenActionPrompt(isFrench, personaContext),
            TransformActionTypes.Expand => GetExpandActionPrompt(isFrench, personaContext),
            TransformActionTypes.Professional => GetProfessionalActionPrompt(isFrench, personaContext),
            TransformActionTypes.Examples => GetExamplesActionPrompt(isFrench, personaContext),
            TransformActionTypes.Simplify => GetSimplifyActionPrompt(isFrench, personaContext),
            _ => GetPolishActionPrompt(isFrench, personaContext)
        };

        // Add business context instruction if available
        var contextInstruction = !string.IsNullOrWhiteSpace(businessContext)
            ? isFrench
                ? "\n\nIMPORTANT: Tu as accès au contexte du projet de l'utilisateur. Utilise ces informations pour personnaliser ta réponse et la rendre spécifique à son entreprise. Mentionne des éléments concrets de son projet quand c'est pertinent."
                : "\n\nIMPORTANT: You have access to the user's project context. Use this information to personalize your response and make it specific to their business. Reference concrete elements from their project when relevant."
            : "";

        var jsonFormat = isFrench
            ? @"Réponds UNIQUEMENT en JSON valide:
{
  ""polishedText"": ""Le texte transformé ici"",
  ""improvements"": [""Ce qui a été changé 1"", ""Ce qui a été changé 2""],
  ""confidence"": 0.95
}"
            : @"Respond ONLY with valid JSON:
{
  ""polishedText"": ""The transformed text here"",
  ""improvements"": [""What was changed 1"", ""What was changed 2""],
  ""confidence"": 0.95
}";

        return $"{actionPrompt}{contextInstruction}\n\n{jsonFormat}";
    }

    private static string GetPersonaContext(string persona, bool isFrench)
    {
        return persona.ToLower() switch
        {
            "consultant" => isFrench
                ? "Le contexte est celui d'un consultant préparant un plan d'affaires pour un client."
                : "The context is a consultant preparing a business plan for a client.",
            "obnl" => isFrench
                ? "Le contexte est celui d'un organisme à but non lucratif (OBNL)."
                : "The context is a non-profit organization (NPO/OBNL).",
            _ => isFrench
                ? "Le contexte est celui d'un entrepreneur préparant son plan d'affaires."
                : "The context is an entrepreneur preparing their business plan."
        };
    }

    private static string GetPolishActionPrompt(bool isFrench, string personaContext)
    {
        return isFrench
            ? $@"Tu es un expert en rédaction de plans d'affaires. {personaContext}

Ta tâche: AMÉLIORER et POLIR le texte pour plus de clarté et de fluidité.

Règles:
1. Conserve TOUTES les informations factuelles originales
2. Améliore la clarté, la structure et le professionnalisme
3. Corrige la grammaire et l'orthographe
4. Ajoute des transitions fluides
5. Ne jamais inventer de données"
            : $@"You are an expert business plan writer. {personaContext}

Your task: IMPROVE and POLISH the text for better clarity and flow.

Rules:
1. Preserve ALL original factual information
2. Improve clarity, structure, and professionalism
3. Fix grammar and spelling
4. Add smooth transitions
5. Never invent data";
    }

    private static string GetShortenActionPrompt(bool isFrench, string personaContext)
    {
        return isFrench
            ? $@"Tu es un expert en rédaction concise. {personaContext}

Ta tâche: RACCOURCIR le texte de 30-50% tout en conservant l'essentiel.

Règles:
1. Garde uniquement les points clés et informations essentielles
2. Élimine les redondances et le verbiage
3. Utilise des formulations plus directes
4. Conserve tous les chiffres et données importantes
5. Le résultat doit être clair et impactant"
            : $@"You are an expert in concise writing. {personaContext}

Your task: SHORTEN the text by 30-50% while keeping the essence.

Rules:
1. Keep only key points and essential information
2. Remove redundancies and filler words
3. Use more direct phrasing
4. Preserve all numbers and important data
5. Result should be clear and impactful";
    }

    private static string GetExpandActionPrompt(bool isFrench, string personaContext)
    {
        return isFrench
            ? $@"Tu es un expert en développement de contenu d'affaires. {personaContext}

Ta tâche: DÉVELOPPER le texte avec plus de détails et de profondeur.

Règles:
1. Ajoute des explications supplémentaires pertinentes
2. Développe les points existants avec plus de contexte
3. Suggère des aspects complémentaires à considérer
4. Reste factuel et cohérent avec le contenu original
5. Augmente le contenu de 50-100% sans répétition
6. N'invente PAS de chiffres ou données spécifiques"
            : $@"You are an expert in business content development. {personaContext}

Your task: EXPAND the text with more details and depth.

Rules:
1. Add relevant additional explanations
2. Develop existing points with more context
3. Suggest complementary aspects to consider
4. Stay factual and consistent with original content
5. Increase content by 50-100% without repetition
6. Do NOT invent specific numbers or data";
    }

    private static string GetProfessionalActionPrompt(bool isFrench, string personaContext)
    {
        return isFrench
            ? $@"Tu es un rédacteur d'affaires senior spécialisé BDC. {personaContext}

Ta tâche: Rendre le texte PLUS PROFESSIONNEL pour un public d'investisseurs et banquiers.

Règles:
1. Utilise un vocabulaire d'affaires sophistiqué
2. Adopte un ton confiant et crédible
3. Structure les idées de manière logique
4. Ajoute des formulations business-ready
5. Conserve toutes les données factuelles
6. Le texte doit inspirer confiance aux décideurs"
            : $@"You are a senior BDC business writer. {personaContext}

Your task: Make the text MORE PROFESSIONAL for an investor and banker audience.

Rules:
1. Use sophisticated business vocabulary
2. Adopt a confident, credible tone
3. Structure ideas logically
4. Add business-ready phrasing
5. Preserve all factual data
6. Text should inspire confidence in decision-makers";
    }

    private static string GetExamplesActionPrompt(bool isFrench, string personaContext)
    {
        return isFrench
            ? $@"Tu es un expert en storytelling d'affaires. {personaContext}

Ta tâche: ENRICHIR le texte avec des exemples concrets et pertinents.

Règles:
1. Ajoute des exemples spécifiques qui illustrent les points
2. Inclus des scénarios d'application réalistes
3. Utilise des comparaisons quand pertinent
4. Garde les exemples alignés avec le secteur d'activité suggéré
5. Les exemples doivent renforcer la crédibilité
6. N'invente pas de statistiques ou chiffres précis"
            : $@"You are an expert in business storytelling. {personaContext}

Your task: ENRICH the text with concrete, relevant examples.

Rules:
1. Add specific examples that illustrate points
2. Include realistic application scenarios
3. Use comparisons when relevant
4. Keep examples aligned with the suggested industry
5. Examples should reinforce credibility
6. Do not invent statistics or precise numbers";
    }

    private static string GetSimplifyActionPrompt(bool isFrench, string personaContext)
    {
        return isFrench
            ? $@"Tu es un expert en communication claire. {personaContext}

Ta tâche: SIMPLIFIER le texte pour le rendre plus accessible et facile à comprendre.

Règles:
1. Utilise un langage simple et direct
2. Évite le jargon technique sauf si essentiel
3. Décompose les phrases complexes
4. Garde le message principal clair
5. Le texte doit être compréhensible par tous
6. Conserve toutes les informations importantes"
            : $@"You are an expert in clear communication. {personaContext}

Your task: SIMPLIFY the text to make it more accessible and easy to understand.

Rules:
1. Use simple, direct language
2. Avoid technical jargon unless essential
3. Break down complex sentences
4. Keep the main message clear
5. Text should be understandable by everyone
6. Preserve all important information";
    }

    private static string GetGenerateActionPrompt(bool isFrench, string personaContext)
    {
        return isFrench
            ? $@"Tu es un expert en rédaction de plans d'affaires BDC. {personaContext}

Ta tâche: GÉNÉRER une réponse professionnelle et pertinente à la question basée sur le contexte fourni.

Règles:
1. Utilise le contexte du projet de l'utilisateur (réponses précédentes, nom de l'entreprise, secteur)
2. Génère une réponse complète et bien structurée
3. Utilise un ton professionnel et crédible
4. Fournis des idées concrètes et pertinentes au secteur d'activité
5. La réponse doit être prête pour un plan d'affaires
6. N'invente PAS de chiffres ou statistiques précis
7. Laisse des placeholders si des données spécifiques sont nécessaires (ex: [insérer le montant])
8. La réponse doit être de longueur moyenne (150-300 mots)"
            : $@"You are an expert BDC business plan writer. {personaContext}

Your task: GENERATE a professional and relevant answer to the question based on the provided context.

Rules:
1. Use the user's project context (previous answers, business name, sector)
2. Generate a complete and well-structured response
3. Use a professional and credible tone
4. Provide concrete ideas relevant to the business sector
5. Response should be business plan ready
6. Do NOT invent specific numbers or statistics
7. Leave placeholders if specific data is needed (e.g., [insert amount])
8. Response should be medium length (150-300 words)";
    }

    private static string BuildTransformUserPrompt(TransformAnswerRequest request, string? businessContext = null)
    {
        var sb = new System.Text.StringBuilder();
        var isFrench = request.Language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var isGenerate = request.Action.Equals(TransformActionTypes.Generate, StringComparison.OrdinalIgnoreCase);

        // Include business context first if available
        if (!string.IsNullOrWhiteSpace(businessContext))
        {
            sb.AppendLine(businessContext);
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        if (isGenerate)
        {
            // For generate action, include the question to answer
            var questionLabel = isFrench ? "Question à laquelle répondre:" : "Question to answer:";
            sb.AppendLine(questionLabel);
            sb.AppendLine("---");
            sb.AppendLine(request.QuestionText ?? request.Context ?? "");
            sb.AppendLine("---");

            if (!string.IsNullOrWhiteSpace(request.Context) && request.Context != request.QuestionText)
            {
                sb.AppendLine();
                var contextLabel = isFrench ? "Contexte additionnel:" : "Additional context:";
                sb.AppendLine($"{contextLabel} {request.Context}");
            }
        }
        else
        {
            // For transform actions, include the text to transform
            var textLabel = isFrench ? "Texte à transformer:" : "Text to transform:";
            sb.AppendLine(textLabel);
            sb.AppendLine("---");
            sb.AppendLine(request.Answer);
            sb.AppendLine("---");

            if (!string.IsNullOrWhiteSpace(request.Context))
            {
                sb.AppendLine();
                var contextLabel = isFrench ? "Contexte de la question:" : "Question context:";
                sb.AppendLine($"{contextLabel} {request.Context}");
            }
        }

        return sb.ToString();
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

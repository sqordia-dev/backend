using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using System.Text;
using Sqordia.Domain.ValueObjects;

namespace Sqordia.Application.Services.Implementations;

public class BusinessPlanGenerationService : IBusinessPlanGenerationService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IAIPromptService _aiPromptService;
    private readonly IEmailService _emailService;
    private readonly ILogger<BusinessPlanGenerationService> _logger;

    public BusinessPlanGenerationService(
        IApplicationDbContext context,
        IAIService aiService,
        IAIPromptService aiPromptService,
        IEmailService emailService,
        ILogger<BusinessPlanGenerationService> logger)
    {
        _context = context;
        _aiService = aiService;
        _aiPromptService = aiPromptService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<BusinessPlan>> GenerateBusinessPlanAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting business plan generation for ID: {BusinessPlanId}", businessPlanId);

            var businessPlan = await _context.BusinessPlans
                .Include(bp => bp.Organization)
                .Include(bp => bp.QuestionnaireResponses)
                    .ThenInclude(qr => qr.QuestionTemplate)
                .Include(bp => bp.QuestionnaireResponses)
                    .ThenInclude(qr => qr.QuestionTemplateV2)
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<BusinessPlan>(Error.NotFound("BusinessPlan.Error.NotFound", $"Business plan with ID {businessPlanId} not found."));
            }

            // Check if questionnaire is complete
            // StartGeneration requires QuestionnaireComplete status, but we allow Draft if questionnaire is actually complete
            if (businessPlan.Status == BusinessPlanStatus.Draft)
            {
                // Check if all required questions are actually answered
                var template = await _context.QuestionnaireTemplates
                    .Include(qt => qt.Questions)
                    .Where(qt => qt.PlanType == businessPlan.PlanType && qt.IsActive)
                    .OrderByDescending(qt => qt.Version)
                    .FirstOrDefaultAsync(cancellationToken);

                if (template != null)
                {
                    var requiredQuestions = template.Questions.Where(q => q.IsRequired).Select(q => q.Id).ToList();
                    var answeredRequiredQuestions = await _context.QuestionnaireResponses
                        .Where(qr => qr.BusinessPlanId == businessPlanId &&
                                     qr.QuestionTemplateId.HasValue &&
                                     requiredQuestions.Contains(qr.QuestionTemplateId.Value))
                        .CountAsync(cancellationToken);

                    // If all required questions are answered, mark as complete
                    if (answeredRequiredQuestions == requiredQuestions.Count && requiredQuestions.Count > 0)
                    {
                        businessPlan.MarkQuestionnaireComplete();
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        return Result.Failure<BusinessPlan>(Error.Validation("BusinessPlan.QuestionnaireIncomplete", "Business plan questionnaire must be completed before generation. Please complete all required questions."));
                    }
                }
            }
            else if (businessPlan.Status != BusinessPlanStatus.QuestionnaireComplete && 
                     businessPlan.Status != BusinessPlanStatus.Generating)
            {
                return Result.Failure<BusinessPlan>(Error.Validation("BusinessPlan.InvalidStatus", $"Business plan must be in Draft or QuestionnaireComplete status to generate. Current status: {businessPlan.Status}"));
            }

            // Mark as generating (this will throw if status is not QuestionnaireComplete, so we ensure it is above)
            if (businessPlan.Status == BusinessPlanStatus.Generating)
            {
                // Already generating, allow retry
                _logger.LogInformation("Business plan {PlanId} is already generating, continuing...", businessPlanId);
            }
            else
            {
                businessPlan.StartGeneration("AI");
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Get questionnaire context
            var context = BuildQuestionnaireContext(businessPlan.QuestionnaireResponses);

            // Generate all sections
            var sections = GetAvailableSections(businessPlan.PlanType.ToString());
            var totalSections = sections.Count;
            var completedSections = 0;

            foreach (var section in sections)
            {
                _logger.LogInformation("Generating section: {Section}", section);

                var content = await GenerateSectionContentAsync(
                    businessPlan.PlanType,
                    section,
                    context,
                    language,
                    cancellationToken);

                // Update the appropriate property
                SetSectionContent(businessPlan, section, content);

                completedSections++;
                _logger.LogInformation("Completed {Completed}/{Total} sections", completedSections, totalSections);

                // Save after each section so frontend can track real-time progress
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Mark as generated
            businessPlan.CompleteGeneration();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Business plan generation completed for ID: {BusinessPlanId}", businessPlanId);

            // Send email notification to the user
            try
            {
                // Get the user who owns this business plan
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id.ToString() == businessPlan.CreatedBy, cancellationToken);

                if (user != null && user.Email != null && !string.IsNullOrEmpty(user.Email.Value))
                {
                    var userName = !string.IsNullOrEmpty(user.FirstName)
                        ? user.FirstName
                        : user.UserName ?? "User";

                    await _emailService.SendBusinessPlanGeneratedAsync(
                        user.Email.Value,
                        userName,
                        businessPlan.Id.ToString(),
                        businessPlan.Title);

                    _logger.LogInformation(
                        "Business plan generation notification email sent to {Email} for plan {PlanId}",
                        user.Email.Value,
                        businessPlanId);
                }
            }
            catch (Exception emailEx)
            {
                // Log but don't fail the generation if email fails
                _logger.LogError(emailEx,
                    "Failed to send business plan generation notification email for plan {PlanId}. Generation completed successfully.",
                    businessPlanId);
            }

            return Result.Success(businessPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating business plan for ID: {BusinessPlanId}. Exception type: {ExceptionType}, Message: {Message}, Inner: {InnerException}", 
                businessPlanId, 
                ex.GetType().Name, 
                ex.Message,
                ex.InnerException?.Message ?? "None");
            
            // Include inner exception details if available
            var errorMessage = $"Failed to generate business plan: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $" ({ex.InnerException.Message})";
            }
            
            return Result.Failure<BusinessPlan>(errorMessage);
        }
    }

    public async Task<Result<BusinessPlan>> RegenerateSectionAsync(
        Guid businessPlanId,
        string sectionName,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Regenerating section {Section} for business plan ID: {BusinessPlanId}", 
                sectionName, businessPlanId);

            var businessPlan = await _context.BusinessPlans
                .Include(bp => bp.QuestionnaireResponses)
                    .ThenInclude(qr => qr.QuestionTemplate)
                .Include(bp => bp.QuestionnaireResponses)
                    .ThenInclude(qr => qr.QuestionTemplateV2)
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<BusinessPlan>($"Business plan with ID {businessPlanId} not found.");
            }

            var context = BuildQuestionnaireContext(businessPlan.QuestionnaireResponses);
            var content = await GenerateSectionContentAsync(
                businessPlan.PlanType,
                sectionName,
                context,
                language,
                cancellationToken);

            SetSectionContent(businessPlan, sectionName, content);

            // LastModified is automatically updated by EF Core interceptor
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Section regeneration completed");

            return Result.Success(businessPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating section {Section} for business plan ID: {BusinessPlanId}", 
                sectionName, businessPlanId);
            return Result.Failure<BusinessPlan>($"Failed to regenerate section: {ex.Message}");
        }
    }

    public async Task<Result<BusinessPlanGenerationStatus>> GetGenerationStatusAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<BusinessPlanGenerationStatus>($"Business plan with ID {businessPlanId} not found.");
            }

            var totalSections = GetAvailableSections(businessPlan.PlanType.ToString()).Count;
            var completedSections = CountCompletedSections(businessPlan);
            
            // Determine current section being generated (if generation is in progress)
            string? currentSection = null;
            if (businessPlan.Status == BusinessPlanStatus.Generating && completedSections < totalSections)
            {
                var sections = GetAvailableSections(businessPlan.PlanType.ToString());
                // The current section is the one at the index of completedSections
                if (completedSections < sections.Count)
                {
                    currentSection = sections[completedSections];
                }
            }

            var status = new BusinessPlanGenerationStatus
            {
                BusinessPlanId = businessPlanId,
                Status = businessPlan.Status.ToString(),
                StartedAt = businessPlan.LastModified ?? businessPlan.Created,
                CompletedAt = businessPlan.Status == BusinessPlanStatus.Generated ? businessPlan.LastModified : null,
                TotalSections = totalSections,
                CompletedSections = completedSections,
                CompletionPercentage = totalSections > 0 ? (decimal)completedSections / totalSections * 100 : 0,
                CurrentSection = currentSection
            };

            return Result.Success(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting generation status for business plan ID: {BusinessPlanId}", businessPlanId);
            return Result.Failure<BusinessPlanGenerationStatus>($"Failed to get generation status: {ex.Message}");
        }
    }

    public List<string> GetAvailableSections(string planType)
    {
        var commonSections = new List<string>
        {
            "ExecutiveSummary",
            "ProblemStatement",
            "Solution",
            "MarketAnalysis",
            "CompetitiveAnalysis",
            "SwotAnalysis",
            "BusinessModel",
            "MarketingStrategy",
            "BrandingStrategy",
            "OperationsPlan",
            "ManagementTeam",
            "FinancialProjections",
            "FundingRequirements",
            "RiskAnalysis"
        };

        if (planType == "StrategicPlan" || planType == "2") // OBNL
        {
            commonSections.Add("MissionStatement");
            commonSections.Add("SocialImpact");
            commonSections.Add("BeneficiaryProfile");
            commonSections.Add("GrantStrategy");
            commonSections.Add("SustainabilityPlan");
        }
        else if (planType == "BusinessPlan" || planType == "0") // Startup
        {
            commonSections.Add("ExitStrategy");
        }

        return commonSections;
    }

    private string BuildQuestionnaireContext(ICollection<QuestionnaireResponse> responses)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== QUESTIONNAIRE RESPONSES ===\n");

        if (responses == null || !responses.Any())
        {
            _logger.LogWarning("No questionnaire responses provided for context building");
            return sb.ToString();
        }

        // Filter responses that have either V1 or V2 template loaded, then sort by order
        var validResponses = responses
            .Where(r => r.QuestionTemplate != null || r.QuestionTemplateV2 != null)
            .OrderBy(r => r.QuestionTemplate?.Order ?? r.QuestionTemplateV2?.Order ?? 0)
            .ToList();

        if (!validResponses.Any())
        {
            _logger.LogWarning("No valid questionnaire responses found (all templates are null). Total responses: {Count}", responses.Count);
            return sb.ToString();
        }

        foreach (var response in validResponses)
        {
            // Get question text from V1 or V2 template
            var questionText = response.QuestionTemplate?.QuestionText
                ?? response.QuestionTemplateV2?.QuestionText
                ?? "Unknown Question";

            // Truncate very long questions (keep first 200 chars)
            if (questionText.Length > 200)
            {
                questionText = questionText.Substring(0, 197) + "...";
            }

            // Get question type from V1 or V2 template
            var questionType = response.QuestionTemplate?.QuestionType
                ?? response.QuestionTemplateV2?.QuestionType
                ?? QuestionType.LongText;

            var answer = questionType switch
            {
                QuestionType.ShortText or QuestionType.LongText => TruncateText(response.ResponseText, 500),
                QuestionType.Number => response.NumericValue?.ToString(),
                QuestionType.Currency => $"${response.NumericValue:N2}",
                QuestionType.Percentage => $"{response.NumericValue}%",
                QuestionType.Date => response.DateValue?.ToString("yyyy-MM-dd"),
                QuestionType.YesNo => response.BooleanValue?.ToString(),
                QuestionType.SingleChoice or QuestionType.MultipleChoice => response.SelectedOptions,
                QuestionType.Scale => response.NumericValue?.ToString(),
                _ => TruncateText(response.ResponseText, 500)
            };

            // Get order from V1 or V2 template
            var order = response.QuestionTemplate?.Order ?? response.QuestionTemplateV2?.Order ?? 0;

            // More concise format: Q{order}: {short question} | A: {answer}
            sb.AppendLine($"Q{order}: {questionText} | A: {answer}");
        }

        return sb.ToString();
    }

    private string TruncateText(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        if (text.Length <= maxLength)
            return text;
        
        // Truncate and add ellipsis
        return text.Substring(0, maxLength - 3) + "...";
    }

    private async Task<string> GenerateSectionContentAsync(
        BusinessPlanType planType,
        string sectionName,
        string questionnaireContext,
        string language,
        CancellationToken cancellationToken)
    {
        var systemPrompt = await GetSystemPromptAsync(language, planType, cancellationToken);
        var userPrompt = await GetSectionPromptAsync(planType, sectionName, questionnaireContext, language, cancellationToken);

        // Increased maxTokens for gpt-4o which supports longer outputs
        // No limit on maxTokens - let the model generate comprehensive content
        var content = await _aiService.GenerateContentWithRetryAsync(
            systemPrompt,
            userPrompt,
            maxTokens: 4000, // Increased from 2000 for better quality sections
            temperature: 0.7f,
            maxRetries: 3,
            cancellationToken);

        return content;
    }

    private async Task<string> GetSystemPromptAsync(string language, BusinessPlanType planType, CancellationToken cancellationToken)
    {
        try
        {
            var promptDto = await _aiPromptService.GetSystemPromptAsync(
                planType.ToString(),
                language,
                cancellationToken);

            if (promptDto != null)
            {
                _logger.LogDebug("Using database system prompt for {PlanType} - {Language}", planType, language);
                return promptDto.SystemPrompt;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load system prompt from database, using fallback");
        }

        // Fallback to hardcoded prompt
        _logger.LogDebug("Using fallback system prompt for {Language}", language);
        return GetSystemPrompt(language);
    }

    private string GetSystemPrompt(string language)
    {
        return language.ToLower() == "en"
            ? @"You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.

VISUAL ELEMENTS: When appropriate, include visual elements to enhance the content using the following JSON code block format:

For charts (use for trends, comparisons, projections):
```json:chart
{""chartType"": ""bar"", ""title"": ""Chart Title"", ""labels"": [""Label1"", ""Label2""], ""datasets"": [{""label"": ""Series Name"", ""data"": [100, 200], ""color"": ""#3B82F6""}]}
```

For tables (use for structured data, comparisons, timelines):
```json:table
{""tableType"": ""comparison"", ""headers"": [""Feature"", ""Us"", ""Competitor""], ""rows"": [{""cells"": [{""value"": ""Price""}, {""value"": ""$99""}, {""value"": ""$149""}]}]}
```

For key metrics (use for KPIs, important numbers):
```json:metrics
{""layout"": ""grid"", ""metrics"": [{""label"": ""Market Size"", ""value"": 5000000, ""format"": ""currency""}, {""label"": ""Growth"", ""value"": 25, ""format"": ""percentage"", ""trend"": ""up""}]}
```

Chart types: line, bar, stacked-bar, pie, donut, area
Table types: financial, comparison, swot, timeline, pricing, custom
Metric formats: currency, percentage, number, text
Metric trends: up, down, neutral

Include 1-3 relevant visual elements per section where they add value. Always include explanatory prose around each visual element."
            : @"Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.

ÉLÉMENTS VISUELS: Lorsque approprié, incluez des éléments visuels pour enrichir le contenu en utilisant le format JSON suivant:

Pour les graphiques (utilisez pour les tendances, comparaisons, projections):
```json:chart
{""chartType"": ""bar"", ""title"": ""Titre du graphique"", ""labels"": [""Label1"", ""Label2""], ""datasets"": [{""label"": ""Nom de série"", ""data"": [100, 200], ""color"": ""#3B82F6""}]}
```

Pour les tableaux (utilisez pour les données structurées, comparaisons, calendriers):
```json:table
{""tableType"": ""comparison"", ""headers"": [""Caractéristique"", ""Nous"", ""Concurrent""], ""rows"": [{""cells"": [{""value"": ""Prix""}, {""value"": ""99$""}, {""value"": ""149$""}]}]}
```

Pour les métriques clés (utilisez pour les KPI, chiffres importants):
```json:metrics
{""layout"": ""grid"", ""metrics"": [{""label"": ""Taille du marché"", ""value"": 5000000, ""format"": ""currency""}, {""label"": ""Croissance"", ""value"": 25, ""format"": ""percentage"", ""trend"": ""up""}]}
```

Types de graphiques: line, bar, stacked-bar, pie, donut, area
Types de tableaux: financial, comparison, swot, timeline, pricing, custom
Formats de métriques: currency, percentage, number, text
Tendances: up, down, neutral

Incluez 1-3 éléments visuels pertinents par section lorsqu'ils ajoutent de la valeur. Incluez toujours du texte explicatif autour de chaque élément visuel.";
    }

    private async Task<string> GetSectionPromptAsync(
        BusinessPlanType planType,
        string sectionName,
        string context,
        string language,
        CancellationToken cancellationToken)
    {
        // Normalize section name: convert kebab-case to PascalCase for database lookup
        var normalizedSectionName = NormalizeSectionName(sectionName);

        try
        {
            var promptDto = await _aiPromptService.GetPromptBySectionAsync(
                normalizedSectionName,
                planType.ToString(),
                language,
                "ContentGeneration",
                cancellationToken);

            if (promptDto != null)
            {
                _logger.LogDebug("Using database prompt for section {SectionName} - {PlanType} - {Language}", 
                    sectionName, planType, language);
                
                // Replace variables in the template
                var userPrompt = promptDto.UserPromptTemplate
                    .Replace("{sectionName}", sectionName)
                    .Replace("{questionnaireContext}", context)
                    .Replace("{context}", context);

                return userPrompt;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load section prompt from database for {SectionName}, using fallback", sectionName);
        }

        // Fallback to hardcoded prompt
        _logger.LogDebug("Using fallback prompt for section {SectionName}", sectionName);
        return GetSectionPrompt(planType, sectionName, context, language);
    }

    private string GetSectionPrompt(BusinessPlanType planType, string sectionName, string context, string language)
    {
        var prompts = language.ToLower() == "en" ? GetEnglishPrompts() : GetFrenchPrompts();

        // Normalize section name: convert kebab-case to PascalCase
        var normalizedName = NormalizeSectionName(sectionName);

        if (!prompts.TryGetValue(normalizedName, out var template))
        {
            // Try original name as fallback
            if (!prompts.TryGetValue(sectionName, out template))
            {
                throw new InvalidOperationException($"No prompt template found for section: {sectionName}");
            }
        }

        return $"{template}\n\n{context}\n\nBased on the questionnaire responses above, write a comprehensive {sectionName} section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.";
    }

    /// <summary>
    /// Normalizes section name from kebab-case to PascalCase
    /// Example: "financial-projections" -> "FinancialProjections"
    /// </summary>
    private static string NormalizeSectionName(string sectionName)
    {
        if (string.IsNullOrEmpty(sectionName))
            return sectionName;

        // If already PascalCase (no hyphens), return as-is
        if (!sectionName.Contains('-'))
            return sectionName;

        // Convert kebab-case to PascalCase
        var parts = sectionName.Split('-');
        var result = string.Concat(parts.Select(part =>
            string.IsNullOrEmpty(part) ? part :
            char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant()
        ));

        return result;
    }

    private Dictionary<string, string> GetFrenchPrompts()
    {
        return new Dictionary<string, string>
        {
            ["ExecutiveSummary"] = @"Rédigez un résumé exécutif captivant qui présente l'entreprise, sa proposition de valeur unique, son marché cible, ses avantages concurrentiels et ses objectifs financiers principaux. Le résumé doit donner envie au lecteur d'en savoir plus.

INCLUEZ un élément visuel de métriques clés montrant 3-4 KPI importants (ex: revenus cibles, taille du marché, taux de croissance, financement requis).",

            ["ProblemStatement"] = @"Identifiez et décrivez le problème ou le besoin non satisfait que votre entreprise/organisation vise à résoudre. Expliquez pourquoi ce problème est important et urgent pour le marché cible.",

            ["Solution"] = @"Présentez en détail les produits ou services offerts. Expliquez leurs caractéristiques, leurs avantages, comment ils résolvent les problèmes des clients et ce qui les différencie de la concurrence.",

            ["MarketAnalysis"] = @"Analysez le marché cible : taille, croissance, tendances, segments. Incluez des données sur l'industrie, les opportunités et les défis. Démontrez une compréhension approfondie du marché.

INCLUEZ des éléments visuels:
1. Un graphique à barres ou en camembert montrant la segmentation ou les parts de marché
2. Des métriques clés montrant TAM/SAM/SOM ou la taille du marché",

            ["CompetitiveAnalysis"] = @"Identifiez les principaux concurrents directs et indirects. Analysez leurs forces et faiblesses. Expliquez clairement le positionnement concurrentiel de l'entreprise et ses avantages distinctifs.

INCLUEZ un tableau comparatif montrant les caractéristiques/capacités clés vs les concurrents (utilisez tableType: 'comparison').",

            ["SwotAnalysis"] = @"Réalisez une analyse SWOT complète : Forces (atouts internes), Faiblesses (limites internes), Opportunités (facteurs externes positifs), Menaces (risques externes). Soyez spécifique et stratégique.

INCLUEZ un tableau SWOT avec tableType 'swot' contenant les quatre quadrants avec des éléments spécifiques pour chacun.",

            ["BusinessModel"] = @"Expliquez le modèle d'affaires : comment l'entreprise crée, délivre et capture de la valeur. Incluez les flux de revenus, la structure de coûts, les ressources clés et les partenariats stratégiques.

INCLUEZ un graphique en camembert montrant la répartition des sources de revenus ou la structure des coûts.",

            ["MarketingStrategy"] = @"Décrivez la stratégie marketing complète : positionnement, branding, canaux de communication, tactiques d'acquisition de clients, stratégie de contenu et budget marketing.

INCLUEZ un graphique montrant l'allocation du budget marketing par canal ou l'acquisition de clients prévue dans le temps.",

            ["BrandingStrategy"] = @"Expliquez la stratégie de marque : identité visuelle, ton de communication, proposition de valeur de la marque, différenciation et comment la marque résonnera avec le public cible.",

            ["OperationsPlan"] = @"Décrivez les opérations quotidiennes : installations, équipements, technologies, processus clés, fournisseurs, chaîne d'approvisionnement et gestion de la qualité.

INCLUEZ un tableau chronologique montrant les jalons opérationnels clés ou les étapes du processus.",

            ["ManagementTeam"] = @"Présentez l'équipe de direction : compétences, expériences, rôles et responsabilités. Mettez en avant comment l'équipe est positionnée pour réussir.",

            ["FinancialProjections"] = @"Résumez les projections financières : revenus prévus, coûts principaux, rentabilité, besoins en trésorerie. Expliquez les hypothèses clés derrière ces projections.

INCLUEZ des éléments visuels:
1. Un graphique linéaire ou à barres montrant les projections de revenus sur 3-5 ans
2. Des métriques clés montrant les KPI financiers importants (seuil de rentabilité, marge brute, etc.)
3. Un tableau financier montrant les projections annuelles (revenus, coûts, profit)",

            ["FundingRequirements"] = @"Détaillez les besoins de financement : montant requis, utilisation des fonds, sources de financement potentielles, structure de financement et plan de remboursement ou retour sur investissement.

INCLUEZ:
1. Un graphique en camembert montrant la répartition de l'utilisation des fonds
2. Des métriques clés montrant le montant du financement, le ROI attendu, le calendrier",

            ["RiskAnalysis"] = @"Identifiez les principaux risques (marché, opérationnels, financiers, réglementaires) et présentez des stratégies concrètes d'atténuation pour chacun.

INCLUEZ un tableau montrant les risques, leur probabilité/impact, et les stratégies d'atténuation.",

            ["ExitStrategy"] = @"Expliquez les options de sortie potentielles pour les investisseurs : acquisition, IPO, buyout. Incluez un calendrier approximatif et les facteurs de valorisation.",

            ["MissionStatement"] = @"Rédigez un énoncé de mission clair et inspirant qui explique la raison d'être de l'organisation, qui elle sert, et l'impact qu'elle souhaite créer dans la communauté.",

            ["SocialImpact"] = @"Décrivez l'impact social attendu : changements positifs dans la communauté, indicateurs de succès social, bénéficiaires directs et indirects, et contribution aux objectifs de développement durable.

INCLUEZ des métriques clés montrant les chiffres d'impact attendus (bénéficiaires servis, résultats atteints, etc.).",

            ["BeneficiaryProfile"] = @"Dressez un portrait détaillé des bénéficiaires : qui ils sont, leurs besoins spécifiques, les défis auxquels ils font face, et comment l'organisation répondra à ces besoins.",

            ["GrantStrategy"] = @"Expliquez la stratégie de financement par subventions : sources identifiées (gouvernementales, fondations privées), processus de demande, calendrier et taux de réussite anticipé.

INCLUEZ un tableau montrant les sources potentielles de subventions, les montants et les calendriers de demande.",

            ["SustainabilityPlan"] = @"Décrivez comment l'organisation assurera sa pérennité financière et opérationnelle à long terme, au-delà du financement initial. Incluez les sources de revenus diversifiées et la stratégie de croissance durable.

INCLUEZ un graphique montrant la diversification projetée des revenus dans le temps."
        };
    }

    private Dictionary<string, string> GetEnglishPrompts()
    {
        return new Dictionary<string, string>
        {
            ["ExecutiveSummary"] = @"Write a compelling executive summary that presents the company, its unique value proposition, target market, competitive advantages, and key financial objectives. The summary should entice the reader to learn more.

INCLUDE a key metrics visual element showing 3-4 important KPIs (e.g., target revenue, market size, growth rate, funding needed).",

            ["ProblemStatement"] = @"Identify and describe the problem or unmet need that your business/organization aims to solve. Explain why this problem is important and urgent for the target market.",

            ["Solution"] = @"Present the products or services offered in detail. Explain their features, benefits, how they solve customer problems, and what differentiates them from the competition.",

            ["MarketAnalysis"] = @"Analyze the target market: size, growth, trends, segments. Include industry data, opportunities, and challenges. Demonstrate a deep understanding of the market.

INCLUDE visual elements:
1. A bar or pie chart showing market segmentation or market share
2. Key metrics showing TAM/SAM/SOM or market size data",

            ["CompetitiveAnalysis"] = @"Identify main direct and indirect competitors. Analyze their strengths and weaknesses. Clearly explain the company's competitive positioning and distinctive advantages.

INCLUDE a comparison table showing key features/capabilities vs competitors (use tableType: 'comparison').",

            ["SwotAnalysis"] = @"Conduct a complete SWOT analysis: Strengths (internal assets), Weaknesses (internal limitations), Opportunities (positive external factors), Threats (external risks). Be specific and strategic.

INCLUDE a SWOT table with tableType 'swot' containing the four quadrants with specific items for each.",

            ["BusinessModel"] = @"Explain the business model: how the company creates, delivers, and captures value. Include revenue streams, cost structure, key resources, and strategic partnerships.

INCLUDE a pie chart showing revenue stream breakdown or cost structure distribution.",

            ["MarketingStrategy"] = @"Describe the complete marketing strategy: positioning, branding, communication channels, customer acquisition tactics, content strategy, and marketing budget.

INCLUDE a chart showing marketing budget allocation across channels or expected customer acquisition over time.",

            ["BrandingStrategy"] = @"Explain the branding strategy: visual identity, tone of communication, brand value proposition, differentiation, and how the brand will resonate with the target audience.",

            ["OperationsPlan"] = @"Describe daily operations: facilities, equipment, technologies, key processes, suppliers, supply chain, and quality management.

INCLUDE a timeline table showing key operational milestones or process steps.",

            ["ManagementTeam"] = @"Present the management team: skills, experiences, roles, and responsibilities. Highlight how the team is positioned to succeed.",

            ["FinancialProjections"] = @"Summarize financial projections: expected revenues, main costs, profitability, cash flow needs. Explain the key assumptions behind these projections.

INCLUDE visual elements:
1. A line or bar chart showing 3-5 year revenue projections
2. Key metrics showing important financial KPIs (break-even point, gross margin, etc.)
3. A financial table showing yearly projections (revenue, costs, profit)",

            ["FundingRequirements"] = @"Detail funding needs: required amount, use of funds, potential funding sources, financing structure, and repayment plan or return on investment.

INCLUDE:
1. A pie chart showing use of funds breakdown
2. Key metrics showing funding amount, expected ROI, timeline",

            ["RiskAnalysis"] = @"Identify main risks (market, operational, financial, regulatory) and present concrete mitigation strategies for each.

INCLUDE a table showing risks, their likelihood/impact, and mitigation strategies.",

            ["ExitStrategy"] = @"Explain potential exit options for investors: acquisition, IPO, buyout. Include approximate timeline and valuation factors.",

            ["MissionStatement"] = @"Write a clear and inspiring mission statement that explains the organization's purpose, who it serves, and the impact it wishes to create in the community.",

            ["SocialImpact"] = @"Describe the expected social impact: positive changes in the community, social success indicators, direct and indirect beneficiaries, and contribution to sustainable development goals.

INCLUDE key metrics showing expected impact numbers (beneficiaries served, outcomes achieved, etc.).",

            ["BeneficiaryProfile"] = @"Draw a detailed portrait of beneficiaries: who they are, their specific needs, the challenges they face, and how the organization will address these needs.",

            ["GrantStrategy"] = @"Explain the grant funding strategy: identified sources (government, private foundations), application process, timeline, and anticipated success rate.

INCLUDE a table showing potential grant sources, amounts, and application timelines.",

            ["SustainabilityPlan"] = @"Describe how the organization will ensure its long-term financial and operational sustainability, beyond initial funding. Include diversified revenue sources and sustainable growth strategy.

INCLUDE a chart showing projected revenue diversification over time."
        };
    }

    private void SetSectionContent(BusinessPlan businessPlan, string sectionName, string content)
    {
        // Normalize section name: convert kebab-case to PascalCase
        var normalizedName = NormalizeSectionName(sectionName);

        // Use reflection to set properties since they have private setters
        var property = typeof(BusinessPlan).GetProperty(normalizedName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(businessPlan, content);
        }
        else
        {
            throw new InvalidOperationException($"Unknown or read-only section: {sectionName}");
        }
    }

    private int CountCompletedSections(BusinessPlan businessPlan)
    {
        var count = 0;
        if (!string.IsNullOrWhiteSpace(businessPlan.ExecutiveSummary)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.ProblemStatement)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.Solution)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.MarketAnalysis)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.CompetitiveAnalysis)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.SwotAnalysis)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.BusinessModel)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.MarketingStrategy)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.BrandingStrategy)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.OperationsPlan)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.ManagementTeam)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.FinancialProjections)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.FundingRequirements)) count++;
        if (!string.IsNullOrWhiteSpace(businessPlan.RiskAnalysis)) count++;
        
        if (businessPlan.PlanType == BusinessPlanType.BusinessPlan)
        {
            if (!string.IsNullOrWhiteSpace(businessPlan.ExitStrategy)) count++;
        }
        else if (businessPlan.PlanType == BusinessPlanType.StrategicPlan)
        {
            if (!string.IsNullOrWhiteSpace(businessPlan.MissionStatement)) count++;
            if (!string.IsNullOrWhiteSpace(businessPlan.SocialImpact)) count++;
            if (!string.IsNullOrWhiteSpace(businessPlan.BeneficiaryProfile)) count++;
            if (!string.IsNullOrWhiteSpace(businessPlan.GrantStrategy)) count++;
            if (!string.IsNullOrWhiteSpace(businessPlan.SustainabilityPlan)) count++;
        }
        
        return count;
    }
}


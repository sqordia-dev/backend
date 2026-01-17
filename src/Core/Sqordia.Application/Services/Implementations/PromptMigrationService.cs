using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Responses.Admin;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service for migrating hardcoded prompts to the database
/// </summary>
public class PromptMigrationService : IPromptMigrationService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIPromptService _aiPromptService;
    private readonly ILogger<PromptMigrationService> _logger;

    public PromptMigrationService(
        IApplicationDbContext context,
        IAIPromptService aiPromptService,
        ILogger<PromptMigrationService> logger)
    {
        _context = context;
        _aiPromptService = aiPromptService;
        _logger = logger;
    }

    public async Task<List<AIPromptDto>> MigrateDefaultPromptsAsync(CancellationToken cancellationToken = default)
    {
        var migratedPrompts = new List<AIPromptDto>();

        try
        {
            _logger.LogInformation("Starting migration of default prompts to database");

            // Migrate system prompts
            await MigrateSystemPromptsAsync(migratedPrompts, cancellationToken);

            // Migrate section prompts for BusinessPlan
            await MigrateSectionPromptsAsync(
                BusinessPlanType.BusinessPlan,
                GetEnglishPrompts(),
                GetFrenchPrompts(),
                migratedPrompts,
                cancellationToken);

            // Migrate section prompts for StrategicPlan
            await MigrateSectionPromptsAsync(
                BusinessPlanType.StrategicPlan,
                GetEnglishPrompts(),
                GetFrenchPrompts(),
                migratedPrompts,
                cancellationToken);

            _logger.LogInformation("Migration completed. {Count} prompts migrated", migratedPrompts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during prompt migration");
            throw;
        }

        return migratedPrompts;
    }

    private async Task MigrateSystemPromptsAsync(
        List<AIPromptDto> migratedPrompts,
        CancellationToken cancellationToken)
    {
        var systemPrompts = new Dictionary<(string PlanType, string Language), string>
        {
            { ("BusinessPlan", "en"), GetEnglishSystemPrompt() },
            { ("BusinessPlan", "fr"), GetFrenchSystemPrompt() },
            { ("StrategicPlan", "en"), GetEnglishSystemPrompt() },
            { ("StrategicPlan", "fr"), GetFrenchSystemPrompt() }
        };

        foreach (var (key, systemPrompt) in systemPrompts)
        {
            var (planType, language) = key;

            // Check if prompt already exists
            var existing = await _aiPromptService.GetSystemPromptAsync(planType, language, cancellationToken);
            if (existing != null)
            {
                _logger.LogDebug("System prompt for {PlanType} - {Language} already exists, skipping", planType, language);
                continue;
            }

            var request = new CreateAIPromptRequest
            {
                Name = $"System Prompt - {planType} - {language.ToUpper()}",
                Description = $"Default system prompt for {planType} in {language}",
                Category = "SystemPrompt",
                PlanType = planType,
                Language = language,
                SystemPrompt = systemPrompt,
                UserPromptTemplate = "", // System prompts don't have user templates
                Variables = "{}",
                Notes = "Migrated from hardcoded prompts"
            };

            var promptId = await _aiPromptService.CreatePromptAsync(request, cancellationToken);
            var prompt = await _aiPromptService.GetPromptAsync(promptId, cancellationToken);
            if (prompt != null)
            {
                migratedPrompts.Add(prompt);
                _logger.LogInformation("Migrated system prompt for {PlanType} - {Language}", planType, language);
            }
        }
    }

    private async Task MigrateSectionPromptsAsync(
        BusinessPlanType planType,
        Dictionary<string, string> englishPrompts,
        Dictionary<string, string> frenchPrompts,
        List<AIPromptDto> migratedPrompts,
        CancellationToken cancellationToken)
    {
        var planTypeString = planType.ToString();
        var sections = GetSectionsForPlanType(planType);

        foreach (var sectionName in sections)
        {
            // Migrate English prompt
            if (englishPrompts.TryGetValue(sectionName, out var englishTemplate))
            {
                await MigrateSectionPromptAsync(
                    sectionName,
                    planTypeString,
                    "en",
                    englishTemplate,
                    migratedPrompts,
                    cancellationToken);
            }

            // Migrate French prompt
            if (frenchPrompts.TryGetValue(sectionName, out var frenchTemplate))
            {
                await MigrateSectionPromptAsync(
                    sectionName,
                    planTypeString,
                    "fr",
                    frenchTemplate,
                    migratedPrompts,
                    cancellationToken);
            }
        }
    }

    private async Task MigrateSectionPromptAsync(
        string sectionName,
        string planType,
        string language,
        string template,
        List<AIPromptDto> migratedPrompts,
        CancellationToken cancellationToken)
    {
        // Check if prompt already exists
        var existing = await _aiPromptService.GetPromptBySectionAsync(
            sectionName,
            planType,
            language,
            "ContentGeneration",
            cancellationToken);

        if (existing != null)
        {
            _logger.LogDebug("Prompt for section {SectionName} - {PlanType} - {Language} already exists, skipping",
                sectionName, planType, language);
            return;
        }

        // Build the user prompt template with variables
        var userPromptTemplate = $"{template}\n\n{{questionnaireContext}}\n\nBased on the questionnaire responses above, write a comprehensive {sectionName} section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.";

        var request = new CreateAIPromptRequest
        {
            Name = $"{sectionName} - {planType} - {language.ToUpper()}",
            Description = $"Prompt for generating {sectionName} section in {planType} plans ({language})",
            Category = "ContentGeneration",
            PlanType = planType,
            Language = language,
            SectionName = sectionName,
            SystemPrompt = language == "en" ? GetEnglishSystemPrompt() : GetFrenchSystemPrompt(),
            UserPromptTemplate = userPromptTemplate,
            Variables = "{\"questionnaireContext\": \"The questionnaire responses context\"}",
            Notes = "Migrated from hardcoded prompts"
        };

        var promptId = await _aiPromptService.CreatePromptAsync(request, cancellationToken);
        var prompt = await _aiPromptService.GetPromptAsync(promptId, cancellationToken);
        if (prompt != null)
        {
            migratedPrompts.Add(prompt);
            _logger.LogInformation("Migrated prompt for section {SectionName} - {PlanType} - {Language}",
                sectionName, planType, language);
        }
    }

    private List<string> GetSectionsForPlanType(BusinessPlanType planType)
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

        if (planType == BusinessPlanType.StrategicPlan)
        {
            commonSections.AddRange(new[]
            {
                "MissionStatement",
                "SocialImpact",
                "BeneficiaryProfile",
                "GrantStrategy",
                "SustainabilityPlan"
            });
        }
        else if (planType == BusinessPlanType.BusinessPlan)
        {
            commonSections.Add("ExitStrategy");
        }

        return commonSections;
    }

    private string GetEnglishSystemPrompt()
    {
        return @"You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.";
    }

    private string GetFrenchSystemPrompt()
    {
        return @"Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.";
    }

    private Dictionary<string, string> GetEnglishPrompts()
    {
        return new Dictionary<string, string>
        {
            ["ExecutiveSummary"] = @"Write a compelling executive summary that presents the company, its unique value proposition, target market, competitive advantages, and key financial objectives. The summary should entice the reader to learn more.",
            ["ProblemStatement"] = @"Identify and describe the problem or unmet need that your business/organization aims to solve. Explain why this problem is important and urgent for the target market.",
            ["Solution"] = @"Present the products or services offered in detail. Explain their features, benefits, how they solve customer problems, and what differentiates them from the competition.",
            ["MarketAnalysis"] = @"Analyze the target market: size, growth, trends, segments. Include industry data, opportunities, and challenges. Demonstrate a deep understanding of the market.",
            ["CompetitiveAnalysis"] = @"Identify main direct and indirect competitors. Analyze their strengths and weaknesses. Clearly explain the company's competitive positioning and distinctive advantages.",
            ["SwotAnalysis"] = @"Conduct a complete SWOT analysis: Strengths (internal assets), Weaknesses (internal limitations), Opportunities (positive external factors), Threats (external risks). Be specific and strategic.",
            ["BusinessModel"] = @"Explain the business model: how the company creates, delivers, and captures value. Include revenue streams, cost structure, key resources, and strategic partnerships.",
            ["MarketingStrategy"] = @"Describe the complete marketing strategy: positioning, branding, communication channels, customer acquisition tactics, content strategy, and marketing budget.",
            ["BrandingStrategy"] = @"Explain the branding strategy: visual identity, tone of communication, brand value proposition, differentiation, and how the brand will resonate with the target audience.",
            ["OperationsPlan"] = @"Describe daily operations: facilities, equipment, technologies, key processes, suppliers, supply chain, and quality management.",
            ["ManagementTeam"] = @"Present the management team: skills, experiences, roles, and responsibilities. Highlight how the team is positioned to succeed.",
            ["FinancialProjections"] = @"Summarize financial projections: expected revenues, main costs, profitability, cash flow needs. Explain the key assumptions behind these projections.",
            ["FundingRequirements"] = @"Detail funding needs: required amount, use of funds, potential funding sources, financing structure, and repayment plan or return on investment.",
            ["RiskAnalysis"] = @"Identify main risks (market, operational, financial, regulatory) and present concrete mitigation strategies for each.",
            ["ExitStrategy"] = @"Explain potential exit options for investors: acquisition, IPO, buyout. Include approximate timeline and valuation factors.",
            ["MissionStatement"] = @"Write a clear and inspiring mission statement that explains the organization's purpose, who it serves, and the impact it wishes to create in the community.",
            ["SocialImpact"] = @"Describe the expected social impact: positive changes in the community, social success indicators, direct and indirect beneficiaries, and contribution to sustainable development goals.",
            ["BeneficiaryProfile"] = @"Draw a detailed portrait of beneficiaries: who they are, their specific needs, the challenges they face, and how the organization will address these needs.",
            ["GrantStrategy"] = @"Explain the grant funding strategy: identified sources (government, private foundations), application process, timeline, and anticipated success rate.",
            ["SustainabilityPlan"] = @"Describe how the organization will ensure its long-term financial and operational sustainability, beyond initial funding. Include diversified revenue sources and sustainable growth strategy."
        };
    }

    private Dictionary<string, string> GetFrenchPrompts()
    {
        return new Dictionary<string, string>
        {
            ["ExecutiveSummary"] = @"Rédigez un résumé exécutif captivant qui présente l'entreprise, sa proposition de valeur unique, son marché cible, ses avantages concurrentiels et ses objectifs financiers principaux. Le résumé doit donner envie au lecteur d'en savoir plus.",
            ["ProblemStatement"] = @"Identifiez et décrivez le problème ou le besoin non satisfait que votre entreprise/organisation vise à résoudre. Expliquez pourquoi ce problème est important et urgent pour le marché cible.",
            ["Solution"] = @"Présentez en détail les produits ou services offerts. Expliquez leurs caractéristiques, leurs avantages, comment ils résolvent les problèmes des clients et ce qui les différencie de la concurrence.",
            ["MarketAnalysis"] = @"Analysez le marché cible : taille, croissance, tendances, segments. Incluez des données sur l'industrie, les opportunités et les défis. Démontrez une compréhension approfondie du marché.",
            ["CompetitiveAnalysis"] = @"Identifiez les principaux concurrents directs et indirects. Analysez leurs forces et faiblesses. Expliquez clairement le positionnement concurrentiel de l'entreprise et ses avantages distinctifs.",
            ["SwotAnalysis"] = @"Réalisez une analyse SWOT complète : Forces (atouts internes), Faiblesses (limites internes), Opportunités (facteurs externes positifs), Menaces (risques externes). Soyez spécifique et stratégique.",
            ["BusinessModel"] = @"Expliquez le modèle d'affaires : comment l'entreprise crée, délivre et capture de la valeur. Incluez les flux de revenus, la structure de coûts, les ressources clés et les partenariats stratégiques.",
            ["MarketingStrategy"] = @"Décrivez la stratégie marketing complète : positionnement, branding, canaux de communication, tactiques d'acquisition de clients, stratégie de contenu et budget marketing.",
            ["BrandingStrategy"] = @"Expliquez la stratégie de marque : identité visuelle, ton de communication, proposition de valeur de la marque, différenciation et comment la marque résonnera avec le public cible.",
            ["OperationsPlan"] = @"Décrivez les opérations quotidiennes : installations, équipements, technologies, processus clés, fournisseurs, chaîne d'approvisionnement et gestion de la qualité.",
            ["ManagementTeam"] = @"Présentez l'équipe de direction : compétences, expériences, rôles et responsabilités. Mettez en avant comment l'équipe est positionnée pour réussir.",
            ["FinancialProjections"] = @"Résumez les projections financières : revenus prévus, coûts principaux, rentabilité, besoins en trésorerie. Expliquez les hypothèses clés derrière ces projections.",
            ["FundingRequirements"] = @"Détaillez les besoins de financement : montant requis, utilisation des fonds, sources de financement potentielles, structure de financement et plan de remboursement ou retour sur investissement.",
            ["RiskAnalysis"] = @"Identifiez les principaux risques (marché, opérationnels, financiers, réglementaires) et présentez des stratégies concrètes d'atténuation pour chacun.",
            ["ExitStrategy"] = @"Expliquez les options de sortie potentielles pour les investisseurs : acquisition, IPO, buyout. Incluez un calendrier approximatif et les facteurs de valorisation.",
            ["MissionStatement"] = @"Rédigez un énoncé de mission clair et inspirant qui explique la raison d'être de l'organisation, qui elle sert, et l'impact qu'elle souhaite créer dans la communauté.",
            ["SocialImpact"] = @"Décrivez l'impact social attendu : changements positifs dans la communauté, indicateurs de succès social, bénéficiaires directs et indirects, et contribution aux objectifs de développement durable.",
            ["BeneficiaryProfile"] = @"Dressez un portrait détaillé des bénéficiaires : qui ils sont, leurs besoins spécifiques, les défis auxquels ils font face, et comment l'organisation répondra à ces besoins.",
            ["GrantStrategy"] = @"Expliquez la stratégie de financement par subventions : sources identifiées (gouvernementales, fondations privées), processus de demande, calendrier et taux de réussite anticipé.",
            ["SustainabilityPlan"] = @"Décrivez comment l'organisation assurera sa pérennité financière et opérationnelle à long terme, au-delà du financement initial. Incluez les sources de revenus diversifiées et la stratégie de croissance durable."
        };
    }
}

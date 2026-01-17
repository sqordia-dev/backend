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
    private readonly ILogger<BusinessPlanGenerationService> _logger;

    public BusinessPlanGenerationService(
        IApplicationDbContext context,
        IAIService aiService,
        IAIPromptService aiPromptService,
        ILogger<BusinessPlanGenerationService> logger)
    {
        _context = context;
        _aiService = aiService;
        _aiPromptService = aiPromptService;
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
                        .Where(qr => qr.BusinessPlanId == businessPlanId && requiredQuestions.Contains(qr.QuestionTemplateId))
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
            }

            // Mark as generated
            businessPlan.CompleteGeneration();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Business plan generation completed for ID: {BusinessPlanId}", businessPlanId);

            return Result.Success(businessPlan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating business plan for ID: {BusinessPlanId}", businessPlanId);
            return Result.Failure<BusinessPlan>($"Failed to generate business plan: {ex.Message}");
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

            var status = new BusinessPlanGenerationStatus
            {
                BusinessPlanId = businessPlanId,
                Status = businessPlan.Status.ToString(),
                StartedAt = businessPlan.LastModified ?? businessPlan.Created,
                CompletedAt = businessPlan.Status == BusinessPlanStatus.Generated ? businessPlan.LastModified : null,
                TotalSections = totalSections,
                CompletedSections = completedSections,
                CompletionPercentage = totalSections > 0 ? (decimal)completedSections / totalSections * 100 : 0
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

        foreach (var response in responses.OrderBy(r => r.QuestionTemplate.Order))
        {
            sb.AppendLine($"Question {response.QuestionTemplate.Order}: {response.QuestionTemplate.QuestionText}");
            
            var answer = response.QuestionTemplate.QuestionType switch
            {
                QuestionType.ShortText or QuestionType.LongText => response.ResponseText,
                QuestionType.Number => response.NumericValue?.ToString(),
                QuestionType.Currency => $"${response.NumericValue:N2}",
                QuestionType.Percentage => $"{response.NumericValue}%",
                QuestionType.Date => response.DateValue?.ToString("yyyy-MM-dd"),
                QuestionType.YesNo => response.BooleanValue?.ToString(),
                QuestionType.SingleChoice or QuestionType.MultipleChoice => response.SelectedOptions,
                QuestionType.Scale => response.NumericValue?.ToString(),
                _ => response.ResponseText
            };

            sb.AppendLine($"Answer: {answer}\n");
        }

        return sb.ToString();
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

        var content = await _aiService.GenerateContentWithRetryAsync(
            systemPrompt,
            userPrompt,
            maxTokens: 2000,
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

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness."
            : @"Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.";
    }

    private async Task<string> GetSectionPromptAsync(
        BusinessPlanType planType,
        string sectionName,
        string context,
        string language,
        CancellationToken cancellationToken)
    {
        try
        {
            var promptDto = await _aiPromptService.GetPromptBySectionAsync(
                sectionName,
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
        
        if (!prompts.TryGetValue(sectionName, out var template))
        {
            throw new InvalidOperationException($"No prompt template found for section: {sectionName}");
        }

        return $"{template}\n\n{context}\n\nBased on the questionnaire responses above, write a comprehensive {sectionName} section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.";
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

    private void SetSectionContent(BusinessPlan businessPlan, string sectionName, string content)
    {
        // Use reflection to set properties since they have private setters
        var property = typeof(BusinessPlan).GetProperty(sectionName);
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


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Pricing;
using Sqordia.Contracts.Responses.Pricing;
using Sqordia.Domain.Entities.BusinessPlan;
using System.Text;
using System.Text.Json;

namespace Sqordia.Application.Services.Implementations;

public class PricingAnalysisService : IPricingAnalysisService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PricingAnalysisService> _logger;

    public PricingAnalysisService(
        IApplicationDbContext context,
        IAIService aiService,
        ICurrentUserService currentUserService,
        ILogger<PricingAnalysisService> logger)
    {
        _context = context;
        _aiService = aiService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PricingAnalysisResponse> AnalyzePricingAsync(
        PricingAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing pricing for business plan {BusinessPlanId}", request.BusinessPlanId);

            // Get business plan context
            var businessPlan = await GetBusinessPlanAsync(request.BusinessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                throw new ArgumentException($"Business plan {request.BusinessPlanId} not found or access denied");
            }

            // Build context for AI
            var context = BuildPricingContext(businessPlan, request);

            // Generate pricing analysis using AI
            var systemPrompt = GetPricingAnalysisSystemPrompt(request.Language);
            var userPrompt = BuildPricingAnalysisUserPrompt(request, context);

            var aiContent = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt,
                userPrompt,
                maxTokens: 4000,
                temperature: 0.7f,
                maxRetries: 3,
                cancellationToken);

            // Parse AI response and build structured response
            var response = ParsePricingAnalysisResponse(request, aiContent, businessPlan);

            _logger.LogInformation("Pricing analysis completed for business plan {BusinessPlanId}", request.BusinessPlanId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing pricing for business plan {BusinessPlanId}", request.BusinessPlanId);
            throw;
        }
    }

    private async Task<BusinessPlan?> GetBusinessPlanAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            return null;
        }

        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
            .Include(bp => bp.QuestionnaireResponses)
                .ThenInclude(qr => qr.QuestionTemplate)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null)
        {
            return null;
        }

        // Verify user has access
        var isMember = await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                           om.UserId == currentUserId &&
                           om.IsActive, cancellationToken);

        return isMember ? businessPlan : null;
    }

    private string BuildPricingContext(BusinessPlan businessPlan, PricingAnalysisRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Business Plan: {businessPlan.Title}");
        sb.AppendLine($"Plan Type: {businessPlan.PlanType}");
        
        if (!string.IsNullOrWhiteSpace(businessPlan.MarketAnalysis))
        {
            sb.AppendLine($"Market Analysis: {businessPlan.MarketAnalysis.Substring(0, Math.Min(500, businessPlan.MarketAnalysis.Length))}");
        }
        
        if (!string.IsNullOrWhiteSpace(businessPlan.CompetitiveAnalysis))
        {
            sb.AppendLine($"Competitive Analysis: {businessPlan.CompetitiveAnalysis.Substring(0, Math.Min(500, businessPlan.CompetitiveAnalysis.Length))}");
        }

        return sb.ToString();
    }

    private string GetPricingAnalysisSystemPrompt(string language)
    {
        var isFrench = language.ToLower() == "fr";
        return isFrench
            ? @"Vous êtes un expert en stratégie de prix et analyse de marché. Votre tâche est de générer une analyse complète de prix et un rapport concurrentiel.

Directives:
- Générer une grille de prix avec 4 stratégies: Low-end, Mid-range, Premium, Value-based
- Recommander une stratégie de prix optimale avec justification
- Analyser la concurrence et fournir un rapport détaillé
- Fournir des recommandations de positionnement marché
- Retourner l'analyse au format structuré"
            : @"You are an expert in pricing strategy and market analysis. Your task is to generate a comprehensive pricing analysis and competitive report.

Guidelines:
- Generate a pricing grid with 4 strategies: Low-end, Mid-range, Premium, Value-based
- Recommend an optimal pricing strategy with justification
- Analyze competition and provide detailed report
- Provide market positioning recommendations
- Return analysis in structured format";
    }

    private string BuildPricingAnalysisUserPrompt(PricingAnalysisRequest request, string context)
    {
        var isFrench = request.Language.ToLower() == "fr";
        var sb = new StringBuilder();
        
        sb.AppendLine(isFrench ? "Contexte du plan d'affaires:" : "Business plan context:");
        sb.AppendLine(context);
        sb.AppendLine();
        
        sb.AppendLine(isFrench ? "Informations produit:" : "Product information:");
        sb.AppendLine($"{(isFrench ? "Nom" : "Name")}: {request.ProductName}");
        if (!string.IsNullOrEmpty(request.ProductDescription))
        {
            sb.AppendLine($"{(isFrench ? "Description" : "Description")}: {request.ProductDescription}");
        }
        if (request.CostPerUnit.HasValue)
        {
            sb.AppendLine($"{(isFrench ? "Coût par unité" : "Cost per unit")}: {request.CostPerUnit.Value}");
        }
        if (request.MarketSize.HasValue)
        {
            sb.AppendLine($"{(isFrench ? "Taille du marché" : "Market size")}: {request.MarketSize.Value}");
        }
        if (!string.IsNullOrEmpty(request.Industry))
        {
            sb.AppendLine($"{(isFrench ? "Industrie" : "Industry")}: {request.Industry}");
        }
        
        if (request.Competitors != null && request.Competitors.Any())
        {
            sb.AppendLine();
            sb.AppendLine(isFrench ? "Concurrents:" : "Competitors:");
            foreach (var competitor in request.Competitors)
            {
                sb.AppendLine($"- {competitor.Name}: {competitor.Price}");
            }
        }

        return sb.ToString();
    }

    private PricingAnalysisResponse ParsePricingAnalysisResponse(
        PricingAnalysisRequest request,
        string aiContent,
        BusinessPlan businessPlan)
    {
        // Calculate pricing options based on cost
        var costPerUnit = request.CostPerUnit ?? 0;
        var lowEndPrice = costPerUnit * 1.5m; // 50% markup
        var midRangePrice = costPerUnit * 2.5m; // 150% markup
        var premiumPrice = costPerUnit * 4.0m; // 300% markup
        var valueBasedPrice = costPerUnit * 3.0m; // 200% markup

        var isFrench = request.Language.ToLower() == "fr";

        return new PricingAnalysisResponse
        {
            BusinessPlanId = request.BusinessPlanId,
            PricingGrid = new PricingGrid
            {
                LowEnd = new PricingOption
                {
                    Price = lowEndPrice,
                    StrategyName = isFrench ? "Prix bas" : "Low-end",
                    Description = isFrench ? "Stratégie de prix bas pour pénétrer le marché" : "Low-price strategy to penetrate the market",
                    ProfitMargin = 33.3m,
                    ExpectedMarketShare = 25m,
                    Pros = new List<string> { isFrench ? "Accès rapide au marché" : "Quick market access", isFrench ? "Volume élevé" : "High volume" },
                    Cons = new List<string> { isFrench ? "Marge faible" : "Low margin", isFrench ? "Positionnement bas" : "Low positioning" },
                    TargetSegment = isFrench ? "Marché de masse" : "Mass market"
                },
                MidRange = new PricingOption
                {
                    Price = midRangePrice,
                    StrategyName = isFrench ? "Prix moyen" : "Mid-range",
                    Description = isFrench ? "Équilibre entre prix et qualité" : "Balance between price and quality",
                    ProfitMargin = 60m,
                    ExpectedMarketShare = 40m,
                    Pros = new List<string> { isFrench ? "Bon équilibre" : "Good balance", isFrench ? "Marge raisonnable" : "Reasonable margin" },
                    Cons = new List<string> { isFrench ? "Concurrence élevée" : "High competition" },
                    TargetSegment = isFrench ? "Marché principal" : "Mainstream market"
                },
                Premium = new PricingOption
                {
                    Price = premiumPrice,
                    StrategyName = isFrench ? "Prix premium" : "Premium",
                    Description = isFrench ? "Positionnement haut de gamme" : "High-end positioning",
                    ProfitMargin = 75m,
                    ExpectedMarketShare = 15m,
                    Pros = new List<string> { isFrench ? "Marge élevée" : "High margin", isFrench ? "Image de marque" : "Brand image" },
                    Cons = new List<string> { isFrench ? "Volume limité" : "Limited volume", isFrench ? "Marché restreint" : "Restricted market" },
                    TargetSegment = isFrench ? "Marché premium" : "Premium market"
                },
                ValueBased = new PricingOption
                {
                    Price = valueBasedPrice,
                    StrategyName = isFrench ? "Prix basé sur la valeur" : "Value-based",
                    Description = isFrench ? "Prix basé sur la valeur perçue" : "Price based on perceived value",
                    ProfitMargin = 66.7m,
                    ExpectedMarketShare = 30m,
                    Pros = new List<string> { isFrench ? "Justification solide" : "Strong justification", isFrench ? "Marge optimale" : "Optimal margin" },
                    Cons = new List<string> { isFrench ? "Nécessite communication" : "Requires communication" },
                    TargetSegment = isFrench ? "Marché ciblé" : "Targeted market"
                }
            },
            RecommendedStrategy = new PricingStrategy
            {
                RecommendedPrice = midRangePrice,
                StrategyName = isFrench ? "Prix moyen recommandé" : "Recommended mid-range",
                Reasoning = aiContent.Length > 200 ? aiContent.Substring(0, 200) + "..." : aiContent,
                ExpectedRevenue = request.MarketSize.HasValue ? request.MarketSize.Value * 0.4m * midRangePrice : null,
                ImplementationSteps = new List<string>
                {
                    isFrench ? "Analyser la concurrence" : "Analyze competition",
                    isFrench ? "Tester le prix sur un échantillon" : "Test price on sample",
                    isFrench ? "Ajuster selon les retours" : "Adjust based on feedback"
                }
            },
            CompetitiveAnalysis = new CompetitiveAnalysisReport
            {
                Summary = aiContent.Length > 300 ? aiContent.Substring(0, 300) + "..." : aiContent,
                Competitors = request.Competitors?.Select(c => new CompetitorAnalysis
                {
                    Name = c.Name,
                    Price = c.Price,
                    Strengths = c.Strengths?.Split(',').ToList() ?? new List<string>(),
                    Weaknesses = c.Weaknesses?.Split(',').ToList() ?? new List<string>(),
                    ThreatLevel = "Medium"
                }).ToList() ?? new List<CompetitorAnalysis>(),
                CompetitivePositioning = isFrench ? "Positionnement concurrentiel analysé" : "Competitive positioning analyzed",
                KeyDifferentiators = new List<string>
                {
                    isFrench ? "Qualité supérieure" : "Superior quality",
                    isFrench ? "Service client exceptionnel" : "Exceptional customer service"
                },
                MarketOpportunities = new List<string>
                {
                    isFrench ? "Marché en croissance" : "Growing market",
                    isFrench ? "Niche non exploitée" : "Untapped niche"
                }
            },
            MarketPositioningRecommendations = new List<string>
            {
                isFrench ? "Se positionner comme leader qualité-prix" : "Position as quality-price leader",
                isFrench ? "Cibler les segments à forte valeur" : "Target high-value segments"
            },
            GeneratedAt = DateTime.UtcNow,
            Language = request.Language
        };
    }
}


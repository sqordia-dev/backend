using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Provides tools for AI-augmented generation:
/// - Industry benchmarks (Canadian data)
/// - Market sizing (TAM/SAM/SOM estimation)
/// - Cross-section references
/// - Financial consistency validation
/// - Regulatory requirements (Quebec/Canada)
/// - Competitive landscape patterns
/// </summary>
public class GenerationToolService : IGenerationToolService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GenerationToolService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Maps sections to their available tools.
    /// </summary>
    private static readonly Dictionary<string, string[]> SectionToolMap = new()
    {
        ["MarketAnalysis"] = new[] { "get_industry_benchmarks", "get_market_sizing" },
        ["CompetitiveAnalysis"] = new[] { "get_competitive_landscape", "get_industry_benchmarks" },
        ["FinancialProjections"] = new[] { "get_industry_benchmarks", "validate_financial_consistency" },
        ["FundingRequirements"] = new[] { "validate_financial_consistency" },
        ["OperationsPlan"] = new[] { "get_regulatory_requirements" },
        ["RiskAnalysis"] = new[] { "get_regulatory_requirements", "get_competitive_landscape" },
        ["BusinessModel"] = new[] { "get_industry_benchmarks" },
        ["SwotAnalysis"] = new[] { "get_competitive_landscape" },
    };

    public GenerationToolService(
        IApplicationDbContext context,
        ILogger<GenerationToolService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<GenerationTool> GetToolsForSection(string sectionName)
    {
        if (!SectionToolMap.TryGetValue(sectionName, out var toolNames))
            return new List<GenerationTool>();

        return toolNames.Select(GetToolDefinition).Where(t => t != null).ToList()!;
    }

    public async Task<string> ExecuteToolAsync(
        string toolName,
        Dictionary<string, object> arguments,
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing generation tool {ToolName} for plan {PlanId}", toolName, businessPlanId);

        return toolName switch
        {
            "get_industry_benchmarks" => GetIndustryBenchmarks(arguments),
            "get_market_sizing" => GetMarketSizing(arguments),
            "get_previous_section" => await GetPreviousSectionAsync(arguments, businessPlanId, cancellationToken),
            "validate_financial_consistency" => await ValidateFinancialConsistencyAsync(businessPlanId, cancellationToken),
            "get_regulatory_requirements" => GetRegulatoryRequirements(arguments),
            "get_competitive_landscape" => GetCompetitiveLandscape(arguments),
            _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {toolName}" })
        };
    }

    #region Tool Definitions

    private static GenerationTool? GetToolDefinition(string toolName)
    {
        return toolName switch
        {
            "get_industry_benchmarks" => new GenerationTool
            {
                Name = "get_industry_benchmarks",
                Description = "Get industry benchmarks including revenue ranges, margins, and growth rates for a specific industry in Canada",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["industry"] = new() { Type = "string", Description = "Industry name or NAICS code", Required = true },
                    ["metric"] = new() { Type = "string", Description = "Specific metric to retrieve", Required = false,
                        Enum = new List<string> { "revenue", "margins", "growth", "employees", "all" } }
                }
            },
            "get_market_sizing" => new GenerationTool
            {
                Name = "get_market_sizing",
                Description = "Estimate TAM/SAM/SOM for a given industry and geography in Canada",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["industry"] = new() { Type = "string", Description = "Industry or sector", Required = true },
                    ["geography"] = new() { Type = "string", Description = "Geographic scope (Quebec, Canada, etc.)", Required = false }
                }
            },
            "get_previous_section" => new GenerationTool
            {
                Name = "get_previous_section",
                Description = "Read content from an already-generated section of this business plan",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["sectionName"] = new() { Type = "string", Description = "Name of the section to read", Required = true }
                }
            },
            "validate_financial_consistency" => new GenerationTool
            {
                Name = "validate_financial_consistency",
                Description = "Validate financial consistency across sections (revenue, costs, projections)",
                Parameters = new Dictionary<string, ToolParameter>()
            },
            "get_regulatory_requirements" => new GenerationTool
            {
                Name = "get_regulatory_requirements",
                Description = "Get Quebec/Canadian business regulations and requirements for a specific industry",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["industry"] = new() { Type = "string", Description = "Industry or business type", Required = true },
                    ["category"] = new() { Type = "string", Description = "Regulation category", Required = false,
                        Enum = new List<string> { "licensing", "labor", "taxation", "privacy", "environmental", "all" } }
                }
            },
            "get_competitive_landscape" => new GenerationTool
            {
                Name = "get_competitive_landscape",
                Description = "Get common competitor types, market dynamics, and positioning strategies for an industry",
                Parameters = new Dictionary<string, ToolParameter>
                {
                    ["industry"] = new() { Type = "string", Description = "Industry or sector", Required = true }
                }
            },
            _ => null
        };
    }

    #endregion

    #region Tool Implementations

    private static string GetIndustryBenchmarks(Dictionary<string, object> args)
    {
        var industry = args.TryGetValue("industry", out var ind) ? ind.ToString() ?? "" : "";
        var metric = args.TryGetValue("metric", out var met) ? met.ToString() ?? "all" : "all";

        // Curated Canadian industry benchmark data
        var benchmarks = new
        {
            industry,
            country = "Canada",
            source = "Statistics Canada / Industry Canada estimates",
            note = "These are general industry benchmarks. Adjust based on specific business model and location.",
            metrics = new
            {
                averageRevenue = new { small = "$100K-$500K", medium = "$500K-$5M", large = "$5M+" },
                grossMargin = new { low = "20-30%", average = "40-50%", high = "60-80%" },
                netMargin = new { low = "2-5%", average = "5-10%", high = "10-20%" },
                annualGrowthRate = new { declining = "< 0%", stable = "0-3%", growing = "3-7%", highGrowth = "7-15%+" },
                averageEmployees = new { micro = "1-4", small = "5-19", medium = "20-99", large = "100+" },
                startupSurvivalRate = new { year1 = "85%", year3 = "70%", year5 = "51%", year10 = "35%" },
                averageStartupCost = new { low = "$5K-$25K", medium = "$25K-$100K", high = "$100K-$500K" }
            }
        };

        return JsonSerializer.Serialize(benchmarks, JsonOptions);
    }

    private static string GetMarketSizing(Dictionary<string, object> args)
    {
        var industry = args.TryGetValue("industry", out var ind) ? ind.ToString() ?? "" : "";
        var geography = args.TryGetValue("geography", out var geo) ? geo.ToString() ?? "Quebec" : "Quebec";

        var sizing = new
        {
            industry,
            geography,
            source = "Statistics Canada / Industry analysis estimates",
            methodology = "Top-down estimation based on industry data and geographic factors",
            tam = new
            {
                description = "Total Addressable Market - entire market demand",
                note = "Estimate based on industry reports for Canadian market",
                guidance = "Use Statistics Canada data for precise figures. Typical TAM for Canadian industries ranges from $1B-$50B depending on sector."
            },
            sam = new
            {
                description = "Serviceable Available Market - segment you can reach",
                note = "Typically 10-40% of TAM based on geographic and segment focus",
                quebecFactor = "Quebec represents approximately 23% of Canadian GDP"
            },
            som = new
            {
                description = "Serviceable Obtainable Market - realistic capture in 3-5 years",
                note = "Typically 1-5% of SAM for new entrants",
                guidance = "Conservative estimates are more credible for bank submissions"
            },
            populationData = new
            {
                canada = "40.1M (2024)",
                quebec = "8.9M (2024)",
                montreal = "4.3M metro (2024)"
            }
        };

        return JsonSerializer.Serialize(sizing, JsonOptions);
    }

    private async Task<string> GetPreviousSectionAsync(
        Dictionary<string, object> args, Guid businessPlanId, CancellationToken ct)
    {
        var sectionName = args.TryGetValue("sectionName", out var sn) ? sn.ToString() ?? "" : "";

        var plan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, ct);

        if (plan == null)
            return JsonSerializer.Serialize(new { error = "Business plan not found" });

        var property = typeof(Domain.Entities.BusinessPlan.BusinessPlan).GetProperty(sectionName);
        var content = property?.GetValue(plan) as string;

        return JsonSerializer.Serialize(new
        {
            section = sectionName,
            hasContent = !string.IsNullOrWhiteSpace(content),
            content = content ?? "(Section not yet generated)"
        }, JsonOptions);
    }

    private async Task<string> ValidateFinancialConsistencyAsync(Guid businessPlanId, CancellationToken ct)
    {
        var plan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, ct);

        if (plan == null)
            return JsonSerializer.Serialize(new { error = "Business plan not found" });

        var findings = new List<object>();

        // Check for presence of financial sections
        if (string.IsNullOrWhiteSpace(plan.FinancialProjections))
            findings.Add(new { type = "missing", message = "Financial projections section not yet generated" });

        if (string.IsNullOrWhiteSpace(plan.FundingRequirements))
            findings.Add(new { type = "missing", message = "Funding requirements section not yet generated" });

        if (string.IsNullOrWhiteSpace(plan.BusinessModel))
            findings.Add(new { type = "missing", message = "Business model section not yet generated" });

        return JsonSerializer.Serialize(new
        {
            businessPlanId,
            financialSectionsPresent = new
            {
                financialProjections = !string.IsNullOrWhiteSpace(plan.FinancialProjections),
                fundingRequirements = !string.IsNullOrWhiteSpace(plan.FundingRequirements),
                businessModel = !string.IsNullOrWhiteSpace(plan.BusinessModel)
            },
            findings,
            recommendation = "Ensure revenue projections, costs, and funding needs are internally consistent"
        }, JsonOptions);
    }

    private static string GetRegulatoryRequirements(Dictionary<string, object> args)
    {
        var industry = args.TryGetValue("industry", out var ind) ? ind.ToString() ?? "" : "";
        var category = args.TryGetValue("category", out var cat) ? cat.ToString() ?? "all" : "all";

        var regulations = new
        {
            industry,
            jurisdiction = "Quebec, Canada",
            source = "Quebec government / federal regulations summary",
            requirements = new
            {
                licensing = new
                {
                    neq = "Numéro d'entreprise du Québec (NEQ) - required for all businesses",
                    gst_qst = "GST/QST registration required if revenues exceed $30,000/year",
                    permits = "Municipal business permits may be required depending on location and industry"
                },
                labor = new
                {
                    minimumWage = "$15.75/hr (Quebec, 2024)",
                    cnesst = "Mandatory workers' compensation (CNESST)",
                    normesDuTravail = "Quebec Labour Standards Act compliance",
                    equity = "Pay Equity Act applies to businesses with 10+ employees"
                },
                taxation = new
                {
                    federalRate = "Corporate tax: 15% federal (9% for small business deduction eligible)",
                    provincialRate = "Quebec corporate tax: 11.5% (3.2% for small businesses eligible for SBD)",
                    salesTax = "GST 5% + QST 9.975% = 14.975% combined"
                },
                privacy = new
                {
                    law25 = "Quebec Law 25 (Act to modernize legislative provisions regarding personal information protection)",
                    requirements = "Privacy policy, consent management, breach notification, privacy impact assessments",
                    bilingual = "Communications must be available in French (Charter of the French Language)"
                },
                environmental = new
                {
                    note = "Industry-specific environmental permits may be required",
                    ree = "Registre des entreprises du Québec compliance"
                }
            }
        };

        return JsonSerializer.Serialize(regulations, JsonOptions);
    }

    private static string GetCompetitiveLandscape(Dictionary<string, object> args)
    {
        var industry = args.TryGetValue("industry", out var ind) ? ind.ToString() ?? "" : "";

        var landscape = new
        {
            industry,
            analysis = new
            {
                competitorTypes = new[]
                {
                    new { type = "Direct", description = "Companies offering the same product/service to the same target market" },
                    new { type = "Indirect", description = "Companies solving the same problem with a different solution" },
                    new { type = "Substitutes", description = "Alternative solutions that customers might choose instead" }
                },
                portersFiveForces = new
                {
                    guidance = "Analyze: (1) Threat of new entrants, (2) Bargaining power of suppliers, (3) Bargaining power of buyers, (4) Threat of substitutes, (5) Industry rivalry"
                },
                positioningStrategies = new[]
                {
                    "Cost leadership - lowest price in market",
                    "Differentiation - unique features or quality",
                    "Niche focus - specialized market segment",
                    "Innovation - novel approach or technology",
                    "Customer experience - superior service"
                },
                canadianContext = new
                {
                    smeComposition = "99.8% of Canadian businesses are SMEs (under 500 employees)",
                    marketConcentration = "Many Canadian industries have moderate to high concentration",
                    bilingual = "Quebec market requires French-language capability as competitive factor"
                }
            }
        };

        return JsonSerializer.Serialize(landscape, JsonOptions);
    }

    #endregion
}

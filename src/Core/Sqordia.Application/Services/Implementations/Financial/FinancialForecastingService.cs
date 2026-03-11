using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Constants;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Engine;
using Sqordia.Application.Financial.Services.Previsio;
using Sqordia.Application.Services.AI;
using Sqordia.Contracts.Responses.Financial.Previsio;

namespace Sqordia.Application.Services.Implementations.Financial;

public class FinancialForecastingService : IFinancialForecastingService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IQuestionnaireContextService _questionnaireContext;
    private readonly ILogger<FinancialForecastingService> _logger;

    private const int ForecastMaxTokens = 3000;
    private const float ForecastTemperature = 0.4f;
    private const int AnalysisMaxTokens = 2500;
    private const float AnalysisTemperature = 0.3f;

    public FinancialForecastingService(
        IApplicationDbContext context,
        IAIService aiService,
        IQuestionnaireContextService questionnaireContext,
        ILogger<FinancialForecastingService> logger)
    {
        _context = context;
        _aiService = aiService;
        _questionnaireContext = questionnaireContext;
        _logger = logger;
    }

    public async Task<Result<FinancialForecastResponse>> GenerateForecastAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
            .Include(bp => bp.QuestionnaireResponses)
                .ThenInclude(qr => qr.QuestionTemplate)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

        if (businessPlan == null)
            return Result.Failure<FinancialForecastResponse>(
                Error.NotFound("BusinessPlan.NotFound", "Business plan not found"));

        var answers = _questionnaireContext.BuildAnswersDictionary(businessPlan.QuestionnaireResponses);
        var businessContext = BuildBusinessContextForForecast(businessPlan, answers, language);

        var systemPrompt = BuildForecastSystemPrompt(language);
        var userPrompt = BuildForecastUserPrompt(businessContext, language);

        try
        {
            var response = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt, userPrompt, ForecastMaxTokens, ForecastTemperature,
                PipelineConstants.ReducedMaxRetries, cancellationToken);

            var forecast = ParseForecastResponse(response);

            _logger.LogInformation(
                "Generated financial forecast for plan {PlanId}: {ProductCount} products, {ExpenseCount} expenses",
                businessPlanId, forecast.SuggestedProducts.Count, forecast.SuggestedExpenses.Count);

            return Result.Success(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate financial forecast for plan {PlanId}", businessPlanId);
            return Result.Failure<FinancialForecastResponse>(
                Error.Failure("Financial.Forecast.Failed", "Failed to generate financial forecast"));
        }
    }

    public async Task<Result<SalesVolumeForecastResponse>> ForecastSalesVolumeAsync(
        Guid businessPlanId,
        Guid salesProductId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var product = await _context.SalesProducts
            .FirstOrDefaultAsync(sp => sp.Id == salesProductId, cancellationToken);

        if (product == null)
            return Result.Failure<SalesVolumeForecastResponse>(
                Error.NotFound("SalesProduct.NotFound", "Sales product not found"));

        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
            .Include(bp => bp.QuestionnaireResponses)
                .ThenInclude(qr => qr.QuestionTemplate)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

        if (businessPlan == null)
            return Result.Failure<SalesVolumeForecastResponse>(
                Error.NotFound("BusinessPlan.NotFound", "Business plan not found"));

        var answers = _questionnaireContext.BuildAnswersDictionary(businessPlan.QuestionnaireResponses);
        var isFrench = language.StartsWith("fr", StringComparison.OrdinalIgnoreCase);

        var systemPrompt = isFrench
            ? """
              Tu es un analyste financier expert en prévisions de ventes. Génère des volumes de ventes mensuels
              réalistes pour un produit/service en tenant compte de la saisonnalité, de la montée en puissance
              et du contexte d'affaires. Réponds uniquement en JSON valide.
              """
            : """
              You are an expert financial analyst specialized in sales forecasting. Generate realistic monthly
              sales volumes for a product/service considering seasonality, ramp-up periods, and business context.
              Respond only with valid JSON.
              """;

        var answersContext = string.Join("\n", answers.Take(5).Select(a =>
            $"Q{a.Key}: {a.Value[..Math.Min(a.Value.Length, 200)]}"));

        var userPrompt = $"""
            Product: {product.Name}
            Unit Price: {product.UnitPrice:C2}
            Industry: {businessPlan.Organization?.Industry ?? "N/A"}
            Location: {businessPlan.Organization?.City ?? "Quebec"}, {businessPlan.Organization?.Province ?? "QC"}

            Business Context:
            {answersContext}

            Return JSON with keys: monthly_volumes (array of 12 numbers), seasonality_pattern, rationale, confidence (0.0-1.0).
            Consider: ramp-up in first 3 months, seasonal patterns for the industry, realistic volumes for a startup.
            """;

        try
        {
            var response = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt, userPrompt, 1000, ForecastTemperature,
                PipelineConstants.ReducedMaxRetries, cancellationToken);

            var parsed = ParseJsonFromResponse<VolumeForecastJson>(response);
            var volumes = parsed?.MonthlyVolumes ?? new decimal[12];
            if (volumes.Length < 12)
                volumes = volumes.Concat(Enumerable.Repeat(0m, 12 - volumes.Length)).ToArray();

            return Result.Success(new SalesVolumeForecastResponse
            {
                ProductName = product.Name,
                MonthlyVolumes = volumes[..12],
                SeasonalityPattern = parsed?.SeasonalityPattern ?? "flat",
                Rationale = parsed?.Rationale ?? "",
                Confidence = parsed?.Confidence ?? 0.4f
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to forecast sales volume for product {ProductId}", salesProductId);
            return Result.Failure<SalesVolumeForecastResponse>(
                Error.Failure("Financial.VolumeForecast.Failed", "Failed to forecast sales volumes"));
        }
    }

    public async Task<Result<FinancialAnalysisResponse>> AnalyzeFinancialsAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        // Load financial plan and run calculation engine
        var financialPlan = await _context.FinancialPlansPrevisio
            .AsNoTracking()
            .Include(fp => fp.SalesProducts).ThenInclude(sp => sp.SalesVolumes)
            .Include(fp => fp.CostOfGoodsSoldItems)
            .Include(fp => fp.PayrollItems)
            .Include(fp => fp.SalesExpenseItems)
            .Include(fp => fp.AdminExpenseItems)
            .Include(fp => fp.CapexAssets)
            .Include(fp => fp.FinancingSources).ThenInclude(fs => fs.AmortizationEntries)
            .Where(fp => !fp.IsDeleted)
            .FirstOrDefaultAsync(fp => fp.BusinessPlanId == businessPlanId, cancellationToken);

        if (financialPlan == null)
            return Result.Failure<FinancialAnalysisResponse>(
                Error.NotFound("FinancialPlan.NotFound", "No financial plan found. Create one first."));

        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

        var isFrench = language.StartsWith("fr", StringComparison.OrdinalIgnoreCase);

        // Build summary of financial data for AI analysis
        var financialSummary = BuildFinancialSummary(financialPlan);

        var systemPrompt = isFrench
            ? """
              Tu es un conseiller financier expert analysant des projections financières pour un plan d'affaires.
              Évalue la santé financière, identifie les forces et faiblesses, et fournis des recommandations
              concrètes. Réponds en JSON valide. Sois honnête et direct.
              """
            : """
              You are an expert financial advisor analyzing financial projections for a business plan.
              Evaluate financial health, identify strengths and weaknesses, and provide actionable
              recommendations. Respond with valid JSON. Be honest and direct.
              """;

        var jsonTemplate = """
            Return JSON with keys:
            - overall_health_score (0-100)
            - strengths (array of objects with category, description, impact)
            - weaknesses (array of objects with category, description, impact)
            - recommendations (array of objects with category, description, impact)
            - benchmark (object with industry, typical_gross_margin, typical_net_margin, typical_break_even_months, data_confidence)
            Impact values: high, medium, or low. data_confidence: 0.0-1.0.
            """;

        var userPrompt = $"""
            Industry: {businessPlan?.Organization?.Industry ?? "N/A"}
            Location: {businessPlan?.Organization?.Province ?? "Quebec"}

            Financial Summary:
            {financialSummary}

            {jsonTemplate}
            """;

        try
        {
            var response = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt, userPrompt, AnalysisMaxTokens, AnalysisTemperature,
                PipelineConstants.ReducedMaxRetries, cancellationToken);

            var analysis = ParseAnalysisResponse(response);
            return Result.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze financials for plan {PlanId}", businessPlanId);
            return Result.Failure<FinancialAnalysisResponse>(
                Error.Failure("Financial.Analysis.Failed", "Failed to analyze financial projections"));
        }
    }

    // --- Private helpers ---

    private static string BuildBusinessContextForForecast(
        Domain.Entities.BusinessPlan.BusinessPlan businessPlan,
        Dictionary<int, string> answers,
        string language)
    {
        var parts = new List<string>();
        var org = businessPlan.Organization;

        if (org != null)
        {
            parts.Add($"Industry: {org.Industry ?? "N/A"}");
            parts.Add($"Location: {org.City ?? "N/A"}, {org.Province ?? "QC"}");
            parts.Add($"Company: {org.Name ?? "N/A"}");
        }

        // Include relevant questionnaire answers (product/service, pricing, team, funding)
        int[] financialQuestions = [8, 13, 14, 16, 19, 20, 21, 22];
        foreach (var qNum in financialQuestions)
        {
            if (answers.TryGetValue(qNum, out var answer) && !string.IsNullOrWhiteSpace(answer))
            {
                parts.Add($"Q{qNum}: {answer[..Math.Min(answer.Length, 300)]}");
            }
        }

        // Include existing business brief if available
        if (!string.IsNullOrEmpty(businessPlan.BusinessBriefJson))
        {
            parts.Add($"Business Brief: {businessPlan.BusinessBriefJson[..Math.Min(businessPlan.BusinessBriefJson.Length, 500)]}");
        }

        return string.Join("\n", parts);
    }

    private static string BuildForecastSystemPrompt(string language)
    {
        var isFrench = language.StartsWith("fr", StringComparison.OrdinalIgnoreCase);
        return isFrench
            ? """
              Tu es un analyste financier expert spécialisé dans les plans d'affaires pour startups au Québec.
              En te basant sur le contexte d'affaires fourni, génère des projections financières réalistes
              incluant les produits/services, les dépenses, la masse salariale et les hypothèses de croissance.
              Base tes estimations sur les benchmarks de l'industrie au Québec/Canada.
              Réponds uniquement en JSON valide. Sois conservateur dans tes estimations.
              """
            : """
              You are an expert financial analyst specialized in startup business plans in Quebec.
              Based on the provided business context, generate realistic financial projections
              including products/services, expenses, payroll, and growth assumptions.
              Base your estimates on Quebec/Canada industry benchmarks.
              Respond only with valid JSON. Be conservative in your estimates.
              """;
    }

    private static string BuildForecastUserPrompt(string businessContext, string language)
    {
        var isFrench = language.StartsWith("fr", StringComparison.OrdinalIgnoreCase);
        var instruction = isFrench ? "Génère des projections financières" : "Generate financial projections";
        var jsonFormat = """
            Return valid JSON with these keys:
            - suggested_products: array of objects (name, suggested_price, suggested_monthly_volume, pricing_rationale)
            - suggested_expenses: array of objects (category: Rent|Utilities|Insurance|Marketing|etc., description, monthly_amount, rationale)
            - suggested_payroll: array of objects (job_title, payroll_type: Employee|Contractor, annual_salary, head_count, rationale)
            - growth_assumptions: object (year1_growth_rate, year2_growth_rate, year3_growth_rate, price_indexation, expense_indexation, rationale)
            - reasoning: overall reasoning string
            - confidence: 0.0-1.0
            """;

        return $"""
            Business Context:
            {businessContext}

            {instruction}:
            {jsonFormat}
            """;
    }

    private static string BuildFinancialSummary(Domain.Entities.Financial.FinancialPlan plan)
    {
        var parts = new List<string>();

        parts.Add($"Projection Years: {plan.ProjectionYears}");
        parts.Add($"Start Year: {plan.StartYear}");
        parts.Add($"Products: {plan.SalesProducts.Count(p => !p.IsDeleted)}");
        parts.Add($"Employees: {plan.PayrollItems.Count(p => !p.IsDeleted)}");
        parts.Add($"Financing Sources: {plan.FinancingSources.Count(f => !f.IsDeleted)}");

        // Revenue summary
        var totalRevenue = plan.SalesProducts
            .Where(p => !p.IsDeleted)
            .Sum(p => p.UnitPrice * p.SalesVolumes.Where(v => v.Year == 1).Sum(v => v.Quantity));
        parts.Add($"Year 1 Estimated Revenue: ${totalRevenue:N2}");

        // Payroll summary
        var totalPayroll = plan.PayrollItems
            .Where(p => !p.IsDeleted)
            .Sum(p => p.SalaryAmount * p.HeadCount);
        parts.Add($"Annual Payroll: ${totalPayroll:N2}");

        // CAPEX summary
        var totalCapex = plan.CapexAssets
            .Where(a => !a.IsDeleted)
            .Sum(a => a.PurchaseValue);
        parts.Add($"Total CAPEX: ${totalCapex:N2}");

        // Financing
        var totalFinancing = plan.FinancingSources
            .Where(f => !f.IsDeleted)
            .Sum(f => f.Amount);
        parts.Add($"Total Financing: ${totalFinancing:N2}");

        return string.Join("\n", parts);
    }

    private static FinancialForecastResponse ParseForecastResponse(string response)
    {
        var json = ExtractJsonBlock(response);
        var parsed = ParseJsonFromResponse<ForecastJson>(json);

        if (parsed == null)
            return new FinancialForecastResponse { Reasoning = "Failed to parse AI response", Confidence = 0 };

        return new FinancialForecastResponse
        {
            SuggestedProducts = parsed.SuggestedProducts?.Select(p => new SuggestedProduct
            {
                Name = p.Name ?? "",
                SuggestedPrice = p.SuggestedPrice,
                SuggestedMonthlyVolume = p.SuggestedMonthlyVolume,
                PricingRationale = p.PricingRationale ?? ""
            }).ToList() ?? [],
            SuggestedExpenses = parsed.SuggestedExpenses?.Select(e => new SuggestedExpense
            {
                Category = e.Category ?? "",
                Description = e.Description ?? "",
                MonthlyAmount = e.MonthlyAmount,
                Rationale = e.Rationale ?? ""
            }).ToList() ?? [],
            SuggestedPayroll = parsed.SuggestedPayroll?.Select(p => new SuggestedPayroll
            {
                JobTitle = p.JobTitle ?? "",
                PayrollType = p.PayrollType ?? "Employee",
                AnnualSalary = p.AnnualSalary,
                HeadCount = p.HeadCount > 0 ? p.HeadCount : 1,
                Rationale = p.Rationale ?? ""
            }).ToList() ?? [],
            GrowthAssumptions = parsed.GrowthAssumptions != null
                ? new SuggestedGrowthAssumptions
                {
                    Year1GrowthRate = parsed.GrowthAssumptions.Year1GrowthRate,
                    Year2GrowthRate = parsed.GrowthAssumptions.Year2GrowthRate,
                    Year3GrowthRate = parsed.GrowthAssumptions.Year3GrowthRate,
                    PriceIndexation = parsed.GrowthAssumptions.PriceIndexation,
                    ExpenseIndexation = parsed.GrowthAssumptions.ExpenseIndexation,
                    Rationale = parsed.GrowthAssumptions.Rationale ?? ""
                }
                : new SuggestedGrowthAssumptions(),
            Reasoning = parsed.Reasoning ?? "",
            Confidence = parsed.Confidence
        };
    }

    private static FinancialAnalysisResponse ParseAnalysisResponse(string response)
    {
        var json = ExtractJsonBlock(response);
        var parsed = ParseJsonFromResponse<AnalysisJson>(json);

        if (parsed == null)
            return new FinancialAnalysisResponse { OverallHealthScore = 0 };

        return new FinancialAnalysisResponse
        {
            OverallHealthScore = parsed.OverallHealthScore,
            Strengths = parsed.Strengths?.Select(MapInsight).ToList() ?? [],
            Weaknesses = parsed.Weaknesses?.Select(MapInsight).ToList() ?? [],
            Recommendations = parsed.Recommendations?.Select(MapInsight).ToList() ?? [],
            Benchmark = parsed.Benchmark != null
                ? new IndustryBenchmark
                {
                    Industry = parsed.Benchmark.Industry ?? "",
                    TypicalGrossMargin = parsed.Benchmark.TypicalGrossMargin,
                    TypicalNetMargin = parsed.Benchmark.TypicalNetMargin,
                    TypicalBreakEvenMonths = parsed.Benchmark.TypicalBreakEvenMonths,
                    DataConfidence = parsed.Benchmark.DataConfidence
                }
                : null
        };
    }

    private static FinancialInsight MapInsight(InsightJson i) => new()
    {
        Category = i.Category ?? "",
        Description = i.Description ?? "",
        Impact = i.Impact ?? "medium"
    };

    private static string ExtractJsonBlock(string text)
    {
        // Try to extract JSON from markdown code blocks
        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');
        if (jsonStart >= 0 && jsonEnd > jsonStart)
            return text[jsonStart..(jsonEnd + 1)];
        return text;
    }

    private static T? ParseJsonFromResponse<T>(string response) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });
        }
        catch
        {
            return null;
        }
    }

    // --- Internal JSON DTOs for parsing AI responses ---

    private class ForecastJson
    {
        public List<ProductJson>? SuggestedProducts { get; set; }
        public List<ExpenseJson>? SuggestedExpenses { get; set; }
        public List<PayrollJson>? SuggestedPayroll { get; set; }
        public GrowthJson? GrowthAssumptions { get; set; }
        public string? Reasoning { get; set; }
        public float Confidence { get; set; }
    }

    private class ProductJson
    {
        public string? Name { get; set; }
        public decimal SuggestedPrice { get; set; }
        public decimal SuggestedMonthlyVolume { get; set; }
        public string? PricingRationale { get; set; }
    }

    private class ExpenseJson
    {
        public string? Category { get; set; }
        public string? Description { get; set; }
        public decimal MonthlyAmount { get; set; }
        public string? Rationale { get; set; }
    }

    private class PayrollJson
    {
        public string? JobTitle { get; set; }
        public string? PayrollType { get; set; }
        public decimal AnnualSalary { get; set; }
        public int HeadCount { get; set; }
        public string? Rationale { get; set; }
    }

    private class GrowthJson
    {
        public decimal Year1GrowthRate { get; set; }
        public decimal Year2GrowthRate { get; set; }
        public decimal Year3GrowthRate { get; set; }
        public decimal PriceIndexation { get; set; }
        public decimal ExpenseIndexation { get; set; }
        public string? Rationale { get; set; }
    }

    private class VolumeForecastJson
    {
        public decimal[]? MonthlyVolumes { get; set; }
        public string? SeasonalityPattern { get; set; }
        public string? Rationale { get; set; }
        public float Confidence { get; set; }
    }

    private class AnalysisJson
    {
        public decimal OverallHealthScore { get; set; }
        public List<InsightJson>? Strengths { get; set; }
        public List<InsightJson>? Weaknesses { get; set; }
        public List<InsightJson>? Recommendations { get; set; }
        public BenchmarkJson? Benchmark { get; set; }
    }

    private class InsightJson
    {
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? Impact { get; set; }
    }

    private class BenchmarkJson
    {
        public string? Industry { get; set; }
        public decimal TypicalGrossMargin { get; set; }
        public decimal TypicalNetMargin { get; set; }
        public int TypicalBreakEvenMonths { get; set; }
        public float DataConfidence { get; set; }
    }
}

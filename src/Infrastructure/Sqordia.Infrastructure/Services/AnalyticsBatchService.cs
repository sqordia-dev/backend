using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Domain.Entities;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Batch analytics service using AI for system insights
/// </summary>
public class AnalyticsBatchService : IAnalyticsBatchService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IAdminDashboardService _dashboardService;
    private readonly ILogger<AnalyticsBatchService> _logger;

    public AnalyticsBatchService(
        IApplicationDbContext context,
        IAIService aiService,
        IAdminDashboardService dashboardService,
        ILogger<AnalyticsBatchService> logger)
    {
        _context = context;
        _aiService = aiService;
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public async Task<Result> RunBatchAnalysisAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting batch analytics analysis");

            var overview = await _dashboardService.GetSystemOverviewAsync(cancellationToken);
            if (!overview.IsSuccess)
                return Result.Failure(Error.Failure("Analytics.DataError", "Failed to get system data"));

            var contextJson = JsonSerializer.Serialize(overview.Value, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var insightTypes = new[] { "user_signups", "ai_usage", "platform_health", "anomalies" };

            foreach (var insightType in insightTypes)
            {
                try
                {
                    await GenerateInsightAsync(insightType, contextJson, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to generate insight: {InsightType}", insightType);
                }
            }

            _logger.LogInformation("Batch analytics analysis completed");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running batch analytics");
            return Result.Failure(Error.Failure("Analytics.Error", ex.Message));
        }
    }

    public async Task<Result<List<AnalyticsInsightDto>>> GetLatestInsightsAsync(CancellationToken cancellationToken = default)
    {
        var insights = await _context.AnalyticsInsights
            .Where(i => i.IsLatest && !i.IsDeleted)
            .OrderByDescending(i => i.Created)
            .Select(i => new AnalyticsInsightDto
            {
                Id = i.Id,
                InsightType = i.InsightType,
                Content = i.Content,
                Period = i.Period,
                ModelUsed = i.ModelUsed,
                TokensUsed = i.TokensUsed,
                IsLatest = i.IsLatest,
                GeneratedAt = i.Created
            })
            .ToListAsync(cancellationToken);

        return Result.Success(insights);
    }

    public async Task<Result<List<AnalyticsInsightDto>>> GetInsightHistoryAsync(string insightType, int count = 10, CancellationToken cancellationToken = default)
    {
        var insights = await _context.AnalyticsInsights
            .Where(i => i.InsightType == insightType && !i.IsDeleted)
            .OrderByDescending(i => i.Created)
            .Take(count)
            .Select(i => new AnalyticsInsightDto
            {
                Id = i.Id,
                InsightType = i.InsightType,
                Content = i.Content,
                Period = i.Period,
                ModelUsed = i.ModelUsed,
                TokensUsed = i.TokensUsed,
                IsLatest = i.IsLatest,
                GeneratedAt = i.Created
            })
            .ToListAsync(cancellationToken);

        return Result.Success(insights);
    }

    private async Task GenerateInsightAsync(string insightType, string contextJson, CancellationToken cancellationToken)
    {
        var prompt = insightType switch
        {
            "user_signups" => "Analyze user signup trends. Identify growth patterns, churn risks, and recommendations for user acquisition.",
            "ai_usage" => "Analyze AI usage patterns. Identify cost optimization opportunities, popular features, and usage trends.",
            "platform_health" => "Analyze overall platform health. Identify performance issues, capacity concerns, and improvement areas.",
            "anomalies" => "Identify any anomalies in the data. Look for unusual patterns in signups, usage, or system behavior.",
            _ => "Provide a general analysis of the platform data."
        };

        var systemPrompt = "You are a data analyst. Analyze the provided system metrics and provide actionable insights in markdown format. Be specific with numbers.";
        var userPrompt = $"{prompt}\n\nSystem Data:\n{contextJson}";

        var content = await _aiService.GenerateContentAsync(systemPrompt, userPrompt, 2000, 0.7f, cancellationToken);

        // Mark previous insights as not latest
        var previousInsights = await _context.AnalyticsInsights
            .Where(i => i.InsightType == insightType && i.IsLatest && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var prev in previousInsights)
        {
            prev.IsLatest = false;
        }

        // Create new insight
        var insight = new AnalyticsInsight
        {
            InsightType = insightType,
            Content = content,
            Period = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ModelUsed = "claude-sonnet-4-6",
            TokensUsed = content.Length / 4, // Rough estimate
            IsLatest = true
        };

        _context.AnalyticsInsights.Add(insight);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static AnalyticsInsightDto MapToDto(AnalyticsInsight i) => new()
    {
        Id = i.Id,
        InsightType = i.InsightType,
        Content = i.Content,
        Period = i.Period,
        ModelUsed = i.ModelUsed,
        TokensUsed = i.TokensUsed,
        IsLatest = i.IsLatest,
        GeneratedAt = i.Created
    };
}

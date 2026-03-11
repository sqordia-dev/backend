using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// HTTP client that calls the Python ai-service ML endpoints
/// for quality prediction, prompt recommendation, and preference learning.
/// </summary>
public class MLPredictionService : IMLPredictionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MLPredictionService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public MLPredictionService(HttpClient httpClient, ILogger<MLPredictionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<QualityPrediction> PredictQualityAsync(
        QualityPredictionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/ml/predict-quality", request, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<QualityPrediction>(JsonOptions, cancellationToken);
            return result ?? new QualityPrediction(70, 0.5, false, "Null response from ML service");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ML quality prediction unavailable, using default");
            return new QualityPrediction(70, 0.0, false, "ML service unavailable");
        }
    }

    public async Task<PromptRecommendation> RecommendPromptAsync(
        PromptRecommendationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/ml/recommend-prompt", request, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PromptRecommendation>(JsonOptions, cancellationToken);
            return result ?? new PromptRecommendation(null, 0, "fallback");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ML prompt recommendation unavailable, using fallback");
            return new PromptRecommendation(null, 0, "fallback");
        }
    }

    public async Task<List<LearnedPreferenceDto>> GetLearnedPreferencesAsync(
        string sectionType,
        string? industry = null,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"/ml/preferences?section_type={sectionType}&language={language}";
            if (!string.IsNullOrEmpty(industry))
                query += $"&industry={Uri.EscapeDataString(industry)}";

            var response = await _httpClient.GetAsync(query, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<LearnedPreferenceDto>>(JsonOptions, cancellationToken);
            return result ?? new List<LearnedPreferenceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ML preferences unavailable for {Section}", sectionType);
            return new List<LearnedPreferenceDto>();
        }
    }

    public async Task<QualityDriftReport> CheckQualityDriftAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/ml/quality-drift", cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<QualityDriftReport>(JsonOptions, cancellationToken);
            return result ?? new QualityDriftReport(false, new List<DriftAlert>(), DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ML quality drift check unavailable");
            return new QualityDriftReport(false, new List<DriftAlert>(), DateTime.UtcNow);
        }
    }

    public async Task<TrainingResult> TriggerTrainingAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsync("/ml/train", null, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TrainingResult>(JsonOptions, cancellationToken);
            return result ?? new TrainingResult(false, null, 0, new Dictionary<string, double>(), "Null response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ML training trigger failed");
            return new TrainingResult(false, null, 0, new Dictionary<string, double>(), ex.Message);
        }
    }
}

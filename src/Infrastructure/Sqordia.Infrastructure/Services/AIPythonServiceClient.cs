using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// HTTP client for communicating with the Python AI microservice.
/// Handles serialization, timeouts, and error mapping.
/// </summary>
public class AIPythonServiceClient : IAIPythonService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIPythonServiceClient> _logger;
    private readonly PythonServiceSettings _settings;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    public AIPythonServiceClient(
        HttpClient httpClient,
        ILogger<AIPythonServiceClient> logger,
        IOptions<PythonServiceSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
    }

    // --- Health ---

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CreateTimeoutCts(TimeSpan.FromSeconds(5), cancellationToken);
            var response = await _httpClient.GetAsync("/health", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Python AI service health check failed");
            return false;
        }
    }

    public async Task<PythonServiceHealthResponse> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<PythonServiceHealthResponse>("/health", TimeSpan.FromSeconds(5), cancellationToken)
            ?? new PythonServiceHealthResponse("unavailable", "0.0.0", new Dictionary<string, bool>());
    }

    public async Task<PythonProvidersStatusResponse> GetProvidersStatusAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<PythonProvidersStatusResponse>("/providers/status", TimeSpan.FromSeconds(30), cancellationToken)
            ?? new PythonProvidersStatusResponse("unknown", new List<string>(), new List<PythonProviderStatus>());
    }

    // --- Completeness Evaluation (Phase 1) ---

    public async Task<AnswerCompletenessResponse> EvaluateAnswerCompletenessAsync(
        AnswerCompletenessRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<AnswerCompletenessRequest, AnswerCompletenessResponse>(
            "/evaluate/answer-completeness", request, EvaluationTimeout, cancellationToken);
    }

    public async Task<StepCompletenessResponse> EvaluateStepCompletenessAsync(
        StepCompletenessRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<StepCompletenessRequest, StepCompletenessResponse>(
            "/evaluate/step-completeness", request, EvaluationTimeout, cancellationToken);
    }

    public async Task<SectionCompletenessResponse> EvaluateSectionCompletenessAsync(
        SectionCompletenessRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<SectionCompletenessRequest, SectionCompletenessResponse>(
            "/evaluate/section-completeness", request, EvaluationTimeout, cancellationToken);
    }

    // --- Generation (Phase 3) ---

    public async Task<GenerateBusinessBriefResponse> GenerateBusinessBriefAsync(
        GenerateBusinessBriefRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<GenerateBusinessBriefRequest, GenerateBusinessBriefResponse>(
            "/generate/business-brief", request, GenerationTimeout, cancellationToken);
    }

    public async Task<GenerateSectionResponse> GenerateSectionAsync(
        GenerateSectionRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<GenerateSectionRequest, GenerateSectionResponse>(
            "/generate/section", request, GenerationTimeout, cancellationToken);
    }

    public async Task<GenerateFullPlanResponse> GenerateFullPlanAsync(
        GenerateFullPlanRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<GenerateFullPlanRequest, GenerateFullPlanResponse>(
            "/generate/full-plan", request, GenerationTimeout, cancellationToken);
    }

    public async Task<GenerationProgressResponse> GetGenerationProgressAsync(
        string jobId, CancellationToken cancellationToken = default)
    {
        return await GetAsync<GenerationProgressResponse>(
            $"/generate/status/{jobId}", EvaluationTimeout, cancellationToken)
            ?? new GenerationProgressResponse(jobId, "unknown", 0, new List<SectionProgressItem>());
    }

    // --- Answer Transform (Phase 3) ---

    public async Task<TransformAnswerPythonResponse> TransformAnswerAsync(
        TransformAnswerPythonRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<TransformAnswerPythonRequest, TransformAnswerPythonResponse>(
            "/transform/answer", request, EvaluationTimeout, cancellationToken);
    }

    // --- Quality Evaluation (Phase 2) ---

    public async Task<JudgeEvaluationResponse> RunJudgeEvaluationAsync(
        JudgeEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<JudgeEvaluationRequest, JudgeEvaluationResponse>(
            "/judge/full-evaluation", request, GenerationTimeout, cancellationToken);
    }

    // --- RAGAS (Phase 5) ---

    public async Task<RagasEvaluationResponse> EvaluateRagasAsync(
        RagasEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<RagasEvaluationRequest, RagasEvaluationResponse>(
            "/evaluate/ragas", request, EvaluationTimeout, cancellationToken);
    }

    // --- AI Coach (Phase 4) ---

    public async Task<CoachAgentResponse> SendCoachMessageAsync(
        CoachAgentRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<CoachAgentRequest, CoachAgentResponse>(
            "/agents/coach", request, EvaluationTimeout, cancellationToken);
    }

    public async Task<IndustryResearchResponse> RunIndustryResearchAsync(
        IndustryResearchRequest request, CancellationToken cancellationToken = default)
    {
        return await PostAsync<IndustryResearchRequest, IndustryResearchResponse>(
            "/agents/industry-research", request, EvaluationTimeout, cancellationToken);
    }

    // --- Private helpers ---

    private TimeSpan GenerationTimeout => TimeSpan.FromSeconds(_settings.GenerationTimeoutSeconds);
    private TimeSpan EvaluationTimeout => TimeSpan.FromSeconds(_settings.EvaluationTimeoutSeconds);

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path, TRequest request, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var cts = CreateTimeoutCts(timeout, cancellationToken);
        try
        {
            _logger.LogDebug("Calling Python AI service: POST {Path}", path);

            var response = await _httpClient.PostAsJsonAsync(path, request, JsonOptions, cts.Token);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cts.Token);
            return result ?? throw new InvalidOperationException($"Python service returned null from {path}");
        }
        catch (TaskCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Python AI service call timed out: POST {Path} (timeout: {Timeout}s)", path, timeout.TotalSeconds);
            throw new TimeoutException($"Python AI service call timed out after {timeout.TotalSeconds}s: POST {path}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Python AI service HTTP error: POST {Path} - {StatusCode}", path, ex.StatusCode);
            throw;
        }
    }

    private async Task<TResponse?> GetAsync<TResponse>(
        string path, TimeSpan timeout, CancellationToken cancellationToken) where TResponse : class
    {
        using var cts = CreateTimeoutCts(timeout, cancellationToken);
        try
        {
            var response = await _httpClient.GetAsync(path, cts.Token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Python AI service GET failed: {Path}", path);
            return null;
        }
    }

    private static CancellationTokenSource CreateTimeoutCts(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        return cts;
    }
}

/// <summary>
/// Configuration for the Python AI service connection.
/// </summary>
public class PythonServiceSettings
{
    public string BaseUrl { get; set; } = "http://localhost:8100";
    public string ServiceKey { get; set; } = "";
    public int GenerationTimeoutSeconds { get; set; } = 120;
    public int EvaluationTimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 2;
    public int RetryBaseDelaySeconds { get; set; } = 1;
}

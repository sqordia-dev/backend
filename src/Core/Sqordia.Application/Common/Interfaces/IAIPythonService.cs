namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Interface for communicating with the Python AI microservice (LangChain, MLflow, RAGAS).
/// </summary>
public interface IAIPythonService
{
    /// <summary>
    /// Checks if the Python AI service is reachable and healthy.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the health status including available providers.
    /// </summary>
    Task<PythonServiceHealthResponse> GetHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets status of all configured AI providers in the Python service.
    /// </summary>
    Task<PythonProvidersStatusResponse> GetProvidersStatusAsync(CancellationToken cancellationToken = default);

    // --- Completeness Evaluation (Phase 1) ---

    /// <summary>
    /// Evaluates completeness of an individual questionnaire answer.
    /// </summary>
    Task<AnswerCompletenessResponse> EvaluateAnswerCompletenessAsync(
        AnswerCompletenessRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates completeness and coherence of an entire questionnaire step.
    /// </summary>
    Task<StepCompletenessResponse> EvaluateStepCompletenessAsync(
        StepCompletenessRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates completeness of a generated section against expected content.
    /// </summary>
    Task<SectionCompletenessResponse> EvaluateSectionCompletenessAsync(
        SectionCompletenessRequest request,
        CancellationToken cancellationToken = default);

    // --- Generation (Phase 3) ---

    /// <summary>
    /// Generates a business brief from questionnaire answers.
    /// </summary>
    Task<GenerateBusinessBriefResponse> GenerateBusinessBriefAsync(
        GenerateBusinessBriefRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a single business plan section.
    /// </summary>
    Task<GenerateSectionResponse> GenerateSectionAsync(
        GenerateSectionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a full business plan (all sections, tiered parallel execution).
    /// Returns a job ID for progress polling.
    /// </summary>
    Task<GenerateFullPlanResponse> GenerateFullPlanAsync(
        GenerateFullPlanRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets generation progress for a running job.
    /// </summary>
    Task<GenerationProgressResponse> GetGenerationProgressAsync(
        string jobId,
        CancellationToken cancellationToken = default);

    // --- Answer Transform (Phase 3) ---

    /// <summary>
    /// Transforms/polishes a questionnaire answer using LangChain router chains.
    /// </summary>
    Task<TransformAnswerPythonResponse> TransformAnswerAsync(
        TransformAnswerPythonRequest request,
        CancellationToken cancellationToken = default);

    // --- Quality Evaluation (Phase 2) ---

    /// <summary>
    /// Runs full LLM-as-Judge evaluation on a generated section or plan.
    /// </summary>
    Task<JudgeEvaluationResponse> RunJudgeEvaluationAsync(
        JudgeEvaluationRequest request,
        CancellationToken cancellationToken = default);

    // --- RAGAS (Phase 5) ---

    /// <summary>
    /// Evaluates faithfulness and relevancy of generated content via RAGAS.
    /// </summary>
    Task<RagasEvaluationResponse> EvaluateRagasAsync(
        RagasEvaluationRequest request,
        CancellationToken cancellationToken = default);

    // --- AI Coach (Phase 4) ---

    /// <summary>
    /// Sends a message to the ReAct AI Coach agent.
    /// </summary>
    Task<CoachAgentResponse> SendCoachMessageAsync(
        CoachAgentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the industry research ReAct agent.
    /// </summary>
    Task<IndustryResearchResponse> RunIndustryResearchAsync(
        IndustryResearchRequest request,
        CancellationToken cancellationToken = default);
}

// --- DTOs for Python service communication ---

public record PythonServiceHealthResponse(
    string Status,
    string Version,
    Dictionary<string, bool> Providers
);

public record PythonProvidersStatusResponse(
    string ActiveProvider,
    List<string> FallbackProviders,
    List<PythonProviderStatus> Providers
);

public record PythonProviderStatus(
    string Name,
    bool Available,
    string Model,
    string? Error
);

// Phase 1 DTOs
public record AnswerCompletenessRequest(
    int QuestionNumber,
    string QuestionText,
    string Answer,
    string Language,
    string Persona
);

public record AnswerCompletenessResponse(
    float CompletenessScore,
    List<string> MissingElements,
    List<string> Suggestions,
    float Confidence
);

public record StepCompletenessRequest(
    int StepNumber,
    List<StepAnswerItem> Answers,
    string Language,
    string Persona
);

public record StepAnswerItem(
    int QuestionNumber,
    string QuestionText,
    string Answer
);

public record StepCompletenessResponse(
    float StepScore,
    float CoherenceScore,
    List<string> Gaps,
    List<string> Contradictions
);

public record SectionCompletenessRequest(
    string SectionName,
    string SectionContent,
    object BusinessBrief,
    string Language
);

public record SectionCompletenessResponse(
    float CompletenessScore,
    List<string> ExpectedElements,
    List<string> PresentElements,
    List<string> MissingElements,
    string Recommendation
);

// Phase 2 DTOs
public record JudgeEvaluationRequest(
    string SectionName,
    string SectionContent,
    string BusinessBrief,
    string Language,
    List<string>? Criteria = null
);

public record JudgeEvaluationResponse(
    float OverallScore,
    Dictionary<string, float> DimensionScores,
    List<JudgeFinding> Findings,
    string MlflowRunId
);

public record JudgeFinding(
    string Dimension,
    string Severity,
    string Message,
    string? Section = null
);

// Phase 3 DTOs
public record GenerateBusinessBriefRequest(
    Dictionary<int, string> Answers,
    object? OrganizationProfile,
    string Language,
    string Persona
);

public record GenerateBusinessBriefResponse(
    object BusinessBrief,
    float MaturityScore,
    int TokensUsed,
    string Model
);

public record GenerateSectionRequest(
    string SectionName,
    object BusinessBrief,
    object? GenerationPlan,
    Dictionary<string, string>? PreviousSections,
    Dictionary<int, string>? QuestionAnswers,
    string Language,
    string Persona,
    object? PromptOverride = null
);

public record GenerateSectionResponse(
    string Content,
    int TokensUsed,
    string Model
);

public record GenerateFullPlanRequest(
    object BusinessBrief,
    Dictionary<int, string> QuestionAnswers,
    string Language,
    string Persona
);

public record GenerateFullPlanResponse(
    string JobId,
    string Status
);

public record GenerationProgressResponse(
    string JobId,
    string Status,
    float ProgressPercent,
    List<SectionProgressItem> Sections
);

public record SectionProgressItem(
    string SectionName,
    string Status,
    float? CompletenessScore,
    float? FaithfulnessScore
);

public record TransformAnswerPythonRequest(
    string Action,
    int QuestionNumber,
    string QuestionText,
    string CurrentAnswer,
    Dictionary<int, string>? PreviousAnswers,
    object? OrganizationContext,
    string Language
);

public record TransformAnswerPythonResponse(
    string TransformedAnswer,
    string Action,
    int TokensUsed
);

// Phase 4 DTOs
public record CoachAgentRequest(
    int QuestionNumber,
    string QuestionText,
    string? CurrentAnswer,
    Dictionary<int, string>? PreviousAnswers,
    List<CoachMessage>? ConversationHistory,
    string Language
);

public record CoachMessage(
    string Role,
    string Content
);

public record CoachAgentResponse(
    string Message,
    List<string> ToolsUsed,
    string? SuggestedAction
);

public record IndustryResearchRequest(
    string Industry,
    string Location,
    string BusinessDescription,
    string Language
);

public record IndustryResearchResponse(
    string? MarketSize,
    string? GrowthRate,
    List<string> Trends,
    List<string> Competitors,
    List<string> Sources,
    float DataConfidence
);

// Phase 5 DTOs
public record RagasEvaluationRequest(
    string Question,
    string Answer,
    List<string> Contexts,
    string GeneratedContent
);

public record RagasEvaluationResponse(
    float Faithfulness,
    float AnswerRelevancy,
    float ContextPrecision,
    object? Details
);

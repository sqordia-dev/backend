using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V2;
using Sqordia.Contracts.Requests.AI;
using Sqordia.Contracts.Requests.V2.Questionnaire;
using Sqordia.Contracts.Responses.AI;

namespace WebAPI.Controllers;

/// <summary>
/// AI endpoints for text enhancement and analysis
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai")]
[Authorize]
public class AIController : BaseApiController
{
    private readonly IQuestionPolishService _polishService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AIController> _logger;

    public AIController(
        IQuestionPolishService polishService,
        IAuditService auditService,
        ILogger<AIController> logger)
    {
        _polishService = polishService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Polish/enhance an answer using AI (field-level feedback)
    /// </summary>
    /// <param name="request">Answer polish request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Polished answer with strength score</returns>
    [HttpPost("polish-answer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PolishAnswer(
        [FromBody] PolishAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Polishing answer for question {QuestionId} with {Length} characters",
            request.QuestionId, request.Answer.Length);

        // Use the existing polish service
        var polishRequest = new PolishTextRequest
        {
            Text = request.Answer,
            Context = request.Context ?? $"Question ID: {request.QuestionId}, Persona: {request.Persona}",
            Language = request.Language ?? "en",
            Tone = "professional"
        };

        var result = await _polishService.PolishTextAsync(polishRequest, cancellationToken);

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        // Map to response format expected by frontend
        var response = new PolishAnswerResponse
        {
            PolishedText = result.Value?.PolishedText,
            StrengthScore = CalculateStrengthScore(request.Answer, result.Value?.PolishedText),
            OriginalText = request.Answer,
            Improvements = result.Value?.Improvements ?? new List<string>()
        };

        return Ok(response);
    }

    /// <summary>
    /// Fully analyze an answer (polish + gaps) - on-demand analysis
    /// </summary>
    /// <param name="request">Answer analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Polished text, strength score, and gaps</returns>
    [HttpPost("analyze-answer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnalyzeAnswer(
        [FromBody] AnalyzeAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Analyzing answer for question {QuestionId} with {Length} characters",
            request.QuestionId, request.Answer.Length);

        var response = new AnalyzeAnswerResponse();

        // Get polished text if requested
        if (request.IncludePolish)
        {
            var polishRequest = new PolishTextRequest
            {
                Text = request.Answer,
                Context = request.Context ?? $"Question ID: {request.QuestionId}, Persona: {request.Persona}",
                Language = request.Language ?? "en",
                Tone = "professional"
            };

            var polishResult = await _polishService.PolishTextAsync(polishRequest, cancellationToken);
            if (polishResult.IsSuccess)
            {
                response.PolishedText = polishResult.Value?.PolishedText;
                response.Improvements = polishResult.Value?.Improvements ?? new List<string>();
            }
        }

        // Calculate strength score
        response.StrengthScore = CalculateStrengthScore(request.Answer, response.PolishedText);

        // Analyze for gaps if requested
        if (request.IncludeGaps)
        {
            response.Gaps = AnalyzeForGaps(request.Answer, request.Persona, request.Location);
        }

        return Ok(response);
    }

    /// <summary>
    /// Analyze a questionnaire step for gaps (section-level feedback)
    /// </summary>
    /// <param name="request">Step analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Gap analysis for the step</returns>
    [HttpPost("analyze-step")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnalyzeStep(
        [FromBody] AnalyzeStepRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Analyzing step {StepNumber} with {AnswerCount} answers",
            request.StepNumber, request.Answers.Count);

        var response = new AnalyzeStepResponse
        {
            Questions = new List<QuestionAnalysis>()
        };

        int totalScore = 0;

        foreach (var answer in request.Answers)
        {
            var analysis = new QuestionAnalysis
            {
                QuestionId = answer.QuestionId,
                Score = CalculateStrengthScore(answer.Answer, null),
                Gaps = AnalyzeForGaps(answer.Answer, request.Persona, request.Location)
            };

            response.Questions.Add(analysis);
            totalScore += analysis.Score;
        }

        response.OverallScore = request.Answers.Count > 0
            ? totalScore / request.Answers.Count
            : 0;

        response.Summary = GenerateStepSummary(response);

        return Ok(response);
    }

    private int CalculateStrengthScore(string originalText, string? polishedText)
    {
        // Base score on text length and quality indicators
        int score = 50; // Base score

        // Length bonus (up to 20 points)
        if (originalText.Length >= 50) score += 5;
        if (originalText.Length >= 100) score += 5;
        if (originalText.Length >= 200) score += 5;
        if (originalText.Length >= 300) score += 5;

        // Quality indicators (up to 20 points)
        if (originalText.Contains(",")) score += 3; // Structured sentences
        if (originalText.Contains(".") && originalText.Split('.').Length > 2) score += 5; // Multiple sentences
        if (char.IsUpper(originalText[0])) score += 2; // Proper capitalization
        if (!originalText.Contains("etc") && !originalText.Contains("...")) score += 5; // Specific, not vague

        // Penalize very short answers
        if (originalText.Length < 30) score -= 20;

        // Bonus if polished text shows improvement
        if (polishedText != null && polishedText.Length > originalText.Length)
        {
            score += 5;
        }

        return Math.Clamp(score, 0, 100);
    }

    private List<AnswerGap> AnalyzeForGaps(string answer, string persona, LocationInfo? location)
    {
        var gaps = new List<AnswerGap>();

        // Check for common gaps based on answer content
        var lowerAnswer = answer.ToLower();

        // Financial gaps
        if (!lowerAnswer.Contains("$") && !lowerAnswer.Contains("revenue") &&
            !lowerAnswer.Contains("cost") && !lowerAnswer.Contains("price") &&
            !lowerAnswer.Contains("budget") && !lowerAnswer.Contains("revenu") &&
            !lowerAnswer.Contains("coût") && !lowerAnswer.Contains("prix"))
        {
            gaps.Add(new AnswerGap
            {
                Category = "Financial",
                Priority = "medium",
                Message = "Consider adding financial details to strengthen your answer.",
                Suggestion = "Include specific numbers, costs, or revenue projections where applicable.",
                QuestionPrompt = "What are the estimated costs or revenue associated with this?"
            });
        }

        // Strategic gaps
        if (answer.Length < 100)
        {
            gaps.Add(new AnswerGap
            {
                Category = "Strategic",
                Priority = "low",
                Message = "Your answer could benefit from more detail.",
                Suggestion = "Expand on the key points to provide a more comprehensive response.",
                QuestionPrompt = "Can you elaborate on the main aspects of your answer?"
            });
        }

        // Quebec compliance for Quebec-based businesses
        if (location?.Province?.ToLower() == "quebec" || location?.Province?.ToLower() == "québec")
        {
            if (!lowerAnswer.Contains("french") && !lowerAnswer.Contains("français") &&
                !lowerAnswer.Contains("bill 96") && !lowerAnswer.Contains("loi 96"))
            {
                gaps.Add(new AnswerGap
                {
                    Category = "QuebecCompliance",
                    Priority = "high",
                    Message = "Consider Quebec language requirements in your business plan.",
                    Suggestion = "Address how your business will comply with Bill 96 and French language requirements.",
                    QuestionPrompt = "How will you ensure compliance with Quebec's language laws?"
                });
            }
        }

        return gaps;
    }

    private string GenerateStepSummary(AnalyzeStepResponse response)
    {
        if (response.OverallScore >= 80)
        {
            return "Great job! This section is well-developed and comprehensive.";
        }
        else if (response.OverallScore >= 60)
        {
            return "Good progress. Consider expanding on some answers for a stronger business plan.";
        }
        else if (response.OverallScore >= 40)
        {
            return "This section needs more detail. Review the suggestions to improve your responses.";
        }
        else
        {
            return "This section requires significant improvement. Take time to provide thorough answers to each question.";
        }
    }

    /// <summary>
    /// Polish/enhance text using AI (v1 compatibility endpoint)
    /// Transforms raw notes into professional, BDC-standard prose
    /// </summary>
    /// <param name="request">Text polish request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Polished text</returns>
    [HttpPost("polish-text")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PolishText(
        [FromBody] PolishTextRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Polishing text with {Length} characters", request.Text.Length);

        var result = await _polishService.PolishTextAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Analyze a business plan section for gaps (Socratic Coach) (v1 compatibility endpoint)
    /// Proxies to V2 audit service
    /// </summary>
    /// <param name="planId">The business plan ID (from query parameter)</param>
    /// <param name="request">Section analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audit issues with Options A/B/C</returns>
    [HttpPost("analyze-section")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalyzeSection(
        [FromQuery] Guid planId,
        [FromBody] AnalyzeSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (planId == Guid.Empty)
        {
            return BadRequest(new { error = "PlanId query parameter is required" });
        }

        _logger.LogInformation("Analyzing section {Section} for plan {PlanId}", request.SectionName, planId);

        var result = await _auditService.AuditSectionAsync(planId, request.SectionName, request.Language, cancellationToken);
        return HandleResult(result);
    }
}

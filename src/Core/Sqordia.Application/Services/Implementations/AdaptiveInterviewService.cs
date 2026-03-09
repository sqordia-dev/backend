using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Questionnaire;
using Sqordia.Contracts.Responses.Questionnaire;
using Sqordia.Domain.Constants;

namespace Sqordia.Application.Services.Implementations;

public class AdaptiveInterviewService : IAdaptiveInterviewService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdaptiveInterviewService> _logger;

    public AdaptiveInterviewService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AdaptiveInterviewService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<AdaptiveQuestionnaireResponse>> GetAdaptiveQuestionsAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserIdAsGuid();
        if (userId == null)
            return Result.Failure<AdaptiveQuestionnaireResponse>(
                Error.Unauthorized("Auth.Error.Unauthorized", "User not authenticated"));

        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

        if (businessPlan == null)
            return Result.Failure<AdaptiveQuestionnaireResponse>(
                Error.NotFound("BusinessPlan.NotFound", "Business plan not found"));

        var organization = businessPlan.Organization;

        // Load all active V3 questions
        var allQuestions = await _context.QuestionTemplatesV3
            .Where(q => q.IsActive)
            .OrderBy(q => q.StepNumber)
            .ThenBy(q => q.DisplayOrder)
            .ToListAsync(cancellationToken);

        // Filter by persona if the plan has one
        if (businessPlan.Persona != null)
        {
            allQuestions = allQuestions
                .Where(q => q.PersonaType == null || q.PersonaType == businessPlan.Persona)
                .ToList();
        }

        // Load existing responses for this plan (with V3 template for question number)
        var existingResponses = await _context.QuestionnaireResponses
            .Include(r => r.QuestionTemplateV3)
            .Where(r => r.BusinessPlanId == businessPlanId && r.QuestionTemplateV3Id != null)
            .ToListAsync(cancellationToken);

        var responsesByQuestionNumber = existingResponses
            .Where(r => r.QuestionTemplateV3 != null)
            .ToDictionary(r => r.QuestionTemplateV3!.QuestionNumber, r => r.ResponseText);

        var isFrench = language.StartsWith("fr", StringComparison.OrdinalIgnoreCase);
        var questions = new List<AdaptiveQuestionDto>();
        var skippedQuestions = new List<SkippedQuestionDto>();

        foreach (var q in allQuestions)
        {
            var profileFieldKey = q.ProfileFieldKey;
            string? profileValue = null;

            // Check if this question maps to a filled org profile field
            if (!string.IsNullOrEmpty(profileFieldKey) && organization != null)
            {
                try
                {
                    profileValue = organization.GetProfileFieldValue(profileFieldKey);
                }
                catch (ArgumentException)
                {
                    // Unknown field key, ignore
                }
            }

            var hasProfileValue = !string.IsNullOrWhiteSpace(profileValue);
            var hasExistingResponse = responsesByQuestionNumber.ContainsKey(q.QuestionNumber);

            // If org profile has a value and there's no existing manual response, skip the question
            if (hasProfileValue && !hasExistingResponse)
            {
                skippedQuestions.Add(new SkippedQuestionDto
                {
                    Id = q.Id,
                    QuestionNumber = q.QuestionNumber,
                    QuestionText = isFrench ? q.QuestionTextFR : q.QuestionTextEN,
                    ProfileFieldKey = profileFieldKey!,
                    ProfileFieldValue = profileValue!
                });
                continue;
            }

            // Determine if this is a gap question (has profileFieldKey but org field is empty)
            var isGapQuestion = !string.IsNullOrEmpty(profileFieldKey) && !hasProfileValue;

            questions.Add(new AdaptiveQuestionDto
            {
                Id = q.Id,
                QuestionNumber = q.QuestionNumber,
                StepNumber = q.StepNumber,
                QuestionText = isFrench ? q.QuestionTextFR : q.QuestionTextEN,
                HelpText = isFrench ? q.HelpTextFR : q.HelpTextEN,
                QuestionType = q.QuestionType.ToString(),
                Options = isFrench ? q.OptionsFR : q.OptionsEN,
                DisplayOrder = q.DisplayOrder,
                IsRequired = q.IsRequired,
                Icon = q.Icon,
                SectionGroup = q.SectionGroup,
                CoachPrompt = isFrench ? q.CoachPromptFR : q.CoachPromptEN,
                ExpertAdvice = isFrench ? q.ExpertAdviceFR : q.ExpertAdviceEN,
                ProfileFieldKey = profileFieldKey,
                PrefilledValue = hasProfileValue ? profileValue : null,
                IsGapQuestion = isGapQuestion,
                ExistingResponse = hasExistingResponse
                    ? responsesByQuestionNumber[q.QuestionNumber]
                    : null
            });
        }

        var totalCount = questions.Count + skippedQuestions.Count;
        var answeredCount = skippedQuestions.Count +
                           questions.Count(q => !string.IsNullOrWhiteSpace(q.ExistingResponse));

        return Result.Success(new AdaptiveQuestionnaireResponse
        {
            Questions = questions,
            SkippedQuestions = skippedQuestions,
            TotalQuestions = totalCount,
            RemainingQuestions = totalCount - answeredCount,
            ProfileCompletenessScore = organization?.ProfileCompletenessScore ?? 0
        });
    }

    public async Task<Result<bool>> SubmitAdaptiveResponseAsync(
        Guid businessPlanId,
        SubmitAdaptiveResponseRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserIdAsGuid();
        if (userId == null)
            return Result.Failure<bool>(
                Error.Unauthorized("Auth.Error.Unauthorized", "User not authenticated"));

        var businessPlan = await _context.BusinessPlans
            .Include(bp => bp.Organization)
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

        if (businessPlan == null)
            return Result.Failure<bool>(
                Error.NotFound("BusinessPlan.NotFound", "Business plan not found"));

        // Find the question template to get profileFieldKey
        var question = await _context.QuestionTemplatesV3
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

        if (question == null)
            return Result.Failure<bool>(
                Error.NotFound("Question.NotFound", "Question template not found"));

        // Write back to org profile if requested and question has a profile field mapping
        if (request.WriteBackToProfile &&
            !string.IsNullOrEmpty(question.ProfileFieldKey) &&
            businessPlan.Organization != null)
        {
            try
            {
                businessPlan.Organization.SetProfileField(question.ProfileFieldKey, request.ResponseText);
                _logger.LogInformation(
                    "Wrote back response for Q{QuestionNumber} to org profile field {FieldKey}",
                    question.QuestionNumber, question.ProfileFieldKey);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Failed to write back to profile field {FieldKey}", question.ProfileFieldKey);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}

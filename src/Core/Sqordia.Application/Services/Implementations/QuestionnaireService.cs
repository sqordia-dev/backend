using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

public class QuestionnaireService : IQuestionnaireService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<QuestionnaireService> _logger;
    private readonly ILocalizationService _localizationService;

    public QuestionnaireService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<QuestionnaireService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
        _localizationService = localizationService;
    }

    public async Task<Result<IEnumerable<QuestionnaireQuestionResponse>>> GetQuestionnaireAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.Unauthorized("User.Unauthorized", "User is not authenticated."));
            }

            // Get business plan (excluding deleted ones)
            var businessPlan = await _context.BusinessPlans
                .Include(bp => bp.Organization)
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.NotFound("BusinessPlan.NotFound", "Business plan not found."));
            }

            // Verify access
            var isMember = await _context.OrganizationMembers
                .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                               om.UserId == currentUserId.Value &&
                               om.IsActive, cancellationToken);

            if (!isMember)
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.Forbidden("BusinessPlan.Forbidden", "You don't have access to this business plan."));
            }

            // Load V3 questions (STRUCTURE FINALE)
            var v3Questions = await _context.QuestionTemplates
                .Where(qt => qt.IsActive)
                .OrderBy(qt => qt.DisplayOrder)
                .ToListAsync(cancellationToken);

            if (!v3Questions.Any())
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.NotFound("Questionnaire.NotFound", "No questionnaire questions found."));
            }

            // Get existing responses keyed by V3 template ID
            var responses = await _context.QuestionnaireResponses
                .Where(qr => qr.BusinessPlanId == businessPlanId && qr.QuestionTemplateId.HasValue)
                .ToDictionaryAsync(qr => qr.QuestionTemplateId!.Value, cancellationToken);

            // Detect current language
            var currentLanguage = _localizationService.GetCurrentLanguage();
            var isEnglish = currentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);

            // Map questions with responses
            var questionnaire = v3Questions
                .Select(q =>
                {
                    var hasResponse = responses.TryGetValue(q.Id, out var response);

                    var questionText = isEnglish && !string.IsNullOrWhiteSpace(q.QuestionTextEN)
                        ? q.QuestionTextEN
                        : q.QuestionTextFR;

                    var helpText = isEnglish && !string.IsNullOrWhiteSpace(q.HelpTextEN)
                        ? q.HelpTextEN
                        : q.HelpTextFR;

                    // Parse options in appropriate language
                    List<string>? options = null;
                    var optionsJson = isEnglish && !string.IsNullOrWhiteSpace(q.OptionsEN)
                        ? q.OptionsEN
                        : q.OptionsFR;

                    if (!string.IsNullOrWhiteSpace(optionsJson))
                    {
                        try { options = JsonConvert.DeserializeObject<List<string>>(optionsJson); }
                        catch { /* malformed JSON */ }
                    }

                    // Parse selected options if present
                    List<string>? selectedOptions = null;
                    if (hasResponse && response != null && !string.IsNullOrWhiteSpace(response.SelectedOptions))
                    {
                        try { selectedOptions = JsonConvert.DeserializeObject<List<string>>(response.SelectedOptions); }
                        catch { /* malformed JSON */ }
                    }

                    return new QuestionnaireQuestionResponse
                    {
                        Id = q.Id,
                        QuestionText = questionText,
                        HelpText = helpText,
                        QuestionType = q.QuestionType.ToString(),
                        Order = q.DisplayOrder,
                        IsRequired = q.IsRequired,
                        Section = q.SectionGroup,
                        Options = options,
                        UserResponse = hasResponse && response != null ? response.ResponseText : null,
                        NumericValue = hasResponse && response != null ? response.NumericValue : null,
                        DateValue = hasResponse && response != null ? response.DateValue : null,
                        BooleanValue = hasResponse && response != null ? response.BooleanValue : null,
                        SelectedOptions = selectedOptions
                    };
                })
                .ToList();

            return Result.Success<IEnumerable<QuestionnaireQuestionResponse>>(questionnaire);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving questionnaire for business plan {PlanId}", businessPlanId);
            return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.InternalServerError("Questionnaire.GetError", "An error occurred while retrieving the questionnaire."));
        }
    }

    public async Task<Result<QuestionnaireQuestionResponse>> SubmitResponseAsync(Guid businessPlanId, SubmitQuestionnaireResponseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<QuestionnaireQuestionResponse>(Error.Unauthorized("User.Unauthorized", "User is not authenticated."));
            }

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<QuestionnaireQuestionResponse>(Error.NotFound("BusinessPlan.NotFound", "Business plan not found."));
            }

            // Verify access
            var isMember = await _context.OrganizationMembers
                .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                               om.UserId == currentUserId.Value &&
                               om.IsActive, cancellationToken);

            if (!isMember)
            {
                return Result.Failure<QuestionnaireQuestionResponse>(Error.Forbidden("BusinessPlan.Forbidden", "You don't have access to this business plan."));
            }

            // Look up V3 template
            var questionTemplate = await _context.QuestionTemplates
                .FirstOrDefaultAsync(qt => qt.Id == request.QuestionTemplateId && qt.IsActive, cancellationToken);

            if (questionTemplate == null)
            {
                return Result.Failure<QuestionnaireQuestionResponse>(Error.NotFound("Question.NotFound", "Question not found."));
            }

            // Check if response already exists
            var existingResponse = await _context.QuestionnaireResponses
                .FirstOrDefaultAsync(qr => qr.BusinessPlanId == businessPlanId &&
                                          qr.QuestionTemplateId == request.QuestionTemplateId, cancellationToken);

            if (existingResponse != null)
            {
                existingResponse.UpdateResponse(request.ResponseText);
                existingResponse.SetNumericValue(request.NumericValue);
                existingResponse.SetDateValue(request.DateValue);
                existingResponse.SetBooleanValue(request.BooleanValue);

                if (request.SelectedOptions != null && request.SelectedOptions.Any())
                {
                    existingResponse.SetSelectedOptions(JsonConvert.SerializeObject(request.SelectedOptions));
                }

                existingResponse.LastModifiedBy = currentUserId.Value.ToString();
            }
            else
            {
                var newResponse = new QuestionnaireResponse(businessPlanId, request.QuestionTemplateId, request.ResponseText);
                newResponse.SetNumericValue(request.NumericValue);
                newResponse.SetDateValue(request.DateValue);
                newResponse.SetBooleanValue(request.BooleanValue);

                if (request.SelectedOptions != null && request.SelectedOptions.Any())
                {
                    newResponse.SetSelectedOptions(JsonConvert.SerializeObject(request.SelectedOptions));
                }

                newResponse.CreatedBy = currentUserId.Value.ToString();
                _context.QuestionnaireResponses.Add(newResponse);
                existingResponse = newResponse;
            }

            if (!existingResponse.HasResponse())
            {
                return Result.Failure<QuestionnaireQuestionResponse>(
                    Error.Validation("Questionnaire.EmptyResponse", "At least one response field must be populated."));
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("23505") == true || ex.InnerException?.Message?.Contains("duplicate key") == true)
            {
                _logger.LogWarning("Duplicate key on questionnaire response for plan {PlanId}, question {QuestionId}. Retrying as update.",
                    businessPlanId, request.QuestionTemplateId);

                var retryResponse = await _context.QuestionnaireResponses.AsNoTracking()
                    .FirstOrDefaultAsync(qr => qr.BusinessPlanId == businessPlanId && qr.QuestionTemplateId == request.QuestionTemplateId, cancellationToken);

                if (retryResponse != null)
                {
                    await _context.QuestionnaireResponses
                        .Where(qr => qr.BusinessPlanId == businessPlanId && qr.QuestionTemplateId == request.QuestionTemplateId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(qr => qr.ResponseText, request.ResponseText)
                            .SetProperty(qr => qr.LastModified, DateTime.UtcNow)
                            .SetProperty(qr => qr.LastModifiedBy, currentUserId.Value.ToString()), cancellationToken);

                    existingResponse = retryResponse;
                }
                else
                {
                    throw;
                }
            }

            // Update completion percentage
            await UpdateCompletionInternalAsync(businessPlanId, cancellationToken);

            _logger.LogInformation("Response submitted for question {QuestionId} in business plan {PlanId} by user {UserId}",
                request.QuestionTemplateId, businessPlanId, currentUserId.Value);

            // Detect current language
            var currentLanguage = _localizationService.GetCurrentLanguage();
            var isEnglish = currentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);

            // Parse options
            List<string>? options = null;
            var optionsJson = isEnglish ? questionTemplate.OptionsEN : questionTemplate.OptionsFR;
            if (!string.IsNullOrWhiteSpace(optionsJson))
            {
                try { options = JsonConvert.DeserializeObject<List<string>>(optionsJson); }
                catch (JsonException ex) { _logger.LogWarning(ex, "Failed to deserialize options JSON: {Json}", optionsJson); }
            }

            List<string>? selectedOptions = null;
            if (!string.IsNullOrWhiteSpace(existingResponse.SelectedOptions))
            {
                try { selectedOptions = JsonConvert.DeserializeObject<List<string>>(existingResponse.SelectedOptions); }
                catch (JsonException ex) { _logger.LogWarning(ex, "Failed to deserialize selectedOptions JSON"); }
            }

            var response = new QuestionnaireQuestionResponse
            {
                Id = questionTemplate.Id,
                QuestionText = isEnglish ? questionTemplate.QuestionTextEN : questionTemplate.QuestionTextFR,
                HelpText = isEnglish ? questionTemplate.HelpTextEN : questionTemplate.HelpTextFR,
                QuestionType = questionTemplate.QuestionType.ToString(),
                Order = questionTemplate.DisplayOrder,
                IsRequired = questionTemplate.IsRequired,
                Section = questionTemplate.SectionGroup,
                Options = options,
                UserResponse = existingResponse.ResponseText,
                NumericValue = existingResponse.NumericValue,
                DateValue = existingResponse.DateValue,
                BooleanValue = existingResponse.BooleanValue,
                SelectedOptions = selectedOptions
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting response for business plan {PlanId}", businessPlanId);
            return Result.Failure<QuestionnaireQuestionResponse>(Error.InternalServerError("Questionnaire.SubmitError", $"An error occurred while submitting the response: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireQuestionResponse>>> GetResponsesAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserIdAsGuid();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.Unauthorized("User.Unauthorized", "User is not authenticated."));
            }

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

            if (businessPlan == null)
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.NotFound("BusinessPlan.NotFound", "Business plan not found."));
            }

            // Verify access
            var isMember = await _context.OrganizationMembers
                .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                               om.UserId == currentUserId.Value &&
                               om.IsActive, cancellationToken);

            if (!isMember)
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.Forbidden("BusinessPlan.Forbidden", "You don't have access to this business plan."));
            }

            // Get responses with V3 templates
            var responses = await _context.QuestionnaireResponses
                .Where(qr => qr.BusinessPlanId == businessPlanId && qr.QuestionTemplateId.HasValue)
                .ToListAsync(cancellationToken);

            if (!responses.Any())
            {
                return Result.Success(Enumerable.Empty<QuestionnaireQuestionResponse>());
            }

            var v3QuestionIds = responses.Select(r => r.QuestionTemplateId!.Value).Distinct().ToList();
            var v3Templates = await _context.QuestionTemplates
                .Where(qt => v3QuestionIds.Contains(qt.Id) && qt.IsActive)
                .ToDictionaryAsync(qt => qt.Id, cancellationToken);

            var currentLanguage = _localizationService.GetCurrentLanguage();
            var isEnglish = currentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);

            var result = responses.Select(r =>
            {
                List<string>? selectedOptions = null;
                if (!string.IsNullOrWhiteSpace(r.SelectedOptions))
                {
                    try { selectedOptions = JsonConvert.DeserializeObject<List<string>>(r.SelectedOptions); }
                    catch (JsonException) { /* malformed */ }
                }

                if (r.QuestionTemplateId.HasValue && v3Templates.TryGetValue(r.QuestionTemplateId.Value, out var v3Template))
                {
                    List<string>? options = null;
                    var optionsJson = isEnglish ? v3Template.OptionsEN : v3Template.OptionsFR;
                    if (!string.IsNullOrWhiteSpace(optionsJson))
                    {
                        try { options = JsonConvert.DeserializeObject<List<string>>(optionsJson); }
                        catch (JsonException) { /* malformed */ }
                    }

                    return new QuestionnaireQuestionResponse
                    {
                        Id = v3Template.Id,
                        QuestionText = isEnglish ? v3Template.QuestionTextEN : v3Template.QuestionTextFR,
                        HelpText = isEnglish ? v3Template.HelpTextEN : v3Template.HelpTextFR,
                        QuestionType = v3Template.QuestionType.ToString(),
                        Order = v3Template.DisplayOrder,
                        IsRequired = v3Template.IsRequired,
                        Section = v3Template.SectionGroup,
                        Options = options,
                        UserResponse = r.ResponseText,
                        NumericValue = r.NumericValue,
                        DateValue = r.DateValue,
                        BooleanValue = r.BooleanValue,
                        SelectedOptions = selectedOptions
                    };
                }

                // Fallback for orphaned responses
                return new QuestionnaireQuestionResponse
                {
                    Id = r.QuestionTemplateId ?? Guid.Empty,
                    QuestionText = "",
                    QuestionType = "LongText",
                    Order = 0,
                    IsRequired = false,
                    UserResponse = r.ResponseText,
                    NumericValue = r.NumericValue,
                    DateValue = r.DateValue,
                    BooleanValue = r.BooleanValue,
                    SelectedOptions = selectedOptions
                };
            }).OrderBy(r => r.Order);

            return Result.Success<IEnumerable<QuestionnaireQuestionResponse>>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving responses for business plan {PlanId}", businessPlanId);
            return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.InternalServerError("Questionnaire.GetResponsesError", "An error occurred while retrieving responses."));
        }
    }

    public async Task<Result> UpdateCompletionAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            await UpdateCompletionInternalAsync(businessPlanId, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating completion for business plan {PlanId}", businessPlanId);
            return Result.Failure(Error.InternalServerError("Questionnaire.UpdateCompletionError", "An error occurred while updating completion."));
        }
    }

    private async Task UpdateCompletionInternalAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var businessPlan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null) return;

        var v3Questions = await _context.QuestionTemplates
            .Where(qt => qt.IsActive)
            .ToListAsync(cancellationToken);

        var totalQuestions = v3Questions.Count;
        if (totalQuestions == 0) return;

        var v3RequiredIds = v3Questions.Where(q => q.IsRequired).Select(q => q.Id).ToList();

        var completedResponses = await _context.QuestionnaireResponses
            .Where(qr => qr.BusinessPlanId == businessPlanId && qr.QuestionTemplateId.HasValue)
            .CountAsync(cancellationToken);

        completedResponses = Math.Min(completedResponses, totalQuestions);

        var answeredRequiredCount = await _context.QuestionnaireResponses
            .Where(qr => qr.BusinessPlanId == businessPlanId &&
                         qr.QuestionTemplateId.HasValue &&
                         v3RequiredIds.Contains(qr.QuestionTemplateId.Value))
            .CountAsync(cancellationToken);

        businessPlan.UpdateQuestionnaire(totalQuestions, completedResponses);

        if (answeredRequiredCount == v3RequiredIds.Count && v3RequiredIds.Count > 0)
        {
            businessPlan.MarkQuestionnaireComplete();
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completion updated for business plan {PlanId}: {Completed}/{Total} ({Percentage}%)",
            businessPlanId, completedResponses, totalQuestions, businessPlan.CompletionPercentage);
    }
}

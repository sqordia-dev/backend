using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;
using Sqordia.Domain.Entities.BusinessPlan;
using System.Security.Claims;

namespace Sqordia.Application.Services.Implementations;

public class QuestionnaireService : IQuestionnaireService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<QuestionnaireService> _logger;
    private readonly ILocalizationService _localizationService;

    public QuestionnaireService(
        IApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<QuestionnaireService> logger,
        ILocalizationService localizationService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _localizationService = localizationService;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString))
        {
            return null;
        }
        return Guid.TryParse(userIdString, out var userId) ? userId : null;
    }

    public async Task<Result<IEnumerable<QuestionnaireQuestionResponse>>> GetQuestionnaireAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
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

            // Get active questionnaire template for this plan type
            var template = await _context.QuestionnaireTemplates
                .Include(qt => qt.Questions)
                .Where(qt => qt.PlanType == businessPlan.PlanType && qt.IsActive)
                .OrderByDescending(qt => qt.Version)
                .FirstOrDefaultAsync(cancellationToken);

            if (template == null)
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.NotFound("Questionnaire.NotFound", $"No questionnaire template found for plan type {businessPlan.PlanType}."));
            }

            // Get existing responses
            var responses = await _context.QuestionnaireResponses
                .Where(qr => qr.BusinessPlanId == businessPlanId)
                .ToDictionaryAsync(qr => qr.QuestionTemplateId, cancellationToken);

            // Detect current language
            var currentLanguage = _localizationService.GetCurrentLanguage();
            var isEnglish = currentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);

            // Map questions with responses (bilingual support)
            var questionnaire = template.Questions
                .OrderBy(q => q.Order)
                .Select(q =>
                {
                    var hasResponse = responses.TryGetValue(q.Id, out var response);
                    
                    // Select appropriate language for question text
                    var questionText = (isEnglish && !string.IsNullOrWhiteSpace(q.QuestionTextEN) 
                        ? q.QuestionTextEN 
                        : q.QuestionText) ?? string.Empty;
                    
                    // Select appropriate language for help text
                    var helpText = isEnglish && !string.IsNullOrWhiteSpace(q.HelpTextEN) 
                        ? q.HelpTextEN 
                        : q.HelpText;
                    
                    // Parse options in appropriate language
                    List<string>? options = null;
                    var optionsJson = isEnglish && !string.IsNullOrWhiteSpace(q.OptionsEN) 
                        ? q.OptionsEN 
                        : q.Options;
                        
                    if (!string.IsNullOrWhiteSpace(optionsJson))
                    {
                        try
                        {
                            options = JsonConvert.DeserializeObject<List<string>>(optionsJson);
                        }
                        catch
                        {
                            // If JSON parsing fails, treat as null
                        }
                    }

                    // Parse selected options if present
                    List<string>? selectedOptions = null;
                    if (hasResponse && response != null && !string.IsNullOrWhiteSpace(response.SelectedOptions))
                    {
                        try
                        {
                            selectedOptions = JsonConvert.DeserializeObject<List<string>>(response.SelectedOptions);
                        }
                        catch
                        {
                            // If JSON parsing fails, treat as null
                        }
                    }

                    return new QuestionnaireQuestionResponse
                    {
                        Id = q.Id,
                        QuestionText = questionText,
                        HelpText = helpText,
                        QuestionType = q.QuestionType.ToString(),
                        Order = q.Order,
                        IsRequired = q.IsRequired,
                        Section = q.Section,
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
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<QuestionnaireQuestionResponse>(Error.Unauthorized("User.Unauthorized", "User is not authenticated."));
            }

            // Get business plan (excluding deleted ones)
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

            // Get question template
            var questionTemplate = await _context.QuestionTemplates
                .FirstOrDefaultAsync(qt => qt.Id == request.QuestionTemplateId, cancellationToken);

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
                // Update existing response
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
                // Create new response
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

            await _context.SaveChangesAsync(cancellationToken);

            // Update completion percentage
            await UpdateCompletionInternalAsync(businessPlanId, cancellationToken);

            _logger.LogInformation("Response submitted for question {QuestionId} in business plan {PlanId} by user {UserId}",
                request.QuestionTemplateId, businessPlanId, currentUserId.Value);

            // Parse options and selected options
            List<string>? options = null;
            if (!string.IsNullOrWhiteSpace(questionTemplate.Options))
            {
                try
                {
                    options = JsonConvert.DeserializeObject<List<string>>(questionTemplate.Options);
                }
                catch { }
            }

            List<string>? selectedOptions = null;
            if (!string.IsNullOrWhiteSpace(existingResponse.SelectedOptions))
            {
                try
                {
                    selectedOptions = JsonConvert.DeserializeObject<List<string>>(existingResponse.SelectedOptions);
                }
                catch { }
            }

            var response = new QuestionnaireQuestionResponse
            {
                Id = questionTemplate.Id,
                QuestionText = questionTemplate.QuestionText,
                HelpText = questionTemplate.HelpText,
                QuestionType = questionTemplate.QuestionType.ToString(),
                Order = questionTemplate.Order,
                IsRequired = questionTemplate.IsRequired,
                Section = questionTemplate.Section,
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
            return Result.Failure<QuestionnaireQuestionResponse>(Error.InternalServerError("Questionnaire.SubmitError", "An error occurred while submitting the response."));
        }
    }

    public async Task<Result<IEnumerable<QuestionnaireQuestionResponse>>> GetResponsesAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.Unauthorized("User.Unauthorized", "User is not authenticated."));
            }

            // Get business plan (excluding deleted ones)
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

            // Get responses with question templates
            var responses = await _context.QuestionnaireResponses
                .Include(qr => qr.QuestionTemplate)
                .Where(qr => qr.BusinessPlanId == businessPlanId)
                .OrderBy(qr => qr.QuestionTemplate.Order)
                .ToListAsync(cancellationToken);

            var result = responses.Select(r =>
            {
                // Parse options
                List<string>? options = null;
                if (!string.IsNullOrWhiteSpace(r.QuestionTemplate.Options))
                {
                    try
                    {
                        options = JsonConvert.DeserializeObject<List<string>>(r.QuestionTemplate.Options);
                    }
                    catch { }
                }

                // Parse selected options
                List<string>? selectedOptions = null;
                if (!string.IsNullOrWhiteSpace(r.SelectedOptions))
                {
                    try
                    {
                        selectedOptions = JsonConvert.DeserializeObject<List<string>>(r.SelectedOptions);
                    }
                    catch { }
                }

                return new QuestionnaireQuestionResponse
                {
                    Id = r.QuestionTemplate.Id,
                    QuestionText = r.QuestionTemplate.QuestionText,
                    HelpText = r.QuestionTemplate.HelpText,
                    QuestionType = r.QuestionTemplate.QuestionType.ToString(),
                    Order = r.QuestionTemplate.Order,
                    IsRequired = r.QuestionTemplate.IsRequired,
                    Section = r.QuestionTemplate.Section,
                    Options = options,
                    UserResponse = r.ResponseText,
                    NumericValue = r.NumericValue,
                    DateValue = r.DateValue,
                    BooleanValue = r.BooleanValue,
                    SelectedOptions = selectedOptions
                };
            });

            return Result.Success(result);
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

        // Get total questions for this plan type
        var template = await _context.QuestionnaireTemplates
            .Include(qt => qt.Questions)
            .Where(qt => qt.PlanType == businessPlan.PlanType && qt.IsActive)
            .OrderByDescending(qt => qt.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (template == null) return;

        var totalQuestions = template.Questions.Count;
        var requiredQuestions = template.Questions.Where(q => q.IsRequired).Select(q => q.Id).ToList();

        // Get completed responses (at least for required questions)
        var completedResponses = await _context.QuestionnaireResponses
            .Where(qr => qr.BusinessPlanId == businessPlanId)
            .CountAsync(cancellationToken);

        // Check if all required questions are answered
        var answeredRequiredQuestions = await _context.QuestionnaireResponses
            .Where(qr => qr.BusinessPlanId == businessPlanId && requiredQuestions.Contains(qr.QuestionTemplateId))
            .CountAsync(cancellationToken);

        businessPlan.UpdateQuestionnaire(totalQuestions, completedResponses);

        // If all required questions are answered, mark as complete
        if (answeredRequiredQuestions == requiredQuestions.Count && requiredQuestions.Count > 0)
        {
            businessPlan.MarkQuestionnaireComplete();
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completion updated for business plan {PlanId}: {Completed}/{Total} ({Percentage}%)",
            businessPlanId, completedResponses, totalQuestions, businessPlan.CompletionPercentage);
    }
}


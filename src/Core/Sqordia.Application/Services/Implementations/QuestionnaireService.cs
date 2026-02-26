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

            // Get active questionnaire template for this plan type
            var template = await _context.QuestionnaireTemplates
                .Include(qt => qt.Questions)
                .Where(qt => qt.PlanType == businessPlan.PlanType && qt.IsActive)
                .OrderByDescending(qt => qt.Version)
                .FirstOrDefaultAsync(cancellationToken);

            // Fallback: if no template for StrategicPlan (or LeanCanvas), use BusinessPlan template until dedicated templates exist
            if (template == null && businessPlan.PlanType != BusinessPlanType.BusinessPlan)
            {
                _logger.LogWarning(
                    "No questionnaire template found for plan type {PlanType}. Using BusinessPlan template as fallback for business plan {BusinessPlanId}.",
                    businessPlan.PlanType,
                    businessPlanId);
                template = await _context.QuestionnaireTemplates
                    .Include(qt => qt.Questions)
                    .Where(qt => qt.PlanType == BusinessPlanType.BusinessPlan && qt.IsActive)
                    .OrderByDescending(qt => qt.Version)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (template == null)
            {
                return Result.Failure<IEnumerable<QuestionnaireQuestionResponse>>(Error.NotFound("Questionnaire.NotFound", $"No questionnaire template found for plan type {businessPlan.PlanType}."));
            }

            // Get existing responses - use effective question ID (V1 or V2)
            var responses = await _context.QuestionnaireResponses
                .Where(qr => qr.BusinessPlanId == businessPlanId)
                .ToDictionaryAsync(qr => qr.QuestionTemplateId ?? qr.QuestionTemplateV2Id ?? Guid.Empty, cancellationToken);

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
            var currentUserId = _currentUserService.GetUserIdAsGuid();
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

            // Get question template - check both V1 and V2 tables
            var questionTemplate = await _context.QuestionTemplates
                .FirstOrDefaultAsync(qt => qt.Id == request.QuestionTemplateId, cancellationToken);

            var questionTemplateV2 = questionTemplate == null
                ? await _context.QuestionTemplatesV2
                    .FirstOrDefaultAsync(qt => qt.Id == request.QuestionTemplateId, cancellationToken)
                : null;

            if (questionTemplate == null && questionTemplateV2 == null)
            {
                return Result.Failure<QuestionnaireQuestionResponse>(Error.NotFound("Question.NotFound", "Question not found."));
            }

            // Check if response already exists - check both V1 and V2 IDs
            var existingResponse = questionTemplate != null
                ? await _context.QuestionnaireResponses
                    .FirstOrDefaultAsync(qr => qr.BusinessPlanId == businessPlanId &&
                                              qr.QuestionTemplateId == request.QuestionTemplateId, cancellationToken)
                : await _context.QuestionnaireResponses
                    .FirstOrDefaultAsync(qr => qr.BusinessPlanId == businessPlanId &&
                                              qr.QuestionTemplateV2Id == request.QuestionTemplateId, cancellationToken);

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
                // Create new response - use appropriate FK based on template version
                QuestionnaireResponse newResponse;
                if (questionTemplate != null)
                {
                    // V1 template
                    newResponse = new QuestionnaireResponse(businessPlanId, request.QuestionTemplateId, request.ResponseText);
                }
                else
                {
                    // V2 template
                    newResponse = QuestionnaireResponse.CreateForV2(businessPlanId, request.QuestionTemplateId, request.ResponseText);
                }

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

            // Validate that at least one response field is populated
            if (!existingResponse.HasResponse())
            {
                return Result.Failure<QuestionnaireQuestionResponse>(
                    Error.Validation("Questionnaire.EmptyResponse", "At least one response field must be populated."));
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Update completion percentage
            await UpdateCompletionInternalAsync(businessPlanId, cancellationToken);

            _logger.LogInformation("Response submitted for question {QuestionId} in business plan {PlanId} by user {UserId}",
                request.QuestionTemplateId, businessPlanId, currentUserId.Value);

            // Parse options and selected options - handle both V1 and V2 templates
            List<string>? options = null;
            string? optionsJson = questionTemplate?.Options ?? questionTemplateV2?.Options;
            if (!string.IsNullOrWhiteSpace(optionsJson))
            {
                try
                {
                    options = JsonConvert.DeserializeObject<List<string>>(optionsJson);
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

            // Build response using V1 or V2 template data
            var response = new QuestionnaireQuestionResponse
            {
                Id = questionTemplate?.Id ?? questionTemplateV2!.Id,
                QuestionText = questionTemplate?.QuestionText ?? questionTemplateV2!.QuestionText,
                HelpText = questionTemplate?.HelpText ?? questionTemplateV2?.HelpText,
                QuestionType = questionTemplate?.QuestionType.ToString() ?? questionTemplateV2!.QuestionType.ToString(),
                Order = questionTemplate?.Order ?? questionTemplateV2!.Order,
                IsRequired = questionTemplate?.IsRequired ?? questionTemplateV2!.IsRequired,
                Section = questionTemplate?.Section ?? questionTemplateV2?.Section,
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

            // Get responses (without Include to support both V1 and V2 templates)
            var responses = await _context.QuestionnaireResponses
                .Where(qr => qr.BusinessPlanId == businessPlanId)
                .ToListAsync(cancellationToken);

            if (!responses.Any())
            {
                return Result.Success(Enumerable.Empty<QuestionnaireQuestionResponse>());
            }

            // Get question template IDs
            var questionIds = responses.Select(r => r.QuestionTemplateId).Distinct().ToList();

            // Load templates from both V1 and V2 tables
            var v1Templates = await _context.QuestionTemplates
                .Where(qt => questionIds.Contains(qt.Id))
                .ToDictionaryAsync(qt => qt.Id, cancellationToken);

            var v2Templates = await _context.QuestionTemplatesV2
                .Where(qt => questionIds.Contains(qt.Id))
                .ToDictionaryAsync(qt => qt.Id, cancellationToken);

            // Detect current language
            var currentLanguage = _localizationService.GetCurrentLanguage();
            var isEnglish = currentLanguage.Equals("en", StringComparison.OrdinalIgnoreCase);

            var result = responses.Select(r =>
            {
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

                // Try V1 template first
                if (r.QuestionTemplateId.HasValue && v1Templates.TryGetValue(r.QuestionTemplateId.Value, out var v1Template))
                {
                    List<string>? options = null;
                    if (!string.IsNullOrWhiteSpace(v1Template.Options))
                    {
                        try { options = JsonConvert.DeserializeObject<List<string>>(v1Template.Options); } catch { }
                    }

                    return new QuestionnaireQuestionResponse
                    {
                        Id = v1Template.Id,
                        QuestionText = v1Template.QuestionText,
                        HelpText = v1Template.HelpText,
                        QuestionType = v1Template.QuestionType.ToString(),
                        Order = v1Template.Order,
                        IsRequired = v1Template.IsRequired,
                        Section = v1Template.Section,
                        Options = options,
                        UserResponse = r.ResponseText,
                        NumericValue = r.NumericValue,
                        DateValue = r.DateValue,
                        BooleanValue = r.BooleanValue,
                        SelectedOptions = selectedOptions
                    };
                }

                // Try V2 template
                var v2TemplateId = r.QuestionTemplateV2Id ?? r.QuestionTemplateId;
                if (v2TemplateId.HasValue && v2Templates.TryGetValue(v2TemplateId.Value, out var v2Template))
                {
                    var questionText = isEnglish && !string.IsNullOrWhiteSpace(v2Template.QuestionTextEN)
                        ? v2Template.QuestionTextEN
                        : v2Template.QuestionText;
                    var helpText = isEnglish && !string.IsNullOrWhiteSpace(v2Template.HelpTextEN)
                        ? v2Template.HelpTextEN
                        : v2Template.HelpText;

                    List<string>? options = null;
                    var optionsJson = isEnglish && !string.IsNullOrWhiteSpace(v2Template.OptionsEN)
                        ? v2Template.OptionsEN
                        : v2Template.Options;
                    if (optionsJson != null)
                    {
                        try { options = JsonConvert.DeserializeObject<List<string>>(optionsJson); } catch { }
                    }

                    return new QuestionnaireQuestionResponse
                    {
                        Id = v2Template.Id,
                        QuestionText = questionText,
                        HelpText = helpText,
                        QuestionType = v2Template.QuestionType.ToString(),
                        Order = v2Template.Order,
                        IsRequired = v2Template.IsRequired,
                        Section = v2Template.Section,
                        Options = options,
                        UserResponse = r.ResponseText,
                        NumericValue = r.NumericValue,
                        DateValue = r.DateValue,
                        BooleanValue = r.BooleanValue,
                        SelectedOptions = selectedOptions
                    };
                }

                // Fallback - return response with just the ID if template not found
                return new QuestionnaireQuestionResponse
                {
                    Id = r.QuestionTemplateId ?? r.QuestionTemplateV2Id ?? Guid.Empty,
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

        // Get total questions for this plan type (with fallback to BusinessPlan template for StrategicPlan/LeanCanvas)
        var template = await _context.QuestionnaireTemplates
            .Include(qt => qt.Questions)
            .Where(qt => qt.PlanType == businessPlan.PlanType && qt.IsActive)
            .OrderByDescending(qt => qt.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (template == null && businessPlan.PlanType != BusinessPlanType.BusinessPlan)
        {
            template = await _context.QuestionnaireTemplates
                .Include(qt => qt.Questions)
                .Where(qt => qt.PlanType == BusinessPlanType.BusinessPlan && qt.IsActive)
                .OrderByDescending(qt => qt.Version)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (template == null) return;

        var totalQuestions = template.Questions.Count;
        var requiredQuestions = template.Questions.Where(q => q.IsRequired).Select(q => q.Id).ToList();

        // Get completed responses (at least for required questions)
        var completedResponses = await _context.QuestionnaireResponses
            .Where(qr => qr.BusinessPlanId == businessPlanId)
            .CountAsync(cancellationToken);

        // Check if all required questions are answered
        var answeredRequiredQuestions = await _context.QuestionnaireResponses
            .Where(qr => qr.BusinessPlanId == businessPlanId &&
                         qr.QuestionTemplateId.HasValue &&
                         requiredQuestions.Contains(qr.QuestionTemplateId.Value))
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


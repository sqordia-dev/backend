using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.SectionPrompt;
using Sqordia.Contracts.Responses.Admin.SectionPrompt;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.V3.Implementations;

/// <summary>
/// Service implementation for managing section prompts with master/override hierarchy
/// </summary>
public class SectionPromptService : ISectionPromptService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly ILogger<SectionPromptService> _logger;

    public SectionPromptService(
        IApplicationDbContext context,
        IAIService aiService,
        ILogger<SectionPromptService> logger)
    {
        _context = context;
        _aiService = aiService;
        _logger = logger;
    }

    #region Query

    public async Task<Result<List<SectionPromptListResponse>>> GetPromptsAsync(
        SectionPromptFilterRequest? filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.SectionPrompts
                .Include(sp => sp.MainSection)
                .Include(sp => sp.SubSection)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Level))
                {
                    if (Enum.TryParse<PromptLevel>(filter.Level, true, out var level))
                    {
                        query = query.Where(sp => sp.Level == level);
                    }
                }

                if (!string.IsNullOrEmpty(filter.PlanType))
                {
                    if (Enum.TryParse<BusinessPlanType>(filter.PlanType, true, out var planType))
                    {
                        query = query.Where(sp => sp.PlanType == planType);
                    }
                }

                if (!string.IsNullOrEmpty(filter.Language))
                {
                    query = query.Where(sp => sp.Language == filter.Language);
                }

                if (filter.MainSectionId.HasValue)
                {
                    query = query.Where(sp => sp.MainSectionId == filter.MainSectionId.Value);
                }

                if (filter.SubSectionId.HasValue)
                {
                    query = query.Where(sp => sp.SubSectionId == filter.SubSectionId.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(sp => sp.IsActive == filter.IsActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var search = filter.Search.ToLower();
                    query = query.Where(sp =>
                        sp.Name.ToLower().Contains(search) ||
                        (sp.Description != null && sp.Description.ToLower().Contains(search)));
                }
            }

            var prompts = await query
                .OrderBy(sp => sp.MainSection != null ? sp.MainSection.DisplayOrder : 0)
                .ThenBy(sp => sp.SubSection != null ? sp.SubSection.DisplayOrder : 0)
                .ThenByDescending(sp => sp.Version)
                .Skip((filter?.Page - 1 ?? 0) * (filter?.PageSize ?? 20))
                .Take(filter?.PageSize ?? 20)
                .ToListAsync(cancellationToken);

            var response = prompts.Select(MapToListResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting section prompts");
            return Result.Failure<List<SectionPromptListResponse>>(
                Error.InternalServerError("SectionPrompt.GetError", "Failed to retrieve section prompts"));
        }
    }

    public async Task<Result<SectionPromptResponse>> GetPromptByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await _context.SectionPrompts
                .Include(sp => sp.MainSection)
                .Include(sp => sp.SubSection)
                .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);

            if (prompt == null)
            {
                return Result.Failure<SectionPromptResponse>(
                    Error.NotFound("SectionPrompt.NotFound", $"Section prompt with ID {id} not found"));
            }

            return Result.Success(MapToResponse(prompt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting section prompt {Id}", id);
            return Result.Failure<SectionPromptResponse>(
                Error.InternalServerError("SectionPrompt.GetError", "Failed to retrieve section prompt"));
        }
    }

    public async Task<Result<SectionPromptResponse>> GetEffectivePromptAsync(
        Guid subSectionId,
        BusinessPlanType planType,
        string language,
        string? industryCategory = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First, try to find an override prompt for the sub-section
            var overridePrompt = await _context.SectionPrompts
                .Include(sp => sp.MainSection)
                .Include(sp => sp.SubSection)
                .Where(sp =>
                    sp.SubSectionId == subSectionId &&
                    sp.Level == PromptLevel.Override &&
                    sp.PlanType == planType &&
                    sp.Language == language &&
                    sp.IsActive)
                .OrderByDescending(sp => sp.Version)
                .FirstOrDefaultAsync(cancellationToken);

            if (overridePrompt != null)
            {
                // If industry-specific prompt exists, prefer it
                if (!string.IsNullOrEmpty(industryCategory))
                {
                    var industryPrompt = await _context.SectionPrompts
                        .Include(sp => sp.MainSection)
                        .Include(sp => sp.SubSection)
                        .Where(sp =>
                            sp.SubSectionId == subSectionId &&
                            sp.Level == PromptLevel.Override &&
                            sp.PlanType == planType &&
                            sp.Language == language &&
                            sp.IndustryCategory == industryCategory &&
                            sp.IsActive)
                        .OrderByDescending(sp => sp.Version)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (industryPrompt != null)
                    {
                        _logger.LogDebug("Using industry-specific override prompt for sub-section {SubSectionId}", subSectionId);
                        return Result.Success(MapToResponse(industryPrompt));
                    }
                }

                _logger.LogDebug("Using override prompt for sub-section {SubSectionId}", subSectionId);
                return Result.Success(MapToResponse(overridePrompt));
            }

            // Fall back to master prompt for the main section
            var subSection = await _context.SubSections
                .FirstOrDefaultAsync(ss => ss.Id == subSectionId, cancellationToken);

            if (subSection == null)
            {
                return Result.Failure<SectionPromptResponse>(
                    Error.NotFound("SectionPrompt.SubSectionNotFound", $"Sub-section with ID {subSectionId} not found"));
            }

            var masterPrompt = await _context.SectionPrompts
                .Include(sp => sp.MainSection)
                .Include(sp => sp.SubSection)
                .Where(sp =>
                    sp.MainSectionId == subSection.MainSectionId &&
                    sp.Level == PromptLevel.Master &&
                    sp.PlanType == planType &&
                    sp.Language == language &&
                    sp.IsActive)
                .OrderByDescending(sp => sp.Version)
                .FirstOrDefaultAsync(cancellationToken);

            if (masterPrompt != null)
            {
                _logger.LogDebug("Using master prompt for main section {MainSectionId}", subSection.MainSectionId);
                return Result.Success(MapToResponse(masterPrompt));
            }

            return Result.Failure<SectionPromptResponse>(
                Error.NotFound("SectionPrompt.NoEffectivePrompt",
                    $"No effective prompt found for sub-section {subSectionId}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting effective prompt for sub-section {SubSectionId}", subSectionId);
            return Result.Failure<SectionPromptResponse>(
                Error.InternalServerError("SectionPrompt.GetError", "Failed to retrieve effective prompt"));
        }
    }

    public async Task<Result<SectionPromptResponse>> GetMasterPromptAsync(
        Guid mainSectionId,
        BusinessPlanType planType,
        string language,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await _context.SectionPrompts
                .Include(sp => sp.MainSection)
                .Include(sp => sp.SubSection)
                .Where(sp =>
                    sp.MainSectionId == mainSectionId &&
                    sp.Level == PromptLevel.Master &&
                    sp.PlanType == planType &&
                    sp.Language == language &&
                    sp.IsActive)
                .OrderByDescending(sp => sp.Version)
                .FirstOrDefaultAsync(cancellationToken);

            if (prompt == null)
            {
                return Result.Failure<SectionPromptResponse>(
                    Error.NotFound("SectionPrompt.NotFound",
                        $"Master prompt not found for main section {mainSectionId}"));
            }

            return Result.Success(MapToResponse(prompt));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting master prompt for main section {MainSectionId}", mainSectionId);
            return Result.Failure<SectionPromptResponse>(
                Error.InternalServerError("SectionPrompt.GetError", "Failed to retrieve master prompt"));
        }
    }

    #endregion

    #region Commands

    public async Task<Result<Guid>> CreatePromptAsync(CreateSectionPromptRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<PromptLevel>(request.Level, true, out var level))
            {
                return Result.Failure<Guid>(
                    Error.Validation("SectionPrompt.InvalidLevel", "Invalid prompt level"));
            }

            if (!Enum.TryParse<BusinessPlanType>(request.PlanType, true, out var planType))
            {
                return Result.Failure<Guid>(
                    Error.Validation("SectionPrompt.InvalidPlanType", "Invalid plan type"));
            }

            if (!Enum.TryParse<OutputFormat>(request.OutputFormat, true, out var outputFormat))
            {
                outputFormat = OutputFormat.Prose; // Default
            }

            SectionPrompt prompt;

            if (level == PromptLevel.Master)
            {
                if (!request.MainSectionId.HasValue)
                {
                    return Result.Failure<Guid>(
                        Error.Validation("SectionPrompt.MainSectionRequired", "Main section ID is required for master prompts"));
                }

                prompt = SectionPrompt.CreateMasterPrompt(
                    request.MainSectionId.Value,
                    planType,
                    request.Language,
                    request.Name,
                    request.SystemPrompt,
                    request.UserPromptTemplate,
                    outputFormat,
                    request.IndustryCategory,
                    request.Description,
                    request.VariablesJson,
                    request.VisualElementsJson,
                    request.ExampleOutput);
            }
            else
            {
                if (!request.SubSectionId.HasValue)
                {
                    return Result.Failure<Guid>(
                        Error.Validation("SectionPrompt.SubSectionRequired", "Sub-section ID is required for override prompts"));
                }

                prompt = SectionPrompt.CreateOverridePrompt(
                    request.SubSectionId.Value,
                    planType,
                    request.Language,
                    request.Name,
                    request.SystemPrompt,
                    request.UserPromptTemplate,
                    outputFormat,
                    request.IndustryCategory,
                    request.Description,
                    request.VariablesJson,
                    request.VisualElementsJson,
                    request.ExampleOutput);
            }

            _context.SectionPrompts.Add(prompt);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created {Level} section prompt {Id} for {Language}", level, prompt.Id, request.Language);
            return Result.Success(prompt.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating section prompt");
            return Result.Failure<Guid>(
                Error.InternalServerError("SectionPrompt.CreateError", "Failed to create section prompt"));
        }
    }

    public async Task<Result> UpdatePromptAsync(Guid id, UpdateSectionPromptRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await _context.SectionPrompts.FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
            if (prompt == null)
            {
                return Result.Failure(
                    Error.NotFound("SectionPrompt.NotFound", $"Section prompt with ID {id} not found"));
            }

            prompt.Update(
                request.Name,
                request.Description,
                request.SystemPrompt,
                request.UserPromptTemplate,
                request.VariablesJson,
                request.OutputFormat,
                request.VisualElementsJson,
                request.ExampleOutput,
                request.IndustryCategory);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated section prompt {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating section prompt {Id}", id);
            return Result.Failure(
                Error.InternalServerError("SectionPrompt.UpdateError", "Failed to update section prompt"));
        }
    }

    public async Task<Result> DeletePromptAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await _context.SectionPrompts.FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
            if (prompt == null)
            {
                return Result.Failure(
                    Error.NotFound("SectionPrompt.NotFound", $"Section prompt with ID {id} not found"));
            }

            // Soft delete by deactivating
            prompt.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted (deactivated) section prompt {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting section prompt {Id}", id);
            return Result.Failure(
                Error.InternalServerError("SectionPrompt.DeleteError", "Failed to delete section prompt"));
        }
    }

    public async Task<Result> ActivatePromptAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await _context.SectionPrompts.FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
            if (prompt == null)
            {
                return Result.Failure(
                    Error.NotFound("SectionPrompt.NotFound", $"Section prompt with ID {id} not found"));
            }

            prompt.Activate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Activated section prompt {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating section prompt {Id}", id);
            return Result.Failure(
                Error.InternalServerError("SectionPrompt.ActivateError", "Failed to activate section prompt"));
        }
    }

    public async Task<Result> DeactivatePromptAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await _context.SectionPrompts.FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
            if (prompt == null)
            {
                return Result.Failure(
                    Error.NotFound("SectionPrompt.NotFound", $"Section prompt with ID {id} not found"));
            }

            prompt.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deactivated section prompt {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating section prompt {Id}", id);
            return Result.Failure(
                Error.InternalServerError("SectionPrompt.DeactivateError", "Failed to deactivate section prompt"));
        }
    }

    public async Task<Result<SectionPromptTestResponse>> TestPromptAsync(Guid id, TestSectionPromptRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = await _context.SectionPrompts.FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
            if (prompt == null)
            {
                return Result.Failure<SectionPromptTestResponse>(
                    Error.NotFound("SectionPrompt.NotFound", $"Section prompt with ID {id} not found"));
            }

            var startTime = DateTime.UtcNow;

            // Build the user prompt by replacing variables
            var userPrompt = prompt.UserPromptTemplate;
            foreach (var variable in request.Variables)
            {
                userPrompt = userPrompt.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }

            // Call AI service
            var generatedContent = await _aiService.GenerateContentAsync(
                prompt.SystemPrompt,
                userPrompt,
                request.MaxTokens,
                (float)request.Temperature,
                cancellationToken);

            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return Result.Success(new SectionPromptTestResponse
            {
                Success = true,
                GeneratedContent = generatedContent,
                TokensUsed = 0, // Token count not available from this interface
                ResponseTimeMs = responseTime,
                Provider = "configured", // Provider info not available
                Model = "configured" // Model info not available
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing section prompt {Id}", id);

            return Result.Success(new SectionPromptTestResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                ResponseTimeMs = 0,
                Provider = "configured",
                Model = "configured"
            });
        }
    }

    #endregion

    #region Version Management

    public async Task<Result<List<SectionPromptListResponse>>> GetPromptVersionsAsync(
        Guid mainSectionId,
        Guid? subSectionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.SectionPrompts
                .Include(sp => sp.MainSection)
                .Include(sp => sp.SubSection)
                .Where(sp => sp.MainSectionId == mainSectionId);

            if (subSectionId.HasValue)
            {
                query = query.Where(sp => sp.SubSectionId == subSectionId.Value);
            }

            var prompts = await query
                .OrderByDescending(sp => sp.Version)
                .ToListAsync(cancellationToken);

            var response = prompts.Select(MapToListResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt versions");
            return Result.Failure<List<SectionPromptListResponse>>(
                Error.InternalServerError("SectionPrompt.GetError", "Failed to retrieve prompt versions"));
        }
    }

    public async Task<Result<Guid>> ClonePromptAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var original = await _context.SectionPrompts.FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
            if (original == null)
            {
                return Result.Failure<Guid>(
                    Error.NotFound("SectionPrompt.NotFound", $"Section prompt with ID {id} not found"));
            }

            var clone = original.Clone();
            _context.SectionPrompts.Add(clone);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cloned section prompt {OriginalId} to {CloneId}", id, clone.Id);
            return Result.Success(clone.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning section prompt {Id}", id);
            return Result.Failure<Guid>(
                Error.InternalServerError("SectionPrompt.CloneError", "Failed to clone section prompt"));
        }
    }

    #endregion

    #region Mapping Helpers

    private static SectionPromptResponse MapToResponse(SectionPrompt prompt)
    {
        return new SectionPromptResponse
        {
            Id = prompt.Id,
            MainSectionId = prompt.MainSectionId,
            SubSectionId = prompt.SubSectionId,
            Level = prompt.Level.ToString(),
            PlanType = prompt.PlanType.ToString(),
            Language = prompt.Language,
            IndustryCategory = prompt.IndustryCategory,
            Name = prompt.Name,
            Description = prompt.Description,
            SystemPrompt = prompt.SystemPrompt,
            UserPromptTemplate = prompt.UserPromptTemplate,
            VariablesJson = prompt.VariablesJson,
            OutputFormat = prompt.OutputFormat.ToString(),
            VisualElementsJson = prompt.VisualElementsJson,
            ExampleOutput = prompt.ExampleOutput,
            Version = prompt.Version,
            IsActive = prompt.IsActive,
            Created = prompt.Created,
            LastModified = prompt.LastModified,
            CreatedBy = prompt.CreatedBy,
            MainSectionCode = prompt.MainSection?.Code,
            MainSectionTitle = prompt.MainSection?.TitleFR,
            SubSectionCode = prompt.SubSection?.Code,
            SubSectionTitle = prompt.SubSection?.TitleFR
        };
    }

    private static SectionPromptListResponse MapToListResponse(SectionPrompt prompt)
    {
        return new SectionPromptListResponse
        {
            Id = prompt.Id,
            Level = prompt.Level.ToString(),
            PlanType = prompt.PlanType.ToString(),
            Language = prompt.Language,
            Name = prompt.Name,
            Version = prompt.Version,
            IsActive = prompt.IsActive,
            MainSectionCode = prompt.MainSection?.Code,
            SubSectionCode = prompt.SubSection?.Code,
            LastModified = prompt.LastModified
        };
    }

    #endregion
}

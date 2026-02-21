using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.PromptRegistry;
using Sqordia.Contracts.Responses.Admin.PromptRegistry;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service implementation for managing prompt templates in the admin registry
/// </summary>
public class PromptRegistryService : IPromptRegistryService
{
    private readonly IPromptRepository _promptRepository;
    private readonly IApplicationDbContext _context;
    private readonly IAIProviderFactory _providerFactory;
    private readonly ILogger<PromptRegistryService> _logger;

    public PromptRegistryService(
        IPromptRepository promptRepository,
        IApplicationDbContext context,
        IAIProviderFactory providerFactory,
        ILogger<PromptRegistryService> logger)
    {
        _promptRepository = promptRepository;
        _context = context;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<Result<PromptTemplateDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var template = await _promptRepository.GetByIdAsync(id, ct);
            if (template == null)
            {
                return Result.Failure<PromptTemplateDto>(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {id} not found"));
            }

            var dto = await MapToDto(template, ct);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt template {PromptId}", id);
            return Result.Failure<PromptTemplateDto>(
                new Error("PromptRegistry.Error", "An error occurred while retrieving the prompt template"));
        }
    }

    public async Task<Result<PaginatedList<PromptTemplateListDto>>> GetAllAsync(
        PromptRegistryFilter filter,
        CancellationToken ct = default)
    {
        try
        {
            var query = _context.PromptTemplates.AsQueryable();

            // Apply filters
            if (filter.SectionType.HasValue)
            {
                query = query.Where(p => p.SectionType == filter.SectionType.Value);
            }

            if (filter.PlanType.HasValue)
            {
                query = query.Where(p => p.PlanType == filter.PlanType.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filter.IsActive.Value);
            }

            if (filter.Alias.HasValue)
            {
                query = query.Where(p => p.Alias == filter.Alias.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.IndustryCategory))
            {
                query = query.Where(p => p.IndustryCategory == filter.IndustryCategory);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search));
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.Name)
                    : query.OrderByDescending(p => p.Name),
                "sectiontype" => filter.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.SectionType)
                    : query.OrderByDescending(p => p.SectionType),
                "plantype" => filter.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.PlanType)
                    : query.OrderByDescending(p => p.PlanType),
                "version" => filter.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.Version)
                    : query.OrderByDescending(p => p.Version),
                "createdat" => filter.SortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.UpdatedAt)
            };

            var count = await query.CountAsync(ct);
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(ct);

            var dtos = new List<PromptTemplateListDto>();
            foreach (var item in items)
            {
                dtos.Add(await MapToListDto(item, ct));
            }

            var result = new PaginatedList<PromptTemplateListDto>(
                dtos,
                count,
                filter.PageNumber,
                filter.PageSize);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt templates");
            return Result.Failure<PaginatedList<PromptTemplateListDto>>(
                new Error("PromptRegistry.Error", "An error occurred while retrieving prompt templates"));
        }
    }

    public async Task<Result<Guid>> CreateAsync(
        CreatePromptTemplateRequest request,
        string userId,
        CancellationToken ct = default)
    {
        try
        {
            var template = new PromptTemplate(
                request.SectionType,
                request.PlanType,
                request.Name,
                request.SystemPrompt,
                request.UserPromptTemplate,
                request.OutputFormat,
                userId,
                request.IndustryCategory,
                request.Description,
                request.VisualElementsJson,
                request.ExampleOutput);

            var created = await _promptRepository.CreateAsync(template, ct);
            _logger.LogInformation("Created prompt template {PromptId} for section {Section}", created.Id, request.SectionType);

            return Result.Success(created.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prompt template");
            return Result.Failure<Guid>(
                new Error("PromptRegistry.CreateError", "An error occurred while creating the prompt template"));
        }
    }

    public async Task<Result> UpdateAsync(
        Guid id,
        UpdatePromptTemplateRequest request,
        string userId,
        CancellationToken ct = default)
    {
        try
        {
            var template = await _promptRepository.GetByIdAsync(id, ct);
            if (template == null)
            {
                return Result.Failure(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {id} not found"));
            }

            // Update fields if provided
            if (request.Name != null)
            {
                template.UpdateName(request.Name);
            }

            if (request.SystemPrompt != null || request.UserPromptTemplate != null)
            {
                template.UpdateContent(
                    request.SystemPrompt ?? template.SystemPrompt,
                    request.UserPromptTemplate ?? template.UserPromptTemplate,
                    request.Description,
                    request.VisualElementsJson,
                    request.ExampleOutput);
            }

            if (request.OutputFormat.HasValue)
            {
                template.UpdateOutputFormat(request.OutputFormat.Value);
            }

            if (request.IndustryCategory != null)
            {
                template.SetIndustryCategory(request.IndustryCategory);
            }

            await _promptRepository.UpdateAsync(template, ct);
            _logger.LogInformation("Updated prompt template {PromptId}", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prompt template {PromptId}", id);
            return Result.Failure(
                new Error("PromptRegistry.UpdateError", "An error occurred while updating the prompt template"));
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var template = await _promptRepository.GetByIdAsync(id, ct);
            if (template == null)
            {
                return Result.Failure(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {id} not found"));
            }

            if (template.IsActive)
            {
                return Result.Failure(
                    new Error("PromptRegistry.CannotDeleteActive", "Cannot delete an active prompt template. Deactivate it first."));
            }

            _context.PromptTemplates.Remove(template);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Deleted prompt template {PromptId}", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prompt template {PromptId}", id);
            return Result.Failure(
                new Error("PromptRegistry.DeleteError", "An error occurred while deleting the prompt template"));
        }
    }

    #endregion

    #region Versioning

    public async Task<Result<Guid>> CreateNewVersionAsync(
        Guid sourceId,
        string userId,
        CancellationToken ct = default)
    {
        try
        {
            var source = await _promptRepository.GetByIdAsync(sourceId, ct);
            if (source == null)
            {
                return Result.Failure<Guid>(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {sourceId} not found"));
            }

            // Get the max version for this section/plan/industry
            var maxVersion = await _context.PromptTemplates
                .Where(p => p.SectionType == source.SectionType
                    && p.PlanType == source.PlanType
                    && p.IndustryCategory == source.IndustryCategory)
                .MaxAsync(p => (int?)p.Version, ct) ?? 0;

            // Create new version
            var newVersion = new PromptTemplate(
                source.SectionType,
                source.PlanType,
                source.Name,
                source.SystemPrompt,
                source.UserPromptTemplate,
                source.OutputFormat,
                userId,
                source.IndustryCategory,
                source.Description,
                source.VisualElementsJson,
                source.ExampleOutput);

            // Set the version number
            for (int i = 1; i <= maxVersion; i++)
            {
                newVersion.IncrementVersion();
            }

            var created = await _promptRepository.CreateAsync(newVersion, ct);
            _logger.LogInformation("Created new version {Version} of prompt {SourceId} as {NewId}",
                created.Version, sourceId, created.Id);

            return Result.Success(created.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new version of prompt {PromptId}", sourceId);
            return Result.Failure<Guid>(
                new Error("PromptRegistry.VersionError", "An error occurred while creating a new version"));
        }
    }

    public async Task<Result<List<PromptVersionHistoryDto>>> GetVersionHistoryAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        string? industryCategory = null,
        CancellationToken ct = default)
    {
        try
        {
            var query = _context.PromptTemplates
                .Where(p => p.SectionType == sectionType && p.PlanType == planType);

            if (!string.IsNullOrEmpty(industryCategory))
            {
                query = query.Where(p => p.IndustryCategory == industryCategory);
            }
            else
            {
                query = query.Where(p => p.IndustryCategory == null);
            }

            var templates = await query
                .OrderByDescending(p => p.Version)
                .ToListAsync(ct);

            var history = new List<PromptVersionHistoryDto>();
            PromptTemplate? previousTemplate = null;

            foreach (var template in templates)
            {
                var metrics = await GetAggregatedMetrics(template.Id, ct);

                history.Add(new PromptVersionHistoryDto
                {
                    Id = template.Id,
                    Version = template.Version,
                    IsActive = template.IsActive,
                    Alias = template.Alias,
                    AliasName = template.Alias?.ToString(),
                    Name = template.Name,
                    CreatedAt = template.CreatedAt,
                    CreatedBy = template.CreatedBy,
                    TotalUsageCount = metrics.usageCount,
                    AverageRating = metrics.avgRating,
                    AcceptanceRate = metrics.acceptanceRate,
                    HasSystemPromptChanges = previousTemplate != null && template.SystemPrompt != previousTemplate.SystemPrompt,
                    HasUserPromptChanges = previousTemplate != null && template.UserPromptTemplate != previousTemplate.UserPromptTemplate,
                    HasOutputFormatChanges = previousTemplate != null && template.OutputFormat != previousTemplate.OutputFormat
                });

                previousTemplate = template;
            }

            return Result.Success(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version history for {SectionType}/{PlanType}", sectionType, planType);
            return Result.Failure<List<PromptVersionHistoryDto>>(
                new Error("PromptRegistry.VersionHistoryError", "An error occurred while retrieving version history"));
        }
    }

    public async Task<Result> RollbackToVersionAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var template = await _promptRepository.GetByIdAsync(id, ct);
            if (template == null)
            {
                return Result.Failure(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {id} not found"));
            }

            // Activate this version (which will deactivate others)
            await _promptRepository.ActivateAsync(id, ct);
            _logger.LogInformation("Rolled back to version {Version} of prompt {PromptId}", template.Version, id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back to prompt {PromptId}", id);
            return Result.Failure(
                new Error("PromptRegistry.RollbackError", "An error occurred while rolling back to this version"));
        }
    }

    #endregion

    #region Activation & Deployment

    public async Task<Result> ActivateAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var template = await _promptRepository.GetByIdAsync(id, ct);
            if (template == null)
            {
                return Result.Failure(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {id} not found"));
            }

            await _promptRepository.ActivateAsync(id, ct);
            _logger.LogInformation("Activated prompt template {PromptId}", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating prompt {PromptId}", id);
            return Result.Failure(
                new Error("PromptRegistry.ActivateError", "An error occurred while activating the prompt template"));
        }
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var template = await _promptRepository.GetByIdAsync(id, ct);
            if (template == null)
            {
                return Result.Failure(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {id} not found"));
            }

            template.Deactivate();
            await _promptRepository.UpdateAsync(template, ct);
            _logger.LogInformation("Deactivated prompt template {PromptId}", id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating prompt {PromptId}", id);
            return Result.Failure(
                new Error("PromptRegistry.DeactivateError", "An error occurred while deactivating the prompt template"));
        }
    }

    public async Task<Result> SetAliasAsync(Guid id, PromptAlias? alias, CancellationToken ct = default)
    {
        try
        {
            var template = await _promptRepository.GetByIdAsync(id, ct);
            if (template == null)
            {
                return Result.Failure(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {id} not found"));
            }

            if (alias.HasValue)
            {
                await _promptRepository.SetAliasAsync(id, alias.Value, ct);
            }
            else
            {
                template.SetAlias(null);
                await _promptRepository.UpdateAsync(template, ct);
            }

            _logger.LogInformation("Set alias {Alias} for prompt template {PromptId}", alias, id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting alias for prompt {PromptId}", id);
            return Result.Failure(
                new Error("PromptRegistry.AliasError", "An error occurred while setting the alias"));
        }
    }

    #endregion

    #region Testing

    public async Task<Result<PromptTestResultDto>> TestPromptAsync(
        Guid promptId,
        TestPromptRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var template = await _promptRepository.GetByIdAsync(promptId, ct);
            if (template == null)
            {
                return Result.Failure<PromptTestResultDto>(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {promptId} not found"));
            }

            return await ExecutePromptTest(
                template.SystemPrompt,
                template.UserPromptTemplate,
                request.SampleVariables,
                request.Provider,
                request.MaxTokens,
                (float)request.Temperature,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing prompt {PromptId}", promptId);
            return Result.Failure<PromptTestResultDto>(
                new Error("PromptRegistry.TestError", "An error occurred while testing the prompt"));
        }
    }

    public async Task<Result<PromptTestResultDto>> TestDraftPromptAsync(
        TestDraftPromptRequest request,
        CancellationToken ct = default)
    {
        try
        {
            return await ExecutePromptTest(
                request.SystemPrompt,
                request.UserPromptTemplate,
                request.SampleVariables,
                request.Provider,
                request.MaxTokens,
                (float)request.Temperature,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing draft prompt");
            return Result.Failure<PromptTestResultDto>(
                new Error("PromptRegistry.TestError", "An error occurred while testing the draft prompt"));
        }
    }

    private async Task<Result<PromptTestResultDto>> ExecutePromptTest(
        string systemPrompt,
        string userPromptTemplate,
        string sampleVariablesJson,
        string? providerName,
        int maxTokens,
        float temperature,
        CancellationToken ct)
    {
        // Get the AI provider
        IAIService? provider;
        string actualProviderName;

        if (!string.IsNullOrEmpty(providerName))
        {
            var providerType = providerName.ToLower() switch
            {
                "openai" => AIProviderType.OpenAI,
                "claude" => AIProviderType.Claude,
                "gemini" => AIProviderType.Gemini,
                _ => AIProviderType.OpenAI
            };
            provider = _providerFactory.GetProvider(providerType);
            actualProviderName = providerName;
        }
        else
        {
            provider = await _providerFactory.GetActiveProviderAsync();
            actualProviderName = "Active Provider";
        }

        if (provider == null)
        {
            return Result.Failure<PromptTestResultDto>(
                new Error("PromptRegistry.NoProvider", "No AI provider available"));
        }

        // Parse sample variables and render template
        Dictionary<string, object>? variables;
        try
        {
            variables = JsonSerializer.Deserialize<Dictionary<string, object>>(sampleVariablesJson);
        }
        catch
        {
            return Result.Failure<PromptTestResultDto>(
                new Error("PromptRegistry.InvalidVariables", "Sample variables must be valid JSON"));
        }

        var renderedUserPrompt = RenderTemplate(userPromptTemplate, variables ?? new Dictionary<string, object>());

        // Execute the prompt
        var stopwatch = Stopwatch.StartNew();
        string output;
        try
        {
            output = await provider.GenerateContentAsync(
                systemPrompt,
                renderedUserPrompt,
                maxTokens,
                temperature,
                ct);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return Result.Success(new PromptTestResultDto
            {
                RenderedSystemPrompt = systemPrompt,
                RenderedUserPrompt = renderedUserPrompt,
                Output = string.Empty,
                Success = false,
                Error = ex.Message,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                Provider = actualProviderName,
                Model = "Unknown"
            });
        }
        stopwatch.Stop();

        // Estimate tokens (rough estimate: 1 token ≈ 4 characters)
        var inputTokens = (systemPrompt.Length + renderedUserPrompt.Length) / 4;
        var outputTokens = output.Length / 4;

        return Result.Success(new PromptTestResultDto
        {
            RenderedSystemPrompt = systemPrompt,
            RenderedUserPrompt = renderedUserPrompt,
            Output = output,
            TokensUsed = inputTokens + outputTokens,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
            Provider = actualProviderName,
            Model = GetModelNameFromProvider(provider),
            Success = true
        });
    }

    private static string RenderTemplate(string template, Dictionary<string, object> variables)
    {
        var result = template;
        foreach (var (key, value) in variables)
        {
            // Replace {{key}}, {key}, and ${key} patterns
            result = Regex.Replace(result, @"\{\{" + Regex.Escape(key) + @"\}\}", value?.ToString() ?? string.Empty);
            result = Regex.Replace(result, @"\{" + Regex.Escape(key) + @"\}", value?.ToString() ?? string.Empty);
            result = Regex.Replace(result, @"\$\{" + Regex.Escape(key) + @"\}", value?.ToString() ?? string.Empty);
        }
        return result;
    }

    private static string GetModelNameFromProvider(IAIService provider)
    {
        // Get model name from provider type name
        var typeName = provider.GetType().Name;
        return typeName switch
        {
            "OpenAIService" => "GPT-4o",
            "ClaudeService" => "Claude 3.5 Sonnet",
            "GeminiService" => "Gemini 1.5 Pro",
            _ => "Unknown"
        };
    }

    #endregion

    #region Performance Metrics

    public async Task<Result<PromptPerformanceDto>> GetPerformanceAsync(
        Guid promptId,
        DateTime? startDate = null,
        CancellationToken ct = default)
    {
        try
        {
            var template = await _promptRepository.GetByIdAsync(promptId, ct);
            if (template == null)
            {
                return Result.Failure<PromptPerformanceDto>(
                    new Error("PromptRegistry.NotFound", $"Prompt template with ID {promptId} not found"));
            }

            var metrics = await _promptRepository.GetPerformanceMetricsAsync(promptId, startDate, ct);
            var metricsList = metrics.ToList();

            var dto = new PromptPerformanceDto
            {
                PromptTemplateId = promptId,
                PromptName = template.Name,
                TotalUsageCount = metricsList.Sum(m => m.UsageCount),
                TotalEditCount = metricsList.Sum(m => m.EditCount),
                TotalRegenerateCount = metricsList.Sum(m => m.RegenerateCount),
                TotalAcceptCount = metricsList.Sum(m => m.AcceptCount),
                TotalRatingCount = metricsList.Sum(m => m.RatingCount)
            };

            // Calculate aggregate rates
            if (dto.TotalUsageCount > 0)
            {
                dto.EditRate = (double)dto.TotalEditCount / dto.TotalUsageCount;
                dto.RegenerateRate = (double)dto.TotalRegenerateCount / dto.TotalUsageCount;
                dto.AcceptanceRate = (double)dto.TotalAcceptCount / dto.TotalUsageCount;
            }

            if (dto.TotalRatingCount > 0)
            {
                var totalRating = metricsList.Sum(m => m.TotalRating);
                dto.AverageRating = totalRating / dto.TotalRatingCount;
            }

            // Add period data
            dto.Periods = metricsList.Select(m => new PerformancePeriodDto
            {
                PeriodStart = m.PeriodStart,
                PeriodEnd = m.PeriodEnd,
                UsageCount = m.UsageCount,
                EditCount = m.EditCount,
                RegenerateCount = m.RegenerateCount,
                AcceptCount = m.AcceptCount,
                RatingCount = m.RatingCount,
                AverageRating = m.AverageRating,
                EditRate = m.EditRate,
                RegenerateRate = m.RegenerateRate,
                AcceptanceRate = m.AcceptanceRate
            }).ToList();

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance for prompt {PromptId}", promptId);
            return Result.Failure<PromptPerformanceDto>(
                new Error("PromptRegistry.PerformanceError", "An error occurred while retrieving performance metrics"));
        }
    }

    public async Task<Result<PromptPerformanceSummaryDto>> GetPerformanceSummaryAsync(CancellationToken ct = default)
    {
        try
        {
            var allTemplates = await _context.PromptTemplates.ToListAsync(ct);
            var activeTemplates = allTemplates.Where(t => t.IsActive).ToList();

            // Get all performance data for the last 30 days
            var startDate = DateTime.UtcNow.AddDays(-30);
            var allPerformance = await _context.PromptPerformance
                .Where(p => p.PeriodStart >= startDate)
                .ToListAsync(ct);

            var summary = new PromptPerformanceSummaryDto
            {
                TotalPrompts = allTemplates.Count,
                ActivePrompts = activeTemplates.Count,
                TotalUsage = allPerformance.Sum(p => p.UsageCount)
            };

            // Calculate overall rates
            if (summary.TotalUsage > 0)
            {
                var totalEdits = allPerformance.Sum(p => p.EditCount);
                var totalRegenerates = allPerformance.Sum(p => p.RegenerateCount);
                var totalAccepts = allPerformance.Sum(p => p.AcceptCount);
                var totalRatings = allPerformance.Sum(p => p.RatingCount);
                var totalRatingSum = allPerformance.Sum(p => p.TotalRating);

                summary.OverallEditRate = (double)totalEdits / summary.TotalUsage;
                summary.OverallRegenerateRate = (double)totalRegenerates / summary.TotalUsage;
                summary.OverallAcceptanceRate = (double)totalAccepts / summary.TotalUsage;

                if (totalRatings > 0)
                {
                    summary.OverallAverageRating = totalRatingSum / totalRatings;
                }
            }

            // Get top performers
            var promptPerformance = allPerformance
                .GroupBy(p => p.PromptTemplateId)
                .Select(g => new
                {
                    PromptId = g.Key,
                    UsageCount = g.Sum(p => p.UsageCount),
                    AcceptCount = g.Sum(p => p.AcceptCount),
                    RatingCount = g.Sum(p => p.RatingCount),
                    TotalRating = g.Sum(p => p.TotalRating)
                })
                .ToList();

            // Most used prompts
            summary.MostUsedPrompts = promptPerformance
                .OrderByDescending(p => p.UsageCount)
                .Take(5)
                .Select(p =>
                {
                    var template = allTemplates.FirstOrDefault(t => t.Id == p.PromptId);
                    return new TopPerformerDto
                    {
                        Id = p.PromptId,
                        Name = template?.Name ?? "Unknown",
                        SectionTypeName = template?.SectionType.ToString() ?? "Unknown",
                        PlanTypeName = template?.PlanType.ToString() ?? "Unknown",
                        UsageCount = p.UsageCount,
                        AverageRating = p.RatingCount > 0 ? p.TotalRating / p.RatingCount : 0,
                        AcceptanceRate = p.UsageCount > 0 ? (double)p.AcceptCount / p.UsageCount : 0
                    };
                })
                .ToList();

            // Highest rated prompts
            summary.HighestRatedPrompts = promptPerformance
                .Where(p => p.RatingCount >= 5) // At least 5 ratings
                .OrderByDescending(p => p.TotalRating / p.RatingCount)
                .Take(5)
                .Select(p =>
                {
                    var template = allTemplates.FirstOrDefault(t => t.Id == p.PromptId);
                    return new TopPerformerDto
                    {
                        Id = p.PromptId,
                        Name = template?.Name ?? "Unknown",
                        SectionTypeName = template?.SectionType.ToString() ?? "Unknown",
                        PlanTypeName = template?.PlanType.ToString() ?? "Unknown",
                        UsageCount = p.UsageCount,
                        AverageRating = p.RatingCount > 0 ? p.TotalRating / p.RatingCount : 0,
                        AcceptanceRate = p.UsageCount > 0 ? (double)p.AcceptCount / p.UsageCount : 0
                    };
                })
                .ToList();

            // Top performing (by acceptance rate)
            summary.TopPerformingPrompts = promptPerformance
                .Where(p => p.UsageCount >= 10) // At least 10 uses
                .OrderByDescending(p => (double)p.AcceptCount / p.UsageCount)
                .Take(5)
                .Select(p =>
                {
                    var template = allTemplates.FirstOrDefault(t => t.Id == p.PromptId);
                    return new TopPerformerDto
                    {
                        Id = p.PromptId,
                        Name = template?.Name ?? "Unknown",
                        SectionTypeName = template?.SectionType.ToString() ?? "Unknown",
                        PlanTypeName = template?.PlanType.ToString() ?? "Unknown",
                        UsageCount = p.UsageCount,
                        AverageRating = p.RatingCount > 0 ? p.TotalRating / p.RatingCount : 0,
                        AcceptanceRate = p.UsageCount > 0 ? (double)p.AcceptCount / p.UsageCount : 0
                    };
                })
                .ToList();

            // Usage trends (last 30 days, grouped by day)
            summary.UsageTrends = allPerformance
                .GroupBy(p => p.PeriodStart.Date)
                .OrderBy(g => g.Key)
                .Select(g => new UsageTrendDto
                {
                    Date = g.Key,
                    UsageCount = g.Sum(p => p.UsageCount),
                    AcceptCount = g.Sum(p => p.AcceptCount),
                    EditCount = g.Sum(p => p.EditCount),
                    RegenerateCount = g.Sum(p => p.RegenerateCount)
                })
                .ToList();

            // Performance by section
            summary.PerformanceBySection = allTemplates
                .GroupBy(t => t.SectionType)
                .Select(g =>
                {
                    var sectionPerf = promptPerformance
                        .Where(p => g.Any(t => t.Id == p.PromptId))
                        .ToList();

                    var totalUsage = sectionPerf.Sum(p => p.UsageCount);
                    var totalAccept = sectionPerf.Sum(p => p.AcceptCount);
                    var totalRating = sectionPerf.Sum(p => p.TotalRating);
                    var totalRatingCount = sectionPerf.Sum(p => p.RatingCount);

                    return new SectionPerformanceDto
                    {
                        SectionType = g.Key.ToString(),
                        SectionTypeName = g.Key.ToString(),
                        PromptCount = g.Count(),
                        UsageCount = totalUsage,
                        AverageRating = totalRatingCount > 0 ? totalRating / totalRatingCount : 0,
                        AcceptanceRate = totalUsage > 0 ? (double)totalAccept / totalUsage : 0
                    };
                })
                .OrderByDescending(s => s.UsageCount)
                .ToList();

            return Result.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance summary");
            return Result.Failure<PromptPerformanceSummaryDto>(
                new Error("PromptRegistry.SummaryError", "An error occurred while retrieving the performance summary"));
        }
    }

    #endregion

    #region Private Helpers

    private async Task<PromptTemplateDto> MapToDto(PromptTemplate template, CancellationToken ct)
    {
        var metrics = await GetAggregatedMetrics(template.Id, ct);

        return new PromptTemplateDto
        {
            Id = template.Id,
            SectionType = template.SectionType,
            SectionTypeName = template.SectionType.ToString(),
            PlanType = template.PlanType,
            PlanTypeName = template.PlanType.ToString(),
            IndustryCategory = template.IndustryCategory,
            Version = template.Version,
            IsActive = template.IsActive,
            Alias = template.Alias,
            AliasName = template.Alias?.ToString(),
            Name = template.Name,
            Description = template.Description,
            SystemPrompt = template.SystemPrompt,
            UserPromptTemplate = template.UserPromptTemplate,
            OutputFormat = template.OutputFormat,
            OutputFormatName = template.OutputFormat.ToString(),
            VisualElementsJson = template.VisualElementsJson,
            ExampleOutput = template.ExampleOutput,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            CreatedBy = template.CreatedBy,
            TotalUsageCount = metrics.usageCount,
            AverageRating = metrics.avgRating,
            AcceptanceRate = metrics.acceptanceRate,
            EditRate = metrics.editRate,
            RegenerateRate = metrics.regenerateRate,
            RatingCount = metrics.ratingCount
        };
    }

    private async Task<PromptTemplateListDto> MapToListDto(PromptTemplate template, CancellationToken ct)
    {
        var metrics = await GetAggregatedMetrics(template.Id, ct);

        return new PromptTemplateListDto
        {
            Id = template.Id,
            SectionType = template.SectionType,
            SectionTypeName = template.SectionType.ToString(),
            PlanType = template.PlanType,
            PlanTypeName = template.PlanType.ToString(),
            IndustryCategory = template.IndustryCategory,
            Version = template.Version,
            IsActive = template.IsActive,
            Alias = template.Alias,
            AliasName = template.Alias?.ToString(),
            Name = template.Name,
            Description = template.Description,
            OutputFormat = template.OutputFormat,
            OutputFormatName = template.OutputFormat.ToString(),
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            CreatedBy = template.CreatedBy,
            TotalUsageCount = metrics.usageCount,
            AverageRating = metrics.avgRating,
            AcceptanceRate = metrics.acceptanceRate
        };
    }

    private async Task<(int usageCount, double avgRating, double acceptanceRate, double editRate, double regenerateRate, int ratingCount)> GetAggregatedMetrics(
        Guid promptId,
        CancellationToken ct)
    {
        var metrics = await _context.PromptPerformance
            .Where(p => p.PromptTemplateId == promptId)
            .ToListAsync(ct);

        if (!metrics.Any())
        {
            return (0, 0, 0, 0, 0, 0);
        }

        var totalUsage = metrics.Sum(m => m.UsageCount);
        var totalAccept = metrics.Sum(m => m.AcceptCount);
        var totalEdit = metrics.Sum(m => m.EditCount);
        var totalRegenerate = metrics.Sum(m => m.RegenerateCount);
        var totalRating = metrics.Sum(m => m.TotalRating);
        var ratingCount = metrics.Sum(m => m.RatingCount);

        return (
            totalUsage,
            ratingCount > 0 ? totalRating / ratingCount : 0,
            totalUsage > 0 ? (double)totalAccept / totalUsage : 0,
            totalUsage > 0 ? (double)totalEdit / totalUsage : 0,
            totalUsage > 0 ? (double)totalRegenerate / totalUsage : 0,
            ratingCount
        );
    }

    #endregion
}

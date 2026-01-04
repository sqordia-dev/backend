using Microsoft.EntityFrameworkCore;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Responses.Admin;
using Sqordia.Domain.Entities;
using System.Text.Json;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service for managing AI prompt templates
/// </summary>
public class AIPromptService : IAIPromptService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly ICurrentUserService _currentUserService;

    public AIPromptService(
        IApplicationDbContext context,
        IAIService aiService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _aiService = aiService;
        _currentUserService = currentUserService;
    }

    public async Task<string> CreatePromptAsync(CreateAIPromptRequest request, CancellationToken cancellationToken = default)
    {
        var prompt = new AIPrompt(
            request.Name,
            request.Description,
            request.Category,
            request.PlanType,
            request.Language,
            request.SystemPrompt,
            request.UserPromptTemplate,
            request.Variables,
            request.Notes);

        _context.AIPrompts.Add(prompt);
        await _context.SaveChangesAsync(cancellationToken);

        return prompt.Id.ToString();
    }

    public async Task<bool> UpdatePromptAsync(string promptId, UpdateAIPromptRequest request, CancellationToken cancellationToken = default)
    {
        var prompt = await _context.AIPrompts
            .FirstOrDefaultAsync(p => p.Id.ToString() == promptId, cancellationToken);

        if (prompt == null)
            return false;

        if (!string.IsNullOrEmpty(request.Name) || !string.IsNullOrEmpty(request.Description))
        {
            prompt.UpdateMetadata(
                request.Name ?? prompt.Name,
                request.Description ?? prompt.Description,
                request.Notes);
        }

        if (!string.IsNullOrEmpty(request.SystemPrompt) || !string.IsNullOrEmpty(request.UserPromptTemplate))
        {
            prompt.UpdateContent(
                request.SystemPrompt ?? prompt.SystemPrompt,
                request.UserPromptTemplate ?? prompt.UserPromptTemplate,
                request.Notes);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                prompt.Activate();
            else
                prompt.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AIPromptDto?> GetPromptAsync(string promptId, CancellationToken cancellationToken = default)
    {
        var prompt = await _context.AIPrompts
            .FirstOrDefaultAsync(p => p.Id.ToString() == promptId, cancellationToken);

        if (prompt == null)
            return null;

        return MapToDto(prompt);
    }

    public async Task<List<AIPromptDto>> GetPromptsAsync(
        string? category = null,
        string? planType = null,
        string? language = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AIPrompts.AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrEmpty(planType))
            query = query.Where(p => p.PlanType == planType);

        if (!string.IsNullOrEmpty(language))
            query = query.Where(p => p.Language == language);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var prompts = await query
            .OrderByDescending(p => p.Created)
            .ToListAsync(cancellationToken);

        return prompts.Select(MapToDto).ToList();
    }

    public async Task<bool> DeletePromptAsync(string promptId, CancellationToken cancellationToken = default)
    {
        var prompt = await _context.AIPrompts
            .FirstOrDefaultAsync(p => p.Id.ToString() == promptId, cancellationToken);

        if (prompt == null)
            return false;

        _context.AIPrompts.Remove(prompt);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AIPromptTestResult> TestPromptAsync(TestAIPromptRequest request, CancellationToken cancellationToken = default)
    {
        var prompt = await _context.AIPrompts
            .FirstOrDefaultAsync(p => p.Id.ToString() == request.PromptId, cancellationToken);

        if (prompt == null)
            throw new ArgumentException("Prompt not found");

        try
        {
            var startTime = DateTime.UtcNow;
            
            // Parse sample variables
            var variables = JsonSerializer.Deserialize<Dictionary<string, string>>(request.SampleVariables) ?? new Dictionary<string, string>();
            
            // Build the user prompt by replacing variables in the template
            var userPrompt = prompt.UserPromptTemplate;
            foreach (var variable in variables)
            {
                userPrompt = userPrompt.Replace($"{{{variable.Key}}}", variable.Value);
            }

            // Generate content using the AI service
            var generatedContent = await _aiService.GenerateContentAsync(
                prompt.SystemPrompt,
                userPrompt,
                request.MaxTokens,
                (float)request.Temperature,
                cancellationToken);

            var endTime = DateTime.UtcNow;
            var responseTime = endTime - startTime;

            // Record usage
            prompt.RecordUsage();
            await _context.SaveChangesAsync(cancellationToken);

            return new AIPromptTestResult
            {
                PromptId = request.PromptId,
                TestInput = userPrompt,
                GeneratedOutput = generatedContent,
                TokensUsed = request.MaxTokens,
                Temperature = request.Temperature,
                TestedAt = endTime,
                Model = "gpt-4", // This should come from the AI service
                ResponseTime = responseTime
            };
        }
        catch (Exception ex)
        {
            return new AIPromptTestResult
            {
                PromptId = request.PromptId,
                TestInput = request.SampleVariables,
                GeneratedOutput = string.Empty,
                TokensUsed = 0,
                Temperature = request.Temperature,
                TestedAt = DateTime.UtcNow,
                Model = "gpt-4",
                ResponseTime = TimeSpan.Zero,
                Error = ex.Message
            };
        }
    }

    public async Task<List<AIPromptStats>> GetPromptStatsAsync(CancellationToken cancellationToken = default)
    {
        var prompts = await _context.AIPrompts
            .GroupBy(p => p.Name)
            .Select(g => new AIPromptStats
            {
                PromptId = g.First().Id.ToString(),
                Name = g.Key,
                TotalUsage = g.Sum(p => p.UsageCount),
                AverageRating = g.Average(p => p.AverageRating),
                RatingCount = g.Sum(p => p.RatingCount),
                LastUsedAt = g.Max(p => p.LastUsedAt),
                ActiveVersions = g.Count(p => p.IsActive),
                TotalVersions = g.Count(),
                MostUsedLanguage = g.GroupBy(p => p.Language).OrderByDescending(l => l.Count()).First().Key,
                MostUsedPlanType = g.GroupBy(p => p.PlanType).OrderByDescending(t => t.Count()).First().Key
            })
            .ToListAsync(cancellationToken);

        return prompts;
    }

    public async Task<bool> TogglePromptStatusAsync(string promptId, bool isActive, CancellationToken cancellationToken = default)
    {
        var prompt = await _context.AIPrompts
            .FirstOrDefaultAsync(p => p.Id.ToString() == promptId, cancellationToken);

        if (prompt == null)
            return false;

        if (isActive)
            prompt.Activate();
        else
            prompt.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<string> CreatePromptVersionAsync(string parentPromptId, CreateAIPromptRequest request, CancellationToken cancellationToken = default)
    {
        var parentPrompt = await _context.AIPrompts
            .FirstOrDefaultAsync(p => p.Id.ToString() == parentPromptId, cancellationToken);

        if (parentPrompt == null)
            throw new ArgumentException("Parent prompt not found");

        var newPrompt = new AIPrompt(
            request.Name,
            request.Description,
            request.Category,
            request.PlanType,
            request.Language,
            request.SystemPrompt,
            request.UserPromptTemplate,
            request.Variables,
            request.Notes);

        newPrompt.SetParentPrompt(parentPromptId);
        _context.AIPrompts.Add(newPrompt);
        await _context.SaveChangesAsync(cancellationToken);

        return newPrompt.Id.ToString();
    }

    public async Task<List<AIPromptDto>> GetPromptVersionsAsync(string parentPromptId, CancellationToken cancellationToken = default)
    {
        var prompts = await _context.AIPrompts
            .Where(p => p.ParentPromptId == parentPromptId)
            .OrderByDescending(p => p.Created)
            .ToListAsync(cancellationToken);

        return prompts.Select(MapToDto).ToList();
    }

    private static AIPromptDto MapToDto(AIPrompt prompt)
    {
        return new AIPromptDto
        {
            Id = prompt.Id.ToString(),
            Name = prompt.Name,
            Description = prompt.Description,
            Category = prompt.Category,
            PlanType = prompt.PlanType,
            Language = prompt.Language,
            SystemPrompt = prompt.SystemPrompt,
            UserPromptTemplate = prompt.UserPromptTemplate,
            Variables = prompt.Variables,
            IsActive = prompt.IsActive,
            Version = prompt.Version,
            ParentPromptId = prompt.ParentPromptId,
            Notes = prompt.Notes,
            UsageCount = prompt.UsageCount,
            LastUsedAt = prompt.LastUsedAt,
            AverageRating = prompt.AverageRating,
            RatingCount = prompt.RatingCount,
            CreatedAt = prompt.Created,
            UpdatedAt = prompt.LastModified ?? prompt.Created,
            CreatedBy = prompt.CreatedBy ?? "System",
            UpdatedBy = prompt.LastModifiedBy ?? "System"
        };
    }
}

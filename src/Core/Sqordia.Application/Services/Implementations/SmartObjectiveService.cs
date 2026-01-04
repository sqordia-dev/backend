using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.SmartObjective;
using Sqordia.Contracts.Responses.SmartObjective;
using Sqordia.Domain.Entities.BusinessPlan;
using System.Text;

namespace Sqordia.Application.Services.Implementations;

public class SmartObjectiveService : ISmartObjectiveService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SmartObjectiveService> _logger;

    public SmartObjectiveService(
        IApplicationDbContext context,
        IAIService aiService,
        ICurrentUserService currentUserService,
        ILogger<SmartObjectiveService> logger)
    {
        _context = context;
        _aiService = aiService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<GenerateSmartObjectivesResponse> GenerateSmartObjectivesAsync(
        GenerateSmartObjectivesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating SMART objectives for business plan {BusinessPlanId}", request.BusinessPlanId);

            var businessPlan = await GetBusinessPlanAsync(request.BusinessPlanId, cancellationToken);
            if (businessPlan == null)
            {
                throw new ArgumentException($"Business plan {request.BusinessPlanId} not found or access denied");
            }

            // Build context
            var context = BuildBusinessPlanContext(businessPlan);

            // Generate objectives using AI
            var systemPrompt = GetSmartObjectiveSystemPrompt(request.Language);
            var userPrompt = BuildSmartObjectiveUserPrompt(request, context);

            var aiContent = await _aiService.GenerateContentWithRetryAsync(
                systemPrompt,
                userPrompt,
                maxTokens: 3000,
                temperature: 0.8f,
                maxRetries: 3,
                cancellationToken);

            // Parse and create objectives
            var objectives = ParseAndCreateObjectives(request, aiContent, businessPlan.Id);

            // Save to database
            foreach (var objective in objectives)
            {
                _context.SmartObjectives.Add(objective);
            }
            await _context.SaveChangesAsync(cancellationToken);

            var response = new GenerateSmartObjectivesResponse
            {
                BusinessPlanId = request.BusinessPlanId,
                Objectives = objectives.Select(MapToResponse).ToList(),
                GeneratedAt = DateTime.UtcNow,
                Language = request.Language
            };

            _logger.LogInformation("Generated {Count} SMART objectives for business plan {BusinessPlanId}",
                objectives.Count, request.BusinessPlanId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SMART objectives for business plan {BusinessPlanId}", request.BusinessPlanId);
            throw;
        }
    }

    public async Task<List<SmartObjectiveResponse>> GetObjectivesAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var objectives = await _context.SmartObjectives
            .Where(o => o.BusinessPlanId == businessPlanId)
            .OrderBy(o => o.Priority)
            .ThenBy(o => o.TargetDate)
            .ToListAsync(cancellationToken);

        return objectives.Select(MapToResponse).ToList();
    }

    public async Task<SmartObjectiveResponse> UpdateProgressAsync(
        Guid objectiveId,
        decimal progressPercentage,
        CancellationToken cancellationToken = default)
    {
        var objective = await _context.SmartObjectives
            .FirstOrDefaultAsync(o => o.Id == objectiveId, cancellationToken);

        if (objective == null)
        {
            throw new ArgumentException($"Objective {objectiveId} not found");
        }

        objective.UpdateProgress(progressPercentage);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToResponse(objective);
    }

    private async Task<BusinessPlan?> GetBusinessPlanAsync(Guid businessPlanId, CancellationToken cancellationToken)
    {
        var currentUserIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
        {
            return null;
        }

        var businessPlan = await _context.BusinessPlans
            .FirstOrDefaultAsync(bp => bp.Id == businessPlanId && !bp.IsDeleted, cancellationToken);

        if (businessPlan == null) return null;

        var isMember = await _context.OrganizationMembers
            .AnyAsync(om => om.OrganizationId == businessPlan.OrganizationId &&
                           om.UserId == currentUserId &&
                           om.IsActive, cancellationToken);

        return isMember ? businessPlan : null;
    }

    private string BuildBusinessPlanContext(BusinessPlan businessPlan)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Plan: {businessPlan.Title}");
        if (!string.IsNullOrWhiteSpace(businessPlan.MarketingStrategy))
        {
            sb.AppendLine($"Marketing Strategy: {businessPlan.MarketingStrategy.Substring(0, Math.Min(300, businessPlan.MarketingStrategy.Length))}");
        }
        if (!string.IsNullOrWhiteSpace(businessPlan.FinancialProjections))
        {
            sb.AppendLine($"Financial Projections: {businessPlan.FinancialProjections.Substring(0, Math.Min(300, businessPlan.FinancialProjections.Length))}");
        }
        return sb.ToString();
    }

    private string GetSmartObjectiveSystemPrompt(string language)
    {
        var isFrench = language.ToLower() == "fr";
        return isFrench
            ? @"Vous êtes un expert en planification stratégique. Votre tâche est de générer des objectifs SMART (Spécifiques, Mesurables, Atteignables, Pertinents, Temporels) pour un plan d'affaires.

Chaque objectif doit être:
- Spécifique: Défini clairement et précisément
- Mesurable: Avec des critères quantifiables de succès
- Atteignable: Réaliste et réalisable avec les ressources disponibles
- Pertinent: Aligné avec les objectifs du plan d'affaires
- Temporel: Avec une date cible claire

Générez des objectifs dans différentes catégories (Revenus, Marketing, Opérations, etc.)."
            : @"You are an expert in strategic planning. Your task is to generate SMART (Specific, Measurable, Achievable, Relevant, Time-bound) objectives for a business plan.

Each objective must be:
- Specific: Clearly and precisely defined
- Measurable: With quantifiable success criteria
- Achievable: Realistic and attainable with available resources
- Relevant: Aligned with business plan goals
- Time-bound: With a clear target date

Generate objectives in different categories (Revenue, Marketing, Operations, etc.).";
    }

    private string BuildSmartObjectiveUserPrompt(GenerateSmartObjectivesRequest request, string context)
    {
        var isFrench = request.Language.ToLower() == "fr";
        var sb = new StringBuilder();
        sb.AppendLine(isFrench ? "Contexte du plan d'affaires:" : "Business plan context:");
        sb.AppendLine(context);
        sb.AppendLine();
        sb.AppendLine($"{(isFrench ? "Nombre d'objectifs" : "Number of objectives")}: {request.ObjectiveCount}");
        sb.AppendLine($"{(isFrench ? "Horizon temporel" : "Time horizon")}: {request.TimeHorizonMonths} {(isFrench ? "mois" : "months")}");
        if (request.Categories != null && request.Categories.Any())
        {
            sb.AppendLine($"{(isFrench ? "Catégories" : "Categories")}: {string.Join(", ", request.Categories)}");
        }
        return sb.ToString();
    }

    private List<SmartObjective> ParseAndCreateObjectives(
        GenerateSmartObjectivesRequest request,
        string aiContent,
        Guid businessPlanId)
    {
        var objectives = new List<SmartObjective>();
        var categories = request.Categories ?? new List<string> { "Revenue", "Marketing", "Operations", "Growth", "Financial" };
        var targetDate = DateTime.UtcNow.AddMonths(request.TimeHorizonMonths);
        var isFrench = request.Language.ToLower() == "fr";

        for (int i = 0; i < request.ObjectiveCount; i++)
        {
            var category = categories[i % categories.Count];
            var priority = i < 2 ? 1 : i < 4 ? 2 : 3;

            objectives.Add(new SmartObjective(
                businessPlanId,
                isFrench ? $"Objectif {i + 1}: {category}" : $"Objective {i + 1}: {category}",
                aiContent.Length > 200 ? aiContent.Substring(0, 200) + "..." : aiContent,
                isFrench ? $"Objectif spécifique pour {category}" : $"Specific objective for {category}",
                isFrench ? "Mesurable avec des métriques claires" : "Measurable with clear metrics",
                isFrench ? "Atteignable avec les ressources disponibles" : "Achievable with available resources",
                isFrench ? "Pertinent pour le plan d'affaires" : "Relevant to the business plan",
                isFrench ? $"À compléter d'ici {targetDate:yyyy-MM-dd}" : $"To be completed by {targetDate:yyyy-MM-dd}",
                targetDate.AddDays(i * 30),
                category,
                priority
            ));
        }

        return objectives;
    }

    private SmartObjectiveResponse MapToResponse(SmartObjective objective)
    {
        return new SmartObjectiveResponse
        {
            Id = objective.Id,
            BusinessPlanId = objective.BusinessPlanId,
            Title = objective.Title,
            Description = objective.Description,
            Specific = objective.Specific,
            Measurable = objective.Measurable,
            Achievable = objective.Achievable,
            Relevant = objective.Relevant,
            TimeBound = objective.TimeBound,
            TargetDate = objective.TargetDate,
            CompletedDate = objective.CompletedDate,
            ProgressPercentage = objective.ProgressPercentage,
            Status = objective.Status,
            Category = objective.Category,
            Priority = objective.Priority,
            CreatedAt = objective.Created,
            UpdatedAt = objective.LastModified
        };
    }
}


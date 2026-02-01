using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Persistence.Repositories;

/// <summary>
/// Repository implementation for prompt template management
/// </summary>
public class PromptRepository : IPromptRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PromptRepository> _logger;

    public PromptRepository(
        IApplicationDbContext context,
        ILogger<PromptRepository> logger)
    {
        _context = (ApplicationDbContext)context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PromptTemplate?> GetActivePromptAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        string? industryCategory = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting active prompt for SectionType: {SectionType}, PlanType: {PlanType}, Industry: {Industry}",
            sectionType,
            planType,
            industryCategory ?? "generic");

        var query = _context.PromptTemplates
            .Where(p => p.SectionType == sectionType
                && p.PlanType == planType
                && p.IsActive);

        if (!string.IsNullOrEmpty(industryCategory))
        {
            query = query.Where(p => p.IndustryCategory == industryCategory);
        }
        else
        {
            query = query.Where(p => p.IndustryCategory == null);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PromptTemplate?> GetByAliasAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        PromptAlias alias,
        string? industryCategory = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting prompt by alias {Alias} for SectionType: {SectionType}, PlanType: {PlanType}",
            alias,
            sectionType,
            planType);

        var query = _context.PromptTemplates
            .Where(p => p.SectionType == sectionType
                && p.PlanType == planType
                && p.Alias == alias);

        if (!string.IsNullOrEmpty(industryCategory))
        {
            query = query.Where(p => p.IndustryCategory == industryCategory);
        }
        else
        {
            query = query.Where(p => p.IndustryCategory == null);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PromptTemplate?> GetByVersionAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        int version,
        string? industryCategory = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting prompt version {Version} for SectionType: {SectionType}, PlanType: {PlanType}",
            version,
            sectionType,
            planType);

        var query = _context.PromptTemplates
            .Where(p => p.SectionType == sectionType
                && p.PlanType == planType
                && p.Version == version);

        if (!string.IsNullOrEmpty(industryCategory))
        {
            query = query.Where(p => p.IndustryCategory == industryCategory);
        }
        else
        {
            query = query.Where(p => p.IndustryCategory == null);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PromptTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting prompt by ID: {PromptId}", id);
        return await _context.PromptTemplates.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PromptTemplate>> GetAllForSectionAsync(
        SectionType sectionType,
        BusinessPlanType planType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting all prompts for SectionType: {SectionType}, PlanType: {PlanType}",
            sectionType,
            planType);

        return await _context.PromptTemplates
            .Where(p => p.SectionType == sectionType && p.PlanType == planType)
            .OrderByDescending(p => p.Version)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PromptTemplate> CreateAsync(PromptTemplate template, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Creating new prompt template for SectionType: {SectionType}, PlanType: {PlanType}",
            template.SectionType,
            template.PlanType);

        await _context.PromptTemplates.AddAsync(template, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created prompt template with ID: {PromptId}", template.Id);
        return template;
    }

    /// <inheritdoc />
    public async Task<PromptTemplate> UpdateAsync(PromptTemplate template, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating prompt template ID: {PromptId}", template.Id);

        _context.PromptTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated prompt template ID: {PromptId}", template.Id);
        return template;
    }

    /// <inheritdoc />
    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating prompt template ID: {PromptId}", id);

        var template = await _context.PromptTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template == null)
        {
            _logger.LogWarning("Prompt template not found: {PromptId}", id);
            throw new InvalidOperationException($"Prompt template with ID {id} not found");
        }

        // Deactivate other prompts for the same section/plan type/industry combination
        var existingActive = await _context.PromptTemplates
            .Where(p => p.SectionType == template.SectionType
                && p.PlanType == template.PlanType
                && p.IndustryCategory == template.IndustryCategory
                && p.IsActive
                && p.Id != id)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingActive)
        {
            existing.Deactivate();
            _logger.LogDebug("Deactivated prompt template ID: {PromptId}", existing.Id);
        }

        template.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Activated prompt template ID: {PromptId} for SectionType: {SectionType}",
            id,
            template.SectionType);
    }

    /// <inheritdoc />
    public async Task SetAliasAsync(Guid id, PromptAlias alias, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting alias {Alias} for prompt template ID: {PromptId}", alias, id);

        var template = await _context.PromptTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template == null)
        {
            _logger.LogWarning("Prompt template not found: {PromptId}", id);
            throw new InvalidOperationException($"Prompt template with ID {id} not found");
        }

        // Remove alias from other prompts with the same section/plan type/industry/alias combination
        var existingWithAlias = await _context.PromptTemplates
            .Where(p => p.SectionType == template.SectionType
                && p.PlanType == template.PlanType
                && p.IndustryCategory == template.IndustryCategory
                && p.Alias == alias
                && p.Id != id)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingWithAlias)
        {
            existing.SetAlias(null);
            _logger.LogDebug("Removed alias from prompt template ID: {PromptId}", existing.Id);
        }

        template.SetAlias(alias);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Set alias {Alias} for prompt template ID: {PromptId}",
            alias,
            id);
    }

    /// <inheritdoc />
    public async Task RecordUsageAsync(
        Guid promptId,
        UsageType usageType,
        int? rating = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Recording usage {UsageType} for prompt ID: {PromptId}",
            usageType,
            promptId);

        // Get or create current period performance record (monthly periods)
        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1).AddSeconds(-1);

        var performance = await _context.PromptPerformance
            .FirstOrDefaultAsync(p => p.PromptTemplateId == promptId
                && p.PeriodStart == periodStart,
                cancellationToken);

        if (performance == null)
        {
            performance = new PromptPerformance(promptId, periodStart, periodEnd);
            await _context.PromptPerformance.AddAsync(performance, cancellationToken);
        }

        // Record the appropriate metric
        switch (usageType)
        {
            case UsageType.Generated:
                performance.RecordUsage();
                break;
            case UsageType.Edited:
                performance.RecordEdit();
                break;
            case UsageType.Regenerated:
                performance.RecordRegenerate();
                break;
            case UsageType.Accepted:
                performance.RecordAccept();
                break;
            case UsageType.Rated:
                if (rating.HasValue)
                {
                    performance.RecordRating(rating.Value);
                }
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Recorded {UsageType} for prompt ID: {PromptId}",
            usageType,
            promptId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PromptPerformance>> GetPerformanceMetricsAsync(
        Guid promptId,
        DateTime? startDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting performance metrics for prompt ID: {PromptId}, StartDate: {StartDate}",
            promptId,
            startDate?.ToString("yyyy-MM-dd") ?? "all time");

        var query = _context.PromptPerformance
            .Where(p => p.PromptTemplateId == promptId);

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PeriodStart >= startDate.Value);
        }

        return await query
            .OrderByDescending(p => p.PeriodStart)
            .ToListAsync(cancellationToken);
    }
}

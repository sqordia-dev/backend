using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Service implementation for selecting and building prompts
/// </summary>
public class PromptSelectorService : IPromptSelectorService
{
    private readonly IPromptRepository _repository;
    private readonly ILogger<PromptSelectorService> _logger;

    public PromptSelectorService(
        IPromptRepository repository,
        ILogger<PromptSelectorService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<PromptTemplate>> SelectPromptAsync(
        PromptSelectionContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Selecting prompt for SectionType: {SectionType}, PlanType: {PlanType}, IndustryCategory: {IndustryCategory}",
                context.SectionType,
                context.PlanType,
                context.IndustryCategory ?? "generic");

            // Priority order:
            // 1. Industry-specific + plan type match
            // 2. Generic + plan type match
            // 3. Generic fallback (any plan type)

            PromptTemplate? prompt = null;

            // Try industry-specific first
            if (!string.IsNullOrEmpty(context.IndustryCategory))
            {
                prompt = await _repository.GetActivePromptAsync(
                    context.SectionType,
                    context.PlanType,
                    context.IndustryCategory,
                    cancellationToken);

                if (prompt != null)
                {
                    _logger.LogDebug(
                        "Found industry-specific prompt {PromptId} for {IndustryCategory}",
                        prompt.Id,
                        context.IndustryCategory);
                }
            }

            // Fallback to generic for the plan type
            if (prompt == null)
            {
                prompt = await _repository.GetActivePromptAsync(
                    context.SectionType,
                    context.PlanType,
                    null,
                    cancellationToken);

                if (prompt != null)
                {
                    _logger.LogDebug(
                        "Found generic prompt {PromptId} for PlanType {PlanType}",
                        prompt.Id,
                        context.PlanType);
                }
            }

            // Fallback to default business plan type if no match
            if (prompt == null && context.PlanType != BusinessPlanType.BusinessPlan)
            {
                prompt = await _repository.GetActivePromptAsync(
                    context.SectionType,
                    BusinessPlanType.BusinessPlan,
                    null,
                    cancellationToken);

                if (prompt != null)
                {
                    _logger.LogDebug(
                        "Found fallback prompt {PromptId} for default BusinessPlan type",
                        prompt.Id);
                }
            }

            if (prompt == null)
            {
                _logger.LogWarning(
                    "No prompt template found for SectionType: {SectionType}, PlanType: {PlanType}",
                    context.SectionType,
                    context.PlanType);

                return Result.Failure<PromptTemplate>(
                    Error.NotFound(
                        "Prompt.NotFound",
                        $"No prompt template found for section '{context.SectionType}' and plan type '{context.PlanType}'"));
            }

            _logger.LogInformation(
                "Selected prompt {PromptId} (Version: {Version}) for {SectionType}",
                prompt.Id,
                prompt.Version,
                context.SectionType);

            return Result.Success(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error selecting prompt for SectionType: {SectionType}, PlanType: {PlanType}",
                context.SectionType,
                context.PlanType);

            return Result.Failure<PromptTemplate>(
                Error.Failure("Prompt.SelectionError", "An error occurred while selecting the prompt template"));
        }
    }

    /// <inheritdoc />
    public string BuildPrompt(PromptTemplate template, Dictionary<string, string> variables)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));

        if (variables == null || variables.Count == 0)
            return template.UserPromptTemplate;

        var prompt = template.UserPromptTemplate;

        foreach (var (key, value) in variables)
        {
            // Support both {{key}} and {key} formats
            prompt = prompt.Replace($"{{{{{key}}}}}", value ?? string.Empty);
            prompt = prompt.Replace($"{{{key}}}", value ?? string.Empty);
        }

        _logger.LogDebug(
            "Built prompt from template {PromptId} with {VariableCount} variables",
            template.Id,
            variables.Count);

        return prompt;
    }

    /// <inheritdoc />
    public async Task<Result<PromptTemplate>> GetPromptByAliasAsync(
        PromptSelectionContext context,
        PromptAlias alias,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Getting prompt by alias {Alias} for SectionType: {SectionType}, PlanType: {PlanType}",
                alias,
                context.SectionType,
                context.PlanType);

            var prompt = await _repository.GetByAliasAsync(
                context.SectionType,
                context.PlanType,
                alias,
                context.IndustryCategory,
                cancellationToken);

            if (prompt == null)
            {
                _logger.LogWarning(
                    "No prompt template found for alias {Alias}, SectionType: {SectionType}",
                    alias,
                    context.SectionType);

                return Result.Failure<PromptTemplate>(
                    Error.NotFound(
                        "Prompt.AliasNotFound",
                        $"No prompt template found for alias '{alias}' and section '{context.SectionType}'"));
            }

            _logger.LogInformation(
                "Found prompt {PromptId} for alias {Alias}",
                prompt.Id,
                alias);

            return Result.Success(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting prompt by alias {Alias} for SectionType: {SectionType}",
                alias,
                context.SectionType);

            return Result.Failure<PromptTemplate>(
                Error.Failure("Prompt.AliasError", "An error occurred while getting the prompt by alias"));
        }
    }
}

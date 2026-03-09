using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Multi-pass generation pipeline that orchestrates:
/// Pass 1: Analysis (generates a generation plan from the Business Brief)
/// Pass 2: Section Generation (dependency-ordered, parallel within tiers)
/// Pass 3: Review &amp; Synthesis (coherence check, executive summary synthesis, quality scoring)
/// </summary>
public interface IGenerationPipelineService
{
    /// <summary>
    /// Executes the full 3-pass generation pipeline for a business plan.
    /// </summary>
    Task<Result<Domain.Entities.BusinessPlan.BusinessPlan>> ExecutePipelineAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default);
}

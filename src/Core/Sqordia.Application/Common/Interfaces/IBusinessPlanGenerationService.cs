using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Service for AI-powered business plan content generation
/// </summary>
public interface IBusinessPlanGenerationService
{
    /// <summary>
    /// Generates all sections of a business plan based on questionnaire responses
    /// </summary>
    /// <param name="businessPlanId">The ID of the business plan to generate</param>
    /// <param name="language">Language for generation (fr or en)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated business plan with generated content</returns>
    Task<Result<BusinessPlan>> GenerateBusinessPlanAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Regenerates a specific section of a business plan
    /// </summary>
    /// <param name="businessPlanId">The ID of the business plan</param>
    /// <param name="sectionName">The name of the section to regenerate</param>
    /// <param name="language">Language for generation (fr or en)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated business plan</returns>
    Task<Result<BusinessPlan>> RegenerateSectionAsync(
        Guid businessPlanId,
        string sectionName,
        string language = "fr",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the generation status of a business plan
    /// </summary>
    /// <param name="businessPlanId">The ID of the business plan</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generation status information</returns>
    Task<Result<BusinessPlanGenerationStatus>> GetGenerationStatusAsync(
        Guid businessPlanId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the list of available sections that can be generated for a business plan type
    /// </summary>
    /// <param name="planType">The type of business plan</param>
    /// <returns>List of section names</returns>
    List<string> GetAvailableSections(string planType);
}

/// <summary>
/// Business plan generation status
/// </summary>
public class BusinessPlanGenerationStatus
{
    public Guid BusinessPlanId { get; set; }
    public string Status { get; set; } = string.Empty; // Draft, Generating, Generated, Failed
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalSections { get; set; }
    public int CompletedSections { get; set; }
    public decimal CompletionPercentage { get; set; }
    public string? CurrentSection { get; set; }
    public string? ErrorMessage { get; set; }
}


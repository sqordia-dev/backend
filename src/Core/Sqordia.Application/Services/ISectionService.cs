using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;

namespace Sqordia.Application.Services;

/// <summary>
/// Service interface for business plan section management
/// </summary>
public interface ISectionService
{
    /// <summary>
    /// Get all sections for a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All sections with metadata and completion status</returns>
    Task<BusinessPlanSectionsDto> GetSectionsAsync(Guid businessPlanId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific section of a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionName">The section name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Specific section with content and metadata</returns>
    Task<BusinessPlanSectionDto> GetSectionAsync(Guid businessPlanId, string sectionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a specific section of a business plan
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="sectionName">The section name</param>
    /// <param name="request">The section update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated section with new metadata</returns>
    Task<BusinessPlanSectionDto> UpdateSectionAsync(Guid businessPlanId, string sectionName, UpdateSectionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate that a section name is valid for the given plan type
    /// </summary>
    /// <param name="sectionName">The section name to validate</param>
    /// <param name="planType">The business plan type</param>
    /// <returns>True if the section name is valid for the plan type</returns>
    bool IsValidSectionName(string sectionName, string planType);

    /// <summary>
    /// Get available section names for a specific plan type
    /// </summary>
    /// <param name="planType">The business plan type</param>
    /// <returns>List of available section names</returns>
    List<string> GetAvailableSectionNames(string planType);

    /// <summary>
    /// Get section metadata for a specific plan type
    /// </summary>
    /// <param name="planType">The business plan type</param>
    /// <returns>Dictionary of section names to metadata</returns>
    Dictionary<string, SectionMetadata> GetSectionMetadata(string planType);
}

/// <summary>
/// Metadata for a business plan section
/// </summary>
public class SectionMetadata
{
    public required string Name { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required bool IsRequired { get; set; }
    public required int Order { get; set; }
    public required List<string> Tags { get; set; }
    public required string Category { get; set; }
}

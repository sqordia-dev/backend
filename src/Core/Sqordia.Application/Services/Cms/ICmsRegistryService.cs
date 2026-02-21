using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Cms;

namespace Sqordia.Application.Services.Cms;

/// <summary>
/// Service for managing the CMS page/section/block registry.
/// Provides database-driven registry access with fallback to static registry.
/// </summary>
public interface ICmsRegistryService
{
    /// <summary>
    /// Gets all active pages with their sections.
    /// Falls back to static registry if database is not available.
    /// </summary>
    Task<Result<List<CmsPageRegistryResponse>>> GetAllPagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific page by key with its sections and block definitions.
    /// </summary>
    Task<Result<CmsPageDetailResponse>> GetPageAsync(string pageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sections for a specific page.
    /// </summary>
    Task<Result<List<CmsSectionResponse>>> GetSectionsAsync(string pageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all block definitions for a specific section.
    /// </summary>
    Task<Result<List<CmsBlockDefinitionResponse>>> GetBlockDefinitionsAsync(string sectionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a section key is valid (exists in the registry).
    /// </summary>
    Task<Result<bool>> IsValidSectionKeyAsync(string sectionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the database registry from the static registry if the database is empty.
    /// </summary>
    Task<Result> SeedFromStaticRegistryAsync(CancellationToken cancellationToken = default);
}

using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.SectionHierarchy;
using Sqordia.Contracts.Responses.Admin.SectionHierarchy;

namespace Sqordia.Application.Services.V3;

/// <summary>
/// Service for managing the section hierarchy (main sections and sub-sections)
/// </summary>
public interface ISectionHierarchyService
{
    // Main Sections
    Task<Result<List<MainSectionResponse>>> GetAllMainSectionsAsync(CancellationToken cancellationToken = default);
    Task<Result<List<MainSectionListResponse>>> GetMainSectionListAsync(CancellationToken cancellationToken = default);
    Task<Result<MainSectionResponse>> GetMainSectionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<MainSectionResponse>> GetMainSectionByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateMainSectionAsync(CreateMainSectionRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateMainSectionAsync(Guid id, UpdateMainSectionRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteMainSectionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> ReorderMainSectionsAsync(ReorderSectionsRequest request, CancellationToken cancellationToken = default);

    // Sub-Sections
    Task<Result<List<SubSectionResponse>>> GetSubSectionsAsync(Guid mainSectionId, CancellationToken cancellationToken = default);
    Task<Result<SubSectionResponse>> GetSubSectionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<SubSectionResponse>> GetSubSectionByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateSubSectionAsync(Guid mainSectionId, CreateSubSectionRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateSubSectionAsync(Guid id, UpdateSubSectionRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteSubSectionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> ReorderSubSectionsAsync(Guid mainSectionId, ReorderSectionsRequest request, CancellationToken cancellationToken = default);

    // Utilities
    Task<Result<bool>> MainSectionExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> SubSectionExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

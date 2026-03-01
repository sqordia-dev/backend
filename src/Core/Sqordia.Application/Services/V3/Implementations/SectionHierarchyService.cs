using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.SectionHierarchy;
using Sqordia.Contracts.Responses.Admin.SectionHierarchy;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Application.Services.V3.Implementations;

/// <summary>
/// Service implementation for managing section hierarchy
/// </summary>
public class SectionHierarchyService : ISectionHierarchyService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SectionHierarchyService> _logger;

    public SectionHierarchyService(
        IApplicationDbContext context,
        ILogger<SectionHierarchyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Main Sections

    public async Task<Result<List<MainSectionResponse>>> GetAllMainSectionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var sections = await _context.MainSections
                .Include(ms => ms.SubSections.Where(ss => ss.IsActive))
                .OrderBy(ms => ms.DisplayOrder)
                .ToListAsync(cancellationToken);

            var response = sections.Select(MapToResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all main sections");
            return Result.Failure<List<MainSectionResponse>>(
                Error.InternalServerError("SectionHierarchy.GetError", "Failed to retrieve main sections"));
        }
    }

    public async Task<Result<List<MainSectionListResponse>>> GetMainSectionListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var sections = await _context.MainSections
                .Include(ms => ms.SubSections)
                .OrderBy(ms => ms.DisplayOrder)
                .ToListAsync(cancellationToken);

            var response = sections.Select(ms => new MainSectionListResponse
            {
                Id = ms.Id,
                Number = ms.Number,
                Code = ms.Code,
                TitleFR = ms.TitleFR,
                TitleEN = ms.TitleEN,
                DisplayOrder = ms.DisplayOrder,
                IsActive = ms.IsActive,
                GeneratedLast = ms.GeneratedLast,
                Icon = ms.Icon,
                SubSectionsCount = ms.SubSections.Count(ss => ss.IsActive)
            }).ToList();

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting main section list");
            return Result.Failure<List<MainSectionListResponse>>(
                Error.InternalServerError("SectionHierarchy.GetError", "Failed to retrieve main sections"));
        }
    }

    public async Task<Result<MainSectionResponse>> GetMainSectionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var section = await _context.MainSections
                .Include(ms => ms.SubSections.Where(ss => ss.IsActive))
                .FirstOrDefaultAsync(ms => ms.Id == id, cancellationToken);

            if (section == null)
            {
                return Result.Failure<MainSectionResponse>(
                    Error.NotFound("SectionHierarchy.NotFound", $"Main section with ID {id} not found"));
            }

            return Result.Success(MapToResponse(section));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting main section {Id}", id);
            return Result.Failure<MainSectionResponse>(
                Error.InternalServerError("SectionHierarchy.GetError", "Failed to retrieve main section"));
        }
    }

    public async Task<Result<MainSectionResponse>> GetMainSectionByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var section = await _context.MainSections
                .Include(ms => ms.SubSections.Where(ss => ss.IsActive))
                .FirstOrDefaultAsync(ms => ms.Code == code, cancellationToken);

            if (section == null)
            {
                return Result.Failure<MainSectionResponse>(
                    Error.NotFound("SectionHierarchy.NotFound", $"Main section with code '{code}' not found"));
            }

            return Result.Success(MapToResponse(section));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting main section by code {Code}", code);
            return Result.Failure<MainSectionResponse>(
                Error.InternalServerError("SectionHierarchy.GetError", "Failed to retrieve main section"));
        }
    }

    public async Task<Result<Guid>> CreateMainSectionAsync(CreateMainSectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for duplicate code
            var exists = await _context.MainSections.AnyAsync(ms => ms.Code == request.Code, cancellationToken);
            if (exists)
            {
                return Result.Failure<Guid>(
                    Error.Conflict("SectionHierarchy.DuplicateCode", $"Main section with code '{request.Code}' already exists"));
            }

            var section = MainSection.Create(
                request.Number,
                request.Code,
                request.TitleFR,
                request.TitleEN,
                request.DescriptionFR,
                request.DescriptionEN,
                request.DisplayOrder,
                request.GeneratedLast,
                request.Icon);

            _context.MainSections.Add(section);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created main section {Code} with ID {Id}", request.Code, section.Id);
            return Result.Success(section.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating main section {Code}", request.Code);
            return Result.Failure<Guid>(
                Error.InternalServerError("SectionHierarchy.CreateError", "Failed to create main section"));
        }
    }

    public async Task<Result> UpdateMainSectionAsync(Guid id, UpdateMainSectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var section = await _context.MainSections.FirstOrDefaultAsync(ms => ms.Id == id, cancellationToken);
            if (section == null)
            {
                return Result.Failure(
                    Error.NotFound("SectionHierarchy.NotFound", $"Main section with ID {id} not found"));
            }

            section.Update(
                request.TitleFR,
                request.TitleEN,
                request.DescriptionFR,
                request.DescriptionEN,
                request.DisplayOrder,
                request.GeneratedLast,
                request.Icon);

            if (!request.IsActive && section.IsActive)
            {
                section.Deactivate();
            }
            else if (request.IsActive && !section.IsActive)
            {
                section.Activate();
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated main section {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating main section {Id}", id);
            return Result.Failure(
                Error.InternalServerError("SectionHierarchy.UpdateError", "Failed to update main section"));
        }
    }

    public async Task<Result> DeleteMainSectionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var section = await _context.MainSections
                .Include(ms => ms.SubSections)
                .FirstOrDefaultAsync(ms => ms.Id == id, cancellationToken);

            if (section == null)
            {
                return Result.Failure(
                    Error.NotFound("SectionHierarchy.NotFound", $"Main section with ID {id} not found"));
            }

            // Soft delete by deactivating
            section.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted (deactivated) main section {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting main section {Id}", id);
            return Result.Failure(
                Error.InternalServerError("SectionHierarchy.DeleteError", "Failed to delete main section"));
        }
    }

    public async Task<Result> ReorderMainSectionsAsync(ReorderSectionsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var sectionIds = request.Items.Select(i => i.Id).ToList();
            var sections = await _context.MainSections
                .Where(ms => sectionIds.Contains(ms.Id))
                .ToListAsync(cancellationToken);

            foreach (var item in request.Items)
            {
                var section = sections.FirstOrDefault(s => s.Id == item.Id);
                section?.SetDisplayOrder(item.DisplayOrder);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reordered {Count} main sections", request.Items.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering main sections");
            return Result.Failure(
                Error.InternalServerError("SectionHierarchy.ReorderError", "Failed to reorder main sections"));
        }
    }

    #endregion

    #region Sub-Sections

    public async Task<Result<List<SubSectionResponse>>> GetSubSectionsAsync(Guid mainSectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subSections = await _context.SubSections
                .Where(ss => ss.MainSectionId == mainSectionId)
                .OrderBy(ss => ss.DisplayOrder)
                .ToListAsync(cancellationToken);

            var response = subSections.Select(MapToSubSectionResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sub-sections for main section {MainSectionId}", mainSectionId);
            return Result.Failure<List<SubSectionResponse>>(
                Error.InternalServerError("SectionHierarchy.GetError", "Failed to retrieve sub-sections"));
        }
    }

    public async Task<Result<SubSectionResponse>> GetSubSectionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var subSection = await _context.SubSections
                .Include(ss => ss.QuestionMappings)
                .Include(ss => ss.Prompts)
                .FirstOrDefaultAsync(ss => ss.Id == id, cancellationToken);

            if (subSection == null)
            {
                return Result.Failure<SubSectionResponse>(
                    Error.NotFound("SectionHierarchy.NotFound", $"Sub-section with ID {id} not found"));
            }

            return Result.Success(MapToSubSectionResponse(subSection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sub-section {Id}", id);
            return Result.Failure<SubSectionResponse>(
                Error.InternalServerError("SectionHierarchy.GetError", "Failed to retrieve sub-section"));
        }
    }

    public async Task<Result<SubSectionResponse>> GetSubSectionByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var subSection = await _context.SubSections
                .Include(ss => ss.QuestionMappings)
                .Include(ss => ss.Prompts)
                .FirstOrDefaultAsync(ss => ss.Code == code, cancellationToken);

            if (subSection == null)
            {
                return Result.Failure<SubSectionResponse>(
                    Error.NotFound("SectionHierarchy.NotFound", $"Sub-section with code '{code}' not found"));
            }

            return Result.Success(MapToSubSectionResponse(subSection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sub-section by code {Code}", code);
            return Result.Failure<SubSectionResponse>(
                Error.InternalServerError("SectionHierarchy.GetError", "Failed to retrieve sub-section"));
        }
    }

    public async Task<Result<Guid>> CreateSubSectionAsync(Guid mainSectionId, CreateSubSectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify main section exists
            var mainExists = await _context.MainSections.AnyAsync(ms => ms.Id == mainSectionId, cancellationToken);
            if (!mainExists)
            {
                return Result.Failure<Guid>(
                    Error.NotFound("SectionHierarchy.NotFound", $"Main section with ID {mainSectionId} not found"));
            }

            // Check for duplicate code
            var exists = await _context.SubSections.AnyAsync(ss => ss.Code == request.Code, cancellationToken);
            if (exists)
            {
                return Result.Failure<Guid>(
                    Error.Conflict("SectionHierarchy.DuplicateCode", $"Sub-section with code '{request.Code}' already exists"));
            }

            var subSection = SubSection.Create(
                mainSectionId,
                request.Code,
                request.TitleFR,
                request.TitleEN,
                request.DescriptionFR,
                request.DescriptionEN,
                request.NoteFR,
                request.NoteEN,
                request.DisplayOrder,
                request.Icon);

            _context.SubSections.Add(subSection);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created sub-section {Code} with ID {Id}", request.Code, subSection.Id);
            return Result.Success(subSection.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sub-section {Code}", request.Code);
            return Result.Failure<Guid>(
                Error.InternalServerError("SectionHierarchy.CreateError", "Failed to create sub-section"));
        }
    }

    public async Task<Result> UpdateSubSectionAsync(Guid id, UpdateSubSectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var subSection = await _context.SubSections.FirstOrDefaultAsync(ss => ss.Id == id, cancellationToken);
            if (subSection == null)
            {
                return Result.Failure(
                    Error.NotFound("SectionHierarchy.NotFound", $"Sub-section with ID {id} not found"));
            }

            subSection.Update(
                request.TitleFR,
                request.TitleEN,
                request.DescriptionFR,
                request.DescriptionEN,
                request.NoteFR,
                request.NoteEN,
                request.DisplayOrder,
                request.Icon);

            if (!request.IsActive && subSection.IsActive)
            {
                subSection.Deactivate();
            }
            else if (request.IsActive && !subSection.IsActive)
            {
                subSection.Activate();
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated sub-section {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sub-section {Id}", id);
            return Result.Failure(
                Error.InternalServerError("SectionHierarchy.UpdateError", "Failed to update sub-section"));
        }
    }

    public async Task<Result> DeleteSubSectionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var subSection = await _context.SubSections.FirstOrDefaultAsync(ss => ss.Id == id, cancellationToken);
            if (subSection == null)
            {
                return Result.Failure(
                    Error.NotFound("SectionHierarchy.NotFound", $"Sub-section with ID {id} not found"));
            }

            // Soft delete
            subSection.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted (deactivated) sub-section {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sub-section {Id}", id);
            return Result.Failure(
                Error.InternalServerError("SectionHierarchy.DeleteError", "Failed to delete sub-section"));
        }
    }

    public async Task<Result> ReorderSubSectionsAsync(Guid mainSectionId, ReorderSectionsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var subSectionIds = request.Items.Select(i => i.Id).ToList();
            var subSections = await _context.SubSections
                .Where(ss => ss.MainSectionId == mainSectionId && subSectionIds.Contains(ss.Id))
                .ToListAsync(cancellationToken);

            foreach (var item in request.Items)
            {
                var subSection = subSections.FirstOrDefault(s => s.Id == item.Id);
                subSection?.SetDisplayOrder(item.DisplayOrder);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reordered {Count} sub-sections in main section {MainSectionId}",
                request.Items.Count, mainSectionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering sub-sections for main section {MainSectionId}", mainSectionId);
            return Result.Failure(
                Error.InternalServerError("SectionHierarchy.ReorderError", "Failed to reorder sub-sections"));
        }
    }

    #endregion

    #region Utilities

    public async Task<Result<bool>> MainSectionExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _context.MainSections.AnyAsync(ms => ms.Id == id, cancellationToken);
        return Result.Success(exists);
    }

    public async Task<Result<bool>> SubSectionExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _context.SubSections.AnyAsync(ss => ss.Id == id, cancellationToken);
        return Result.Success(exists);
    }

    #endregion

    #region Mapping Helpers

    private static MainSectionResponse MapToResponse(MainSection section)
    {
        return new MainSectionResponse
        {
            Id = section.Id,
            Number = section.Number,
            Code = section.Code,
            TitleFR = section.TitleFR,
            TitleEN = section.TitleEN,
            DescriptionFR = section.DescriptionFR,
            DescriptionEN = section.DescriptionEN,
            DisplayOrder = section.DisplayOrder,
            IsActive = section.IsActive,
            GeneratedLast = section.GeneratedLast,
            Icon = section.Icon,
            Created = section.Created,
            LastModified = section.LastModified,
            SubSections = section.SubSections
                .OrderBy(ss => ss.DisplayOrder)
                .Select(MapToSubSectionResponse)
                .ToList()
        };
    }

    private static SubSectionResponse MapToSubSectionResponse(SubSection subSection)
    {
        return new SubSectionResponse
        {
            Id = subSection.Id,
            MainSectionId = subSection.MainSectionId,
            Code = subSection.Code,
            TitleFR = subSection.TitleFR,
            TitleEN = subSection.TitleEN,
            DescriptionFR = subSection.DescriptionFR,
            DescriptionEN = subSection.DescriptionEN,
            NoteFR = subSection.NoteFR,
            NoteEN = subSection.NoteEN,
            DisplayOrder = subSection.DisplayOrder,
            IsActive = subSection.IsActive,
            Icon = subSection.Icon,
            Created = subSection.Created,
            LastModified = subSection.LastModified,
            QuestionMappingsCount = subSection.QuestionMappings?.Count(m => m.IsActive) ?? 0,
            PromptsCount = subSection.Prompts?.Count(p => p.IsActive) ?? 0
        };
    }

    #endregion
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.QuestionMapping;
using Sqordia.Contracts.Responses.Admin.QuestionMapping;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Application.Services.V3.Implementations;

/// <summary>
/// Service implementation for managing question-to-section mappings
/// </summary>
public class QuestionSectionMappingService : IQuestionSectionMappingService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<QuestionSectionMappingService> _logger;

    public QuestionSectionMappingService(
        IApplicationDbContext context,
        ILogger<QuestionSectionMappingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Query

    public async Task<Result<List<QuestionMappingResponse>>> GetMappingsAsync(
        QuestionMappingFilterRequest? filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.QuestionSectionMappings
                .Include(m => m.QuestionTemplate)
                .Include(m => m.SubSection)
                    .ThenInclude(ss => ss.MainSection)
                .AsQueryable();

            if (filter != null)
            {
                if (filter.QuestionTemplateV3Id.HasValue)
                {
                    query = query.Where(m => m.QuestionTemplateV3Id == filter.QuestionTemplateV3Id.Value);
                }

                if (filter.SubSectionId.HasValue)
                {
                    query = query.Where(m => m.SubSectionId == filter.SubSectionId.Value);
                }

                if (filter.MainSectionId.HasValue)
                {
                    query = query.Where(m => m.SubSection.MainSectionId == filter.MainSectionId.Value);
                }

                if (!string.IsNullOrEmpty(filter.MappingContext))
                {
                    query = query.Where(m => m.MappingContext == filter.MappingContext);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(m => m.IsActive == filter.IsActive.Value);
                }
            }

            var mappings = await query
                .OrderBy(m => m.QuestionTemplate.QuestionNumber)
                .ThenBy(m => m.SubSection.MainSection.DisplayOrder)
                .ThenBy(m => m.SubSection.DisplayOrder)
                .ToListAsync(cancellationToken);

            var response = mappings.Select(MapToResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting question mappings");
            return Result.Failure<List<QuestionMappingResponse>>(
                Error.InternalServerError("QuestionMapping.GetError", "Failed to retrieve mappings"));
        }
    }

    public async Task<Result<QuestionMappingResponse>> GetMappingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var mapping = await _context.QuestionSectionMappings
                .Include(m => m.QuestionTemplate)
                .Include(m => m.SubSection)
                    .ThenInclude(ss => ss.MainSection)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

            if (mapping == null)
            {
                return Result.Failure<QuestionMappingResponse>(
                    Error.NotFound("QuestionMapping.NotFound", $"Mapping with ID {id} not found"));
            }

            return Result.Success(MapToResponse(mapping));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting question mapping {Id}", id);
            return Result.Failure<QuestionMappingResponse>(
                Error.InternalServerError("QuestionMapping.GetError", "Failed to retrieve mapping"));
        }
    }

    public async Task<Result<List<QuestionMappingResponse>>> GetMappingsForQuestionAsync(
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var mappings = await _context.QuestionSectionMappings
                .Include(m => m.QuestionTemplate)
                .Include(m => m.SubSection)
                    .ThenInclude(ss => ss.MainSection)
                .Where(m => m.QuestionTemplateV3Id == questionId && m.IsActive)
                .OrderBy(m => m.SubSection.MainSection.DisplayOrder)
                .ThenBy(m => m.SubSection.DisplayOrder)
                .ToListAsync(cancellationToken);

            var response = mappings.Select(MapToResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mappings for question {QuestionId}", questionId);
            return Result.Failure<List<QuestionMappingResponse>>(
                Error.InternalServerError("QuestionMapping.GetError", "Failed to retrieve mappings"));
        }
    }

    public async Task<Result<List<QuestionMappingResponse>>> GetMappingsForSubSectionAsync(
        Guid subSectionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var mappings = await _context.QuestionSectionMappings
                .Include(m => m.QuestionTemplate)
                .Include(m => m.SubSection)
                    .ThenInclude(ss => ss.MainSection)
                .Where(m => m.SubSectionId == subSectionId && m.IsActive)
                .OrderBy(m => m.QuestionTemplate.QuestionNumber)
                .ToListAsync(cancellationToken);

            var response = mappings.Select(MapToResponse).ToList();
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mappings for sub-section {SubSectionId}", subSectionId);
            return Result.Failure<List<QuestionMappingResponse>>(
                Error.InternalServerError("QuestionMapping.GetError", "Failed to retrieve mappings"));
        }
    }

    public async Task<Result<MappingMatrixResponse>> GetMappingMatrixAsync(
        MappingMatrixRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get questions
            var questionsQuery = _context.QuestionTemplatesV3.AsQueryable();
            if (request?.StepNumber.HasValue == true)
            {
                questionsQuery = questionsQuery.Where(q => q.StepNumber == request.StepNumber.Value);
            }
            if (request?.IncludeInactive != true)
            {
                questionsQuery = questionsQuery.Where(q => q.IsActive);
            }
            var questions = await questionsQuery
                .OrderBy(q => q.StepNumber)
                .ThenBy(q => q.DisplayOrder)
                .ToListAsync(cancellationToken);

            // Get sub-sections
            var subSectionsQuery = _context.SubSections
                .Include(ss => ss.MainSection)
                .AsQueryable();
            if (request?.MainSectionId.HasValue == true)
            {
                subSectionsQuery = subSectionsQuery.Where(ss => ss.MainSectionId == request.MainSectionId.Value);
            }
            if (request?.IncludeInactive != true)
            {
                subSectionsQuery = subSectionsQuery.Where(ss => ss.IsActive);
            }
            var subSections = await subSectionsQuery
                .OrderBy(ss => ss.MainSection.DisplayOrder)
                .ThenBy(ss => ss.DisplayOrder)
                .ToListAsync(cancellationToken);

            // Get all mappings
            var questionIds = questions.Select(q => q.Id).ToList();
            var subSectionIds = subSections.Select(ss => ss.Id).ToList();
            var mappingsQuery = _context.QuestionSectionMappings
                .Where(m => questionIds.Contains(m.QuestionTemplateV3Id) &&
                           subSectionIds.Contains(m.SubSectionId));
            if (request?.IncludeInactive != true)
            {
                mappingsQuery = mappingsQuery.Where(m => m.IsActive);
            }
            var mappings = await mappingsQuery.ToListAsync(cancellationToken);

            // Build matrix
            var mappingLookup = mappings
                .GroupBy(m => m.QuestionTemplateV3Id)
                .ToDictionary(g => g.Key, g => g.ToDictionary(m => m.SubSectionId));

            var questionRows = questions.Select(q =>
            {
                var questionMappings = mappingLookup.TryGetValue(q.Id, out var qMappings)
                    ? qMappings
                    : new Dictionary<Guid, QuestionSectionMapping>();

                return new MatrixQuestionRow
                {
                    QuestionId = q.Id,
                    QuestionNumber = q.QuestionNumber,
                    QuestionTextFR = q.QuestionTextFR,
                    QuestionTextEN = q.QuestionTextEN,
                    StepNumber = q.StepNumber,
                    Mappings = subSections.Select(ss =>
                    {
                        var hasMapping = questionMappings.TryGetValue(ss.Id, out var mapping);
                        return new MatrixCell
                        {
                            MappingId = hasMapping ? mapping!.Id : null,
                            SubSectionId = ss.Id,
                            IsMapped = hasMapping,
                            Context = hasMapping ? mapping!.MappingContext : null,
                            Weight = hasMapping ? mapping!.Weight : null
                        };
                    }).ToList()
                };
            }).ToList();

            var subSectionColumns = subSections.Select(ss => new MatrixSubSectionColumn
            {
                SubSectionId = ss.Id,
                Code = ss.Code,
                TitleFR = ss.TitleFR,
                TitleEN = ss.TitleEN,
                MainSectionId = ss.MainSectionId,
                MainSectionCode = ss.MainSection.Code,
                MainSectionNumber = ss.MainSection.Number
            }).ToList();

            return Result.Success(new MappingMatrixResponse
            {
                Questions = questionRows,
                SubSections = subSectionColumns
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mapping matrix");
            return Result.Failure<MappingMatrixResponse>(
                Error.InternalServerError("QuestionMapping.GetError", "Failed to retrieve mapping matrix"));
        }
    }

    public async Task<Result<MappingStatsResponse>> GetMappingStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var totalQuestions = await _context.QuestionTemplatesV3.CountAsync(q => q.IsActive, cancellationToken);
            var totalSubSections = await _context.SubSections.CountAsync(ss => ss.IsActive, cancellationToken);
            var totalMappings = await _context.QuestionSectionMappings.CountAsync(m => m.IsActive, cancellationToken);

            var questionsWithMappings = await _context.QuestionSectionMappings
                .Where(m => m.IsActive)
                .Select(m => m.QuestionTemplateV3Id)
                .Distinct()
                .CountAsync(cancellationToken);

            var subSectionsWithMappings = await _context.QuestionSectionMappings
                .Where(m => m.IsActive)
                .Select(m => m.SubSectionId)
                .Distinct()
                .CountAsync(cancellationToken);

            return Result.Success(new MappingStatsResponse
            {
                TotalQuestions = totalQuestions,
                TotalSubSections = totalSubSections,
                TotalMappings = totalMappings,
                UnmappedQuestions = totalQuestions - questionsWithMappings,
                UnmappedSubSections = totalSubSections - subSectionsWithMappings,
                AverageMappingsPerQuestion = totalQuestions > 0 ? (double)totalMappings / totalQuestions : 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mapping statistics");
            return Result.Failure<MappingStatsResponse>(
                Error.InternalServerError("QuestionMapping.GetError", "Failed to retrieve mapping statistics"));
        }
    }

    #endregion

    #region Commands

    public async Task<Result<Guid>> CreateMappingAsync(CreateQuestionMappingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if mapping already exists
            var exists = await _context.QuestionSectionMappings.AnyAsync(m =>
                m.QuestionTemplateV3Id == request.QuestionTemplateV3Id &&
                m.SubSectionId == request.SubSectionId,
                cancellationToken);

            if (exists)
            {
                return Result.Failure<Guid>(
                    Error.Conflict("QuestionMapping.AlreadyExists", "This mapping already exists"));
            }

            // Verify question exists
            var questionExists = await _context.QuestionTemplatesV3.AnyAsync(
                q => q.Id == request.QuestionTemplateV3Id, cancellationToken);
            if (!questionExists)
            {
                return Result.Failure<Guid>(
                    Error.NotFound("QuestionMapping.QuestionNotFound", "Question not found"));
            }

            // Verify sub-section exists
            var subSectionExists = await _context.SubSections.AnyAsync(
                ss => ss.Id == request.SubSectionId, cancellationToken);
            if (!subSectionExists)
            {
                return Result.Failure<Guid>(
                    Error.NotFound("QuestionMapping.SubSectionNotFound", "Sub-section not found"));
            }

            var mapping = QuestionSectionMapping.Create(
                request.QuestionTemplateV3Id,
                request.SubSectionId,
                request.MappingContext,
                request.Weight,
                request.TransformationHint);

            _context.QuestionSectionMappings.Add(mapping);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created mapping {Id} between question {QuestionId} and sub-section {SubSectionId}",
                mapping.Id, request.QuestionTemplateV3Id, request.SubSectionId);
            return Result.Success(mapping.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question mapping");
            return Result.Failure<Guid>(
                Error.InternalServerError("QuestionMapping.CreateError", "Failed to create mapping"));
        }
    }

    public async Task<Result> UpdateMappingAsync(Guid id, UpdateQuestionMappingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var mapping = await _context.QuestionSectionMappings.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (mapping == null)
            {
                return Result.Failure(
                    Error.NotFound("QuestionMapping.NotFound", $"Mapping with ID {id} not found"));
            }

            mapping.Update(request.MappingContext, request.Weight, request.TransformationHint);

            if (!request.IsActive && mapping.IsActive)
            {
                mapping.Deactivate();
            }
            else if (request.IsActive && !mapping.IsActive)
            {
                mapping.Activate();
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated mapping {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mapping {Id}", id);
            return Result.Failure(
                Error.InternalServerError("QuestionMapping.UpdateError", "Failed to update mapping"));
        }
    }

    public async Task<Result> DeleteMappingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var mapping = await _context.QuestionSectionMappings.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (mapping == null)
            {
                return Result.Failure(
                    Error.NotFound("QuestionMapping.NotFound", $"Mapping with ID {id} not found"));
            }

            // Hard delete since this is a join entity
            _context.QuestionSectionMappings.Remove(mapping);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted mapping {Id}", id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting mapping {Id}", id);
            return Result.Failure(
                Error.InternalServerError("QuestionMapping.DeleteError", "Failed to delete mapping"));
        }
    }

    public async Task<Result<BulkUpdateResult>> BulkUpdateMappingsAsync(BulkUpdateMappingsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new BulkUpdateResult();
            var errors = new List<string>();

            foreach (var update in request.Updates)
            {
                try
                {
                    switch (update.Action)
                    {
                        case MappingAction.Create:
                            var createResult = await CreateMappingAsync(new CreateQuestionMappingRequest
                            {
                                QuestionTemplateV3Id = update.QuestionTemplateV3Id,
                                SubSectionId = update.SubSectionId,
                                MappingContext = update.MappingContext,
                                Weight = update.Weight
                            }, cancellationToken);

                            if (createResult.IsSuccess)
                                result = result with { Created = result.Created + 1 };
                            else
                            {
                                errors.Add($"Create failed: {createResult.Error?.Message}");
                                result = result with { Failed = result.Failed + 1 };
                            }
                            break;

                        case MappingAction.Update:
                            var existingMapping = await _context.QuestionSectionMappings.FirstOrDefaultAsync(m =>
                                m.QuestionTemplateV3Id == update.QuestionTemplateV3Id &&
                                m.SubSectionId == update.SubSectionId,
                                cancellationToken);

                            if (existingMapping != null)
                            {
                                existingMapping.Update(update.MappingContext, update.Weight, null);
                                result = result with { Updated = result.Updated + 1 };
                            }
                            else
                            {
                                errors.Add($"Update failed: Mapping not found");
                                result = result with { Failed = result.Failed + 1 };
                            }
                            break;

                        case MappingAction.Delete:
                            var mappingToDelete = await _context.QuestionSectionMappings.FirstOrDefaultAsync(m =>
                                m.QuestionTemplateV3Id == update.QuestionTemplateV3Id &&
                                m.SubSectionId == update.SubSectionId,
                                cancellationToken);

                            if (mappingToDelete != null)
                            {
                                _context.QuestionSectionMappings.Remove(mappingToDelete);
                                result = result with { Deleted = result.Deleted + 1 };
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing update: {ex.Message}");
                    result = result with { Failed = result.Failed + 1 };
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bulk update completed: Created={Created}, Updated={Updated}, Deleted={Deleted}, Failed={Failed}",
                result.Created, result.Updated, result.Deleted, result.Failed);

            return Result.Success(result with { Errors = errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk update of mappings");
            return Result.Failure<BulkUpdateResult>(
                Error.InternalServerError("QuestionMapping.BulkUpdateError", "Failed to perform bulk update"));
        }
    }

    #endregion

    #region Utilities

    public async Task<Result<bool>> MappingExistsAsync(Guid questionId, Guid subSectionId, CancellationToken cancellationToken = default)
    {
        var exists = await _context.QuestionSectionMappings.AnyAsync(m =>
            m.QuestionTemplateV3Id == questionId &&
            m.SubSectionId == subSectionId &&
            m.IsActive,
            cancellationToken);
        return Result.Success(exists);
    }

    public async Task<Result> ToggleMappingAsync(Guid questionId, Guid subSectionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingMapping = await _context.QuestionSectionMappings.FirstOrDefaultAsync(m =>
                m.QuestionTemplateV3Id == questionId &&
                m.SubSectionId == subSectionId,
                cancellationToken);

            if (existingMapping != null)
            {
                // Toggle: delete if exists
                _context.QuestionSectionMappings.Remove(existingMapping);
                _logger.LogInformation("Toggled off mapping between question {QuestionId} and sub-section {SubSectionId}",
                    questionId, subSectionId);
            }
            else
            {
                // Toggle: create if doesn't exist
                var mapping = QuestionSectionMapping.Create(questionId, subSectionId, "primary", null, null);
                _context.QuestionSectionMappings.Add(mapping);
                _logger.LogInformation("Toggled on mapping between question {QuestionId} and sub-section {SubSectionId}",
                    questionId, subSectionId);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling mapping between question {QuestionId} and sub-section {SubSectionId}",
                questionId, subSectionId);
            return Result.Failure(
                Error.InternalServerError("QuestionMapping.ToggleError", "Failed to toggle mapping"));
        }
    }

    #endregion

    #region Mapping Helpers

    private static QuestionMappingResponse MapToResponse(QuestionSectionMapping mapping)
    {
        return new QuestionMappingResponse
        {
            Id = mapping.Id,
            QuestionTemplateV3Id = mapping.QuestionTemplateV3Id,
            SubSectionId = mapping.SubSectionId,
            MappingContext = mapping.MappingContext,
            Weight = mapping.Weight,
            TransformationHint = mapping.TransformationHint,
            IsActive = mapping.IsActive,
            QuestionNumber = mapping.QuestionTemplate?.QuestionNumber ?? 0,
            QuestionTextFR = mapping.QuestionTemplate?.QuestionTextFR ?? "",
            QuestionTextEN = mapping.QuestionTemplate?.QuestionTextEN ?? "",
            SubSectionCode = mapping.SubSection?.Code ?? "",
            SubSectionTitleFR = mapping.SubSection?.TitleFR ?? "",
            SubSectionTitleEN = mapping.SubSection?.TitleEN ?? "",
            MainSectionCode = mapping.SubSection?.MainSection?.Code ?? "",
            MainSectionTitleFR = mapping.SubSection?.MainSection?.TitleFR ?? ""
        };
    }

    #endregion
}

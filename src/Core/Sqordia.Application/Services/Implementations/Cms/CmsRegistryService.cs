using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services.Cms;
using Sqordia.Contracts.Responses.Cms;
using Sqordia.Domain.Entities.Cms;

namespace Sqordia.Application.Services.Implementations.Cms;

/// <summary>
/// Service for managing the CMS page/section/block registry.
/// Provides database-driven registry access with fallback to static registry.
/// </summary>
public class CmsRegistryService : ICmsRegistryService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CmsRegistryService> _logger;

    private static readonly Error CmsTablesNotAvailableError = new(
        "CmsRegistry.TablesNotAvailable",
        "CMS registry tables are not available. Using static fallback.");

    public CmsRegistryService(
        IApplicationDbContext context,
        ILogger<CmsRegistryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<CmsPageRegistryResponse>>> GetAllPagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to load from database first
            var dbPages = await _context.CmsPages
                .Where(p => p.IsActive)
                .Include(p => p.Sections.Where(s => s.IsActive))
                .OrderBy(p => p.SortOrder)
                .ToListAsync(cancellationToken);

            if (dbPages.Count > 0)
            {
                var responses = dbPages.Select(MapToPageRegistryResponse).ToList();
                return Result.Success(responses);
            }

            // Fall back to static registry
            _logger.LogInformation("No pages in database, falling back to static registry");
            return Result.Success(GetStaticRegistryPages());
        }
        catch (Exception ex) when (IsCmsTableMissing(ex))
        {
            _logger.LogWarning(ex, "CMS registry tables not available, using static fallback");
            return Result.Success(GetStaticRegistryPages());
        }
    }

    public async Task<Result<CmsPageDetailResponse>> GetPageAsync(string pageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var page = await _context.CmsPages
                .Include(p => p.Sections)
                    .ThenInclude(s => s.BlockDefinitions)
                .FirstOrDefaultAsync(p => p.Key == pageKey.ToLowerInvariant(), cancellationToken);

            if (page == null)
            {
                return Result.Failure<CmsPageDetailResponse>(
                    new Error("CmsRegistry.PageNotFound", $"Page with key '{pageKey}' not found"));
            }

            return Result.Success(MapToPageDetailResponse(page));
        }
        catch (Exception ex) when (IsCmsTableMissing(ex))
        {
            _logger.LogWarning(ex, "CMS registry tables not available");
            return Result.Failure<CmsPageDetailResponse>(CmsTablesNotAvailableError);
        }
    }

    public async Task<Result<List<CmsSectionResponse>>> GetSectionsAsync(string pageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var page = await _context.CmsPages
                .Include(p => p.Sections.Where(s => s.IsActive))
                .FirstOrDefaultAsync(p => p.Key == pageKey.ToLowerInvariant(), cancellationToken);

            if (page == null)
            {
                // Fall back to static registry
                var staticPage = CmsPageRegistry.GetPage(pageKey);
                if (staticPage == null)
                {
                    return Result.Failure<List<CmsSectionResponse>>(
                        new Error("CmsRegistry.PageNotFound", $"Page with key '{pageKey}' not found"));
                }

                return Result.Success(staticPage.Sections.Select(s => new CmsSectionResponse
                {
                    Id = Guid.Empty,
                    Key = s.Key,
                    Label = s.Label,
                    SortOrder = s.SortOrder,
                    IconName = null
                }).ToList());
            }

            var sections = page.Sections
                .OrderBy(s => s.SortOrder)
                .Select(MapToSectionResponse)
                .ToList();

            return Result.Success(sections);
        }
        catch (Exception ex) when (IsCmsTableMissing(ex))
        {
            _logger.LogWarning(ex, "CMS registry tables not available");
            return Result.Failure<List<CmsSectionResponse>>(CmsTablesNotAvailableError);
        }
    }

    public async Task<Result<List<CmsBlockDefinitionResponse>>> GetBlockDefinitionsAsync(string sectionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var section = await _context.CmsSections
                .Include(s => s.BlockDefinitions.Where(b => b.IsActive))
                .FirstOrDefaultAsync(s => s.Key == sectionKey.ToLowerInvariant(), cancellationToken);

            if (section == null)
            {
                return Result.Failure<List<CmsBlockDefinitionResponse>>(
                    new Error("CmsRegistry.SectionNotFound", $"Section with key '{sectionKey}' not found"));
            }

            var blockDefs = section.BlockDefinitions
                .OrderBy(b => b.SortOrder)
                .Select(MapToBlockDefinitionResponse)
                .ToList();

            return Result.Success(blockDefs);
        }
        catch (Exception ex) when (IsCmsTableMissing(ex))
        {
            _logger.LogWarning(ex, "CMS registry tables not available");
            return Result.Failure<List<CmsBlockDefinitionResponse>>(CmsTablesNotAvailableError);
        }
    }

    public async Task<Result<bool>> IsValidSectionKeyAsync(string sectionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database first
            var exists = await _context.CmsSections
                .AnyAsync(s => s.Key == sectionKey.ToLowerInvariant() && s.IsActive, cancellationToken);

            if (exists)
            {
                return Result.Success(true);
            }

            // Check static registry as fallback
            return Result.Success(CmsPageRegistry.IsValidSectionKey(sectionKey));
        }
        catch (Exception ex) when (IsCmsTableMissing(ex))
        {
            _logger.LogWarning(ex, "CMS registry tables not available, checking static registry");
            return Result.Success(CmsPageRegistry.IsValidSectionKey(sectionKey));
        }
    }

    public async Task<Result> SeedFromStaticRegistryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if there are already pages in the database
            var hasPages = await _context.CmsPages.AnyAsync(cancellationToken);
            if (hasPages)
            {
                _logger.LogInformation("Database registry already contains pages, skipping seed");
                return Result.Success();
            }

            _logger.LogInformation("Seeding CMS registry from static registry...");

            foreach (var staticPage in CmsPageRegistry.Pages)
            {
                var page = new CmsPage(
                    key: staticPage.Key,
                    label: staticPage.Label,
                    sortOrder: CmsPageRegistry.Pages.ToList().IndexOf(staticPage),
                    description: null,
                    iconName: GetIconNameForPage(staticPage.Key),
                    specialRenderer: GetSpecialRendererForPage(staticPage.Key));

                _context.CmsPages.Add(page);
                await _context.SaveChangesAsync(cancellationToken);

                foreach (var staticSection in staticPage.Sections)
                {
                    var section = new CmsSection(
                        cmsPageId: page.Id,
                        key: staticSection.Key,
                        label: staticSection.Label,
                        sortOrder: staticSection.SortOrder,
                        description: null,
                        iconName: GetIconNameForSection(staticSection.Key));

                    _context.CmsSections.Add(section);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Successfully seeded {Count} pages from static registry", CmsPageRegistry.Pages.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed CMS registry from static registry");
            return Result.Failure(new Error("CmsRegistry.SeedFailed", ex.Message));
        }
    }

    #region Mapping Methods

    private static CmsPageRegistryResponse MapToPageRegistryResponse(CmsPage page)
    {
        return new CmsPageRegistryResponse
        {
            Id = page.Id,
            Key = page.Key,
            Label = page.Label,
            Description = page.Description,
            SortOrder = page.SortOrder,
            IconName = page.IconName,
            SpecialRenderer = page.SpecialRenderer,
            Sections = page.Sections
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .Select(MapToSectionResponse)
                .ToList()
        };
    }

    private static CmsPageDetailResponse MapToPageDetailResponse(CmsPage page)
    {
        return new CmsPageDetailResponse
        {
            Id = page.Id,
            Key = page.Key,
            Label = page.Label,
            Description = page.Description,
            SortOrder = page.SortOrder,
            IsActive = page.IsActive,
            IconName = page.IconName,
            SpecialRenderer = page.SpecialRenderer,
            Created = page.Created,
            LastModified = page.LastModified,
            Sections = page.Sections
                .OrderBy(s => s.SortOrder)
                .Select(MapToSectionDetailResponse)
                .ToList()
        };
    }

    private static CmsSectionResponse MapToSectionResponse(CmsSection section)
    {
        return new CmsSectionResponse
        {
            Id = section.Id,
            Key = section.Key,
            Label = section.Label,
            Description = section.Description,
            SortOrder = section.SortOrder,
            IconName = section.IconName
        };
    }

    private static CmsSectionDetailResponse MapToSectionDetailResponse(CmsSection section)
    {
        return new CmsSectionDetailResponse
        {
            Id = section.Id,
            CmsPageId = section.CmsPageId,
            Key = section.Key,
            Label = section.Label,
            Description = section.Description,
            SortOrder = section.SortOrder,
            IsActive = section.IsActive,
            IconName = section.IconName,
            BlockDefinitions = section.BlockDefinitions
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .Select(MapToBlockDefinitionResponse)
                .ToList()
        };
    }

    private static CmsBlockDefinitionResponse MapToBlockDefinitionResponse(CmsBlockDefinition blockDef)
    {
        return new CmsBlockDefinitionResponse
        {
            Id = blockDef.Id,
            CmsSectionId = blockDef.CmsSectionId,
            BlockKey = blockDef.BlockKey,
            BlockType = blockDef.BlockType.ToString(),
            Label = blockDef.Label,
            Description = blockDef.Description,
            DefaultContent = blockDef.DefaultContent,
            SortOrder = blockDef.SortOrder,
            IsRequired = blockDef.IsRequired,
            IsActive = blockDef.IsActive,
            ValidationRules = blockDef.ValidationRules,
            MetadataSchema = blockDef.MetadataSchema,
            Placeholder = blockDef.Placeholder,
            MaxLength = blockDef.MaxLength
        };
    }

    #endregion

    #region Static Registry Fallback

    private static List<CmsPageRegistryResponse> GetStaticRegistryPages()
    {
        return CmsPageRegistry.Pages.Select((page, index) => new CmsPageRegistryResponse
        {
            Id = Guid.Empty,
            Key = page.Key,
            Label = page.Label,
            Description = null,
            SortOrder = index,
            IconName = GetIconNameForPage(page.Key),
            SpecialRenderer = GetSpecialRendererForPage(page.Key),
            Sections = page.Sections.Select(s => new CmsSectionResponse
            {
                Id = Guid.Empty,
                Key = s.Key,
                Label = s.Label,
                Description = null,
                SortOrder = s.SortOrder,
                IconName = GetIconNameForSection(s.Key)
            }).ToList()
        }).ToList();
    }

    private static string? GetIconNameForPage(string pageKey)
    {
        return pageKey switch
        {
            "landing" => "Globe",
            "dashboard" => "LayoutDashboard",
            "profile" => "UserCircle",
            "questionnaire" => "ClipboardList",
            "question_templates" => "HelpCircle",
            "create_plan" => "PenLine",
            "subscription" => "CreditCard",
            "onboarding" => "Rocket",
            "auth" => "LogIn",
            "legal" => "Scale",
            "global" => "Globe",
            _ => "FileText"
        };
    }

    private static string? GetSpecialRendererForPage(string pageKey)
    {
        return pageKey switch
        {
            "question_templates" => "question-templates",
            _ => null
        };
    }

    private static string? GetIconNameForSection(string sectionKey)
    {
        var sectionName = sectionKey.Split('.').LastOrDefault() ?? "";
        return sectionName switch
        {
            "hero" => "Type",
            "features" => "LayoutGrid",
            "faq" => "HelpCircle",
            "testimonials" => "MessageSquare",
            "labels" => "Type",
            "empty_states" => "FileText",
            "tips" => "Lightbulb",
            "security" => "ShieldCheck",
            "sessions" => "Monitor",
            "steps" => "Layers",
            "types" => "Target",
            "plans" => "CreditCard",
            "welcome" => "Rocket",
            "completion" => "MailCheck",
            "login" => "LogIn",
            "register" => "UserPlus",
            "forgot_password" => "KeyRound",
            "reset_password" => "Lock",
            "verify_email" => "MailCheck",
            "terms" => "FileText",
            "privacy" => "Lock",
            "branding" => "Palette",
            "social" => "Globe",
            "contact" => "Building2",
            "footer" => "FileText",
            "navigation" => "Navigation",
            _ => "FileText"
        };
    }

    #endregion

    #region Helpers

    private static bool IsCmsTableMissing(Exception ex)
    {
        // PostgreSQL: relation does not exist
        // SQL Server: Invalid object name
        var message = ex.Message.ToLowerInvariant();
        return message.Contains("does not exist") ||
               message.Contains("invalid object name") ||
               message.Contains("no such table") ||
               (ex.InnerException != null && IsCmsTableMissing(ex.InnerException));
    }

    #endregion
}

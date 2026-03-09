using Sqordia.Application.Common.Models;
using Sqordia.Application.Models.Export;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for resolving export themes based on feature flags and user preferences.
/// </summary>
public interface IExportThemeService
{
    /// <summary>
    /// Returns the list of available themes. Returns 2 when ProfessionalExportThemes is off, 10 when on.
    /// </summary>
    Task<Result<List<ExportTemplate>>> GetAvailableThemesAsync(CancellationToken ct = default);

    /// <summary>
    /// Resolves a theme by ID, optionally overriding the primary color from cover page settings.
    /// </summary>
    Task<Result<ExportTheme>> ResolveThemeAsync(string? themeId, string? primaryColorOverride, CancellationToken ct = default);
}

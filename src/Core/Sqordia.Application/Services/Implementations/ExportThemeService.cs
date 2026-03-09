using Sqordia.Application.Common.Models;
using Sqordia.Application.Models.Export;

namespace Sqordia.Application.Services.Implementations;

public class ExportThemeService : IExportThemeService
{
    private readonly IFeatureFlagsService _featureFlagsService;

    public ExportThemeService(IFeatureFlagsService featureFlagsService)
    {
        _featureFlagsService = featureFlagsService;
    }

    public async Task<Result<List<ExportTemplate>>> GetAvailableThemesAsync(CancellationToken ct = default)
    {
        var flagResult = await _featureFlagsService.IsEnabledAsync("ProfessionalExportThemes", ct);
        var isEnabled = flagResult.IsSuccess && flagResult.Value;

        var themes = isEnabled
            ? ExportThemeRegistry.GetAllThemes()
            : ExportThemeRegistry.GetDefaultThemes();

        var templates = themes.Select(t => new ExportTemplate
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            IsDefault = t.Id == "classic",
            SupportedFormats = new List<string> { "pdf", "docx", "html", "pptx" },
            SupportedLanguages = new List<string> { "fr", "en" }
        }).ToList();

        return Result.Success(templates);
    }

    public async Task<Result<ExportTheme>> ResolveThemeAsync(string? themeId, string? primaryColorOverride, CancellationToken ct = default)
    {
        await Task.CompletedTask;

        var theme = ExportThemeRegistry.GetTheme(themeId);

        if (!string.IsNullOrWhiteSpace(primaryColorOverride))
        {
            // Clone the theme and apply the override color
            theme = CloneWithPrimaryOverride(theme, primaryColorOverride);
        }

        return Result.Success(theme);
    }

    private static ExportTheme CloneWithPrimaryOverride(ExportTheme source, string primaryColor)
    {
        return new ExportTheme
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            PrimaryColor = primaryColor,
            SecondaryColor = source.SecondaryColor,
            AccentColor = primaryColor,
            HeadingColor = primaryColor,
            Heading2Color = source.Heading2Color,
            TextColor = source.TextColor,
            MutedTextColor = source.MutedTextColor,
            SeparatorColor = source.SeparatorColor,
            CoverTitleColor = primaryColor,
            CoverSubtitleColor = source.CoverSubtitleColor,
            CoverGradientEnd = source.CoverGradientEnd,
            TableHeaderBg = source.TableHeaderBg,
            TableHeaderFg = source.TableHeaderFg,
            TableAlternateRowBg = source.TableAlternateRowBg,
            TableBorderColor = source.TableBorderColor,
            TableHighlightBg = source.TableHighlightBg,
            ChartColorPalette = source.ChartColorPalette,
            MetricCardBg = source.MetricCardBg,
            MetricLabelColor = source.MetricLabelColor,
            MetricValueColor = source.MetricValueColor,
            TrendUpColor = source.TrendUpColor,
            TrendDownColor = source.TrendDownColor,
            TrendNeutralColor = source.TrendNeutralColor,
            InfographicNumberBg = primaryColor,
            InfographicNumberFg = source.InfographicNumberFg,
            ChartPlaceholderBg = source.ChartPlaceholderBg,
            PageBackgroundColor = source.PageBackgroundColor,
            BodyBackgroundColor = source.BodyBackgroundColor,
            TocBackgroundColor = source.TocBackgroundColor,
            ErrorColor = source.ErrorColor
        };
    }
}

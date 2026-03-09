namespace Sqordia.Application.Models.Export;

/// <summary>
/// Static registry of predefined professional export themes.
/// Each theme has a distinct visual identity and color palette.
/// </summary>
public static class ExportThemeRegistry
{
    private static readonly Dictionary<string, ExportTheme> _themes = new(StringComparer.OrdinalIgnoreCase)
    {
        // ────────────────────────────────────────────────────
        // 1. CLASSIC — Timeless navy + steel blue
        // ────────────────────────────────────────────────────
        ["classic"] = new ExportTheme
        {
            Id = "classic",
            Name = "Classic",
            Description = "Timeless navy and steel blue for any audience",
            PrimaryColor = "#1B2E4A",
            SecondaryColor = "#4A90D9",
            AccentColor = "#4A90D9",
            HeadingColor = "#1B2E4A",
            Heading2Color = "#4A90D9",
            TextColor = "#2D3748",
            MutedTextColor = "#718096",
            SeparatorColor = "#E2E8F0",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#CBD5E0",
            CoverGradientEnd = "#2B5797",
            TableHeaderBg = "#1B2E4A",
            TableHeaderFg = "#FFFFFF",
            TableAlternateRowBg = "#F7FAFC",
            TableBorderColor = "#E2E8F0",
            TableHighlightBg = "#EBF4FF",
            ChartColorPalette = new() { "#4A90D9", "#48BB78", "#ED8936", "#E53E3E", "#9F7AEA", "#38B2AC" },
            MetricCardBg = "#F7FAFC",
            MetricLabelColor = "#718096",
            MetricValueColor = "#1B2E4A",
            TrendUpColor = "#48BB78",
            TrendDownColor = "#E53E3E",
            TrendNeutralColor = "#A0AEC0",
            InfographicNumberBg = "#4A90D9",
            InfographicNumberFg = "#FFFFFF",
            ChartPlaceholderBg = "#EDF2F7",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#F7FAFC",
            TocBackgroundColor = "#F7FAFC",
            ErrorColor = "#E53E3E"
        },

        // ────────────────────────────────────────────────────
        // 2. MODERN — Slate + vibrant teal, clean geometry
        // ────────────────────────────────────────────────────
        ["modern"] = new ExportTheme
        {
            Id = "modern",
            Name = "Modern",
            Description = "Clean contemporary design with teal accents",
            PrimaryColor = "#0F766E",
            SecondaryColor = "#2DD4BF",
            AccentColor = "#0F766E",
            HeadingColor = "#134E4A",
            Heading2Color = "#0F766E",
            TextColor = "#1E293B",
            MutedTextColor = "#64748B",
            SeparatorColor = "#E2E8F0",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#99F6E4",
            CoverGradientEnd = "#134E4A",
            TableHeaderBg = "#0F766E",
            TableHeaderFg = "#FFFFFF",
            TableAlternateRowBg = "#F0FDFA",
            TableBorderColor = "#CCFBF1",
            TableHighlightBg = "#CCFBF1",
            ChartColorPalette = new() { "#0F766E", "#6366F1", "#F59E0B", "#EC4899", "#06B6D4", "#84CC16" },
            MetricCardBg = "#F0FDFA",
            MetricLabelColor = "#64748B",
            MetricValueColor = "#134E4A",
            TrendUpColor = "#0F766E",
            TrendDownColor = "#E11D48",
            TrendNeutralColor = "#94A3B8",
            InfographicNumberBg = "#0F766E",
            InfographicNumberFg = "#FFFFFF",
            ChartPlaceholderBg = "#F0FDFA",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#F8FAFC",
            TocBackgroundColor = "#F0FDFA",
            ErrorColor = "#E11D48"
        },

        // ────────────────────────────────────────────────────
        // 3. CORPORATE — Deep navy + warm gold, bank-ready
        // ────────────────────────────────────────────────────
        ["corporate"] = new ExportTheme
        {
            Id = "corporate",
            Name = "Corporate",
            Description = "Bank-ready navy and gold for formal presentations",
            PrimaryColor = "#0C1E33",
            SecondaryColor = "#C9962B",
            AccentColor = "#C9962B",
            HeadingColor = "#0C1E33",
            Heading2Color = "#C9962B",
            TextColor = "#1A202C",
            MutedTextColor = "#718096",
            SeparatorColor = "#C9962B",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#C9962B",
            CoverGradientEnd = "#1A365D",
            TableHeaderBg = "#0C1E33",
            TableHeaderFg = "#FFFFFF",
            TableAlternateRowBg = "#FFFBEB",
            TableBorderColor = "#E2E8F0",
            TableHighlightBg = "#FEF3C7",
            ChartColorPalette = new() { "#0C1E33", "#C9962B", "#2563EB", "#059669", "#7C3AED", "#DC2626" },
            MetricCardBg = "#FFFBEB",
            MetricLabelColor = "#718096",
            MetricValueColor = "#0C1E33",
            TrendUpColor = "#059669",
            TrendDownColor = "#DC2626",
            TrendNeutralColor = "#718096",
            InfographicNumberBg = "#C9962B",
            InfographicNumberFg = "#0C1E33",
            ChartPlaceholderBg = "#FEF3C7",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#FAFAF9",
            TocBackgroundColor = "#FFFBEB",
            ErrorColor = "#DC2626"
        },

        // ────────────────────────────────────────────────────
        // 4. STARTUP — Electric purple + hot pink, bold energy
        // ────────────────────────────────────────────────────
        ["startup"] = new ExportTheme
        {
            Id = "startup",
            Name = "Startup",
            Description = "Bold electric purple for pitch decks and investors",
            PrimaryColor = "#7C3AED",
            SecondaryColor = "#A78BFA",
            AccentColor = "#EC4899",
            HeadingColor = "#5B21B6",
            Heading2Color = "#7C3AED",
            TextColor = "#1E1B4B",
            MutedTextColor = "#6B7280",
            SeparatorColor = "#E9D5FF",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#DDD6FE",
            CoverGradientEnd = "#EC4899",
            TableHeaderBg = "#7C3AED",
            TableHeaderFg = "#FFFFFF",
            TableAlternateRowBg = "#FAF5FF",
            TableBorderColor = "#E9D5FF",
            TableHighlightBg = "#FCE7F3",
            ChartColorPalette = new() { "#7C3AED", "#EC4899", "#06B6D4", "#F59E0B", "#10B981", "#F43F5E" },
            MetricCardBg = "#FAF5FF",
            MetricLabelColor = "#7C3AED",
            MetricValueColor = "#5B21B6",
            TrendUpColor = "#10B981",
            TrendDownColor = "#F43F5E",
            TrendNeutralColor = "#A78BFA",
            InfographicNumberBg = "#7C3AED",
            InfographicNumberFg = "#FFFFFF",
            ChartPlaceholderBg = "#EDE9FE",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#FAF5FF",
            TocBackgroundColor = "#F5F3FF",
            ErrorColor = "#F43F5E"
        },

        // ────────────────────────────────────────────────────
        // 5. MINIMAL — Swiss typography, mono black + white
        // ────────────────────────────────────────────────────
        ["minimal"] = new ExportTheme
        {
            Id = "minimal",
            Name = "Minimal",
            Description = "Swiss typography-focused black and white",
            PrimaryColor = "#18181B",
            SecondaryColor = "#71717A",
            AccentColor = "#18181B",
            HeadingColor = "#09090B",
            Heading2Color = "#3F3F46",
            TextColor = "#27272A",
            MutedTextColor = "#A1A1AA",
            SeparatorColor = "#E4E4E7",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#A1A1AA",
            CoverGradientEnd = "#3F3F46",
            TableHeaderBg = "#18181B",
            TableHeaderFg = "#FAFAFA",
            TableAlternateRowBg = "#FAFAFA",
            TableBorderColor = "#E4E4E7",
            TableHighlightBg = "#F4F4F5",
            ChartColorPalette = new() { "#18181B", "#71717A", "#A1A1AA", "#3F3F46", "#52525B", "#D4D4D8" },
            MetricCardBg = "#FAFAFA",
            MetricLabelColor = "#A1A1AA",
            MetricValueColor = "#09090B",
            TrendUpColor = "#16A34A",
            TrendDownColor = "#DC2626",
            TrendNeutralColor = "#A1A1AA",
            InfographicNumberBg = "#18181B",
            InfographicNumberFg = "#FAFAFA",
            ChartPlaceholderBg = "#F4F4F5",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#FAFAFA",
            TocBackgroundColor = "#FAFAFA",
            ErrorColor = "#DC2626"
        },

        // ────────────────────────────────────────────────────
        // 6. EXECUTIVE — Charcoal + burgundy, C-suite ready
        // ────────────────────────────────────────────────────
        ["executive"] = new ExportTheme
        {
            Id = "executive",
            Name = "Executive",
            Description = "Charcoal and burgundy for C-suite presentations",
            PrimaryColor = "#1F2937",
            SecondaryColor = "#881337",
            AccentColor = "#881337",
            HeadingColor = "#111827",
            Heading2Color = "#881337",
            TextColor = "#1F2937",
            MutedTextColor = "#6B7280",
            SeparatorColor = "#881337",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#FCA5A5",
            CoverGradientEnd = "#881337",
            TableHeaderBg = "#1F2937",
            TableHeaderFg = "#FFFFFF",
            TableAlternateRowBg = "#F9FAFB",
            TableBorderColor = "#E5E7EB",
            TableHighlightBg = "#FFF1F2",
            ChartColorPalette = new() { "#881337", "#1F2937", "#0369A1", "#15803D", "#A16207", "#7E22CE" },
            MetricCardBg = "#F9FAFB",
            MetricLabelColor = "#6B7280",
            MetricValueColor = "#111827",
            TrendUpColor = "#15803D",
            TrendDownColor = "#881337",
            TrendNeutralColor = "#6B7280",
            InfographicNumberBg = "#881337",
            InfographicNumberFg = "#FFFFFF",
            ChartPlaceholderBg = "#F3F4F6",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#F9FAFB",
            TocBackgroundColor = "#F9FAFB",
            ErrorColor = "#881337"
        },

        // ────────────────────────────────────────────────────
        // 7. ECO — Forest green + lime, sustainability focus
        // ────────────────────────────────────────────────────
        ["eco"] = new ExportTheme
        {
            Id = "eco",
            Name = "Eco",
            Description = "Forest green for OBNL and sustainability projects",
            PrimaryColor = "#14532D",
            SecondaryColor = "#65A30D",
            AccentColor = "#65A30D",
            HeadingColor = "#14532D",
            Heading2Color = "#15803D",
            TextColor = "#1C1917",
            MutedTextColor = "#57534E",
            SeparatorColor = "#BBF7D0",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#BBF7D0",
            CoverGradientEnd = "#15803D",
            TableHeaderBg = "#14532D",
            TableHeaderFg = "#FFFFFF",
            TableAlternateRowBg = "#F0FDF4",
            TableBorderColor = "#BBF7D0",
            TableHighlightBg = "#DCFCE7",
            ChartColorPalette = new() { "#14532D", "#65A30D", "#CA8A04", "#0E7490", "#9333EA", "#C2410C" },
            MetricCardBg = "#F0FDF4",
            MetricLabelColor = "#14532D",
            MetricValueColor = "#052E16",
            TrendUpColor = "#16A34A",
            TrendDownColor = "#DC2626",
            TrendNeutralColor = "#57534E",
            InfographicNumberBg = "#15803D",
            InfographicNumberFg = "#FFFFFF",
            ChartPlaceholderBg = "#DCFCE7",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#F0FDF4",
            TocBackgroundColor = "#ECFDF5",
            ErrorColor = "#DC2626"
        },

        // ────────────────────────────────────────────────────
        // 8. CREATIVE — Coral + deep navy, attention-grabbing
        // ────────────────────────────────────────────────────
        ["creative"] = new ExportTheme
        {
            Id = "creative",
            Name = "Creative",
            Description = "Bold coral and navy for impactful presentations",
            PrimaryColor = "#E11D48",
            SecondaryColor = "#1E3A5F",
            AccentColor = "#E11D48",
            HeadingColor = "#1E3A5F",
            Heading2Color = "#E11D48",
            TextColor = "#1F2937",
            MutedTextColor = "#6B7280",
            SeparatorColor = "#FECDD3",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#FFE4E6",
            CoverGradientEnd = "#1E3A5F",
            TableHeaderBg = "#E11D48",
            TableHeaderFg = "#FFFFFF",
            TableAlternateRowBg = "#FFF1F2",
            TableBorderColor = "#FECDD3",
            TableHighlightBg = "#DBEAFE",
            ChartColorPalette = new() { "#E11D48", "#1E3A5F", "#8B5CF6", "#06B6D4", "#F59E0B", "#10B981" },
            MetricCardBg = "#FFF1F2",
            MetricLabelColor = "#E11D48",
            MetricValueColor = "#1E3A5F",
            TrendUpColor = "#10B981",
            TrendDownColor = "#E11D48",
            TrendNeutralColor = "#6B7280",
            InfographicNumberBg = "#E11D48",
            InfographicNumberFg = "#FFFFFF",
            ChartPlaceholderBg = "#FFE4E6",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#FFF5F5",
            TocBackgroundColor = "#FFF1F2",
            ErrorColor = "#DC2626"
        },

        // ────────────────────────────────────────────────────
        // 9. FINANCE — Midnight + electric blue, data-focused
        // ────────────────────────────────────────────────────
        ["finance"] = new ExportTheme
        {
            Id = "finance",
            Name = "Finance",
            Description = "Midnight blue for financial documents and forecasts",
            PrimaryColor = "#0F172A",
            SecondaryColor = "#3B82F6",
            AccentColor = "#3B82F6",
            HeadingColor = "#0F172A",
            Heading2Color = "#2563EB",
            TextColor = "#1E293B",
            MutedTextColor = "#94A3B8",
            SeparatorColor = "#CBD5E1",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#93C5FD",
            CoverGradientEnd = "#1E3A8A",
            TableHeaderBg = "#0F172A",
            TableHeaderFg = "#F8FAFC",
            TableAlternateRowBg = "#F8FAFC",
            TableBorderColor = "#CBD5E1",
            TableHighlightBg = "#DBEAFE",
            ChartColorPalette = new() { "#2563EB", "#0F172A", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6" },
            MetricCardBg = "#F8FAFC",
            MetricLabelColor = "#94A3B8",
            MetricValueColor = "#0F172A",
            TrendUpColor = "#10B981",
            TrendDownColor = "#EF4444",
            TrendNeutralColor = "#94A3B8",
            InfographicNumberBg = "#2563EB",
            InfographicNumberFg = "#FFFFFF",
            ChartPlaceholderBg = "#F1F5F9",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#F8FAFC",
            TocBackgroundColor = "#F1F5F9",
            ErrorColor = "#EF4444"
        },

        // ────────────────────────────────────────────────────
        // 10. QUÉBEC — Fleur-de-lis blue + white, local identity
        // ────────────────────────────────────────────────────
        ["quebec"] = new ExportTheme
        {
            Id = "quebec",
            Name = "Québec",
            Description = "Fleur-de-lis blue inspired by Quebec identity",
            PrimaryColor = "#003DA5",
            SecondaryColor = "#2563EB",
            AccentColor = "#003DA5",
            HeadingColor = "#003DA5",
            Heading2Color = "#1D4ED8",
            TextColor = "#1E293B",
            MutedTextColor = "#64748B",
            SeparatorColor = "#BFDBFE",
            CoverTitleColor = "#FFFFFF",
            CoverSubtitleColor = "#93C5FD",
            CoverGradientEnd = "#1E40AF",
            TableHeaderBg = "#003DA5",
            TableHeaderFg = "#FFFFFF",
            TableAlternateRowBg = "#EFF6FF",
            TableBorderColor = "#BFDBFE",
            TableHighlightBg = "#DBEAFE",
            ChartColorPalette = new() { "#003DA5", "#2563EB", "#06B6D4", "#10B981", "#F59E0B", "#8B5CF6" },
            MetricCardBg = "#EFF6FF",
            MetricLabelColor = "#003DA5",
            MetricValueColor = "#1E3A8A",
            TrendUpColor = "#16A34A",
            TrendDownColor = "#DC2626",
            TrendNeutralColor = "#64748B",
            InfographicNumberBg = "#003DA5",
            InfographicNumberFg = "#FFFFFF",
            ChartPlaceholderBg = "#DBEAFE",
            PageBackgroundColor = "#FFFFFF",
            BodyBackgroundColor = "#EFF6FF",
            TocBackgroundColor = "#EFF6FF",
            ErrorColor = "#DC2626"
        }
    };

    /// <summary>
    /// Gets a theme by ID. Falls back to Classic if not found.
    /// </summary>
    public static ExportTheme GetTheme(string? id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Equals("default", StringComparison.OrdinalIgnoreCase))
            return _themes["classic"];

        return _themes.TryGetValue(id, out var theme) ? theme : _themes["classic"];
    }

    /// <summary>
    /// Returns all 10 registered themes.
    /// </summary>
    public static List<ExportTheme> GetAllThemes() => _themes.Values.ToList();

    /// <summary>
    /// Returns the default set of themes — all 10 are available regardless of feature flag.
    /// </summary>
    public static List<ExportTheme> GetDefaultThemes() => GetAllThemes();
}

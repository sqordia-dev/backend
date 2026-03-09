namespace Sqordia.Application.Models.Export;

/// <summary>
/// Defines the complete visual theme for document exports (PDF, Word, HTML, PPTX).
/// </summary>
public class ExportTheme
{
    // Identity
    public string Id { get; set; } = "classic";
    public string Name { get; set; } = "Classic";
    public string Description { get; set; } = string.Empty;

    // Primary palette
    public string PrimaryColor { get; set; } = "#1E3A5F";
    public string SecondaryColor { get; set; } = "#5B9BD5";
    public string AccentColor { get; set; } = "#2563EB";

    // Text colors
    public string HeadingColor { get; set; } = "#1E3A5F";
    public string Heading2Color { get; set; } = "#5B9BD5";
    public string TextColor { get; set; } = "#2C3E50";
    public string MutedTextColor { get; set; } = "#666666";
    public string SeparatorColor { get; set; } = "#D1D5DB";

    // Cover page
    public string CoverTitleColor { get; set; } = "#1E3A5F";
    public string CoverSubtitleColor { get; set; } = "#666666";
    public string CoverGradientEnd { get; set; } = "#1E40AF";

    // Table colors
    public string TableHeaderBg { get; set; } = "#E0E0E0";
    public string TableHeaderFg { get; set; } = "#000000";
    public string TableAlternateRowBg { get; set; } = "#F5F5F5";
    public string TableBorderColor { get; set; } = "#DDDDDD";
    public string TableHighlightBg { get; set; } = "#FFF3CD";

    // Chart color palette (6 ordered colors for datasets)
    public List<string> ChartColorPalette { get; set; } = new()
    {
        "#2563EB", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6", "#EC4899"
    };

    // Metric card colors
    public string MetricCardBg { get; set; } = "#F8F9FA";
    public string MetricLabelColor { get; set; } = "#666666";
    public string MetricValueColor { get; set; } = "#333333";
    public string TrendUpColor { get; set; } = "#22C55E";
    public string TrendDownColor { get; set; } = "#EF4444";
    public string TrendNeutralColor { get; set; } = "#6B7280";

    // Infographic
    public string InfographicNumberBg { get; set; } = "#2563EB";
    public string InfographicNumberFg { get; set; } = "#FFFFFF";

    // Chart placeholder
    public string ChartPlaceholderBg { get; set; } = "#F0F0F0";

    // Background
    public string PageBackgroundColor { get; set; } = "#FFFFFF";
    public string BodyBackgroundColor { get; set; } = "#F5F5F5";
    public string TocBackgroundColor { get; set; } = "#F8F9FA";

    // Error text
    public string ErrorColor { get; set; } = "#DC2626";
}

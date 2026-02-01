namespace Sqordia.Functions.ExportHandler.Models;

/// <summary>
/// Data model for business plan export containing all sections
/// </summary>
public class BusinessPlanExportData
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }

    // Organization info
    public string OrganizationName { get; set; } = string.Empty;

    // Content sections
    public string? ExecutiveSummary { get; set; }
    public string? ProblemStatement { get; set; }
    public string? Solution { get; set; }
    public string? MarketAnalysis { get; set; }
    public string? CompetitiveAnalysis { get; set; }
    public string? SwotAnalysis { get; set; }
    public string? BusinessModel { get; set; }
    public string? MarketingStrategy { get; set; }
    public string? BrandingStrategy { get; set; }
    public string? OperationsPlan { get; set; }
    public string? ManagementTeam { get; set; }
    public string? FinancialProjections { get; set; }
    public string? FundingRequirements { get; set; }
    public string? RiskAnalysis { get; set; }
    public string? ExitStrategy { get; set; }
    public string? AppendixData { get; set; }

    // OBNL-specific sections
    public string? MissionStatement { get; set; }
    public string? SocialImpact { get; set; }
    public string? BeneficiaryProfile { get; set; }
    public string? GrantStrategy { get; set; }
    public string? SustainabilityPlan { get; set; }

    // Visual elements support
    public List<ExportSectionWithVisuals>? SectionsWithVisuals { get; set; }
    public ExportCoverPageData? CoverPage { get; set; }
    public bool IncludeTableOfContents { get; set; } = true;
    public bool IncludeVisuals { get; set; } = true;

    /// <summary>
    /// Returns all non-empty content sections in display order
    /// </summary>
    public IEnumerable<(string Title, string Content)> GetSections()
    {
        var sections = new List<(string, string?)>
        {
            ("Executive Summary", ExecutiveSummary),
            ("Mission Statement", MissionStatement),
            ("Problem Statement", ProblemStatement),
            ("Solution", Solution),
            ("Market Analysis", MarketAnalysis),
            ("Competitive Analysis", CompetitiveAnalysis),
            ("SWOT Analysis", SwotAnalysis),
            ("Business Model", BusinessModel),
            ("Marketing Strategy", MarketingStrategy),
            ("Branding Strategy", BrandingStrategy),
            ("Operations Plan", OperationsPlan),
            ("Management Team", ManagementTeam),
            ("Financial Projections", FinancialProjections),
            ("Funding Requirements", FundingRequirements),
            ("Risk Analysis", RiskAnalysis),
            ("Social Impact", SocialImpact),
            ("Beneficiary Profile", BeneficiaryProfile),
            ("Grant Strategy", GrantStrategy),
            ("Sustainability Plan", SustainabilityPlan),
            ("Exit Strategy", ExitStrategy),
            ("Appendix", AppendixData)
        };

        return sections
            .Where(s => !string.IsNullOrWhiteSpace(s.Item2))
            .Select(s => (s.Item1, s.Item2!));
    }

    /// <summary>
    /// Returns sections with visual elements if available, otherwise falls back to standard sections
    /// </summary>
    public IEnumerable<ExportSectionWithVisuals> GetSectionsWithVisuals()
    {
        if (SectionsWithVisuals != null && SectionsWithVisuals.Any())
        {
            return SectionsWithVisuals.OrderBy(s => s.Order);
        }

        // Fallback to standard sections without visuals
        return GetSections().Select((s, index) => new ExportSectionWithVisuals
        {
            SectionKey = s.Title.Replace(" ", ""),
            Title = s.Title,
            Content = s.Content,
            Order = index,
            VisualElements = new List<ExportVisualElementData>()
        });
    }
}

/// <summary>
/// Section content with visual elements for export
/// </summary>
public class ExportSectionWithVisuals
{
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int Order { get; set; }
    public List<ExportVisualElementData> VisualElements { get; set; } = new();
}

/// <summary>
/// Visual element data for export
/// </summary>
public class ExportVisualElementData
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // table, chart, metric, infographic
    public string? Title { get; set; }
    public string Position { get; set; } = "inline"; // inline, full-width, float-left, float-right
    public object? Data { get; set; }
}

/// <summary>
/// Cover page data for export
/// </summary>
public class ExportCoverPageData
{
    public string? CompanyName { get; set; }
    public string? DocumentTitle { get; set; }
    public string? Subtitle { get; set; }
    public string? PrimaryColor { get; set; }
    public string? LogoUrl { get; set; }
    public string? PreparedFor { get; set; }
    public string? PreparedBy { get; set; }
    public DateTime? PreparedDate { get; set; }
}

/// <summary>
/// Table data for visual element
/// </summary>
public class ExportTableDataModel
{
    public string TableType { get; set; } = "custom";
    public List<string> Headers { get; set; } = new();
    public List<ExportTableRowModel> Rows { get; set; } = new();
    public ExportTableRowModel? Footer { get; set; }
    public List<string>? ColumnTypes { get; set; }
}

public class ExportTableRowModel
{
    public List<ExportTableCellModel> Cells { get; set; } = new();
    public bool IsHighlighted { get; set; }
}

public class ExportTableCellModel
{
    public object Value { get; set; } = "";
    public string? Format { get; set; }
    public int? Colspan { get; set; }
    public int? Rowspan { get; set; }
}

/// <summary>
/// Chart data for visual element
/// </summary>
public class ExportChartDataModel
{
    public string ChartType { get; set; } = "bar";
    public List<string> Labels { get; set; } = new();
    public List<ExportChartDatasetModel> Datasets { get; set; } = new();
    public ExportChartOptionsModel? Options { get; set; }
}

public class ExportChartDatasetModel
{
    public string Label { get; set; } = string.Empty;
    public List<decimal> Data { get; set; } = new();
    public string? Color { get; set; }
}

public class ExportChartOptionsModel
{
    public bool ShowLegend { get; set; } = true;
    public bool ShowGrid { get; set; } = true;
    public string? Currency { get; set; }
    public bool PercentageFormat { get; set; }
    public bool Stacked { get; set; }
}

/// <summary>
/// Metric data for visual element
/// </summary>
public class ExportMetricDataModel
{
    public List<ExportMetricModel> Metrics { get; set; } = new();
    public string Layout { get; set; } = "grid";
}

public class ExportMetricModel
{
    public string Label { get; set; } = string.Empty;
    public object Value { get; set; } = "";
    public string? Format { get; set; }
    public string? Trend { get; set; }
    public string? TrendValue { get; set; }
    public string? Icon { get; set; }
}

/// <summary>
/// Infographic data for visual element
/// </summary>
public class ExportInfographicDataModel
{
    public string InfographicType { get; set; } = "icon-list";
    public List<ExportInfographicItemModel> Items { get; set; } = new();
}

public class ExportInfographicItemModel
{
    public string? Icon { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Order { get; set; }
}

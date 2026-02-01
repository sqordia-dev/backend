using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sqordia.Functions.ExportHandler.Models;
using System.Text.Json;

namespace Sqordia.Functions.ExportHandler.Services;

/// <summary>
/// PDF export generator using QuestPDF with visual elements support
/// </summary>
public class PdfExportGenerator : IExportGenerator
{
    private readonly ILogger<PdfExportGenerator> _logger;

    public string ExportType => "pdf";
    public string ContentType => "application/pdf";
    public string FileExtension => ".pdf";

    public PdfExportGenerator(ILogger<PdfExportGenerator> logger)
    {
        _logger = logger;
        // Configure QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateAsync(BusinessPlanExportData data, string language, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF for business plan {PlanId}", data.Id);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Cover page if provided
                if (data.CoverPage != null)
                {
                    page.Header().Element(c => ComposeCoverPage(c, data));
                }
                else
                {
                    page.Header().Element(c => ComposeHeader(c, data));
                }

                page.Content().Element(c => ComposeContentWithVisuals(c, data, language));
                page.Footer().Element(ComposeFooter);
            });
        });

        var bytes = document.GeneratePdf();
        _logger.LogInformation("PDF generated successfully, size: {Size} bytes", bytes.Length);

        return Task.FromResult(bytes);
    }

    private void ComposeCoverPage(IContainer container, BusinessPlanExportData data)
    {
        var cover = data.CoverPage!;
        var primaryColor = ParseColor(cover.PrimaryColor) ?? Colors.Blue.Darken3;

        container.Column(column =>
        {
            column.Item().PaddingBottom(40);

            // Company name
            column.Item().AlignCenter()
                .Text(cover.CompanyName ?? data.OrganizationName)
                .FontSize(32)
                .Bold()
                .FontColor(primaryColor);

            column.Item().PaddingTop(20);

            // Document title
            column.Item().AlignCenter()
                .Text(cover.DocumentTitle ?? "Business Plan")
                .FontSize(24);

            // Subtitle
            if (!string.IsNullOrWhiteSpace(cover.Subtitle))
            {
                column.Item().PaddingTop(10);
                column.Item().AlignCenter()
                    .Text(cover.Subtitle)
                    .FontSize(14)
                    .FontColor(Colors.Grey.Darken1);
            }

            column.Item().PaddingTop(60);

            // Prepared for/by
            if (!string.IsNullOrWhiteSpace(cover.PreparedFor))
            {
                column.Item().AlignCenter()
                    .Text($"Prepared for: {cover.PreparedFor}")
                    .FontSize(12);
            }
            if (!string.IsNullOrWhiteSpace(cover.PreparedBy))
            {
                column.Item().PaddingTop(5);
                column.Item().AlignCenter()
                    .Text($"Prepared by: {cover.PreparedBy}")
                    .FontSize(12);
            }

            // Date
            var date = cover.PreparedDate ?? DateTime.UtcNow;
            column.Item().PaddingTop(30);
            column.Item().AlignCenter()
                .Text(date.ToString("MMMM dd, yyyy"))
                .FontSize(11)
                .FontColor(Colors.Grey.Darken1);

            column.Item().PageBreak();
        });
    }

    private void ComposeHeader(IContainer container, BusinessPlanExportData data)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item()
                    .Text(data.Title)
                    .FontSize(24)
                    .Bold()
                    .FontColor(Colors.Blue.Darken3);

                column.Item()
                    .Text(data.OrganizationName)
                    .FontSize(14)
                    .FontColor(Colors.Grey.Darken2);

                column.Item()
                    .PaddingTop(5)
                    .Text($"Version {data.Version} | {data.Status}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Medium);
            });
        });

        container.PaddingBottom(20);
    }

    private void ComposeContentWithVisuals(IContainer container, BusinessPlanExportData data, string language)
    {
        container.Column(column =>
        {
            // Document info section
            column.Item().Element(c => ComposeDocumentInfo(c, data));
            column.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Table of contents
            if (data.IncludeTableOfContents)
            {
                ComposeTableOfContents(column, data);
                column.Item().PageBreak();
            }

            // Sections with visual elements
            if (data.SectionsWithVisuals != null && data.SectionsWithVisuals.Any())
            {
                foreach (var section in data.GetSectionsWithVisuals())
                {
                    column.Item().Element(c => ComposeSectionWithVisuals(c, section, data.IncludeVisuals));
                }
            }
            else
            {
                // Fallback to standard sections
                foreach (var (title, content) in data.GetSections())
                {
                    column.Item().Element(c => ComposeSection(c, title, content));
                }
            }
        });
    }

    private void ComposeTableOfContents(ColumnDescriptor column, BusinessPlanExportData data)
    {
        column.Item().Text("Table of Contents").FontSize(18).Bold();
        column.Item().PaddingTop(10);

        var sections = data.SectionsWithVisuals != null && data.SectionsWithVisuals.Any()
            ? data.GetSectionsWithVisuals().Where(s => !string.IsNullOrWhiteSpace(s.Content))
            : data.GetSections().Select((s, i) => new ExportSectionWithVisuals { Title = s.Title, Order = i });

        var sectionNumber = 1;
        foreach (var section in sections)
        {
            column.Item().Text($"{sectionNumber}. {section.Title}").FontSize(11);
            sectionNumber++;
        }

        column.Item().PaddingTop(20);
    }

    private void ComposeSectionWithVisuals(IContainer container, ExportSectionWithVisuals section, bool includeVisuals)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Section title
            column.Item()
                .Text(section.Title)
                .FontSize(16)
                .Bold()
                .FontColor(Colors.Blue.Darken2);

            column.Item()
                .PaddingTop(5)
                .PaddingBottom(5)
                .LineHorizontal(1)
                .LineColor(Colors.Grey.Lighten2);

            // Prose content
            if (!string.IsNullOrWhiteSpace(section.Content))
            {
                column.Item()
                    .PaddingTop(5)
                    .Text(StripHtml(section.Content))
                    .FontSize(11)
                    .LineHeight(1.4f);
            }

            // Visual elements
            if (includeVisuals && section.VisualElements != null && section.VisualElements.Any())
            {
                foreach (var visual in section.VisualElements)
                {
                    column.Item().PaddingTop(15);
                    RenderVisualElement(column, visual);
                }
            }

            column.Item()
                .PaddingTop(15)
                .LineHorizontal(0.5f)
                .LineColor(Colors.Grey.Lighten3);
        });
    }

    private void RenderVisualElement(ColumnDescriptor column, ExportVisualElementData visual)
    {
        // Visual element title
        if (!string.IsNullOrWhiteSpace(visual.Title))
        {
            column.Item().Text(visual.Title).SemiBold().FontSize(12);
            column.Item().PaddingBottom(5);
        }

        try
        {
            switch (visual.Type.ToLower())
            {
                case "table":
                    RenderTable(column, visual.Data);
                    break;
                case "chart":
                    RenderChartPlaceholder(column, visual);
                    break;
                case "metric":
                    RenderMetrics(column, visual.Data);
                    break;
                case "infographic":
                    RenderInfographic(column, visual.Data);
                    break;
                default:
                    column.Item().Text($"[Unknown visual type: {visual.Type}]")
                        .FontSize(10).FontColor(Colors.Grey.Medium);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render visual element {VisualId} of type {VisualType}", visual.Id, visual.Type);
            column.Item().Text($"[Error rendering {visual.Type}]")
                .FontSize(10).FontColor(Colors.Red.Medium);
        }
    }

    private void RenderTable(ColumnDescriptor column, object? data)
    {
        var tableData = DeserializeData<ExportTableDataModel>(data);
        if (tableData?.Headers == null || tableData.Rows == null)
            return;

        column.Item().Table(table =>
        {
            // Define columns
            table.ColumnsDefinition(columns =>
            {
                foreach (var _ in tableData.Headers)
                {
                    columns.RelativeColumn();
                }
            });

            // Header row
            table.Header(header =>
            {
                foreach (var headerText in tableData.Headers)
                {
                    header.Cell()
                        .Background(Colors.Grey.Lighten3)
                        .Padding(5)
                        .Text(headerText)
                        .Bold()
                        .FontSize(10);
                }
            });

            // Data rows
            var rowIndex = 0;
            foreach (var row in tableData.Rows)
            {
                var bgColor = row.IsHighlighted
                    ? Colors.Yellow.Lighten4
                    : rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                foreach (var cell in row.Cells)
                {
                    var cellContainer = table.Cell().Background(bgColor).Padding(5);
                    var valueText = cell.Value?.ToString() ?? "";
                    var textDescriptor = cellContainer.Text(valueText).FontSize(10);

                    if (cell.Format == "bold")
                        textDescriptor.Bold();
                }
                rowIndex++;
            }
        });
    }

    private void RenderChartPlaceholder(ColumnDescriptor column, ExportVisualElementData visual)
    {
        var chartData = DeserializeData<ExportChartDataModel>(visual.Data);

        column.Item()
            .Width(450)
            .Height(180)
            .Background(Colors.Grey.Lighten4)
            .Padding(10)
            .Column(chartColumn =>
            {
                chartColumn.Item().AlignCenter()
                    .Text($"[{chartData?.ChartType?.ToUpper() ?? "CHART"} Chart]")
                    .Bold().FontSize(12);

                chartColumn.Item().PaddingTop(10);

                // Show legend/data summary
                if (chartData?.Datasets != null)
                {
                    foreach (var dataset in chartData.Datasets.Take(3))
                    {
                        var sum = dataset.Data?.Sum() ?? 0;
                        chartColumn.Item().Text($"- {dataset.Label}: Total {sum:N0}")
                            .FontSize(10);
                    }
                }
            });
    }

    private void RenderMetrics(ColumnDescriptor column, object? data)
    {
        var metricData = DeserializeData<ExportMetricDataModel>(data);
        if (metricData?.Metrics == null)
            return;

        column.Item().Row(row =>
        {
            foreach (var metric in metricData.Metrics.Take(4))
            {
                row.RelativeItem()
                    .Background(Colors.Grey.Lighten4)
                    .Padding(10)
                    .Column(metricColumn =>
                    {
                        // Label
                        metricColumn.Item().Text(metric.Label)
                            .FontSize(9).FontColor(Colors.Grey.Darken2);

                        // Value
                        var formattedValue = FormatMetricValue(metric);
                        metricColumn.Item().Text(formattedValue)
                            .Bold().FontSize(16);

                        // Trend
                        if (!string.IsNullOrWhiteSpace(metric.TrendValue))
                        {
                            var trendColor = metric.Trend == "up"
                                ? Colors.Green.Darken1
                                : metric.Trend == "down"
                                    ? Colors.Red.Darken1
                                    : Colors.Grey.Darken1;

                            metricColumn.Item().Text(metric.TrendValue)
                                .FontSize(10).FontColor(trendColor);
                        }
                    });
            }
        });
    }

    private void RenderInfographic(ColumnDescriptor column, object? data)
    {
        var infographicData = DeserializeData<ExportInfographicDataModel>(data);
        if (infographicData?.Items == null)
            return;

        var orderedItems = infographicData.Items.OrderBy(i => i.Order ?? 0);

        column.Item().Column(infoColumn =>
        {
            var itemNumber = 1;
            foreach (var item in orderedItems)
            {
                infoColumn.Item().Row(row =>
                {
                    // Number/icon placeholder
                    row.ConstantItem(30)
                        .Background(Colors.Blue.Lighten4)
                        .AlignCenter()
                        .AlignMiddle()
                        .Text($"{itemNumber}")
                        .Bold().FontSize(12);

                    // Content
                    row.RelativeItem()
                        .PaddingLeft(10)
                        .Column(contentColumn =>
                        {
                            contentColumn.Item().Text(item.Title).Bold().FontSize(11);
                            if (!string.IsNullOrWhiteSpace(item.Description))
                            {
                                contentColumn.Item().Text(item.Description).FontSize(10);
                            }
                        });
                });
                infoColumn.Item().PaddingBottom(5);
                itemNumber++;
            }
        });
    }

    private void ComposeDocumentInfo(IContainer container, BusinessPlanExportData data)
    {
        container.Background(Colors.Grey.Lighten4).Padding(15).Column(column =>
        {
            column.Item().Text("Document Information").FontSize(14).Bold();
            column.Item().PaddingTop(10);

            column.Item().Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("Plan Type: ").Bold();
                    t.Span(data.PlanType);
                });
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span("Created: ").Bold();
                    t.Span(data.CreatedAt.ToString("MMMM dd, yyyy"));
                });
            });

            if (data.FinalizedAt.HasValue)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Finalized: ").Bold();
                        t.Span(data.FinalizedAt.Value.ToString("MMMM dd, yyyy"));
                    });
                });
            }

            if (!string.IsNullOrWhiteSpace(data.Description))
            {
                column.Item().PaddingTop(10).Text(t =>
                {
                    t.Span("Description: ").Bold();
                    t.Span(data.Description);
                });
            }
        });
    }

    private void ComposeSection(IContainer container, string title, string content)
    {
        container.PaddingVertical(10).Column(column =>
        {
            column.Item()
                .Text(title)
                .FontSize(16)
                .Bold()
                .FontColor(Colors.Blue.Darken2);

            column.Item()
                .PaddingTop(5)
                .Text(content)
                .FontSize(11)
                .LineHeight(1.4f);

            column.Item()
                .PaddingTop(15)
                .LineHorizontal(0.5f)
                .LineColor(Colors.Grey.Lighten3);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.CurrentPageNumber();
            text.Span(" / ");
            text.TotalPages();
        });
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }

    private static string FormatMetricValue(ExportMetricModel metric)
    {
        if (metric.Value == null)
            return "";

        if (decimal.TryParse(metric.Value.ToString(), out var decimalValue))
        {
            return metric.Format?.ToLower() switch
            {
                "currency" => decimalValue.ToString("C0"),
                "percentage" => (decimalValue * 100).ToString("F1") + "%",
                "number" => decimalValue.ToString("N0"),
                _ => decimalValue.ToString("N0")
            };
        }

        return metric.Value.ToString() ?? "";
    }

    private static Color? ParseColor(string? hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
            return null;

        try
        {
            var hex = hexColor.TrimStart('#');
            if (hex.Length == 6)
            {
                return Color.FromHex(hex);
            }
        }
        catch { }

        return null;
    }

    private T? DeserializeData<T>(object? data) where T : class
    {
        if (data == null)
            return null;

        if (data is T typedData)
            return typedData;

        try
        {
            if (data is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            var json = JsonSerializer.Serialize(data);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize data to {Type}", typeof(T).Name);
            return null;
        }
    }
}

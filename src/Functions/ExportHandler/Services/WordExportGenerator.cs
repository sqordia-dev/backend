using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Sqordia.Functions.ExportHandler.Models;
using System.Text.Json;

namespace Sqordia.Functions.ExportHandler.Services;

/// <summary>
/// Word document export generator using OpenXML with visual elements support
/// </summary>
public class WordExportGenerator : IExportGenerator
{
    private readonly ILogger<WordExportGenerator> _logger;

    public string ExportType => "word";
    public string ContentType => "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    public string FileExtension => ".docx";

    public WordExportGenerator(ILogger<WordExportGenerator> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> GenerateAsync(BusinessPlanExportData data, string language, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Word document for business plan {PlanId}", data.Id);

        using var memoryStream = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Add styles
            AddStyles(mainPart);

            // Cover page
            if (data.CoverPage != null)
            {
                AddCoverPage(body, data);
                AddPageBreak(body);
            }
            else
            {
                // Simple title
                AddTitle(body, data.Title);
                AddSubtitle(body, data.OrganizationName);
                AddParagraph(body, $"Version {data.Version} | {data.Status}", isItalic: true);
                AddHorizontalLine(body);
            }

            // Document info
            AddHeading(body, "Document Information", 2);
            AddParagraph(body, $"Plan Type: {data.PlanType}");
            AddParagraph(body, $"Created: {data.CreatedAt:MMMM dd, yyyy}");
            if (data.FinalizedAt.HasValue)
            {
                AddParagraph(body, $"Finalized: {data.FinalizedAt.Value:MMMM dd, yyyy}");
            }
            if (!string.IsNullOrWhiteSpace(data.Description))
            {
                AddParagraph(body, $"Description: {data.Description}");
            }
            AddHorizontalLine(body);

            // Table of contents
            if (data.IncludeTableOfContents)
            {
                AddTableOfContents(body, data);
                AddPageBreak(body);
            }

            // Content sections with visual elements
            if (data.SectionsWithVisuals != null && data.SectionsWithVisuals.Any())
            {
                foreach (var section in data.GetSectionsWithVisuals())
                {
                    AddSectionWithVisuals(body, section, data.IncludeVisuals);
                }
            }
            else
            {
                // Fallback to standard sections
                foreach (var (title, content) in data.GetSections())
                {
                    AddHeading(body, title, 1);
                    AddParagraph(body, content);
                    AddParagraph(body, string.Empty); // Spacing
                }
            }

            wordDocument.Save();
        }

        var bytes = memoryStream.ToArray();
        _logger.LogInformation("Word document generated successfully, size: {Size} bytes", bytes.Length);

        return Task.FromResult(bytes);
    }

    private void AddCoverPage(Body body, BusinessPlanExportData data)
    {
        var cover = data.CoverPage!;
        var primaryColor = cover.PrimaryColor?.TrimStart('#') ?? "2E75B6";

        // Add spacing at top
        for (int i = 0; i < 3; i++)
            body.Append(new Paragraph());

        // Company name
        var companyPara = new Paragraph();
        var companyProps = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
        companyPara.Append(companyProps);
        var companyRun = new Run();
        companyRun.Append(new RunProperties(new Bold(), new FontSize { Val = "56" }, new Color { Val = primaryColor }));
        companyRun.Append(new Text(cover.CompanyName ?? data.OrganizationName));
        companyPara.Append(companyRun);
        body.Append(companyPara);

        body.Append(new Paragraph()); // Spacing

        // Document title
        var titlePara = new Paragraph();
        var titleProps = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
        titlePara.Append(titleProps);
        var titleRun = new Run();
        titleRun.Append(new RunProperties(new FontSize { Val = "40" }));
        titleRun.Append(new Text(cover.DocumentTitle ?? "Business Plan"));
        titlePara.Append(titleRun);
        body.Append(titlePara);

        // Subtitle
        if (!string.IsNullOrWhiteSpace(cover.Subtitle))
        {
            var subtitlePara = new Paragraph();
            var subtitleProps = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
            subtitlePara.Append(subtitleProps);
            var subtitleRun = new Run();
            subtitleRun.Append(new RunProperties(new FontSize { Val = "28" }, new Italic(), new Color { Val = "666666" }));
            subtitleRun.Append(new Text(cover.Subtitle));
            subtitlePara.Append(subtitleRun);
            body.Append(subtitlePara);
        }

        // Add spacing
        for (int i = 0; i < 6; i++)
            body.Append(new Paragraph());

        // Prepared for/by
        if (!string.IsNullOrWhiteSpace(cover.PreparedFor))
        {
            AddCenteredText(body, $"Prepared for: {cover.PreparedFor}");
        }
        if (!string.IsNullOrWhiteSpace(cover.PreparedBy))
        {
            AddCenteredText(body, $"Prepared by: {cover.PreparedBy}");
        }

        // Date
        var date = cover.PreparedDate ?? DateTime.UtcNow;
        body.Append(new Paragraph());
        AddCenteredText(body, date.ToString("MMMM dd, yyyy"), "888888");
    }

    private void AddCenteredText(Body body, string text, string? color = null)
    {
        var para = new Paragraph();
        var props = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
        para.Append(props);
        var run = new Run();
        if (color != null)
        {
            run.Append(new RunProperties(new Color { Val = color }));
        }
        run.Append(new Text(text));
        para.Append(run);
        body.Append(para);
    }

    private void AddTableOfContents(Body body, BusinessPlanExportData data)
    {
        AddHeading(body, "Table of Contents", 1);

        var sections = data.SectionsWithVisuals != null && data.SectionsWithVisuals.Any()
            ? data.GetSectionsWithVisuals().Where(s => !string.IsNullOrWhiteSpace(s.Content))
            : data.GetSections().Select((s, i) => new ExportSectionWithVisuals { Title = s.Title, Order = i });

        var sectionNumber = 1;
        foreach (var section in sections)
        {
            AddParagraph(body, $"    {sectionNumber}. {section.Title}");
            sectionNumber++;
        }

        body.Append(new Paragraph()); // Spacing
    }

    private void AddSectionWithVisuals(Body body, ExportSectionWithVisuals section, bool includeVisuals)
    {
        // Section title
        AddHeading(body, section.Title, 1);

        // Prose content
        if (!string.IsNullOrWhiteSpace(section.Content))
        {
            AddParagraph(body, StripHtml(section.Content));
        }

        // Visual elements
        if (includeVisuals && section.VisualElements != null && section.VisualElements.Any())
        {
            foreach (var visual in section.VisualElements)
            {
                body.Append(new Paragraph()); // Spacing
                RenderVisualElement(body, visual);
            }
        }

        body.Append(new Paragraph()); // Spacing between sections
    }

    private void RenderVisualElement(Body body, ExportVisualElementData visual)
    {
        // Visual element title
        if (!string.IsNullOrWhiteSpace(visual.Title))
        {
            AddHeading(body, visual.Title, 2);
        }

        try
        {
            switch (visual.Type.ToLower())
            {
                case "table":
                    RenderTable(body, visual.Data);
                    break;
                case "chart":
                    RenderChartPlaceholder(body, visual);
                    break;
                case "metric":
                    RenderMetrics(body, visual.Data);
                    break;
                case "infographic":
                    RenderInfographic(body, visual.Data);
                    break;
                default:
                    AddParagraph(body, $"[Unknown visual type: {visual.Type}]", isItalic: true);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to render visual element {VisualId} of type {VisualType}", visual.Id, visual.Type);
            AddParagraph(body, $"[Error rendering {visual.Type}]", isItalic: true);
        }
    }

    private void RenderTable(Body body, object? data)
    {
        var tableData = DeserializeData<ExportTableDataModel>(data);
        if (tableData?.Headers == null || tableData.Rows == null)
            return;

        var table = new Table();

        // Table properties with borders
        var tableProperties = new TableProperties(
            new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
                new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
                new RightBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = "CCCCCC" }
            ),
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
        );
        table.Append(tableProperties);

        // Header row
        var headerRow = new TableRow();
        foreach (var header in tableData.Headers)
        {
            var cell = new TableCell();
            cell.Append(new TableCellProperties(new Shading { Fill = "E0E0E0" }));
            var para = new Paragraph();
            var run = new Run();
            run.Append(new RunProperties(new Bold()));
            run.Append(new Text(header));
            para.Append(run);
            cell.Append(para);
            headerRow.Append(cell);
        }
        table.Append(headerRow);

        // Data rows
        var rowIndex = 0;
        foreach (var row in tableData.Rows)
        {
            var tableRow = new TableRow();
            var bgColor = row.IsHighlighted ? "FFF3CD" : rowIndex % 2 == 0 ? "FFFFFF" : "F8F8F8";

            foreach (var cellData in row.Cells)
            {
                var cell = new TableCell();
                cell.Append(new TableCellProperties(new Shading { Fill = bgColor }));
                var para = new Paragraph();
                var run = new Run();
                if (cellData.Format == "bold")
                    run.Append(new RunProperties(new Bold()));
                run.Append(new Text(cellData.Value?.ToString() ?? ""));
                para.Append(run);
                cell.Append(para);
                tableRow.Append(cell);
            }
            table.Append(tableRow);
            rowIndex++;
        }

        body.Append(table);
    }

    private void RenderChartPlaceholder(Body body, ExportVisualElementData visual)
    {
        var chartData = DeserializeData<ExportChartDataModel>(visual.Data);

        var para = new Paragraph();
        var props = new ParagraphProperties(new Shading { Fill = "F0F0F0" });
        props.Append(new SpacingBetweenLines { Before = "120", After = "120" });
        para.Append(props);

        var run = new Run();
        run.Append(new Text($"[{chartData?.ChartType?.ToUpper() ?? "CHART"} Chart"));

        if (chartData?.Datasets != null && chartData.Datasets.Any())
        {
            run.Append(new Text($" - {chartData.Datasets.Count} data series"));
        }

        run.Append(new Text("]"));
        para.Append(run);
        body.Append(para);
    }

    private void RenderMetrics(Body body, object? data)
    {
        var metricData = DeserializeData<ExportMetricDataModel>(data);
        if (metricData?.Metrics == null)
            return;

        // Render metrics as a table for Word
        var table = new Table();
        var tableProperties = new TableProperties(
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }
        );
        table.Append(tableProperties);

        var row = new TableRow();
        foreach (var metric in metricData.Metrics.Take(4))
        {
            var cell = new TableCell();
            cell.Append(new TableCellProperties(new Shading { Fill = "F8F8F8" }));

            // Label
            var labelPara = new Paragraph();
            var labelRun = new Run();
            labelRun.Append(new RunProperties(new FontSize { Val = "18" }, new Color { Val = "666666" }));
            labelRun.Append(new Text(metric.Label));
            labelPara.Append(labelRun);
            cell.Append(labelPara);

            // Value
            var valuePara = new Paragraph();
            var valueRun = new Run();
            valueRun.Append(new RunProperties(new Bold(), new FontSize { Val = "28" }));
            valueRun.Append(new Text(FormatMetricValue(metric)));
            valuePara.Append(valueRun);
            cell.Append(valuePara);

            // Trend
            if (!string.IsNullOrWhiteSpace(metric.TrendValue))
            {
                var trendPara = new Paragraph();
                var trendRun = new Run();
                var trendColor = metric.Trend == "up" ? "22C55E" : metric.Trend == "down" ? "EF4444" : "666666";
                trendRun.Append(new RunProperties(new FontSize { Val = "18" }, new Color { Val = trendColor }));
                trendRun.Append(new Text(metric.TrendValue));
                trendPara.Append(trendRun);
                cell.Append(trendPara);
            }

            row.Append(cell);
        }
        table.Append(row);
        body.Append(table);
    }

    private void RenderInfographic(Body body, object? data)
    {
        var infographicData = DeserializeData<ExportInfographicDataModel>(data);
        if (infographicData?.Items == null)
            return;

        var orderedItems = infographicData.Items.OrderBy(i => i.Order ?? 0);

        var itemNumber = 1;
        foreach (var item in orderedItems)
        {
            var para = new Paragraph();
            var run = new Run();
            run.Append(new RunProperties(new Bold()));
            run.Append(new Text($"{itemNumber}. {item.Title}"));
            para.Append(run);
            body.Append(para);

            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                AddParagraph(body, $"   {item.Description}");
            }

            itemNumber++;
        }
    }

    private void AddStyles(MainDocumentPart mainPart)
    {
        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        stylesPart.Styles = new Styles();

        // Title style
        var titleStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Title",
            CustomStyle = true
        };
        titleStyle.Append(new StyleName { Val = "Title" });
        titleStyle.Append(new StyleRunProperties(
            new Bold(),
            new FontSize { Val = "48" },
            new Color { Val = "1F4E79" }
        ));
        stylesPart.Styles.Append(titleStyle);

        // Heading 1 style
        var heading1Style = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Heading1",
            CustomStyle = true
        };
        heading1Style.Append(new StyleName { Val = "Heading 1" });
        heading1Style.Append(new StyleRunProperties(
            new Bold(),
            new FontSize { Val = "32" },
            new Color { Val = "2E75B6" }
        ));
        stylesPart.Styles.Append(heading1Style);

        // Heading 2 style
        var heading2Style = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Heading2",
            CustomStyle = true
        };
        heading2Style.Append(new StyleName { Val = "Heading 2" });
        heading2Style.Append(new StyleRunProperties(
            new Bold(),
            new FontSize { Val = "28" },
            new Color { Val = "5B9BD5" }
        ));
        stylesPart.Styles.Append(heading2Style);

        stylesPart.Styles.Save();
    }

    private void AddTitle(Body body, string text)
    {
        var paragraph = new Paragraph();
        var run = new Run();
        var runProperties = new RunProperties();
        runProperties.Append(new Bold());
        runProperties.Append(new FontSize { Val = "48" });
        runProperties.Append(new Color { Val = "1F4E79" });
        run.Append(runProperties);
        run.Append(new Text(text));
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private void AddSubtitle(Body body, string text)
    {
        var paragraph = new Paragraph();
        var run = new Run();
        var runProperties = new RunProperties();
        runProperties.Append(new FontSize { Val = "28" });
        runProperties.Append(new Color { Val = "666666" });
        run.Append(runProperties);
        run.Append(new Text(text));
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private void AddHeading(Body body, string text, int level)
    {
        var paragraph = new Paragraph();
        var paragraphProperties = new ParagraphProperties();
        paragraphProperties.Append(new SpacingBetweenLines { Before = "240", After = "120" });
        paragraph.Append(paragraphProperties);

        var run = new Run();
        var runProperties = new RunProperties();
        runProperties.Append(new Bold());
        runProperties.Append(new FontSize { Val = level == 1 ? "32" : "28" });
        runProperties.Append(new Color { Val = level == 1 ? "2E75B6" : "5B9BD5" });
        run.Append(runProperties);
        run.Append(new Text(text));
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private void AddParagraph(Body body, string text, bool isItalic = false)
    {
        var paragraph = new Paragraph();
        var paragraphProperties = new ParagraphProperties();
        paragraphProperties.Append(new SpacingBetweenLines { After = "120", Line = "276", LineRule = LineSpacingRuleValues.Auto });
        paragraph.Append(paragraphProperties);

        var run = new Run();
        if (isItalic)
        {
            var runProperties = new RunProperties();
            runProperties.Append(new Italic());
            runProperties.Append(new Color { Val = "666666" });
            run.Append(runProperties);
        }
        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private void AddHorizontalLine(Body body)
    {
        var paragraph = new Paragraph();
        var paragraphProperties = new ParagraphProperties();
        var pBdr = new ParagraphBorders();
        pBdr.Append(new BottomBorder { Val = BorderValues.Single, Size = 6, Color = "CCCCCC" });
        paragraphProperties.Append(pBdr);
        paragraphProperties.Append(new SpacingBetweenLines { Before = "120", After = "240" });
        paragraph.Append(paragraphProperties);
        body.Append(paragraph);
    }

    private void AddPageBreak(Body body)
    {
        var paragraph = new Paragraph(new Run(new Break { Type = BreakValues.Page }));
        body.Append(paragraph);
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

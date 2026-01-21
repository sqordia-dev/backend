using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Sqordia.Functions.ExportHandler.Models;

namespace Sqordia.Functions.ExportHandler.Services;

/// <summary>
/// Word document export generator using OpenXML
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

            // Title
            AddTitle(body, data.Title);
            AddSubtitle(body, data.OrganizationName);
            AddParagraph(body, $"Version {data.Version} | {data.Status}", isItalic: true);
            AddHorizontalLine(body);

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

            // Content sections
            foreach (var (title, content) in data.GetSections())
            {
                AddHeading(body, title, 1);
                AddParagraph(body, content);
                AddParagraph(body, string.Empty); // Spacing
            }

            wordDocument.Save();
        }

        var bytes = memoryStream.ToArray();
        _logger.LogInformation("Word document generated successfully, size: {Size} bytes", bytes.Length);

        return Task.FromResult(bytes);
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
}

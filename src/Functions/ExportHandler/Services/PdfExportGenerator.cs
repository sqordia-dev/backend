using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sqordia.Functions.ExportHandler.Models;

namespace Sqordia.Functions.ExportHandler.Services;

/// <summary>
/// PDF export generator using QuestPDF
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

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().Element(c => ComposeContent(c, data));
                page.Footer().Element(ComposeFooter);
            });
        });

        var bytes = document.GeneratePdf();
        _logger.LogInformation("PDF generated successfully, size: {Size} bytes", bytes.Length);

        return Task.FromResult(bytes);
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

    private void ComposeContent(IContainer container, BusinessPlanExportData data)
    {
        container.Column(column =>
        {
            // Document info section
            column.Item().Element(c => ComposeDocumentInfo(c, data));
            column.Item().PaddingVertical(15).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Content sections
            foreach (var (title, content) in data.GetSections())
            {
                column.Item().Element(c => ComposeSection(c, title, content));
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
}

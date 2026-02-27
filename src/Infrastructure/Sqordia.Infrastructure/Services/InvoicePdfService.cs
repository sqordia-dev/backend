using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sqordia.Application.Contracts.Responses;
using Sqordia.Application.Services;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Service for generating professional invoice PDFs using QuestPDF
/// </summary>
public class InvoicePdfService : IInvoicePdfService
{
    // Brand colors as Color objects for proper QuestPDF rendering
    private static readonly Color StrategyBlue = Color.FromHex("#1A2B47");
    private static readonly Color MomentumOrange = Color.FromHex("#FF6B00");
    private static readonly Color LightGrey = Color.FromHex("#F4F7FA");
    private static readonly Color PaidGreen = Color.FromHex("#059669");
    private static readonly Color PaidGreenBg = Color.FromHex("#ECFDF5");
    private static readonly Color PendingAmber = Color.FromHex("#D97706");
    private static readonly Color PendingAmberBg = Color.FromHex("#FFFBEB");

    static InvoicePdfService()
    {
        // Configure QuestPDF license (Community license for open source / small business)
        QuestPDF.Settings.License = LicenseType.Community;

        // Enable font fallback for Docker/Linux environments without system fonts
        QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
    }

    public InvoicePdfService()
    {
    }

    public byte[] GenerateInvoicePdf(InvoiceDto invoice, string organizationName, string customerName, string customerEmail)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.MarginVertical(40);
                page.MarginHorizontal(50);
                page.PageColor(Colors.White);

                // Explicitly set font to ensure text renders
                page.DefaultTextStyle(x => x
                    .FontSize(10)
                    .FontColor(Colors.Black));

                page.Header().Element(c => ComposeHeader(c, invoice));
                page.Content().Element(c => ComposeContent(c, invoice, organizationName, customerName, customerEmail));
                page.Footer().Element(c => ComposeFooter(c, invoice));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, InvoiceDto invoice)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                // Logo / Company Name
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("SQORDIA")
                        .FontSize(28)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);
                    col.Item().Text("Business Plan Platform")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });

                // Invoice Badge
                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item()
                        .Background(Colors.Orange.Medium)
                        .Padding(10)
                        .AlignCenter()
                        .Text("INVOICE")
                        .FontSize(16)
                        .Bold()
                        .FontColor(Colors.White);
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeContent(IContainer container, InvoiceDto invoice, string organizationName, string customerName, string customerEmail)
    {
        container.PaddingVertical(20).Column(column =>
        {
            // Invoice Details Row
            column.Item().Row(row =>
            {
                // Bill To
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BILL TO").FontSize(9).FontColor(Colors.Grey.Medium).Bold();
                    col.Item().PaddingTop(5).Text(customerName).Bold().FontSize(12);
                    col.Item().Text(customerEmail).FontSize(10).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrEmpty(organizationName))
                    {
                        col.Item().Text(organizationName).FontSize(10).FontColor(Colors.Grey.Darken1);
                    }
                });

                // Invoice Info
                row.ConstantItem(200).AlignRight().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Invoice #:").FontColor(Colors.Grey.Medium);
                        r.ConstantItem(120).AlignRight().Text(invoice.InvoiceNumber).Bold();
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Text("Issue Date:").FontColor(Colors.Grey.Medium);
                        r.ConstantItem(120).AlignRight().Text(invoice.IssueDate.ToString("MMM dd, yyyy"));
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Text("Due Date:").FontColor(Colors.Grey.Medium);
                        r.ConstantItem(120).AlignRight().Text(invoice.DueDate.ToString("MMM dd, yyyy"));
                    });
                    if (invoice.PaidDate.HasValue)
                    {
                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("Paid Date:").FontColor(Colors.Grey.Medium);
                            r.ConstantItem(120).AlignRight().Text(invoice.PaidDate.Value.ToString("MMM dd, yyyy")).FontColor(MomentumOrange).Bold();
                        });
                    }
                });
            });

            column.Item().PaddingTop(30);

            // Service Period Banner
            column.Item()
                .Background(Colors.Grey.Lighten4)
                .Padding(15)
                .Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("SERVICE PERIOD").FontSize(9).FontColor(Colors.Grey.Medium).Bold();
                        col.Item().PaddingTop(3).Text($"{invoice.PeriodStart:MMM dd, yyyy} - {invoice.PeriodEnd:MMM dd, yyyy}")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);
                    });
                });

            column.Item().PaddingTop(20);

            // Items Table
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); // Description
                    columns.ConstantColumn(100); // Qty
                    columns.ConstantColumn(100); // Unit Price
                    columns.ConstantColumn(100); // Amount
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Background(Colors.Blue.Darken3).Text("Description").FontColor(Colors.White).Bold();
                    header.Cell().Element(CellStyle).Background(Colors.Blue.Darken3).AlignCenter().Text("Qty").FontColor(Colors.White).Bold();
                    header.Cell().Element(CellStyle).Background(Colors.Blue.Darken3).AlignRight().Text("Unit Price").FontColor(Colors.White).Bold();
                    header.Cell().Element(CellStyle).Background(Colors.Blue.Darken3).AlignRight().Text("Amount").FontColor(Colors.White).Bold();

                    static IContainer CellStyle(IContainer container) =>
                        container.DefaultTextStyle(x => x.FontSize(10)).Padding(10);
                });

                // Item Row
                table.Cell().Element(CellStyle).Text(invoice.Description);
                table.Cell().Element(CellStyle).AlignCenter().Text("1");
                table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(invoice.Subtotal, invoice.Currency));
                table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(invoice.Subtotal, invoice.Currency));

                static IContainer CellStyle(IContainer container) =>
                    container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10);
            });

            column.Item().PaddingTop(20);

            // Totals Section
            column.Item().AlignRight().Width(250).Column(totalsColumn =>
            {
                totalsColumn.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:").FontColor(Colors.Grey.Darken1);
                    row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.Subtotal, invoice.Currency));
                });

                if (invoice.Tax > 0)
                {
                    totalsColumn.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Tax:").FontColor(Colors.Grey.Darken1);
                        row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.Tax, invoice.Currency));
                    });
                }

                totalsColumn.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                totalsColumn.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Text("Total:").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
                    row.ConstantItem(100).AlignRight().Text(FormatCurrency(invoice.Total, invoice.Currency))
                        .Bold()
                        .FontSize(14)
                        .FontColor(Colors.Orange.Darken1);
                });

                // Payment Status Badge
                totalsColumn.Item().PaddingTop(15).AlignRight().Element(c =>
                {
                    var statusColor = invoice.Status.ToLower() == "paid" ? PaidGreen : PendingAmber;
                    var statusBg = invoice.Status.ToLower() == "paid" ? PaidGreenBg : PendingAmberBg;

                    c.Background(statusBg)
                        .Border(1)
                        .BorderColor(statusColor)
                        .Padding(8)
                        .PaddingHorizontal(15)
                        .Text(invoice.Status.ToUpper())
                        .FontSize(11)
                        .Bold()
                        .FontColor(statusColor);
                });
            });

            column.Item().PaddingTop(40);

            // Payment Information
            column.Item().Background(Colors.Grey.Lighten4).Padding(20).Column(paymentCol =>
            {
                paymentCol.Item().Text("PAYMENT INFORMATION").FontSize(10).Bold().FontColor(Colors.Blue.Darken3);
                paymentCol.Item().PaddingTop(10).Text("Payment processed securely via Stripe.")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
                paymentCol.Item().PaddingTop(5).Text("For questions about this invoice, please contact support@sqordia.com")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeFooter(IContainer container, InvoiceDto invoice)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Sqordia Inc.")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                    col.Item().Text("Montreal, Quebec, Canada")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });

                row.RelativeItem().AlignCenter().Text(text =>
                {
                    text.Span("Page ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                    text.Span(" of ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("www.sqordia.com")
                        .FontSize(9)
                        .FontColor(Colors.Orange.Medium);
                    col.Item().Text($"Invoice #{invoice.InvoiceNumber}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
            });
        });
    }

    private static string FormatCurrency(decimal amount, string currency)
    {
        return currency.ToUpper() switch
        {
            "USD" => $"${amount:N2} USD",
            "EUR" => $"€{amount:N2}",
            "GBP" => $"£{amount:N2}",
            "CAD" => $"${amount:N2} CAD",
            _ => $"${amount:N2} {currency}"
        };
    }
}

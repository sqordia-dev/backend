using Sqordia.Application.Contracts.Responses;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for generating invoice PDFs
/// </summary>
public interface IInvoicePdfService
{
    /// <summary>
    /// Generate a PDF document for an invoice
    /// </summary>
    /// <param name="invoice">Invoice data</param>
    /// <param name="organizationName">Organization name for the invoice</param>
    /// <param name="customerName">Customer name</param>
    /// <param name="customerEmail">Customer email</param>
    /// <returns>PDF document as byte array</returns>
    byte[] GenerateInvoicePdf(InvoiceDto invoice, string organizationName, string customerName, string customerEmail);
}

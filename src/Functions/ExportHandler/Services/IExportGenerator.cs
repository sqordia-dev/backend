using Sqordia.Functions.ExportHandler.Models;

namespace Sqordia.Functions.ExportHandler.Services;

/// <summary>
/// Interface for document export generators
/// </summary>
public interface IExportGenerator
{
    /// <summary>
    /// The export type this generator handles (pdf, word, excel)
    /// </summary>
    string ExportType { get; }

    /// <summary>
    /// The content type for the generated document
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// The file extension for the generated document
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Generates the document bytes from business plan data
    /// </summary>
    Task<byte[]> GenerateAsync(BusinessPlanExportData data, string language, CancellationToken cancellationToken = default);
}

using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Services;

/// <summary>
/// Generates themed PDFs via Puppeteer (headless Chromium), producing PDFs with
/// selectable text and proper page breaks that match the frontend preview exactly.
/// </summary>
public interface IThemedPdfService
{
    /// <summary>
    /// Generate a themed PDF for a business plan.
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="themeId">Theme ID (e.g. "classic", "modern"). Defaults to "classic".</param>
    /// <param name="language">Language code ("fr" or "en")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with the PDF export data</returns>
    Task<Result<ExportResult>> GenerateThemedPdfAsync(
        Guid businessPlanId,
        string? themeId,
        string language = "fr",
        CancellationToken cancellationToken = default);
}

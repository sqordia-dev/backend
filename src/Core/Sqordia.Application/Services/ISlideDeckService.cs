using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Export;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for generating full PowerPoint slide deck presentations from business plans.
/// </summary>
public interface ISlideDeckService
{
    /// <summary>
    /// Generates a complete PPTX slide deck with AI-summarized content slides.
    /// </summary>
    Task<Result<ExportResult>> GenerateSlideDeckAsync(
        Guid businessPlanId, string? themeId, string language, CancellationToken ct = default);
}

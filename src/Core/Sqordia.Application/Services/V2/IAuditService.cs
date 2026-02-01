using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.V2.Audit;

namespace Sqordia.Application.Services.V2;

/// <summary>
/// Socratic Coach audit service - returns Nudge + Triad for sections
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Performs Socratic Coach audit on a business plan section
    /// Returns a Nudge (probing question) and Triad (3 smart suggestions)
    /// </summary>
    Task<Result<AuditSectionResponse>> AuditSectionAsync(
        Guid businessPlanId,
        string sectionName,
        string language = "fr",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit summary for entire business plan
    /// </summary>
    Task<Result<AuditSummaryResponse>> GetAuditSummaryAsync(
        Guid businessPlanId,
        string language = "fr",
        CancellationToken cancellationToken = default);
}

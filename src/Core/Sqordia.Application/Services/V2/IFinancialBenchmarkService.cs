using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.V2.Benchmark;

namespace Sqordia.Application.Services.V2;

/// <summary>
/// Financial benchmark comparison service
/// Compares business plan metrics against industry standards
/// </summary>
public interface IFinancialBenchmarkService
{
    /// <summary>
    /// Compares a business plan's financial metrics to industry benchmarks
    /// </summary>
    Task<Result<BenchmarkComparisonResponse>> CompareToBenchmarksAsync(
        Guid businessPlanId,
        string? industryCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available industry benchmarks
    /// </summary>
    Task<Result<IEnumerable<IndustryBenchmarkResponse>>> GetAvailableBenchmarksAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets benchmark data for a specific industry
    /// </summary>
    Task<Result<IndustryBenchmarkResponse>> GetBenchmarkByIndustryAsync(
        string industryCode,
        CancellationToken cancellationToken = default);
}

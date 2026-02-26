using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services.V2;

namespace WebAPI.Controllers;

/// <summary>
/// Financial benchmark comparison endpoints
/// Compares business plan metrics against industry standards
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/benchmarks")]
[Authorize]
public class BenchmarkController : BaseApiController
{
    private readonly IFinancialBenchmarkService _benchmarkService;
    private readonly ILogger<BenchmarkController> _logger;

    public BenchmarkController(
        IFinancialBenchmarkService benchmarkService,
        ILogger<BenchmarkController> logger)
    {
        _benchmarkService = benchmarkService;
        _logger = logger;
    }

    /// <summary>
    /// Get all available industry benchmarks
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableBenchmarks(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting available industry benchmarks");

        var result = await _benchmarkService.GetAvailableBenchmarksAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get benchmark data for a specific industry
    /// </summary>
    /// <param name="industryCode">Industry code: SAAS, CONSULTING, RETAIL, RESTAURANT, NONPROFIT, MANUFACTURING</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("{industryCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBenchmarkByIndustry(
        string industryCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting benchmark for industry {Industry}", industryCode);

        var result = await _benchmarkService.GetBenchmarkByIndustryAsync(industryCode, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Compare a business plan against industry benchmarks
    /// </summary>
    /// <param name="businessPlanId">The business plan ID</param>
    /// <param name="industryCode">Optional industry code (auto-detected if not provided)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("compare/{businessPlanId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompareToBenchmarks(
        Guid businessPlanId,
        [FromQuery] string? industryCode = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Comparing business plan {PlanId} to benchmarks for industry {Industry}",
            businessPlanId, industryCode ?? "auto-detect");

        var result = await _benchmarkService.CompareToBenchmarksAsync(businessPlanId, industryCode, cancellationToken);
        return HandleResult(result);
    }
}

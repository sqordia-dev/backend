using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;

namespace WebAPI.Controllers;

/// <summary>
/// Admin endpoints for ML monitoring, training, and quality drift detection.
/// </summary>
[ApiController]
[Route("api/v1/admin/ml")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminMLController : ControllerBase
{
    private readonly IMLPredictionService _mlPrediction;
    private readonly ILogger<AdminMLController> _logger;

    public AdminMLController(
        IMLPredictionService mlPrediction,
        ILogger<AdminMLController> logger)
    {
        _mlPrediction = mlPrediction;
        _logger = logger;
    }

    /// <summary>Check for quality drift across sections and models.</summary>
    [HttpGet("quality-drift")]
    public async Task<IActionResult> GetQualityDrift(CancellationToken cancellationToken)
    {
        var report = await _mlPrediction.CheckQualityDriftAsync(cancellationToken);
        return Ok(report);
    }

    /// <summary>Get learned preferences for a section type.</summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(
        [FromQuery] string sectionType,
        [FromQuery] string? industry = null,
        [FromQuery] string language = "fr",
        CancellationToken cancellationToken = default)
    {
        var prefs = await _mlPrediction.GetLearnedPreferencesAsync(
            sectionType, industry, language, cancellationToken);
        return Ok(prefs);
    }

    /// <summary>Trigger model training (re-calibration of quality prediction model).</summary>
    [HttpPost("train")]
    public async Task<IActionResult> TriggerTraining(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin triggered ML model training");
        var result = await _mlPrediction.TriggerTrainingAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Predict quality for a hypothetical section (testing endpoint).</summary>
    [HttpPost("predict-quality")]
    public async Task<IActionResult> PredictQuality(
        [FromBody] QualityPredictionRequest request,
        CancellationToken cancellationToken)
    {
        var prediction = await _mlPrediction.PredictQualityAsync(request, cancellationToken);
        return Ok(prediction);
    }
}

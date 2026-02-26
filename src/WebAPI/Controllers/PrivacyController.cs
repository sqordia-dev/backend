using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Privacy;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for privacy-related operations (Quebec Bill 25 compliance)
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/privacy")]
[Authorize]
public class PrivacyController : BaseApiController
{
    private readonly IPrivacyService _privacyService;

    public PrivacyController(IPrivacyService privacyService)
    {
        _privacyService = privacyService;
    }

    /// <summary>
    /// Export user's personal data as JSON (Bill 25 - data portability)
    /// </summary>
    /// <returns>User's personal data in machine-readable format</returns>
    [HttpGet("export")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportData(CancellationToken cancellationToken = default)
    {
        var result = await _privacyService.ExportUserDataAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete user account (deactivate or permanent deletion)
    /// </summary>
    /// <param name="request">Account deletion request with type and password confirmation</param>
    /// <returns>Deletion confirmation with details</returns>
    [HttpPost("delete-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount([FromBody] AccountDeletionRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _privacyService.DeleteAccountAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get current consent status for Terms of Service and Privacy Policy
    /// </summary>
    /// <returns>List of consent items with their acceptance status</returns>
    [HttpGet("consents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConsents(CancellationToken cancellationToken = default)
    {
        var result = await _privacyService.GetConsentsAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update consent (accept Terms of Service or Privacy Policy)
    /// </summary>
    /// <param name="request">Consent update request with type and version</param>
    /// <returns>Updated consent status</returns>
    [HttpPut("consents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateConsent([FromBody] UpdateConsentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _privacyService.UpdateConsentAsync(request, cancellationToken);
        return HandleResult(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.User;

namespace WebAPI.Controllers;

/// <summary>
/// User management endpoints
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/user")]
[Authorize]
public class UserController : BaseApiController
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserProfileService userProfileService,
        ILogger<UserController> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
    }

    /// <summary>
    /// Set the user's persona type (Entrepreneur, Consultant, or OBNL)
    /// </summary>
    /// <param name="request">Persona selection request</param>
    /// <returns>Updated user profile</returns>
    [HttpPost("persona")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetPersona([FromBody] SetPersonaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Setting persona to {Persona} for user", request.Persona);

        var result = await _userProfileService.SetPersonaAsync(request);
        return HandleResult(result);
    }
}

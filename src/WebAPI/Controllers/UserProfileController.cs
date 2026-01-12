using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.User;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/profile")]
public class UserProfileController : BaseApiController
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// Get the current user's profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userProfileService.GetProfileAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Update the current user's profile
    /// </summary>
    /// <param name="request">Profile update request</param>
    /// <returns>Updated user profile</returns>
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await _userProfileService.UpdateProfileAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Upload a profile picture
    /// </summary>
    /// <param name="file">Profile picture file</param>
    /// <returns>URL of the uploaded profile picture</returns>
    [HttpPost("upload-picture")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var result = await _userProfileService.UploadProfilePictureAsync(file);
        return HandleResult(result);
    }

    /// <summary>
    /// Change the current user's password
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Success or failure</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _userProfileService.ChangePasswordAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete the current user's account (soft delete)
    /// </summary>
    /// <returns>Success or failure</returns>
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        var result = await _userProfileService.DeleteAccountAsync();
        return HandleResult(result);
    }
}


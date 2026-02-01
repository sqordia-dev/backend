using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.User;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/profile")]
public class UserProfileController : BaseApiController
{
    private readonly IUserProfileService _userProfileService;
    private readonly IStorageService _storageService;

    public UserProfileController(
        IUserProfileService userProfileService,
        IStorageService storageService)
    {
        _userProfileService = userProfileService;
        _storageService = storageService;
    }

    /// <summary>
    /// Get the current user's profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken = default)
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
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken = default)
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
    public async Task<IActionResult> UploadProfilePicture(IFormFile file, CancellationToken cancellationToken = default)
    {
        var result = await _userProfileService.UploadProfilePictureAsync(file);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a profile picture by storage key
    /// </summary>
    /// <param name="key">The storage key (e.g., profile-pictures/userId/filename.jpg)</param>
    /// <returns>The profile picture image file</returns>
    [HttpGet("picture/{*key}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProfilePicture(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate that the key is for a profile picture
            if (!key.StartsWith("profile-pictures/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Invalid picture key" });
            }

            // Check if file exists
            if (!await _storageService.FileExistsAsync(key))
            {
                return NotFound(new { error = "Picture not found" });
            }

            // Download the file from storage
            var fileStream = await _storageService.DownloadFileAsync(key);

            // Determine content type from file extension
            var extension = Path.GetExtension(key).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return File(fileStream, contentType);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "Picture not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error retrieving picture", details = ex.Message });
        }
    }

    /// <summary>
    /// Change the current user's password
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Success or failure</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
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
    public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken = default)
    {
        var result = await _userProfileService.DeleteAccountAsync();
        return HandleResult(result);
    }
}


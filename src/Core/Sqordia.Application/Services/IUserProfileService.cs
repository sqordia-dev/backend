using Microsoft.AspNetCore.Http;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.User;
using Sqordia.Contracts.Responses.User;

namespace Sqordia.Application.Services;

public interface IUserProfileService
{
    Task<Result<UserProfileResponse>> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<Result<UserProfileResponse>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<string>> UploadProfilePictureAsync(IFormFile file, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAccountAsync(CancellationToken cancellationToken = default);
    Task<Result<UserProfileResponse>> SetPersonaAsync(SetPersonaRequest request, CancellationToken cancellationToken = default);
}


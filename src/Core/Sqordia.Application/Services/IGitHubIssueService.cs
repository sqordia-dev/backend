using Microsoft.AspNetCore.Http;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.GitHub;
using Sqordia.Contracts.Responses.GitHub;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for creating GitHub issues from bug reports.
/// </summary>
public interface IGitHubIssueService
{
    /// <summary>
    /// Creates a new GitHub issue in the specified repository.
    /// </summary>
    Task<Result<GitHubIssueResponse>> CreateIssueAsync(
        CreateGitHubIssueRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a screenshot and returns the URL.
    /// </summary>
    Task<Result<string>> UploadScreenshotAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);
}

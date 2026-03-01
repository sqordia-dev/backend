using Microsoft.AspNetCore.Http;
using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.GitHub;
using Sqordia.Contracts.Responses.GitHub;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for creating and managing GitHub issues from bug reports.
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
    /// Lists GitHub issues with filtering and pagination.
    /// </summary>
    Task<Result<GitHubIssueListResponse>> ListIssuesAsync(
        ListGitHubIssuesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single GitHub issue by number.
    /// </summary>
    Task<Result<GitHubIssueDetailResponse>> GetIssueAsync(
        string repository,
        int issueNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets issue statistics (counts by state, priority, etc.)
    /// </summary>
    Task<Result<GitHubIssueStatsResponse>> GetIssueStatsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the state of a GitHub issue (open/close).
    /// </summary>
    Task<Result<GitHubIssueDetailResponse>> UpdateIssueStateAsync(
        string repository,
        int issueNumber,
        string state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a GitHub issue (title, body, labels, state).
    /// </summary>
    Task<Result<GitHubIssueDetailResponse>> UpdateIssueAsync(
        string repository,
        int issueNumber,
        UpdateGitHubIssueRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives a GitHub issue (closes it and adds 'archived' label).
    /// Note: GitHub API doesn't support deleting issues, so we archive instead.
    /// </summary>
    Task<Result<GitHubIssueDetailResponse>> ArchiveIssueAsync(
        string repository,
        int issueNumber,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a screenshot and returns the URL.
    /// </summary>
    Task<Result<string>> UploadScreenshotAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about GitHub issues
/// </summary>
public class GitHubIssueStatsResponse
{
    public int TotalOpen { get; set; }
    public int TotalClosed { get; set; }
    public int FrontendOpen { get; set; }
    public int FrontendClosed { get; set; }
    public int BackendOpen { get; set; }
    public int BackendClosed { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int LowCount { get; set; }
}

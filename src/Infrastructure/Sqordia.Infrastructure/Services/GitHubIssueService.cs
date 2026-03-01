using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.GitHub;
using Sqordia.Contracts.Responses.GitHub;
using Sqordia.Infrastructure.Settings;

namespace Sqordia.Infrastructure.Services;

public class GitHubIssueService : IGitHubIssueService
{
    private readonly HttpClient _httpClient;
    private readonly GitHubSettings _settings;
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GitHubIssueService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public GitHubIssueService(
        HttpClient httpClient,
        IOptions<GitHubSettings> settings,
        IStorageService storageService,
        ICurrentUserService currentUserService,
        ILogger<GitHubIssueService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _storageService = storageService;
        _currentUserService = currentUserService;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.github.com/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Sqordia", "1.0"));
        _httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        if (!string.IsNullOrEmpty(_settings.PersonalAccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.PersonalAccessToken);
        }
    }

    public async Task<Result<GitHubIssueResponse>> CreateIssueAsync(
        CreateGitHubIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.PersonalAccessToken))
            {
                _logger.LogError("GitHub Personal Access Token is not configured");
                return Result.Failure<GitHubIssueResponse>(
                    Error.Failure("GitHub.NotConfigured", "GitHub integration is not configured"));
            }

            var (owner, repo) = GetRepoInfo(request.Repository);
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                return Result.Failure<GitHubIssueResponse>(
                    Error.Validation("GitHub.InvalidRepository", $"Repository '{request.Repository}' is not configured"));
            }

            var issueBody = BuildIssueBody(request);
            var labels = BuildLabels(request);

            var issueRequest = new GitHubCreateIssueRequest
            {
                Title = request.Title,
                Body = issueBody,
                Labels = labels
            };

            var url = $"repos/{owner}/{repo}/issues";
            var response = await _httpClient.PostAsJsonAsync(url, issueRequest, JsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("GitHub API error: {StatusCode} - {Content}", response.StatusCode, errorContent);

                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized =>
                        Result.Failure<GitHubIssueResponse>(
                            Error.Unauthorized("GitHub.Unauthorized", "GitHub authentication failed - check token configuration")),
                    System.Net.HttpStatusCode.Forbidden when errorContent.Contains("rate limit", StringComparison.OrdinalIgnoreCase) =>
                        Result.Failure<GitHubIssueResponse>(
                            Error.Failure("GitHub.RateLimited", "GitHub API rate limit exceeded, try again later")),
                    System.Net.HttpStatusCode.Forbidden =>
                        Result.Failure<GitHubIssueResponse>(
                            Error.Forbidden("GitHub.Forbidden", "Insufficient permissions to create issues in this repository")),
                    System.Net.HttpStatusCode.NotFound =>
                        Result.Failure<GitHubIssueResponse>(
                            Error.NotFound("GitHub.RepoNotFound", $"Repository '{owner}/{repo}' not found")),
                    System.Net.HttpStatusCode.UnprocessableEntity =>
                        Result.Failure<GitHubIssueResponse>(
                            Error.Validation("GitHub.InvalidData", "Invalid issue data provided")),
                    _ =>
                        Result.Failure<GitHubIssueResponse>(
                            Error.Failure("GitHub.ApiError", $"GitHub API error: {response.StatusCode}"))
                };
            }

            var gitHubIssue = await response.Content.ReadFromJsonAsync<GitHubIssueApiResponse>(JsonOptions, cancellationToken);
            if (gitHubIssue == null)
            {
                return Result.Failure<GitHubIssueResponse>(
                    Error.Failure("GitHub.ParseError", "Failed to parse GitHub API response"));
            }

            var issueResponse = new GitHubIssueResponse
            {
                IssueNumber = gitHubIssue.Number,
                IssueUrl = gitHubIssue.Url,
                HtmlUrl = gitHubIssue.HtmlUrl,
                Title = gitHubIssue.Title,
                State = gitHubIssue.State,
                Repository = request.Repository,
                CreatedAt = gitHubIssue.CreatedAt
            };

            _logger.LogInformation("Created GitHub issue #{IssueNumber} in {Repo}",
                issueResponse.IssueNumber, $"{owner}/{repo}");

            return Result.Success(issueResponse);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while creating GitHub issue");
            return Result.Failure<GitHubIssueResponse>(
                Error.Failure("GitHub.NetworkError", "Failed to connect to GitHub API"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating GitHub issue");
            return Result.Failure<GitHubIssueResponse>(
                Error.InternalServerError("GitHub.UnexpectedError", "An unexpected error occurred"));
        }
    }

    public async Task<Result<string>> UploadScreenshotAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var allowedTypes = new[] { "image/png", "image/jpeg", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return Result.Failure<string>(
                    Error.Validation("Screenshot.InvalidType", "Only PNG, JPEG, GIF, and WebP images are allowed"));
            }

            const long maxSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxSize)
            {
                return Result.Failure<string>(
                    Error.Validation("Screenshot.TooLarge", "Screenshot must be less than 10MB"));
            }

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"bug-reports/{Guid.NewGuid()}{extension}";

            using var stream = file.OpenReadStream();
            var url = await _storageService.UploadFileAsync(fileName, stream, file.ContentType, cancellationToken);

            _logger.LogInformation("Uploaded screenshot: {FileName}", fileName);
            return Result.Success(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading screenshot");
            return Result.Failure<string>(
                Error.Failure("Screenshot.UploadFailed", "Failed to upload screenshot"));
        }
    }

    public async Task<Result<GitHubIssueListResponse>> ListIssuesAsync(
        ListGitHubIssuesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.PersonalAccessToken))
            {
                return Result.Failure<GitHubIssueListResponse>(
                    Error.Failure("GitHub.NotConfigured", "GitHub integration is not configured"));
            }

            var allIssues = new List<GitHubIssueListItem>();
            var repositories = GetRepositoriesToQuery(request.Repository);

            foreach (var (repoKey, owner, repo) in repositories)
            {
                var issues = await FetchIssuesFromRepo(owner, repo, repoKey, request, cancellationToken);
                allIssues.AddRange(issues);
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLowerInvariant();
                allIssues = allIssues.Where(i =>
                    i.Title.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                    i.IssueNumber.ToString().Contains(searchLower)).ToList();
            }

            // Sort
            allIssues = request.Sort.ToLowerInvariant() switch
            {
                "updated" => request.Direction == "asc"
                    ? allIssues.OrderBy(i => i.UpdatedAt ?? i.CreatedAt).ToList()
                    : allIssues.OrderByDescending(i => i.UpdatedAt ?? i.CreatedAt).ToList(),
                _ => request.Direction == "asc"
                    ? allIssues.OrderBy(i => i.CreatedAt).ToList()
                    : allIssues.OrderByDescending(i => i.CreatedAt).ToList()
            };

            var totalCount = allIssues.Count;
            var pagedIssues = allIssues
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Result.Success(new GitHubIssueListResponse
            {
                Issues = pagedIssues,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                HasMore = request.Page * request.PageSize < totalCount
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while listing GitHub issues");
            return Result.Failure<GitHubIssueListResponse>(
                Error.Failure("GitHub.NetworkError", "Failed to connect to GitHub API"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while listing GitHub issues");
            return Result.Failure<GitHubIssueListResponse>(
                Error.InternalServerError("GitHub.UnexpectedError", "An unexpected error occurred"));
        }
    }

    public async Task<Result<GitHubIssueDetailResponse>> GetIssueAsync(
        string repository,
        int issueNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.PersonalAccessToken))
            {
                return Result.Failure<GitHubIssueDetailResponse>(
                    Error.Failure("GitHub.NotConfigured", "GitHub integration is not configured"));
            }

            var (owner, repo) = GetRepoInfo(repository);
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                return Result.Failure<GitHubIssueDetailResponse>(
                    Error.Validation("GitHub.InvalidRepository", $"Repository '{repository}' is not configured"));
            }

            var url = $"repos/{owner}/{repo}/issues/{issueNumber}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Result.Failure<GitHubIssueDetailResponse>(
                        Error.NotFound("GitHub.IssueNotFound", $"Issue #{issueNumber} not found"));
                }
                return Result.Failure<GitHubIssueDetailResponse>(
                    Error.Failure("GitHub.ApiError", $"GitHub API error: {response.StatusCode}"));
            }

            var issue = await response.Content.ReadFromJsonAsync<GitHubIssueDetailApiResponse>(JsonOptions, cancellationToken);
            if (issue == null)
            {
                return Result.Failure<GitHubIssueDetailResponse>(
                    Error.Failure("GitHub.ParseError", "Failed to parse GitHub API response"));
            }

            return Result.Success(MapToDetailResponse(issue, repository));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub issue #{IssueNumber}", issueNumber);
            return Result.Failure<GitHubIssueDetailResponse>(
                Error.InternalServerError("GitHub.UnexpectedError", "An unexpected error occurred"));
        }
    }

    public async Task<Result<GitHubIssueStatsResponse>> GetIssueStatsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.PersonalAccessToken))
            {
                return Result.Failure<GitHubIssueStatsResponse>(
                    Error.Failure("GitHub.NotConfigured", "GitHub integration is not configured"));
            }

            var stats = new GitHubIssueStatsResponse();
            var repositories = GetRepositoriesToQuery("all");

            foreach (var (repoKey, owner, repo) in repositories)
            {
                // Fetch open issues
                var openRequest = new ListGitHubIssuesRequest { State = "open", PageSize = 100 };
                var openIssues = await FetchIssuesFromRepo(owner, repo, repoKey, openRequest, cancellationToken);

                // Fetch closed issues (limited)
                var closedRequest = new ListGitHubIssuesRequest { State = "closed", PageSize = 100 };
                var closedIssues = await FetchIssuesFromRepo(owner, repo, repoKey, closedRequest, cancellationToken);

                if (repoKey == "frontend")
                {
                    stats.FrontendOpen = openIssues.Count;
                    stats.FrontendClosed = closedIssues.Count;
                }
                else
                {
                    stats.BackendOpen = openIssues.Count;
                    stats.BackendClosed = closedIssues.Count;
                }

                // Count priorities from open issues
                foreach (var issue in openIssues)
                {
                    switch (issue.Priority?.ToLowerInvariant())
                    {
                        case "critical": stats.CriticalCount++; break;
                        case "high": stats.HighCount++; break;
                        case "medium": stats.MediumCount++; break;
                        case "low": stats.LowCount++; break;
                    }
                }
            }

            stats.TotalOpen = stats.FrontendOpen + stats.BackendOpen;
            stats.TotalClosed = stats.FrontendClosed + stats.BackendClosed;

            return Result.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub issue stats");
            return Result.Failure<GitHubIssueStatsResponse>(
                Error.InternalServerError("GitHub.UnexpectedError", "An unexpected error occurred"));
        }
    }

    public async Task<Result<GitHubIssueDetailResponse>> UpdateIssueStateAsync(
        string repository,
        int issueNumber,
        string state,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.PersonalAccessToken))
            {
                return Result.Failure<GitHubIssueDetailResponse>(
                    Error.Failure("GitHub.NotConfigured", "GitHub integration is not configured"));
            }

            var validStates = new[] { "open", "closed" };
            if (!validStates.Contains(state.ToLowerInvariant()))
            {
                return Result.Failure<GitHubIssueDetailResponse>(
                    Error.Validation("GitHub.InvalidState", "State must be 'open' or 'closed'"));
            }

            var (owner, repo) = GetRepoInfo(repository);
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                return Result.Failure<GitHubIssueDetailResponse>(
                    Error.Validation("GitHub.InvalidRepository", $"Repository '{repository}' is not configured"));
            }

            var url = $"repos/{owner}/{repo}/issues/{issueNumber}";
            var payload = new { state = state.ToLowerInvariant() };

            var response = await _httpClient.PatchAsJsonAsync(url, payload, JsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("GitHub API error updating issue: {StatusCode} - {Content}", response.StatusCode, errorContent);

                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound =>
                        Result.Failure<GitHubIssueDetailResponse>(
                            Error.NotFound("GitHub.IssueNotFound", $"Issue #{issueNumber} not found")),
                    System.Net.HttpStatusCode.Forbidden =>
                        Result.Failure<GitHubIssueDetailResponse>(
                            Error.Forbidden("GitHub.Forbidden", "Insufficient permissions to update this issue")),
                    _ =>
                        Result.Failure<GitHubIssueDetailResponse>(
                            Error.Failure("GitHub.ApiError", $"GitHub API error: {response.StatusCode}"))
                };
            }

            var issue = await response.Content.ReadFromJsonAsync<GitHubIssueDetailApiResponse>(JsonOptions, cancellationToken);
            if (issue == null)
            {
                return Result.Failure<GitHubIssueDetailResponse>(
                    Error.Failure("GitHub.ParseError", "Failed to parse GitHub API response"));
            }

            _logger.LogInformation("Updated GitHub issue #{IssueNumber} state to {State} in {Repo}",
                issueNumber, state, $"{owner}/{repo}");

            return Result.Success(MapToDetailResponse(issue, repository));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating GitHub issue #{IssueNumber}", issueNumber);
            return Result.Failure<GitHubIssueDetailResponse>(
                Error.InternalServerError("GitHub.UnexpectedError", "An unexpected error occurred"));
        }
    }


    public async Task<Result<GitHubIssueDetailResponse>> UpdateIssueAsync(
        string repository,
        int issueNumber,
        UpdateGitHubIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.PersonalAccessToken))
                return Result.Failure<GitHubIssueDetailResponse>(Error.Failure("GitHub.NotConfigured", "GitHub integration is not configured"));

            var (owner, repo) = GetRepoInfo(repository);
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
                return Result.Failure<GitHubIssueDetailResponse>(Error.Validation("GitHub.InvalidRepository", $"Repository '{repository}' is not configured"));

            var getResult = await GetIssueAsync(repository, issueNumber, cancellationToken);
            if (!getResult.IsSuccess) return getResult;

            var currentIssue = getResult.Value!;
            var url = $"repos/{owner}/{repo}/issues/{issueNumber}";
            var updatePayload = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(request.Title)) updatePayload["title"] = request.Title;
            if (!string.IsNullOrEmpty(request.Body)) updatePayload["body"] = request.Body;

            if (!string.IsNullOrEmpty(request.State))
            {
                var validStates = new[] { "open", "closed" };
                if (!validStates.Contains(request.State.ToLowerInvariant()))
                    return Result.Failure<GitHubIssueDetailResponse>(Error.Validation("GitHub.InvalidState", "State must be 'open' or 'closed'"));
                updatePayload["state"] = request.State.ToLowerInvariant();
            }

            if (!string.IsNullOrEmpty(request.Priority) || !string.IsNullOrEmpty(request.Category))
            {
                var labels = currentIssue.Labels?.Select(l => l.Name).ToList() ?? new List<string>();
                if (!string.IsNullOrEmpty(request.Priority))
                {
                    labels.RemoveAll(l => l.StartsWith("priority:", StringComparison.OrdinalIgnoreCase));
                    labels.Add($"priority: {request.Priority.ToLowerInvariant()}");
                }
                if (!string.IsNullOrEmpty(request.Category))
                {
                    var categoryLabels = new[] { "bug", "enhancement", "documentation", "performance", "feature" };
                    labels.RemoveAll(l => categoryLabels.Contains(l.ToLowerInvariant()));
                    var categoryLabel = request.Category.ToLowerInvariant() switch
                    {
                        "bug" => "bug", "feature" => "enhancement", "enhancement" => "enhancement",
                        "documentation" => "documentation", "performance" => "performance",
                        _ => request.Category.ToLowerInvariant()
                    };
                    labels.Add(categoryLabel);
                }
                updatePayload["labels"] = labels;
            }

            if (updatePayload.Count == 0)
                return Result.Failure<GitHubIssueDetailResponse>(Error.Validation("GitHub.NoChanges", "No valid fields provided for update"));

            var response = await _httpClient.PatchAsJsonAsync(url, updatePayload, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("GitHub API error updating issue: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => Result.Failure<GitHubIssueDetailResponse>(Error.NotFound("GitHub.IssueNotFound", $"Issue #{issueNumber} not found")),
                    System.Net.HttpStatusCode.Forbidden => Result.Failure<GitHubIssueDetailResponse>(Error.Forbidden("GitHub.Forbidden", "Insufficient permissions to update this issue")),
                    _ => Result.Failure<GitHubIssueDetailResponse>(Error.Failure("GitHub.ApiError", $"GitHub API error: {response.StatusCode}"))
                };
            }

            var issue = await response.Content.ReadFromJsonAsync<GitHubIssueDetailApiResponse>(JsonOptions, cancellationToken);
            if (issue == null)
                return Result.Failure<GitHubIssueDetailResponse>(Error.Failure("GitHub.ParseError", "Failed to parse GitHub API response"));

            _logger.LogInformation("Updated GitHub issue #{IssueNumber} in {Repo}", issueNumber, $"{owner}/{repo}");
            return Result.Success(MapToDetailResponse(issue, repository));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating GitHub issue #{IssueNumber}", issueNumber);
            return Result.Failure<GitHubIssueDetailResponse>(Error.InternalServerError("GitHub.UnexpectedError", "An unexpected error occurred"));
        }
    }

    public async Task<Result<GitHubIssueDetailResponse>> ArchiveIssueAsync(
        string repository, int issueNumber, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.PersonalAccessToken))
                return Result.Failure<GitHubIssueDetailResponse>(Error.Failure("GitHub.NotConfigured", "GitHub integration is not configured"));

            var (owner, repo) = GetRepoInfo(repository);
            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
                return Result.Failure<GitHubIssueDetailResponse>(Error.Validation("GitHub.InvalidRepository", $"Repository '{repository}' is not configured"));

            var getResult = await GetIssueAsync(repository, issueNumber, cancellationToken);
            if (!getResult.IsSuccess) return getResult;

            var currentIssue = getResult.Value!;
            var labels = currentIssue.Labels?.Select(l => l.Name).ToList() ?? new List<string>();
            if (!labels.Contains("archived", StringComparer.OrdinalIgnoreCase)) labels.Add("archived");

            var url = $"repos/{owner}/{repo}/issues/{issueNumber}";
            var archivePayload = new Dictionary<string, object> { ["state"] = "closed", ["labels"] = labels };

            if (!string.IsNullOrEmpty(reason))
            {
                var newBody = currentIssue.Body ?? "";
                newBody += $"\n\n---\n**Archived**: {reason}\n*Archived on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC*";
                archivePayload["body"] = newBody;
            }

            var response = await _httpClient.PatchAsJsonAsync(url, archivePayload, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("GitHub API error archiving issue: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => Result.Failure<GitHubIssueDetailResponse>(Error.NotFound("GitHub.IssueNotFound", $"Issue #{issueNumber} not found")),
                    System.Net.HttpStatusCode.Forbidden => Result.Failure<GitHubIssueDetailResponse>(Error.Forbidden("GitHub.Forbidden", "Insufficient permissions to archive this issue")),
                    _ => Result.Failure<GitHubIssueDetailResponse>(Error.Failure("GitHub.ApiError", $"GitHub API error: {response.StatusCode}"))
                };
            }

            var issue = await response.Content.ReadFromJsonAsync<GitHubIssueDetailApiResponse>(JsonOptions, cancellationToken);
            if (issue == null)
                return Result.Failure<GitHubIssueDetailResponse>(Error.Failure("GitHub.ParseError", "Failed to parse GitHub API response"));

            _logger.LogInformation("Archived GitHub issue #{IssueNumber} in {Repo}", issueNumber, $"{owner}/{repo}");
            return Result.Success(MapToDetailResponse(issue, repository));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving GitHub issue #{IssueNumber}", issueNumber);
            return Result.Failure<GitHubIssueDetailResponse>(Error.InternalServerError("GitHub.UnexpectedError", "An unexpected error occurred"));
        }
    }

    private List<(string repoKey, string owner, string repo)> GetRepositoriesToQuery(string repository)
    {
        var repos = new List<(string, string, string)>();

        if (repository == "all" || repository == "frontend")
        {
            if (!string.IsNullOrEmpty(_settings.FrontendRepoOwner) && !string.IsNullOrEmpty(_settings.FrontendRepoName))
            {
                repos.Add(("frontend", _settings.FrontendRepoOwner, _settings.FrontendRepoName));
            }
        }

        if (repository == "all" || repository == "backend")
        {
            if (!string.IsNullOrEmpty(_settings.BackendRepoOwner) && !string.IsNullOrEmpty(_settings.BackendRepoName))
            {
                repos.Add(("backend", _settings.BackendRepoOwner, _settings.BackendRepoName));
            }
        }

        return repos;
    }

    private async Task<List<GitHubIssueListItem>> FetchIssuesFromRepo(
        string owner,
        string repo,
        string repoKey,
        ListGitHubIssuesRequest request,
        CancellationToken cancellationToken)
    {
        var state = request.State == "all" ? "all" : request.State;
        var url = $"repos/{owner}/{repo}/issues?state={state}&per_page={request.PageSize}&sort={request.Sort}&direction={request.Direction}";

        if (!string.IsNullOrWhiteSpace(request.Label))
        {
            url += $"&labels={Uri.EscapeDataString(request.Label)}";
        }

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch issues from {Owner}/{Repo}: {StatusCode}", owner, repo, response.StatusCode);
            return new List<GitHubIssueListItem>();
        }

        var issues = await response.Content.ReadFromJsonAsync<List<GitHubIssueDetailApiResponse>>(JsonOptions, cancellationToken);

        return issues?
            .Where(i => i.PullRequest == null) // Exclude pull requests
            .Select(i => MapToListItem(i, repoKey))
            .ToList() ?? new List<GitHubIssueListItem>();
    }

    private GitHubIssueListItem MapToListItem(GitHubIssueDetailApiResponse issue, string repository)
    {
        var priority = ExtractPriority(issue.Labels);
        var category = ExtractCategory(issue.Labels);

        return new GitHubIssueListItem
        {
            IssueNumber = issue.Number,
            HtmlUrl = issue.HtmlUrl,
            Title = issue.Title,
            State = issue.State,
            Repository = repository,
            CreatedAt = issue.CreatedAt,
            UpdatedAt = issue.UpdatedAt,
            Labels = issue.Labels?.Select(l => new GitHubLabelDto
            {
                Name = l.Name,
                Color = l.Color,
                Description = l.Description
            }).ToList() ?? new List<GitHubLabelDto>(),
            Author = issue.User != null ? new GitHubUserDto
            {
                Login = issue.User.Login,
                AvatarUrl = issue.User.AvatarUrl,
                HtmlUrl = issue.User.HtmlUrl
            } : null,
            CommentsCount = issue.Comments,
            Priority = priority,
            Category = category
        };
    }

    private GitHubIssueDetailResponse MapToDetailResponse(GitHubIssueDetailApiResponse issue, string repository)
    {
        return new GitHubIssueDetailResponse
        {
            IssueNumber = issue.Number,
            IssueUrl = issue.Url,
            HtmlUrl = issue.HtmlUrl,
            Title = issue.Title,
            Body = issue.Body ?? string.Empty,
            State = issue.State,
            Repository = repository,
            CreatedAt = issue.CreatedAt,
            UpdatedAt = issue.UpdatedAt,
            ClosedAt = issue.ClosedAt,
            Labels = issue.Labels?.Select(l => new GitHubLabelDto
            {
                Name = l.Name,
                Color = l.Color,
                Description = l.Description
            }).ToList() ?? new List<GitHubLabelDto>(),
            Author = issue.User != null ? new GitHubUserDto
            {
                Login = issue.User.Login,
                AvatarUrl = issue.User.AvatarUrl,
                HtmlUrl = issue.User.HtmlUrl
            } : null,
            Assignee = issue.Assignee != null ? new GitHubUserDto
            {
                Login = issue.Assignee.Login,
                AvatarUrl = issue.Assignee.AvatarUrl,
                HtmlUrl = issue.Assignee.HtmlUrl
            } : null,
            CommentsCount = issue.Comments
        };
    }

    private static string? ExtractPriority(List<GitHubLabelApiResponse>? labels)
    {
        var priorityLabel = labels?.FirstOrDefault(l =>
            l.Name.StartsWith("priority:", StringComparison.OrdinalIgnoreCase));

        if (priorityLabel != null)
        {
            return priorityLabel.Name.Replace("priority:", "").Trim();
        }

        return null;
    }

    private static string? ExtractCategory(List<GitHubLabelApiResponse>? labels)
    {
        var categoryLabels = new[] { "bug", "enhancement", "documentation", "performance", "feature" };
        var categoryLabel = labels?.FirstOrDefault(l =>
            categoryLabels.Contains(l.Name.ToLowerInvariant()));

        return categoryLabel?.Name;
    }

    private (string owner, string repo) GetRepoInfo(string repository)
    {
        return repository.ToLowerInvariant() switch
        {
            "frontend" => (_settings.FrontendRepoOwner, _settings.FrontendRepoName),
            "backend" => (_settings.BackendRepoOwner, _settings.BackendRepoName),
            _ => (string.Empty, string.Empty)
        };
    }

    private string BuildIssueBody(CreateGitHubIssueRequest request)
    {
        var sb = new StringBuilder();

        // Description
        sb.AppendLine("## Description");
        sb.AppendLine(request.Description);
        sb.AppendLine();

        // Reproduction Steps
        if (!string.IsNullOrWhiteSpace(request.ReproductionSteps))
        {
            sb.AppendLine("## Steps to Reproduce");
            sb.AppendLine(request.ReproductionSteps);
            sb.AppendLine();
        }

        // System Information
        sb.AppendLine("## Environment");
        sb.AppendLine("| Property | Value |");
        sb.AppendLine("|----------|-------|");

        if (!string.IsNullOrWhiteSpace(request.Browser))
            sb.AppendLine($"| Browser | {request.Browser} |");

        if (!string.IsNullOrWhiteSpace(request.OperatingSystem))
            sb.AppendLine($"| OS | {request.OperatingSystem} |");

        if (!string.IsNullOrWhiteSpace(request.ScreenSize))
            sb.AppendLine($"| Screen Size | {request.ScreenSize} |");

        if (!string.IsNullOrWhiteSpace(request.CurrentPageUrl))
            sb.AppendLine($"| Page URL | {request.CurrentPageUrl} |");

        sb.AppendLine();

        // Reporter Info
        var userEmail = _currentUserService.UserEmail;
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            sb.AppendLine("## Reporter");
            sb.AppendLine($"Reported by: {userEmail}");
            sb.AppendLine();
        }

        // Screenshots
        if (request.ScreenshotUrls?.Count > 0)
        {
            sb.AppendLine("## Screenshots");
            foreach (var url in request.ScreenshotUrls)
            {
                sb.AppendLine($"![Screenshot]({url})");
            }
            sb.AppendLine();
        }

        // Metadata footer
        sb.AppendLine("---");
        sb.AppendLine($"*Submitted via Sqordia Admin Panel*");

        return sb.ToString();
    }

    private List<string> BuildLabels(CreateGitHubIssueRequest request)
    {
        var labels = new List<string>();

        // Category label
        var categoryLabel = request.Category.ToLowerInvariant() switch
        {
            "bug" => "bug",
            "feature" => "enhancement",
            "enhancement" => "enhancement",
            "documentation" => "documentation",
            "performance" => "performance",
            _ => request.Category.ToLowerInvariant()
        };
        labels.Add(categoryLabel);

        // Severity/Priority label
        var severityLabel = request.Severity.ToLowerInvariant() switch
        {
            "low" => "priority: low",
            "medium" => "priority: medium",
            "high" => "priority: high",
            "critical" => "priority: critical",
            _ => $"priority: {request.Severity.ToLowerInvariant()}"
        };
        labels.Add(severityLabel);

        // Source label
        labels.Add("from: admin-panel");

        return labels;
    }

    private class GitHubCreateIssueRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<string> Labels { get; set; } = new();
    }

    private class GitHubIssueApiResponse
    {
        public int Number { get; set; }
        public string Url { get; set; } = string.Empty;
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    private class GitHubIssueDetailApiResponse
    {
        public int Number { get; set; }
        public string Url { get; set; } = string.Empty;
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string State { get; set; } = string.Empty;
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [JsonPropertyName("closed_at")]
        public DateTime? ClosedAt { get; set; }
        public List<GitHubLabelApiResponse>? Labels { get; set; }
        public GitHubUserApiResponse? User { get; set; }
        public GitHubUserApiResponse? Assignee { get; set; }
        public int Comments { get; set; }
        [JsonPropertyName("pull_request")]
        public object? PullRequest { get; set; }
    }

    private class GitHubLabelApiResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    private class GitHubUserApiResponse
    {
        public string Login { get; set; } = string.Empty;
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; } = string.Empty;
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
    }
}

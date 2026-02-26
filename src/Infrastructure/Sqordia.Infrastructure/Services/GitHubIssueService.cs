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
}

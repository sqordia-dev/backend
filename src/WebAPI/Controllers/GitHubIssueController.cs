using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.GitHub;
using Sqordia.Contracts.Responses.GitHub;

namespace WebAPI.Controllers;

/// <summary>
/// Controller for creating GitHub issues from the admin panel.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/github-issues")]
[Authorize(Roles = "Admin")]
public class GitHubIssueController : BaseApiController
{
    private readonly IGitHubIssueService _gitHubIssueService;
    private readonly IStorageService _storageService;
    private readonly ILogger<GitHubIssueController> _logger;

    public GitHubIssueController(
        IGitHubIssueService gitHubIssueService,
        IStorageService storageService,
        ILogger<GitHubIssueController> logger)
    {
        _gitHubIssueService = gitHubIssueService;
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new GitHub issue.
    /// </summary>
    /// <param name="request">The issue creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created GitHub issue details</returns>
    /// <remarks>
    /// Creates a new GitHub issue in either the frontend or backend repository.
    /// The issue will include the description, severity, category, reproduction steps,
    /// and system information formatted as Markdown.
    ///
    /// Sample request:
    ///     POST /api/v1/github-issues
    ///     {
    ///         "title": "Login button not working",
    ///         "description": "The login button does not respond when clicked...",
    ///         "severity": "High",
    ///         "category": "Bug",
    ///         "repository": "frontend",
    ///         "reproductionSteps": "1. Go to login page\n2. Click login button\n3. Nothing happens",
    ///         "browser": "Chrome 120",
    ///         "operatingSystem": "Windows",
    ///         "screenSize": "1920x1080",
    ///         "currentPageUrl": "https://app.sqordia.com/login"
    ///     }
    /// </remarks>
    /// <response code="200">Issue created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="403">Forbidden - admin role required</response>
    [HttpPost]
    [ProducesResponseType(typeof(GitHubIssueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateIssue(
        [FromBody] CreateGitHubIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating GitHub issue: {Title} in {Repository}", request.Title, request.Repository);
        var result = await _gitHubIssueService.CreateIssueAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Upload a screenshot for a bug report.
    /// </summary>
    /// <param name="file">The screenshot file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The URL of the uploaded screenshot</returns>
    /// <remarks>
    /// Uploads a screenshot to cloud storage and returns the URL.
    /// Supported formats: PNG, JPEG, GIF, WebP. Max size: 10MB.
    /// </remarks>
    /// <response code="200">Screenshot uploaded successfully</response>
    /// <response code="400">Invalid file type or size</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="403">Forbidden - admin role required</response>
    [HttpPost("upload-screenshot")]
    [ProducesResponseType(typeof(UploadScreenshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadScreenshot(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided" });
        }

        _logger.LogInformation("Uploading screenshot: {FileName}, {Size} bytes", file.FileName, file.Length);
        var result = await _gitHubIssueService.UploadScreenshotAsync(file, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(new UploadScreenshotResponse { Url = result.Value! });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// List GitHub issues with filtering and pagination.
    /// </summary>
    /// <param name="repository">Filter by repository: frontend, backend, or all</param>
    /// <param name="state">Filter by state: open, closed, or all</param>
    /// <param name="label">Filter by label</param>
    /// <param name="search">Search in title</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="sort">Sort by: created or updated</param>
    /// <param name="direction">Sort direction: asc or desc</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of GitHub issues</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GitHubIssueListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListIssues(
        [FromQuery] string repository = "all",
        [FromQuery] string state = "all",
        [FromQuery] string? label = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "created",
        [FromQuery] string direction = "desc",
        CancellationToken cancellationToken = default)
    {
        var request = new ListGitHubIssuesRequest
        {
            Repository = repository,
            State = state,
            Label = label,
            Search = search,
            Page = page,
            PageSize = Math.Min(pageSize, 100),
            Sort = sort,
            Direction = direction
        };

        _logger.LogInformation("Listing GitHub issues: Repo={Repository}, State={State}", repository, state);
        var result = await _gitHubIssueService.ListIssuesAsync(request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a single GitHub issue by number.
    /// </summary>
    /// <param name="repository">Repository: frontend or backend</param>
    /// <param name="issueNumber">Issue number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>GitHub issue details</returns>
    [HttpGet("{repository}/{issueNumber:int}")]
    [ProducesResponseType(typeof(GitHubIssueDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetIssue(
        string repository,
        int issueNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting GitHub issue #{IssueNumber} from {Repository}", issueNumber, repository);
        var result = await _gitHubIssueService.GetIssueAsync(repository, issueNumber, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get issue statistics (counts by state, priority, etc.)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Issue statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(GitHubIssueStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetIssueStats(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting GitHub issue statistics");
        var result = await _gitHubIssueService.GetIssueStatsAsync(cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Update the state of a GitHub issue (open/close).
    /// </summary>
    /// <param name="repository">Repository: frontend or backend</param>
    /// <param name="issueNumber">Issue number</param>
    /// <param name="request">The update request with new state</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated GitHub issue details</returns>
    /// <remarks>
    /// Sample request:
    ///     PATCH /api/v1/github-issues/frontend/123
    ///     {
    ///         "state": "closed"
    ///     }
    /// </remarks>
    /// <response code="200">Issue updated successfully</response>
    /// <response code="400">Invalid state value</response>
    /// <response code="404">Issue not found</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="403">Forbidden - admin role required</response>
    [HttpPatch("{repository}/{issueNumber:int}")]
    [ProducesResponseType(typeof(GitHubIssueDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateIssue(
        string repository,
        int issueNumber,
        [FromBody] UpdateGitHubIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating GitHub issue #{IssueNumber} in {Repository}", issueNumber, repository);
        var result = await _gitHubIssueService.UpdateIssueAsync(repository, issueNumber, request, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Archive (soft delete) a GitHub issue.
    /// </summary>
    /// <param name="repository">Repository: frontend or backend</param>
    /// <param name="issueNumber">Issue number</param>
    /// <param name="request">Optional archive reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Archived GitHub issue details</returns>
    /// <remarks>
    /// GitHub does not support deleting issues via API. This endpoint closes the issue
    /// and adds an "archived" label to mark it as deleted.
    /// </remarks>
    [HttpDelete("{repository}/{issueNumber:int}")]
    [ProducesResponseType(typeof(GitHubIssueDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveIssue(
        string repository,
        int issueNumber,
        [FromBody] ArchiveGitHubIssueRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Archiving GitHub issue #{IssueNumber} in {Repository}", issueNumber, repository);
        var result = await _gitHubIssueService.ArchiveIssueAsync(repository, issueNumber, request?.Reason, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Get a bug report screenshot by storage key.
    /// </summary>
    /// <param name="key">The storage key (e.g., bug-reports/guid.png)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The screenshot image file</returns>
    /// <remarks>
    /// This endpoint is anonymous to allow GitHub to display the screenshots.
    /// Only serves files from the bug-reports/ folder for security.
    /// </remarks>
    /// <response code="200">Screenshot retrieved successfully</response>
    /// <response code="400">Invalid key (not in bug-reports folder)</response>
    /// <response code="404">Screenshot not found</response>
    [HttpGet("screenshot/{*key}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScreenshot(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Security: Only allow bug-reports folder
            if (!key.StartsWith("bug-reports/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Invalid screenshot key" });
            }

            // Check if file exists
            if (!await _storageService.FileExistsAsync(key, cancellationToken))
            {
                return NotFound(new { error = "Screenshot not found" });
            }

            // Download the file from storage
            var fileStream = await _storageService.DownloadFileAsync(key, cancellationToken);

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
            return NotFound(new { error = "Screenshot not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving screenshot: {Key}", key);
            return StatusCode(500, new { error = "Error retrieving screenshot" });
        }
    }
}

/// <summary>
/// Response for screenshot upload.
/// </summary>
public class UploadScreenshotResponse
{
    public string Url { get; set; } = string.Empty;
}

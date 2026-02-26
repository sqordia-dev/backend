using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly ILogger<GitHubIssueController> _logger;

    public GitHubIssueController(
        IGitHubIssueService gitHubIssueService,
        ILogger<GitHubIssueController> logger)
    {
        _gitHubIssueService = gitHubIssueService;
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
}

/// <summary>
/// Response for screenshot upload.
/// </summary>
public class UploadScreenshotResponse
{
    public string Url { get; set; } = string.Empty;
}

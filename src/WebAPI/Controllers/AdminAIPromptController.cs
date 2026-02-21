using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.Admin;
using Sqordia.Contracts.Responses.Admin;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/ai-prompts")]
[Authorize(Roles = "Admin")]
public class AdminAIPromptController : BaseApiController
{
    private readonly IAIPromptService _aiPromptService;
    private readonly IPromptMigrationService _promptMigrationService;

    public AdminAIPromptController(
        IAIPromptService aiPromptService,
        IPromptMigrationService promptMigrationService)
    {
        _aiPromptService = aiPromptService;
        _promptMigrationService = promptMigrationService;
    }

    /// <summary>
    /// Create a new AI prompt template
    /// </summary>
    /// <param name="request">The prompt creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created prompt ID</returns>
    /// <remarks>
    /// Creates a new AI prompt template that can be used for generating content.
    /// Admins can create prompts in both French and English for different business plan types.
    /// 
    /// Sample request:
    ///     POST /api/v1/admin/ai-prompts
    ///     {
    ///         "name": "Question Suggestions - Business Plan",
    ///         "description": "Generates AI suggestions for business plan questionnaire questions",
    ///         "category": "QuestionSuggestions",
    ///         "planType": "BusinessPlan",
    ///         "language": "fr",
    ///         "systemPrompt": "Vous êtes un expert consultant en affaires...",
    ///         "userPromptTemplate": "Question: {questionText}\n\nContexte: {organizationContext}",
    ///         "variables": "{\"questionText\": \"The question to answer\", \"organizationContext\": \"Organization context\"}",
    ///         "notes": "Optimized for French business plan questions"
    ///     }
    /// </remarks>
    /// <response code="201">Prompt created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    /// <response code="403">Forbidden - admin role required</response>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePrompt(
        [FromBody] CreateAIPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var promptId = await _aiPromptService.CreatePromptAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPrompt), new { promptId }, new { Id = promptId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get an AI prompt template by ID
    /// </summary>
    /// <param name="promptId">The prompt ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The prompt template</returns>
    [HttpGet("{promptId}")]
    [ProducesResponseType(typeof(AIPromptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPrompt(
        string promptId,
        CancellationToken cancellationToken = default)
    {
        var prompt = await _aiPromptService.GetPromptAsync(promptId, cancellationToken);
        if (prompt == null)
            return NotFound();

        return Ok(prompt);
    }

    /// <summary>
    /// Get all AI prompt templates with optional filtering
    /// </summary>
    /// <param name="category">Filter by category</param>
    /// <param name="planType">Filter by plan type</param>
    /// <param name="language">Filter by language</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of prompt templates</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<AIPromptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPrompts(
        [FromQuery] string? category = null,
        [FromQuery] string? planType = null,
        [FromQuery] string? language = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var prompts = await _aiPromptService.GetPromptsAsync(
            category, planType, language, isActive, cancellationToken);

        return Ok(prompts);
    }

    /// <summary>
    /// Update an AI prompt template
    /// </summary>
    /// <param name="promptId">The prompt ID</param>
    /// <param name="request">The update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPut("{promptId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePrompt(
        string promptId,
        [FromBody] UpdateAIPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _aiPromptService.UpdatePromptAsync(promptId, request, cancellationToken);
            if (!success)
                return NotFound();

            return Ok(new { message = "Prompt updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an AI prompt template
    /// </summary>
    /// <param name="promptId">The prompt ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("{promptId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePrompt(
        string promptId,
        CancellationToken cancellationToken = default)
    {
        var success = await _aiPromptService.DeletePromptAsync(promptId, cancellationToken);
        if (!success)
            return NotFound();

        return Ok(new { message = "Prompt deleted successfully" });
    }

    /// <summary>
    /// Test an AI prompt with sample data
    /// </summary>
    /// <param name="request">The test request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test result</returns>
    /// <remarks>
    /// Tests an AI prompt template with sample variables to see how it performs.
    /// This is useful for admins to validate and refine prompts before making them active.
    /// 
    /// Sample request:
    ///     POST /api/v1/admin/ai-prompts/test
    ///     {
    ///         "promptId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///         "sampleVariables": "{\"questionText\": \"What is your target market?\", \"organizationContext\": \"Tech startup\"}",
    ///         "testContext": "Testing question suggestions",
    ///         "maxTokens": 1000,
    ///         "temperature": 0.7
    ///     }
    /// </remarks>
    [HttpPost("test")]
    [ProducesResponseType(typeof(AIPromptTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TestPrompt(
        [FromBody] TestAIPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _aiPromptService.TestPromptAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get prompt usage statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Prompt statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(List<AIPromptStats>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPromptStats(CancellationToken cancellationToken = default)
    {
        var stats = await _aiPromptService.GetPromptStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Toggle prompt active status
    /// </summary>
    /// <param name="promptId">The prompt ID</param>
    /// <param name="isActive">Active status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPatch("{promptId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TogglePromptStatus(
        string promptId,
        [FromBody] bool isActive,
        CancellationToken cancellationToken = default)
    {
        var success = await _aiPromptService.TogglePromptStatusAsync(promptId, isActive, cancellationToken);
        if (!success)
            return NotFound();

        return Ok(new { message = $"Prompt {(isActive ? "activated" : "deactivated")} successfully" });
    }

    /// <summary>
    /// Update prompt status (PUT endpoint for frontend compatibility)
    /// </summary>
    /// <param name="promptId">The prompt ID</param>
    /// <param name="request">Status update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPut("{promptId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePromptStatus(
        string promptId,
        [FromBody] UpdatePromptStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var isActive = request.Status?.ToLower() == "active" || request.Status?.ToLower() == "enabled";
        var success = await _aiPromptService.TogglePromptStatusAsync(promptId, isActive, cancellationToken);
        if (!success)
            return NotFound();

        return Ok(new { message = $"Prompt {(isActive ? "activated" : "deactivated")} successfully" });
    }

    /// <summary>
    /// Create a new version of an existing prompt
    /// </summary>
    /// <param name="parentPromptId">The parent prompt ID</param>
    /// <param name="request">The new version request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The new version ID</returns>
    [HttpPost("{parentPromptId}/versions")]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePromptVersion(
        string parentPromptId,
        [FromBody] CreateAIPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var versionId = await _aiPromptService.CreatePromptVersionAsync(parentPromptId, request, cancellationToken);
            return CreatedAtAction(nameof(GetPrompt), new { promptId = versionId }, new { Id = versionId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all versions of a prompt
    /// </summary>
    /// <param name="parentPromptId">The parent prompt ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of prompt versions</returns>
    [HttpGet("{parentPromptId}/versions")]
    [ProducesResponseType(typeof(List<AIPromptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPromptVersions(
        string parentPromptId,
        CancellationToken cancellationToken = default)
    {
        var versions = await _aiPromptService.GetPromptVersionsAsync(parentPromptId, cancellationToken);
        return Ok(versions);
    }

    /// <summary>
    /// Get version history for a prompt (content snapshots)
    /// </summary>
    /// <param name="promptId">The prompt ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of version history entries</returns>
    /// <remarks>
    /// Returns all saved version snapshots for a prompt, ordered by version number descending.
    /// Each entry contains the full prompt content at that point in time.
    /// </remarks>
    [HttpGet("{promptId}/history")]
    [ProducesResponseType(typeof(List<AIPromptVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetVersionHistory(
        string promptId,
        CancellationToken cancellationToken = default)
    {
        var history = await _aiPromptService.GetVersionHistoryAsync(promptId, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Rollback a prompt to a previous version
    /// </summary>
    /// <param name="promptId">The prompt ID</param>
    /// <param name="request">The rollback request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    /// <remarks>
    /// Restores the prompt content to a previous version. The current content is saved
    /// as a new version before rollback, so no data is lost.
    ///
    /// Sample request:
    ///     POST /api/v1/admin/ai-prompts/{promptId}/rollback
    ///     {
    ///         "targetVersion": 2,
    ///         "notes": "Rolling back due to issues with version 3"
    ///     }
    /// </remarks>
    [HttpPost("{promptId}/rollback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RollbackToVersion(
        string promptId,
        [FromBody] RollbackAIPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _aiPromptService.RollbackToVersionAsync(
                promptId, request.TargetVersion, request.Notes, null, cancellationToken);

            if (!success)
                return NotFound(new { error = "Prompt or target version not found" });

            return Ok(new { message = $"Successfully rolled back to version {request.TargetVersion}" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test a draft prompt without saving to database
    /// </summary>
    /// <param name="request">The draft test request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test result</returns>
    /// <remarks>
    /// Tests a prompt before saving it. This allows admins to iterate on prompt design
    /// without cluttering the database with test versions.
    ///
    /// Sample request:
    ///     POST /api/v1/admin/ai-prompts/test-draft
    ///     {
    ///         "systemPrompt": "You are an expert business consultant...",
    ///         "userPromptTemplate": "Generate a {sectionName} for the following context:\n\n{questionnaireContext}",
    ///         "sampleVariables": "{\"sectionName\": \"executive summary\"}",
    ///         "testContext": "A tech startup building AI tools...",
    ///         "maxTokens": 1000,
    ///         "temperature": 0.7
    ///     }
    /// </remarks>
    [HttpPost("test-draft")]
    [ProducesResponseType(typeof(AIPromptTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TestDraftPrompt(
        [FromBody] TestDraftAIPromptRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _aiPromptService.TestDraftPromptAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Migrate default hardcoded prompts to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Migration result with count and list of migrated prompts</returns>
    /// <remarks>
    /// This endpoint migrates all hardcoded prompts from BusinessPlanGenerationService to the database.
    /// Only prompts that don't already exist will be created. This is useful for initial setup or
    /// when adding new default prompts.
    /// 
    /// Sample request:
    ///     POST /api/v1/admin/ai-prompts/migrate-defaults
    /// </remarks>
    [HttpPost("migrate-defaults")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MigrateDefaultPrompts(CancellationToken cancellationToken = default)
    {
        try
        {
            var migratedPrompts = await _promptMigrationService.MigrateDefaultPromptsAsync(cancellationToken);
            return Ok(new { 
                migrated = migratedPrompts.Count, 
                prompts = migratedPrompts 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Request model for updating prompt status
/// </summary>
public class UpdatePromptStatusRequest
{
    public string? Status { get; set; }
}

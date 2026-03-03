using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.Admin.PromptRegistry;
using Sqordia.Contracts.Responses.Admin.PromptRegistry;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for AI-powered prompt improvement using Anthropic's experimental API
/// </summary>
public interface IPromptImprovementService
{
    /// <summary>
    /// Improves a prompt using AI analysis and suggestions
    /// </summary>
    /// <param name="request">The improvement request containing the prompts to improve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing improved prompts and explanations</returns>
    Task<Result<PromptImprovementResultDto>> ImprovePromptAsync(
        PromptImprovementRequest request,
        CancellationToken cancellationToken = default);
}

using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Requests.V2.Questionnaire;
using Sqordia.Contracts.Responses.V2.Questionnaire;

namespace Sqordia.Application.Services.V2;

/// <summary>
/// AI text enhancement service for questionnaire responses
/// Transforms raw notes into professional, BDC-standard prose
/// </summary>
public interface IQuestionPolishService
{
    /// <summary>
    /// Polishes/enhances a questionnaire response using AI
    /// </summary>
    Task<Result<PolishedTextResponse>> PolishTextAsync(
        PolishTextRequest request,
        CancellationToken cancellationToken = default);
}

using Sqordia.Functions.EmailHandler.Models;

namespace Sqordia.Functions.EmailHandler.Services;

/// <summary>
/// Service for processing email jobs
/// </summary>
public interface IEmailProcessor
{
    /// <summary>
    /// Process an email job message
    /// </summary>
    Task<bool> ProcessEmailJobAsync(EmailJobMessage message, CancellationToken cancellationToken = default);
}


using Sqordia.Contracts.Responses.Admin;

namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Service for migrating hardcoded prompts to the database
/// </summary>
public interface IPromptMigrationService
{
    /// <summary>
    /// Migrates all default hardcoded prompts to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of migrated prompts</returns>
    Task<List<AIPromptDto>> MigrateDefaultPromptsAsync(CancellationToken cancellationToken = default);
}

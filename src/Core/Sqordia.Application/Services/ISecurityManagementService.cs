using Sqordia.Application.Common.Models;
using Sqordia.Contracts.Responses.Security;

namespace Sqordia.Application.Services;

public interface ISecurityManagementService
{
    // Session Management
    Task<Result<List<ActiveSessionResponse>>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<Result> RevokeAllSessionsExceptCurrentAsync(Guid userId, string currentSessionToken, CancellationToken cancellationToken = default);
    Task<Result> RevokeAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Login History
    Task<Result<List<LoginHistoryResponse>>> GetLoginHistoryAsync(Guid userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    // Account Security
    Task<Result> UnlockAccountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> ForcePasswordChangeAsync(Guid userId, CancellationToken cancellationToken = default);
}


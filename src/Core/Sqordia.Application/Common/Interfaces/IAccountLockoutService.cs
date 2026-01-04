using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Application.Common.Interfaces;

public interface IAccountLockoutService
{
    Task<bool> IsLockedOutAsync(User user);
    Task<bool> ShouldLockAccountAsync(User user);
    Task LockAccountAsync(User user);
    Task ResetFailedAttemptsAsync(User user);
    Task RecordFailedLoginAsync(User user);
    int GetMaxFailedAttempts();
    TimeSpan GetLockoutDuration();
}
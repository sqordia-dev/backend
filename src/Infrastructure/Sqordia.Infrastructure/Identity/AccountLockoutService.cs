using Microsoft.Extensions.Configuration;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.Entities.Identity;

namespace Sqordia.Infrastructure.Identity;

public class AccountLockoutService : IAccountLockoutService
{
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AccountLockoutService(IConfiguration configuration, IEmailService emailService)
    {
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<bool> IsLockedOutAsync(User user)
    {
        return await Task.FromResult(user.IsLockedOut);
    }

    public async Task<bool> ShouldLockAccountAsync(User user)
    {
        var maxAttempts = GetMaxFailedAttempts();
        return await Task.FromResult(user.AccessFailedCount >= maxAttempts);
    }

    public async Task LockAccountAsync(User user)
    {
        var lockoutDuration = GetLockoutDuration();
        var lockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
        
        user.LockAccount(lockoutEnd);

        // Send lockout notification email
        await _emailService.SendAccountLockedAsync(
            user.Email.Value, 
            user.GetFullName(), 
            lockoutEnd);
    }

    public async Task ResetFailedAttemptsAsync(User user)
    {
        user.ResetAccessFailedCount();
        await Task.CompletedTask;
    }

    public async Task RecordFailedLoginAsync(User user)
    {
        user.RecordFailedLogin();
        await Task.CompletedTask;
    }

    public int GetMaxFailedAttempts()
    {
        return int.Parse(_configuration["Security:MaxFailedLoginAttempts"] ?? "5");
    }

    public TimeSpan GetLockoutDuration()
    {
        var minutes = int.Parse(_configuration["Security:LockoutDurationMinutes"] ?? "15");
        return TimeSpan.FromMinutes(minutes);
    }
}
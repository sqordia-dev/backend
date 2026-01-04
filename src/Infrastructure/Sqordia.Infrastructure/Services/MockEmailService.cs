using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.ValueObjects;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Mock email service for testing and development environments
/// </summary>
public class MockEmailService : IEmailService
{
    public Task SendEmailAsync(EmailAddress to, string subject, string body)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(IEnumerable<EmailAddress> to, string subject, string body)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendHtmlEmailAsync(EmailAddress to, string subject, string htmlBody)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendHtmlEmailAsync(IEnumerable<EmailAddress> to, string subject, string htmlBody)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendWelcomeWithVerificationAsync(string email, string firstName, string lastName, string userName, string verificationToken)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string firstName, string lastName)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(string email, string userName, string verificationToken)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string userName, string resetToken)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendAccountLockedAsync(string email, string userName, DateTime lockedUntil)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendLoginAlertAsync(string email, string userName, string ipAddress, DateTime loginTime)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendAccountLockoutNotificationAsync(string email, string firstName, TimeSpan lockoutDuration, DateTime lockedAt)
    {
        // No-op for testing
        return Task.CompletedTask;
    }

    public Task SendOrganizationInvitationAsync(string email, string invitationToken, string? message = null)
    {
        // No-op for testing
        return Task.CompletedTask;
    }
}


using Sqordia.Domain.ValueObjects;

namespace Sqordia.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(EmailAddress to, string subject, string body);
    Task SendEmailAsync(IEnumerable<EmailAddress> to, string subject, string body);
    Task SendHtmlEmailAsync(EmailAddress to, string subject, string htmlBody);
    Task SendHtmlEmailAsync(IEnumerable<EmailAddress> to, string subject, string htmlBody);
    
    // Authentication-specific email methods
    Task SendWelcomeWithVerificationAsync(string email, string firstName, string lastName, string userName, string verificationToken);
    Task SendWelcomeEmailAsync(string email, string firstName, string lastName);
    Task SendEmailVerificationAsync(string email, string userName, string verificationToken);
    Task SendPasswordResetAsync(string email, string userName, string resetToken);
    Task SendAccountLockedAsync(string email, string userName, DateTime lockedUntil);
    Task SendLoginAlertAsync(string email, string userName, string ipAddress, DateTime loginTime);
    Task SendAccountLockoutNotificationAsync(string email, string firstName, TimeSpan lockoutDuration, DateTime lockedAt);
    
    // Organization-specific email methods
    Task SendOrganizationInvitationAsync(string email, string invitationToken, string? message = null);

    // Business Plan-specific email methods
    Task SendBusinessPlanGeneratedAsync(string email, string userName, string businessPlanId, string businessPlanTitle);
}

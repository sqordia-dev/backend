using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.ValueObjects;
using System.Text.Json;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Azure Service Bus email service configuration
/// </summary>
public class ServiceBusEmailSettings
{
    public string EmailTopic { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Email service that sends emails via Azure Service Bus topic (processed by Azure Functions)
/// </summary>
public class ServiceBusEmailService : IEmailService, IDisposable
{
    private readonly ServiceBusClient? _serviceBusClient;
    private readonly ServiceBusSender? _serviceBusSender;
    private readonly string? _emailTopic;
    private readonly ILogger<ServiceBusEmailService> _logger;
    private readonly ILocalizationService _localizationService;

    public ServiceBusEmailService(
        ServiceBusClient? serviceBusClient,
        string? emailTopic,
        ILogger<ServiceBusEmailService> logger,
        ILocalizationService localizationService)
    {
        _serviceBusClient = serviceBusClient;
        _emailTopic = emailTopic;
        _logger = logger;
        _localizationService = localizationService;
        
        if (_serviceBusClient != null && !string.IsNullOrWhiteSpace(_emailTopic))
        {
            _serviceBusSender = _serviceBusClient.CreateSender(_emailTopic);
        }
    }

    private async Task SendToTopicAsync(string emailType, string toEmail, string? toName, string subject, string body, string? htmlBody = null, Dictionary<string, string>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(_emailTopic) || _serviceBusSender == null)
        {
            _logger.LogWarning(
                "Email topic not configured. Email would be sent: Type={EmailType}, To={ToEmail}, Subject={Subject}",
                emailType, toEmail, subject);
            return;
        }

        try
        {
            var jobId = Guid.NewGuid().ToString();
            var message = new
            {
                jobId = jobId,
                emailType = emailType,
                toEmail = toEmail,
                toName = toName,
                subject = subject,
                body = body,
                htmlBody = htmlBody,
                metadata = metadata ?? new Dictionary<string, string>()
            };

            var messageBody = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                MessageId = jobId,
                Subject = emailType
            };

            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
            _logger.LogInformation(
                "Email job {JobId} published successfully. Type={EmailType}, To={ToEmail}",
                jobId, emailType, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish email job. Type={EmailType}, To={ToEmail}", emailType, toEmail);
            throw;
        }
    }

    public async Task SendEmailAsync(EmailAddress to, string subject, string body)
    {
        await SendToTopicAsync("simple", to.Value, null, subject, body);
    }

    public async Task SendEmailAsync(IEnumerable<EmailAddress> to, string subject, string body)
    {
        var tasks = to.Select(email => SendToTopicAsync("simple", email.Value, null, subject, body));
        await Task.WhenAll(tasks);
    }

    public async Task SendHtmlEmailAsync(EmailAddress to, string subject, string htmlBody)
    {
        await SendToTopicAsync("html", to.Value, null, subject, string.Empty, htmlBody);
    }

    public async Task SendHtmlEmailAsync(IEnumerable<EmailAddress> to, string subject, string htmlBody)
    {
        var tasks = to.Select(email => SendToTopicAsync("html", email.Value, null, subject, string.Empty, htmlBody));
        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        // Only dispose the sender, not the client (which is a singleton)
        _serviceBusSender?.DisposeAsync().AsTask().Wait();
    }

    public async Task SendWelcomeWithVerificationAsync(string email, string firstName, string lastName, string userName, string verificationToken)
    {
        var htmlBody = GetWelcomeWithVerificationTemplate(firstName, lastName, verificationToken);
        var metadata = new Dictionary<string, string>
        {
            { "firstName", firstName },
            { "lastName", lastName },
            { "userName", userName },
            { "verificationToken", verificationToken }
        };
        await SendToTopicAsync("welcome_verification", email, $"{firstName} {lastName}", "Welcome to Sqordia - Verify Your Email", string.Empty, htmlBody, metadata);
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName, string lastName)
    {
        var subject = _localizationService.GetString("Email.Subject.Welcome");
        var htmlBody = GetWelcomeEmailTemplate(firstName, lastName);
        var metadata = new Dictionary<string, string>
        {
            { "firstName", firstName },
            { "lastName", lastName }
        };
        await SendToTopicAsync("welcome", email, $"{firstName} {lastName}", subject, string.Empty, htmlBody, metadata);
    }

    public async Task SendEmailVerificationAsync(string email, string userName, string verificationToken)
    {
        var subject = _localizationService.GetString("Email.Subject.Verification");
        var htmlBody = GetEmailVerificationTemplate(userName, verificationToken);
        var metadata = new Dictionary<string, string>
        {
            { "userName", userName },
            { "verificationToken", verificationToken }
        };
        await SendToTopicAsync("verification", email, userName, subject, string.Empty, htmlBody, metadata);
    }

    public async Task SendPasswordResetAsync(string email, string userName, string resetToken)
    {
        var subject = _localizationService.GetString("Email.Subject.PasswordReset");
        var htmlBody = GetPasswordResetTemplate(userName, resetToken);
        var metadata = new Dictionary<string, string>
        {
            { "userName", userName },
            { "resetToken", resetToken }
        };
        await SendToTopicAsync("password_reset", email, userName, subject, string.Empty, htmlBody, metadata);
    }

    public async Task SendAccountLockedAsync(string email, string userName, DateTime lockedUntil)
    {
        var subject = _localizationService.GetString("Email.Subject.AccountLocked");
        var htmlBody = GetAccountLockedTemplate(userName, lockedUntil);
        var metadata = new Dictionary<string, string>
        {
            { "userName", userName },
            { "lockedUntil", lockedUntil.ToString("O") }
        };
        await SendToTopicAsync("account_locked", email, userName, subject, string.Empty, htmlBody, metadata);
    }

    public async Task SendLoginAlertAsync(string email, string userName, string ipAddress, DateTime loginTime)
    {
        var subject = _localizationService.GetString("Email.Subject.LoginAlert");
        var htmlBody = GetLoginAlertTemplate(userName, ipAddress, loginTime);
        var metadata = new Dictionary<string, string>
        {
            { "userName", userName },
            { "ipAddress", ipAddress },
            { "loginTime", loginTime.ToString("O") }
        };
        await SendToTopicAsync("login_alert", email, userName, subject, string.Empty, htmlBody, metadata);
    }

    public async Task SendAccountLockoutNotificationAsync(string email, string firstName, TimeSpan lockoutDuration, DateTime lockedAt)
    {
        var subject = _localizationService.GetString("Email.Subject.AccountLocked");
        var htmlBody = GetAccountLockoutTemplate(firstName, lockoutDuration, lockedAt);
        var metadata = new Dictionary<string, string>
        {
            { "firstName", firstName },
            { "lockoutDuration", lockoutDuration.TotalMinutes.ToString() },
            { "lockedAt", lockedAt.ToString("O") }
        };
        await SendToTopicAsync("account_lockout", email, firstName, subject, string.Empty, htmlBody, metadata);
    }

    public async Task SendOrganizationInvitationAsync(string email, string invitationToken, string? message = null)
    {
        var subject = _localizationService.GetString("Email.Subject.OrganizationInvitation");
        var htmlBody = GetOrganizationInvitationTemplate(email, invitationToken, message);
        var metadata = new Dictionary<string, string>
        {
            { "invitationToken", invitationToken }
        };
        if (!string.IsNullOrEmpty(message))
        {
            metadata["message"] = message;
        }
        await SendToTopicAsync("organization_invitation", email, email, subject, string.Empty, htmlBody, metadata);
    }

    public async Task SendBusinessPlanGeneratedAsync(string email, string userName, string businessPlanId, string businessPlanTitle)
    {
        var subject = $"✅ Your Business Plan '{businessPlanTitle}' is Ready!";
        var htmlBody = GetBusinessPlanGeneratedTemplate(userName, businessPlanId, businessPlanTitle);
        var metadata = new Dictionary<string, string>
        {
            { "businessPlanId", businessPlanId },
            { "businessPlanTitle", businessPlanTitle },
            { "userName", userName }
        };
        await SendToTopicAsync("business_plan_generated", email, userName, subject, string.Empty, htmlBody, metadata);
    }

    // Template methods - same as EmailService
    private string GetEmailVerificationTemplate(string userName, string verificationToken)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #3b82f6; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Sqordia</div>
        </div>
        <h2>{_localizationService.GetString("Email.Verification.Title")}</h2>
        <p>{_localizationService.GetString("Email.Verification.Greeting", userName)}</p>
        <p>{_localizationService.GetString("Email.Verification.ThankYou")}</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='https://localhost:7001/verify-email?token={verificationToken}' class='button'>{_localizationService.GetString("Email.Verification.ButtonText")}</a>
        </p>
        <p>{_localizationService.GetString("Email.Verification.AlternativeText")}</p>
        <p style='word-break: break-all; color: #666;'>https://localhost:7001/verify-email?token={verificationToken}</p>
        <p>{_localizationService.GetString("Email.Verification.ExpiryNote")}</p>
        <div class='footer'>
            <p>{_localizationService.GetString("Email.Verification.IgnoreNote")}</p>
            <p>{_localizationService.GetString("Email.Footer.Copyright")}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPasswordResetTemplate(string userName, string resetToken)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #3b82f6; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #ef4444; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Sqordia</div>
        </div>
        <h2>{_localizationService.GetString("Email.PasswordReset.Title")}</h2>
        <p>{_localizationService.GetString("Email.PasswordReset.Greeting", userName)}</p>
        <p>{_localizationService.GetString("Email.PasswordReset.RequestReceived")}</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='https://localhost:7001/reset-password?token={resetToken}' class='button'>{_localizationService.GetString("Email.PasswordReset.ButtonText")}</a>
        </p>
        <p>{_localizationService.GetString("Email.PasswordReset.AlternativeText")}</p>
        <p style='word-break: break-all; color: #666;'>https://localhost:7001/reset-password?token={resetToken}</p>
        <p>{_localizationService.GetString("Email.PasswordReset.ExpiryNote")}</p>
        <div class='footer'>
            <p>{_localizationService.GetString("Email.PasswordReset.IgnoreNote")}</p>
            <p>{_localizationService.GetString("Email.Footer.Copyright")}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetAccountLockedTemplate(string userName, DateTime lockedUntil)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #3b82f6; }}
        .alert {{ background-color: #fef2f2; border: 1px solid #fecaca; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Sqordia</div>
        </div>
        <h2>{_localizationService.GetString("Email.AccountLocked.Title")}</h2>
        <p>{_localizationService.GetString("Email.AccountLocked.Greeting", userName)}</p>
        <div class='alert'>
            <p><strong>{_localizationService.GetString("Email.AccountLocked.Notification")}</strong></p>
            <p>{_localizationService.GetString("Email.LoginAlert.Time", lockedUntil.ToString("yyyy-MM-dd HH:mm"))} UTC</p>
        </div>
        <p>{_localizationService.GetString("Email.AccountLocked.AutoUnlock")}</p>
        <p>{_localizationService.GetString("Email.AccountLocked.ContactSupport")}</p>
        <div class='footer'>
            <p>{_localizationService.GetString("Email.AccountLocked.ContactSupport")}</p>
            <p>{_localizationService.GetString("Email.Footer.Copyright")}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetLoginAlertTemplate(string userName, string ipAddress, DateTime loginTime)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #3b82f6; }}
        .info {{ background-color: #f0f9ff; border: 1px solid #0ea5e9; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Sqordia</div>
        </div>
        <h2>{_localizationService.GetString("Email.LoginAlert.Title")}</h2>
        <p>{_localizationService.GetString("Email.LoginAlert.Greeting", userName)}</p>
        <p>{_localizationService.GetString("Email.LoginAlert.Notification")}</p>
        <div class='info'>
            <p><strong>{_localizationService.GetString("Email.LoginAlert.Time", loginTime.ToString("yyyy-MM-dd HH:mm"))}</strong> UTC</p>
            <p><strong>{_localizationService.GetString("Email.LoginAlert.IpAddress", ipAddress)}</strong></p>
        </div>
        <p><strong>{_localizationService.GetString("Email.LoginAlert.WasYou")}</strong> {_localizationService.GetString("Email.LoginAlert.NoAction")}</p>
        <p><strong>{_localizationService.GetString("Email.LoginAlert.NotYou")}</strong> {_localizationService.GetString("Email.LoginAlert.TakeAction")}</p>
        <div class='footer'>
            <p>{_localizationService.GetString("Email.AccountLocked.ContactSupport")}</p>
            <p>{_localizationService.GetString("Email.Footer.Copyright")}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetWelcomeWithVerificationTemplate(string firstName, string lastName, string verificationToken)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 28px; font-weight: bold; color: #3b82f6; margin-bottom: 10px; }}
        .welcome {{ font-size: 24px; color: #059669; font-weight: bold; margin-bottom: 20px; }}
        .highlight {{ background-color: #fff7ed; border-left: 4px solid #f59e0b; padding: 20px; margin: 25px 0; border-radius: 5px; }}
        .verify-section {{ background-color: #f0f9ff; border: 2px solid #3b82f6; padding: 25px; margin: 25px 0; border-radius: 8px; text-align: center; }}
        .button {{ display: inline-block; padding: 14px 35px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Sqordia</div>
            <div class='welcome'>{_localizationService.GetString("Email.Welcome.Greeting", firstName)}</div>
        </div>
        
        <p>{_localizationService.GetString("Email.Welcome.ThankYou")}</p>
        
        <div class='highlight'>
            <p><strong>{_localizationService.GetString("Email.Welcome.NextStep")}</strong> {_localizationService.GetString("Email.Welcome.VerificationInstruction")}</p>
        </div>
        
        <div class='verify-section'>
            <h3 style='margin-top: 0; color: #1e40af;'>{_localizationService.GetString("Email.Verification.Title")}</h3>
            <p style='margin: 15px 0;'>{_localizationService.GetString("Email.Verification.ThankYou")}</p>
            <p style='margin: 25px 0;'>
                <a href='https://localhost:7001/verify-email?token={verificationToken}' class='button'>{_localizationService.GetString("Email.Verification.ButtonText")}</a>
            </p>
            <p style='font-size: 12px; color: #666; margin-top: 20px;'>{_localizationService.GetString("Email.Verification.ExpiryNote")}</p>
        </div>
        
        <p style='font-size: 13px; color: #666;'>{_localizationService.GetString("Email.Verification.AlternativeText")}</p>
        <p style='word-break: break-all; font-size: 12px; color: #3b82f6; background-color: #f8fafc; padding: 10px; border-radius: 4px;'>https://localhost:7001/verify-email?token={verificationToken}</p>
        
        <div class='footer'>
            <p>{_localizationService.GetString("Email.Verification.IgnoreNote")}</p>
            <p>{_localizationService.GetString("Email.Welcome.Support")}</p>
            <p>{_localizationService.GetString("Email.Footer.Copyright")}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetWelcomeEmailTemplate(string firstName, string lastName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 28px; font-weight: bold; color: #3b82f6; margin-bottom: 10px; }}
        .welcome {{ font-size: 24px; color: #059669; font-weight: bold; margin-bottom: 20px; }}
        .highlight {{ background-color: #f0f9ff; border-left: 4px solid #3b82f6; padding: 15px; margin: 20px 0; }}
        .features {{ margin: 30px 0; }}
        .feature {{ display: flex; align-items: center; margin: 15px 0; }}
        .feature-icon {{ width: 24px; height: 24px; background-color: #3b82f6; border-radius: 50%; margin-right: 15px; display: flex; align-items: center; justify-content: center; color: white; font-weight: bold; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
        .cta {{ text-align: center; margin: 30px 0; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Sqordia</div>
            <div class='welcome'>🎉 {_localizationService.GetString("Email.Welcome.Greeting", firstName)}</div>
        </div>
        
        <p>{_localizationService.GetString("Email.Welcome.ThankYou")}</p>
        
        <div class='highlight'>
            <p><strong>{_localizationService.GetString("Email.Welcome.NextStep")}</strong> {_localizationService.GetString("Email.Welcome.VerificationInstruction")}</p>
        </div>
        
        <h3>{_localizationService.GetString("Email.Welcome.WhatYouCanDo")}</h3>
        <div class='features'>
            <div class='feature'>
                <div class='feature-icon'>📊</div>
                <span>{_localizationService.GetString("Email.Welcome.Feature1")}</span>
            </div>
            <div class='feature'>
                <div class='feature-icon'>📈</div>
                <span>{_localizationService.GetString("Email.Welcome.Feature2")}</span>
            </div>
            <div class='feature'>
                <div class='feature-icon'>🤝</div>
                <span>{_localizationService.GetString("Email.Welcome.Feature3")}</span>
            </div>
            <div class='feature'>
                <div class='feature-icon'>📋</div>
                <span>{_localizationService.GetString("Email.Welcome.Feature4")}</span>
            </div>
        </div>
        
        <div class='cta'>
            <p>{_localizationService.GetString("Email.Welcome.ReadyToStart")}</p>
        </div>
        
        <div class='footer'>
            <p>{_localizationService.GetString("Email.Welcome.Support")}</p>
            <p>{_localizationService.GetString("Email.Welcome.HereToHelp")}</p>
            <p>{_localizationService.GetString("Email.Footer.Copyright")}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetOrganizationInvitationTemplate(string email, string invitationToken, string? message)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ font-size: 24px; font-weight: bold; color: #3b82f6; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
        .message {{ background-color: #f0f9ff; border-left: 4px solid #3b82f6; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Sqordia</div>
        </div>
        <h2>Organization Invitation</h2>
        <p>Hi,</p>
        <p>You have been invited to join an organization on Sqordia! This invitation will allow you to collaborate on business plans and access shared resources.</p>
        
        {(string.IsNullOrEmpty(message) ? "" : $@"
        <div class='message'>
            <p><strong>Personal Message:</strong></p>
            <p>{message}</p>
        </div>")}
        
        <p style='text-align: center; margin: 30px 0;'>
            <a href='https://localhost:7001/accept-invitation?token={invitationToken}' class='button'>Accept Invitation</a>
        </p>
        
        <p>If you're unable to click the button, copy and paste this link into your browser:</p>
        <p style='word-break: break-all; color: #666;'>https://localhost:7001/accept-invitation?token={invitationToken}</p>
        
        <p>This invitation will expire in 7 days.</p>
        
        <div class='footer'>
            <p>If you didn't expect this invitation, you can safely ignore this email.</p>
            <p>&copy; 2024 Sqordia. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetAccountLockoutTemplate(string firstName, TimeSpan lockoutDuration, DateTime lockedAt)
    {
        var unlockTime = lockedAt.Add(lockoutDuration);
        var minutesRemaining = (int)lockoutDuration.TotalMinutes;

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; background-color: #ef4444; color: white; padding: 20px; border-radius: 8px; }}
        .logo {{ font-size: 28px; font-weight: bold; margin-bottom: 10px; }}
        .alert {{ font-size: 24px; font-weight: bold; margin-bottom: 10px; }}
        .warning-box {{ background-color: #fef2f2; border-left: 4px solid #ef4444; padding: 20px; margin: 25px 0; border-radius: 5px; }}
        .info-box {{ background-color: #f0f9ff; border-left: 4px solid: #3b82f6; padding: 20px; margin: 25px 0; border-radius: 5px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
        .time {{ font-size: 18px; font-weight: bold; color: #ef4444; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Sqordia</div>
            <div class='alert'>{_localizationService.GetString("Email.AccountLocked.Title")}</div>
        </div>

        <p>{_localizationService.GetString("Email.AccountLocked.Greeting", firstName)}</p>

        <div class='warning-box'>
            <p><strong>{_localizationService.GetString("Email.AccountLocked.Title")}</strong></p>
            <p>{_localizationService.GetString("Email.AccountLocked.Notification")}</p>
        </div>

        <div class='info-box'>
            <p><strong>{_localizationService.GetString("Email.LoginAlert.Details")}</strong></p>
            <p>{_localizationService.GetString("Email.LoginAlert.Time", lockedAt.ToString("yyyy-MM-dd HH:mm:ss"))} UTC</p>
            <p>{_localizationService.GetString("Email.AccountLocked.Duration", minutesRemaining)}</p>
            <p>{_localizationService.GetString("Email.LoginAlert.Time", unlockTime.ToString("yyyy-MM-dd HH:mm:ss"))} UTC</p>
        </div>

        <p><strong>{_localizationService.GetString("Email.AccountLocked.NotYou")}</strong></p>
        <ul>
            <li>{_localizationService.GetString("Email.AccountLocked.AutoUnlock")}</li>
            <li>{_localizationService.GetString("Email.AccountLocked.SecurityAdvice")}</li>
            <li>{_localizationService.GetString("Email.AccountLocked.ContactSupport")}</li>
        </ul>

        <p><strong>{_localizationService.GetString("Email.AccountLocked.NotYou")}</strong></p>
        <p>{_localizationService.GetString("Email.AccountLocked.ContactSupport")}</p>

        <div class='footer'>
            <p>{_localizationService.GetString("Email.Footer.DoNotReply")}</p>
            <p>{_localizationService.GetString("Email.AccountLocked.ContactSupport")}</p>
            <p>{_localizationService.GetString("Email.Footer.Copyright")}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetBusinessPlanGeneratedTemplate(string userName, string businessPlanId, string businessPlanTitle)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; background-color: #059669; color: white; padding: 25px; border-radius: 8px; }}
        .logo {{ font-size: 28px; font-weight: bold; margin-bottom: 10px; }}
        .success {{ font-size: 24px; font-weight: bold; margin-bottom: 10px; }}
        .success-icon {{ font-size: 48px; margin-bottom: 15px; }}
        .highlight {{ background-color: #f0fdf4; border-left: 4px solid #059669; padding: 20px; margin: 25px 0; border-radius: 5px; }}
        .plan-title {{ font-size: 20px; font-weight: bold; color: #047857; margin: 10px 0; }}
        .button {{ display: inline-block; padding: 14px 35px; background-color: #059669; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; margin: 20px 0; }}
        .features {{ margin: 30px 0; }}
        .feature {{ display: flex; align-items: center; margin: 15px 0; padding: 12px; background-color: #f8fafc; border-radius: 5px; }}
        .feature-icon {{ width: 32px; height: 32px; background-color: #e0f2fe; border-radius: 50%; margin-right: 15px; display: flex; align-items: center; justify-content: center; font-size: 18px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Sqordia</div>
            <div class='success-icon'>🎉</div>
            <div class='success'>Your Business Plan is Ready!</div>
        </div>

        <p>Hi {userName},</p>

        <div class='highlight'>
            <p><strong>Great news!</strong> Your business plan has been successfully generated and is ready for review.</p>
            <div class='plan-title'>📊 {businessPlanTitle}</div>
        </div>

        <p>Your comprehensive business plan includes all the essential sections you need to succeed:</p>

        <div class='features'>
            <div class='feature'>
                <div class='feature-icon'>📝</div>
                <span><strong>Executive Summary</strong> - Overview of your business vision</span>
            </div>
            <div class='feature'>
                <div class='feature-icon'>🎯</div>
                <span><strong>Market Analysis</strong> - Deep insights into your target market</span>
            </div>
            <div class='feature'>
                <div class='feature-icon'>💡</div>
                <span><strong>Strategy & Operations</strong> - Detailed implementation roadmap</span>
            </div>
            <div class='feature'>
                <div class='feature-icon'>💰</div>
                <span><strong>Financial Projections</strong> - Complete financial forecasts</span>
            </div>
        </div>

        <p style='text-align: center;'>
            <a href='https://sqordia.app/plans/{businessPlanId}' class='button'>View Your Business Plan →</a>
        </p>

        <p><strong>What's next?</strong></p>
        <ul>
            <li>Review each section and customize as needed</li>
            <li>Use our AI tools to expand or refine content</li>
            <li>Export to PDF or Word when ready</li>
            <li>Share with your team or investors</li>
        </ul>

        <div class='highlight'>
            <p><strong>💡 Pro Tip:</strong> You can regenerate individual sections or use AI suggestions to enhance any part of your plan at any time!</p>
        </div>

        <div class='footer'>
            <p>If you have any questions or need assistance, our support team is here to help!</p>
            <p>&copy; 2025 Sqordia. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}


using Azure;
using Azure.Communication.Email;
using AzureEmailAddress = Azure.Communication.Email.EmailAddress;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.ValueObjects;
using System.Globalization;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Azure Communication Services Email configuration
/// </summary>
public class AzureCommunicationEmailSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Sqordia";
    public string FrontendBaseUrl { get; set; } = "https://sqordia.app";
}

/// <summary>
/// Email service that sends emails directly via Azure Communication Services Email
/// </summary>
public class AzureCommunicationEmailService : IEmailService, IDisposable
{
    private readonly EmailClient? _emailClient;
    private readonly AzureCommunicationEmailSettings _settings;
    private readonly ILogger<AzureCommunicationEmailService> _logger;
    private readonly ILocalizationService _localizationService;
    private readonly IStringLocalizerFactory _localizerFactory;

    public AzureCommunicationEmailService(
        EmailClient? emailClient,
        IOptions<AzureCommunicationEmailSettings> settings,
        ILogger<AzureCommunicationEmailService> logger,
        ILocalizationService localizationService,
        IStringLocalizerFactory localizerFactory)
    {
        _emailClient = emailClient;
        _settings = settings.Value;
        _logger = logger;
        _localizationService = localizationService;
        _localizerFactory = localizerFactory;
    }

    /// <summary>
    /// Gets a localized string in the specified language
    /// </summary>
    private string GetLocalizedString(string language, string key, params object[] args)
    {
        var localizer = _localizerFactory.Create("Messages", "Sqordia.Application");
        
        // Normalize language code
        var lang = language?.ToLowerInvariant() switch
        {
            "en" or "en-us" or "en-ca" => "en",
            "fr" or "fr-ca" or "fr-fr" => "fr",
            _ => "fr" // Default to French
        };
        
        // Set culture for the localizer
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUICulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentCulture = new CultureInfo(lang);
        CultureInfo.CurrentUICulture = new CultureInfo(lang);
        
        var localizedString = localizer[key];
        string result = localizedString.ResourceNotFound 
            ? key 
            : (args?.Length > 0 ? string.Format(localizedString.Value, args) : localizedString.Value);
        
        // Restore original culture
        CultureInfo.CurrentCulture = originalCulture;
        CultureInfo.CurrentUICulture = originalUICulture;
        
        return result;
    }
    
    /// <summary>
    /// Determines the language to use for emails (defaults to French)
    /// </summary>
    private string GetEmailLanguage(string? preferredLanguage = null)
    {
        // If language is explicitly provided, use it
        if (!string.IsNullOrWhiteSpace(preferredLanguage))
        {
            return preferredLanguage.ToLowerInvariant() switch
            {
                "en" or "en-us" or "en-ca" => "en",
                "fr" or "fr-ca" or "fr-fr" => "fr",
                _ => "fr"
            };
        }
        
        // Try to get from HTTP context if available
        try
        {
            var currentLang = _localizationService.GetCurrentLanguage();
            return currentLang;
        }
        catch
        {
            // If no HTTP context, default to French
            return "fr";
        }
    }
    
    /// <summary>
    /// Gets the frontend base URL for email links
    /// </summary>
    private string GetFrontendBaseUrl()
    {
        var frontendUrl = _settings.FrontendBaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(frontendUrl))
        {
            // Fallback to environment variable or default
            frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL")?.TrimEnd('/')
                       ?? "https://sqordia.app";
        }
        return frontendUrl;
    }

    private async Task SendEmailInternalAsync(
        string toEmail,
        string? toName,
        string subject,
        string? plainTextBody = null,
        string? htmlBody = null,
        CancellationToken cancellationToken = default)
    {
        if (_emailClient == null)
        {
            _logger.LogWarning(
                "Azure Communication Services Email client not configured. Email would be sent: To={ToEmail}, Subject={Subject}",
                toEmail, subject);
            return;
        }

        try
        {
            // Prepare email content
            var emailContent = new EmailContent(subject);
            
            if (!string.IsNullOrEmpty(htmlBody))
            {
                emailContent.Html = htmlBody;
            }
            
            if (!string.IsNullOrEmpty(plainTextBody))
            {
                emailContent.PlainText = plainTextBody;
            }
            else if (!string.IsNullOrEmpty(htmlBody))
            {
                // If only HTML is provided, use it as plain text fallback
                emailContent.PlainText = htmlBody;
            }

            // Prepare recipients
            var toRecipients = new List<AzureEmailAddress>
            {
                new AzureEmailAddress(toEmail, displayName: toName)
            };

            var emailRecipients = new EmailRecipients(toRecipients);
            var emailMessage = new EmailMessage(_settings.FromEmail, emailRecipients, emailContent);

            // Send email via Azure Communication Services
            var emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Started,
                emailMessage,
                cancellationToken);

            _logger.LogInformation(
                "Email queued for sending. Operation ID: {OperationId}, To: {ToEmail}, Subject: {Subject}",
                emailSendOperation.Id,
                toEmail,
                subject);

            // Wait for completion asynchronously (don't block)
            _ = Task.Run(async () =>
            {
                try
                {
                    await emailSendOperation.WaitForCompletionAsync(cancellationToken);
                    if (emailSendOperation.HasCompleted && emailSendOperation.HasValue)
                    {
                        var result = emailSendOperation.Value;
                        if (result.Status == EmailSendStatus.Succeeded)
                        {
                            _logger.LogInformation(
                                "Successfully sent email to {ToEmail}, Subject: {Subject}",
                                toEmail,
                                subject);
                        }
                        else
                        {
                            _logger.LogError(
                                "Email failed with status: {Status}, To: {ToEmail}, Subject: {Subject}",
                                result.Status,
                                toEmail,
                                subject);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error waiting for email completion. To: {ToEmail}, Subject: {Subject}",
                        toEmail,
                        subject);
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email. To: {ToEmail}, Subject: {Subject}",
                toEmail,
                subject);
            throw;
        }
    }

    public async Task SendEmailAsync(Sqordia.Domain.ValueObjects.EmailAddress to, string subject, string body)
    {
        await SendEmailInternalAsync(to.Value, null, subject, plainTextBody: body);
    }

    public async Task SendEmailAsync(IEnumerable<Sqordia.Domain.ValueObjects.EmailAddress> to, string subject, string body)
    {
        var tasks = to.Select(email => SendEmailInternalAsync(email.Value, null, subject, plainTextBody: body));
        await Task.WhenAll(tasks);
    }

    public async Task SendHtmlEmailAsync(Sqordia.Domain.ValueObjects.EmailAddress to, string subject, string htmlBody)
    {
        await SendEmailInternalAsync(to.Value, null, subject, htmlBody: htmlBody);
    }

    public async Task SendHtmlEmailAsync(IEnumerable<Sqordia.Domain.ValueObjects.EmailAddress> to, string subject, string htmlBody)
    {
        var tasks = to.Select(email => SendEmailInternalAsync(email.Value, null, subject, htmlBody: htmlBody));
        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        // EmailClient is managed by DI container, no need to dispose here
    }

    public async Task SendWelcomeWithVerificationAsync(string email, string firstName, string lastName, string userName, string verificationToken)
    {
        var language = GetEmailLanguage();
        var subject = GetLocalizedString(language, "Email.Subject.WelcomeVerification");
        var htmlBody = GetWelcomeWithVerificationTemplate(firstName, lastName, verificationToken, language);
        await SendEmailInternalAsync(
            email,
            $"{firstName} {lastName}",
            subject,
            htmlBody: htmlBody);
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName, string lastName)
    {
        var language = GetEmailLanguage();
        var subject = GetLocalizedString(language, "Email.Subject.Welcome");
        var htmlBody = GetWelcomeEmailTemplate(firstName, lastName, language);
        await SendEmailInternalAsync(
            email,
            $"{firstName} {lastName}",
            subject,
            htmlBody: htmlBody);
    }

    public async Task SendEmailVerificationAsync(string email, string userName, string verificationToken)
    {
        var language = GetEmailLanguage();
        var subject = GetLocalizedString(language, "Email.Subject.Verification");
        var htmlBody = GetEmailVerificationTemplate(userName, verificationToken, language);
        await SendEmailInternalAsync(
            email,
            userName,
            subject,
            htmlBody: htmlBody);
    }

    public async Task SendPasswordResetAsync(string email, string userName, string resetToken)
    {
        var language = GetEmailLanguage();
        var subject = GetLocalizedString(language, "Email.Subject.PasswordReset");
        var htmlBody = GetPasswordResetTemplate(userName, resetToken, language);
        await SendEmailInternalAsync(
            email,
            userName,
            subject,
            htmlBody: htmlBody);
    }

    public async Task SendAccountLockedAsync(string email, string userName, DateTime lockedUntil)
    {
        var language = GetEmailLanguage();
        var subject = GetLocalizedString(language, "Email.Subject.AccountLocked");
        var htmlBody = GetAccountLockedTemplate(userName, lockedUntil, language);
        await SendEmailInternalAsync(
            email,
            userName,
            subject,
            htmlBody: htmlBody);
    }

    public async Task SendLoginAlertAsync(string email, string userName, string ipAddress, DateTime loginTime)
    {
        var language = GetEmailLanguage();
        var subject = GetLocalizedString(language, "Email.Subject.LoginAlert");
        var htmlBody = GetLoginAlertTemplate(userName, ipAddress, loginTime, language);
        await SendEmailInternalAsync(
            email,
            userName,
            subject,
            htmlBody: htmlBody);
    }

    public async Task SendAccountLockoutNotificationAsync(string email, string firstName, TimeSpan lockoutDuration, DateTime lockedAt)
    {
        var language = GetEmailLanguage();
        var subject = GetLocalizedString(language, "Email.Subject.AccountLocked");
        var htmlBody = GetAccountLockoutTemplate(firstName, lockoutDuration, lockedAt, language);
        await SendEmailInternalAsync(
            email,
            firstName,
            subject,
            htmlBody: htmlBody);
    }

    public async Task SendOrganizationInvitationAsync(string email, string invitationToken, string? message = null)
    {
        var language = GetEmailLanguage();
        var subject = GetLocalizedString(language, "Email.Subject.OrganizationInvitation");
        var htmlBody = GetOrganizationInvitationTemplate(email, invitationToken, message, language);
        await SendEmailInternalAsync(
            email,
            email,
            subject,
            htmlBody: htmlBody);
    }

    // Template methods - Single language (French or English)
    private string GetEmailVerificationTemplate(string userName, string verificationToken, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var frontendUrl = GetFrontendBaseUrl();
        var verifyUrl = $"{frontendUrl}/verify-email?token={verificationToken}";
        var title = GetLocalizedString(lang, "Email.Verification.Title");
        var greeting = GetLocalizedString(lang, "Email.Verification.Greeting", userName);
        var thankYou = GetLocalizedString(lang, "Email.Verification.ThankYou");
        var buttonText = GetLocalizedString(lang, "Email.Verification.ButtonText");
        var altText = GetLocalizedString(lang, "Email.Verification.AlternativeText");
        var expiry = GetLocalizedString(lang, "Email.Verification.ExpiryNote");
        var ignore = GetLocalizedString(lang, "Email.Verification.IgnoreNote");
        var copyright = GetLocalizedString(lang, "Email.Footer.Copyright");
        
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
        <h2>{title}</h2>
        <p>{greeting}</p>
        <p>{thankYou}</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{verifyUrl}' class='button'>{buttonText}</a>
        </p>
        <p>{altText}</p>
        <p style='word-break: break-all; color: #666;'>{verifyUrl}</p>
        <p>{expiry}</p>
        <div class='footer'>
            <p>{ignore}</p>
            <p>{copyright}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPasswordResetTemplate(string userName, string resetToken, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var frontendUrl = GetFrontendBaseUrl();
        var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}";
        var title = GetLocalizedString(lang, "Email.PasswordReset.Title");
        var greeting = GetLocalizedString(lang, "Email.PasswordReset.Greeting", userName);
        var request = GetLocalizedString(lang, "Email.PasswordReset.RequestReceived");
        var buttonText = GetLocalizedString(lang, "Email.PasswordReset.ButtonText");
        var altText = GetLocalizedString(lang, "Email.PasswordReset.AlternativeText");
        var expiry = GetLocalizedString(lang, "Email.PasswordReset.ExpiryNote");
        var ignore = GetLocalizedString(lang, "Email.PasswordReset.IgnoreNote");
        var copyright = GetLocalizedString(lang, "Email.Footer.Copyright");
        
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
        <h2>{title}</h2>
        <p>{greeting}</p>
        <p>{request}</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{resetUrl}' class='button'>{buttonText}</a>
        </p>
        <p>{altText}</p>
        <p style='word-break: break-all; color: #666;'>{resetUrl}</p>
        <p>{expiry}</p>
        <div class='footer'>
            <p>{ignore}</p>
            <p>{copyright}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetAccountLockedTemplate(string userName, DateTime lockedUntil, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var title = GetLocalizedString(lang, "Email.AccountLocked.Title");
        var greeting = GetLocalizedString(lang, "Email.AccountLocked.Greeting", userName);
        var notification = GetLocalizedString(lang, "Email.AccountLocked.Notification");
        var time = GetLocalizedString(lang, "Email.LoginAlert.Time", lockedUntil.ToString("yyyy-MM-dd HH:mm"));
        var autoUnlock = GetLocalizedString(lang, "Email.AccountLocked.AutoUnlock");
        var contactSupport = GetLocalizedString(lang, "Email.AccountLocked.ContactSupport");
        var copyright = GetLocalizedString(lang, "Email.Footer.Copyright");
        
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
        <h2>{title}</h2>
        <p>{greeting}</p>
        <div class='alert'>
            <p><strong>{notification}</strong></p>
            <p>{time} UTC</p>
        </div>
        <p>{autoUnlock}</p>
        <p>{contactSupport}</p>
        <div class='footer'>
            <p>{contactSupport}</p>
            <p>{copyright}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetLoginAlertTemplate(string userName, string ipAddress, DateTime loginTime, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var title = GetLocalizedString(lang, "Email.LoginAlert.Title");
        var greeting = GetLocalizedString(lang, "Email.LoginAlert.Greeting", userName);
        var notification = GetLocalizedString(lang, "Email.LoginAlert.Notification");
        var time = GetLocalizedString(lang, "Email.LoginAlert.Time", loginTime.ToString("yyyy-MM-dd HH:mm"));
        var ipAddressText = GetLocalizedString(lang, "Email.LoginAlert.IpAddress", ipAddress);
        var wasYou = GetLocalizedString(lang, "Email.LoginAlert.WasYou");
        var noAction = GetLocalizedString(lang, "Email.LoginAlert.NoAction");
        var notYou = GetLocalizedString(lang, "Email.LoginAlert.NotYou");
        var takeAction = GetLocalizedString(lang, "Email.LoginAlert.TakeAction");
        var contactSupport = GetLocalizedString(lang, "Email.AccountLocked.ContactSupport");
        var copyright = GetLocalizedString(lang, "Email.Footer.Copyright");
        
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
        <h2>{title}</h2>
        <p>{greeting}</p>
        <p>{notification}</p>
        <div class='info'>
            <p><strong>{time}</strong> UTC</p>
            <p><strong>{ipAddressText}</strong></p>
        </div>
        <p><strong>{wasYou}</strong> {noAction}</p>
        <p><strong>{notYou}</strong> {takeAction}</p>
        <div class='footer'>
            <p>{contactSupport}</p>
            <p>{copyright}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetWelcomeWithVerificationTemplate(string firstName, string lastName, string verificationToken, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var frontendUrl = GetFrontendBaseUrl();
        var verifyUrl = $"{frontendUrl}/verify-email?token={verificationToken}";
        var greeting = GetLocalizedString(lang, "Email.Welcome.Greeting", firstName);
        var thankYou = GetLocalizedString(lang, "Email.Welcome.ThankYou");
        var nextStep = GetLocalizedString(lang, "Email.Welcome.NextStep");
        var verificationInstruction = GetLocalizedString(lang, "Email.Welcome.VerificationInstruction");
        var verificationTitle = GetLocalizedString(lang, "Email.Verification.Title");
        var verificationThankYou = GetLocalizedString(lang, "Email.Verification.ThankYou");
        var buttonText = GetLocalizedString(lang, "Email.Verification.ButtonText");
        var expiryNote = GetLocalizedString(lang, "Email.Verification.ExpiryNote");
        var altText = GetLocalizedString(lang, "Email.Verification.AlternativeText");
        var ignoreNote = GetLocalizedString(lang, "Email.Verification.IgnoreNote");
        var support = GetLocalizedString(lang, "Email.Welcome.Support");
        var copyright = GetLocalizedString(lang, "Email.Footer.Copyright");
        
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
            <div class='welcome'>{greeting}</div>
        </div>
        
        <p>{thankYou}</p>
        
        <div class='highlight'>
            <p><strong>{nextStep}</strong> {verificationInstruction}</p>
        </div>
        
        <div class='verify-section'>
            <h3 style='margin-top: 0; color: #1e40af;'>{verificationTitle}</h3>
            <p style='margin: 15px 0;'>{verificationThankYou}</p>
            <p style='margin: 25px 0;'>
                <a href='{verifyUrl}' class='button'>{buttonText}</a>
            </p>
            <p style='font-size: 12px; color: #666; margin-top: 20px;'>{expiryNote}</p>
        </div>
        
        <p style='font-size: 13px; color: #666;'>{altText}</p>
        <p style='word-break: break-all; font-size: 12px; color: #3b82f6; background-color: #f8fafc; padding: 10px; border-radius: 4px;'>{verifyUrl}</p>
        
        <div class='footer'>
            <p>{ignoreNote}</p>
            <p>{support}</p>
            <p>{copyright}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetWelcomeEmailTemplate(string firstName, string lastName, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var greeting = GetLocalizedString(lang, "Email.Welcome.Greeting", firstName);
        var thankYou = GetLocalizedString(lang, "Email.Welcome.ThankYou");
        var nextStep = GetLocalizedString(lang, "Email.Welcome.NextStep");
        var verificationInstruction = GetLocalizedString(lang, "Email.Welcome.VerificationInstruction");
        var whatYouCanDo = GetLocalizedString(lang, "Email.Welcome.WhatYouCanDo");
        var feature1 = GetLocalizedString(lang, "Email.Welcome.Feature1");
        var feature2 = GetLocalizedString(lang, "Email.Welcome.Feature2");
        var feature3 = GetLocalizedString(lang, "Email.Welcome.Feature3");
        var feature4 = GetLocalizedString(lang, "Email.Welcome.Feature4");
        var readyToStart = GetLocalizedString(lang, "Email.Welcome.ReadyToStart");
        var support = GetLocalizedString(lang, "Email.Welcome.Support");
        var hereToHelp = GetLocalizedString(lang, "Email.Welcome.HereToHelp");
        var copyright = GetLocalizedString(lang, "Email.Footer.Copyright");
        
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
            <div class='welcome'>🎉 {greeting}</div>
        </div>
        
        <p>{thankYou}</p>
        
        <div class='highlight'>
            <p><strong>{nextStep}</strong> {verificationInstruction}</p>
        </div>
        
        <h3>{whatYouCanDo}</h3>
        <div class='features'>
            <div class='feature'>
                <div class='feature-icon'>📊</div>
                <span>{feature1}</span>
            </div>
            <div class='feature'>
                <div class='feature-icon'>📈</div>
                <span>{feature2}</span>
            </div>
            <div class='feature'>
                <div class='feature-icon'>🤝</div>
                <span>{feature3}</span>
            </div>
            <div class='feature'>
                <div class='feature-icon'>📋</div>
                <span>{feature4}</span>
            </div>
        </div>
        
        <div class='cta'>
            <p>{readyToStart}</p>
        </div>
        
        <div class='footer'>
            <p>{support}</p>
            <p>{hereToHelp}</p>
            <p>{copyright}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetOrganizationInvitationTemplate(string email, string invitationToken, string? message, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var frontendUrl = GetFrontendBaseUrl();
        var acceptUrl = $"{frontendUrl}/accept-invitation?token={invitationToken}";
        var title = GetLocalizedString(lang, "Email.OrganizationInvitation.Title");
        var greeting = GetLocalizedString(lang, "Email.OrganizationInvitation.Greeting");
        var description = GetLocalizedString(lang, "Email.OrganizationInvitation.Description");
        var personalMessage = GetLocalizedString(lang, "Email.OrganizationInvitation.PersonalMessage");
        var buttonText = GetLocalizedString(lang, "Email.OrganizationInvitation.ButtonText");
        var altText = GetLocalizedString(lang, "Email.OrganizationInvitation.AlternativeText");
        var expiry = GetLocalizedString(lang, "Email.OrganizationInvitation.Expiry");
        var ignoreNote = GetLocalizedString(lang, "Email.OrganizationInvitation.IgnoreNote");
        var copyright = GetLocalizedString(lang, "Email.Footer.Copyright");
        
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
        <h2>{title}</h2>
        <p>{greeting}</p>
        <p>{description}</p>
        
        {(string.IsNullOrEmpty(message) ? "" : $@"
        <div class='message'>
            <p><strong>{personalMessage}</strong></p>
            <p>{message}</p>
        </div>")}
        
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{acceptUrl}' class='button'>{buttonText}</a>
        </p>
        
        <p>{altText}</p>
        <p style='word-break: break-all; color: #666;'>{acceptUrl}</p>
        
        <p>{expiry}</p>
        
        <div class='footer'>
            <p>{ignoreNote}</p>
            <p>{copyright}</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetAccountLockoutTemplate(string firstName, TimeSpan lockoutDuration, DateTime lockedAt, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var unlockTime = lockedAt.Add(lockoutDuration);
        var minutesRemaining = (int)lockoutDuration.TotalMinutes;
        
        var title = GetLocalizedString(lang, "Email.AccountLocked.Title");
        var greeting = GetLocalizedString(lang, "Email.AccountLocked.Greeting", firstName);
        var notification = GetLocalizedString(lang, "Email.AccountLocked.Notification");
        var details = GetLocalizedString(lang, "Email.LoginAlert.Details");
        var lockedAtTime = GetLocalizedString(lang, "Email.LoginAlert.Time", lockedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        var duration = GetLocalizedString(lang, "Email.AccountLocked.Duration", minutesRemaining);
        var unlockTimeText = GetLocalizedString(lang, "Email.LoginAlert.Time", unlockTime.ToString("yyyy-MM-dd HH:mm:ss"));
        var notYou = GetLocalizedString(lang, "Email.AccountLocked.NotYou");
        var autoUnlock = GetLocalizedString(lang, "Email.AccountLocked.AutoUnlock");
        var securityAdvice = GetLocalizedString(lang, "Email.AccountLocked.SecurityAdvice");
        var contactSupport = GetLocalizedString(lang, "Email.AccountLocked.ContactSupport");
        var doNotReply = GetLocalizedString(lang, "Email.Footer.DoNotReply");
        var copyright = GetLocalizedString(lang, "Email.Footer.Copyright");
        
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
            <div class='alert'>{title}</div>
        </div>
        
        <p>{greeting}</p>
        
        <div class='warning-box'>
            <p><strong>{title}</strong></p>
            <p>{notification}</p>
        </div>
        
        <div class='info-box'>
            <p><strong>{details}</strong></p>
            <p>{lockedAtTime} UTC</p>
            <p>{duration}</p>
            <p>{unlockTimeText} UTC</p>
        </div>
        
        <p><strong>{notYou}</strong></p>
        <ul>
            <li>{autoUnlock}</li>
            <li>{securityAdvice}</li>
            <li>{contactSupport}</li>
        </ul>
        
        <p><strong>{notYou}</strong></p>
        <p>{contactSupport}</p>
        
        <div class='footer'>
            <p>{doNotReply}</p>
            <p>{contactSupport}</p>
            <p>{copyright}</p>
        </div>
    </div>
</body>
</html>";
    }
}

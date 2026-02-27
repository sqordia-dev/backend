using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using Sqordia.Application.Common.Interfaces;
using System.Globalization;
using DomainEmailAddress = Sqordia.Domain.ValueObjects.EmailAddress;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Resend Email configuration
/// </summary>
public class ResendEmailSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Sqordia";
    public string FrontendBaseUrl { get; set; } = "https://sqordia.app";
}

/// <summary>
/// Email service that sends emails via Resend API
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly IResend _resendClient;
    private readonly ResendEmailSettings _settings;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly ILocalizationService _localizationService;
    private readonly IStringLocalizerFactory _localizerFactory;

    public ResendEmailService(
        IResend resendClient,
        IOptions<ResendEmailSettings> settings,
        ILogger<ResendEmailService> logger,
        ILocalizationService localizationService,
        IStringLocalizerFactory localizerFactory)
    {
        _resendClient = resendClient;
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
        if (!string.IsNullOrWhiteSpace(preferredLanguage))
        {
            return preferredLanguage.ToLowerInvariant() switch
            {
                "en" or "en-us" or "en-ca" => "en",
                "fr" or "fr-ca" or "fr-fr" => "fr",
                _ => "fr"
            };
        }

        try
        {
            var currentLang = _localizationService.GetCurrentLanguage();
            return currentLang;
        }
        catch
        {
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
        try
        {
            var fromAddress = string.IsNullOrEmpty(_settings.FromName)
                ? _settings.FromEmail
                : $"{_settings.FromName} <{_settings.FromEmail}>";

            var message = new EmailMessage
            {
                From = fromAddress,
                To = { toEmail },
                Subject = subject,
                HtmlBody = htmlBody ?? string.Empty,
                TextBody = plainTextBody ?? htmlBody ?? string.Empty
            };

            var response = await _resendClient.EmailSendAsync(message, cancellationToken);

            _logger.LogInformation(
                "Email sent successfully via Resend. MessageId: {MessageId}, To: {ToEmail}, Subject: {Subject}",
                response.Content,
                toEmail,
                subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email via Resend. To: {ToEmail}, Subject: {Subject}",
                toEmail,
                subject);
            throw;
        }
    }

    public async Task SendEmailAsync(DomainEmailAddress to, string subject, string body)
    {
        await SendEmailInternalAsync(to.Value, null, subject, plainTextBody: body);
    }

    public async Task SendEmailAsync(IEnumerable<DomainEmailAddress> to, string subject, string body)
    {
        var tasks = to.Select(email => SendEmailInternalAsync(email.Value, null, subject, plainTextBody: body));
        await Task.WhenAll(tasks);
    }

    public async Task SendHtmlEmailAsync(DomainEmailAddress to, string subject, string htmlBody)
    {
        await SendEmailInternalAsync(to.Value, null, subject, htmlBody: htmlBody);
    }

    public async Task SendHtmlEmailAsync(IEnumerable<DomainEmailAddress> to, string subject, string htmlBody)
    {
        var tasks = to.Select(email => SendEmailInternalAsync(email.Value, null, subject, htmlBody: htmlBody));
        await Task.WhenAll(tasks);
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

    public async Task SendBusinessPlanGeneratedAsync(string email, string userName, string businessPlanId, string businessPlanTitle)
    {
        var subject = $"Your Business Plan '{businessPlanTitle}' is Ready!";
        var htmlBody = GetBusinessPlanGeneratedTemplate(userName, businessPlanId, businessPlanTitle);
        await SendEmailInternalAsync(
            email,
            userName,
            subject,
            htmlBody: htmlBody);
    }

    #region Email Templates

    /// <summary>
    /// Brain icon SVG for email header (Lucide Brain icon)
    /// </summary>
    private const string BrainIconSvg = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""32"" height=""32"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#FF6B00"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""><path d=""M12 5a3 3 0 1 0-5.997.125 4 4 0 0 0-2.526 5.77 4 4 0 0 0 .556 6.588A4 4 0 1 0 12 18Z""/><path d=""M12 5a3 3 0 1 1 5.997.125 4 4 0 0 1 2.526 5.77 4 4 0 0 1-.556 6.588A4 4 0 1 1 12 18Z""/><path d=""M15 13a4.5 4.5 0 0 1-3-4 4.5 4.5 0 0 1-3 4""/><path d=""M17.599 6.5a3 3 0 0 0 .399-1.375""/><path d=""M6.003 5.125A3 3 0 0 0 6.401 6.5""/><path d=""M3.477 10.896a4 4 0 0 1 .585-.396""/><path d=""M19.938 10.5a4 4 0 0 1 .585.396""/><path d=""M6 18a4 4 0 0 1-1.967-.516""/><path d=""M19.967 17.484A4 4 0 0 1 18 18""/></svg>";

    private string GetEmailVerificationTemplate(string userName, string verificationToken, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var frontendUrl = GetFrontendBaseUrl();
        var verifyUrl = $"{frontendUrl}/verify-email?token={verificationToken}";

        var isEnglish = lang == "en";
        var title = isEnglish ? "Verify Your Email" : "Vérifiez votre email";
        var greeting = isEnglish ? $"Hi {userName}," : $"Bonjour {userName},";
        var thankYou = isEnglish
            ? "Thank you for signing up with Sqordia! Please verify your email address to complete your registration."
            : "Merci de vous être inscrit chez Sqordia ! Veuillez vérifier votre adresse email pour compléter votre inscription.";
        var buttonText = isEnglish ? "Verify Email" : "Vérifier mon email";
        var altText = isEnglish
            ? "Or copy and paste this link into your browser:"
            : "Ou copiez et collez ce lien dans votre navigateur :";
        var expiry = isEnglish
            ? "This link will expire in 24 hours."
            : "Ce lien expirera dans 24 heures.";
        var ignore = isEnglish
            ? "If you didn't create an account with Sqordia, you can safely ignore this email."
            : "Si vous n'avez pas créé de compte chez Sqordia, vous pouvez ignorer cet email.";

        return GetBaseEmailTemplate(lang, $@"
            <h1 style=""color: #1A2B47; font-size: 24px; font-weight: 600; margin: 0 0 24px 0; text-align: center;"">{title}</h1>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">{greeting}</p>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 32px 0;"">{thankYou}</p>
            <div style=""text-align: center; margin: 32px 0;"">
                <a href=""{verifyUrl}"" style=""display: inline-block; padding: 14px 32px; background: #FF6B00; color: white; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 16px;"">{buttonText}</a>
            </div>
            <p style=""color: #6B7280; font-size: 14px; margin: 24px 0 8px 0;"">{altText}</p>
            <p style=""word-break: break-all; color: #FF6B00; font-size: 13px; background: #FFF7ED; padding: 12px 16px; border-radius: 8px; margin: 0 0 24px 0;"">{verifyUrl}</p>
            <p style=""color: #9CA3AF; font-size: 13px; margin: 0 0 8px 0;"">{expiry}</p>
            <p style=""color: #9CA3AF; font-size: 13px; margin: 0;"">{ignore}</p>");
    }

    private string GetPasswordResetTemplate(string userName, string resetToken, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var frontendUrl = GetFrontendBaseUrl();
        var resetUrl = $"{frontendUrl}/reset-password?token={resetToken}";

        var isEnglish = lang == "en";
        var title = isEnglish ? "Reset Your Password" : "Réinitialisez votre mot de passe";
        var greeting = isEnglish ? $"Hi {userName}," : $"Bonjour {userName},";
        var request = isEnglish
            ? "We received a request to reset your password. Click the button below to create a new password."
            : "Nous avons reçu une demande de réinitialisation de votre mot de passe. Cliquez sur le bouton ci-dessous pour créer un nouveau mot de passe.";
        var buttonText = isEnglish ? "Reset Password" : "Réinitialiser";
        var altText = isEnglish
            ? "Or copy and paste this link into your browser:"
            : "Ou copiez et collez ce lien dans votre navigateur :";
        var expiry = isEnglish
            ? "This link will expire in 1 hour."
            : "Ce lien expirera dans 1 heure.";
        var ignore = isEnglish
            ? "If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged."
            : "Si vous n'avez pas demandé de réinitialisation, ignorez cet email. Votre mot de passe restera inchangé.";

        return GetBaseEmailTemplate(lang, $@"
            <div style=""text-align: center; margin-bottom: 24px;"">
                <div style=""display: inline-block; width: 64px; height: 64px; background: #FEF2F2; border-radius: 50%; line-height: 64px;"">
                    <span style=""font-size: 32px;"">🔐</span>
                </div>
            </div>
            <h1 style=""color: #1A2B47; font-size: 24px; font-weight: 600; margin: 0 0 24px 0; text-align: center;"">{title}</h1>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">{greeting}</p>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 32px 0;"">{request}</p>
            <div style=""text-align: center; margin: 32px 0;"">
                <a href=""{resetUrl}"" style=""display: inline-block; padding: 14px 32px; background: #DC2626; color: white; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 16px;"">{buttonText}</a>
            </div>
            <p style=""color: #6B7280; font-size: 14px; margin: 24px 0 8px 0;"">{altText}</p>
            <p style=""word-break: break-all; color: #DC2626; font-size: 13px; background: #FEF2F2; padding: 12px 16px; border-radius: 8px; margin: 0 0 24px 0;"">{resetUrl}</p>
            <p style=""color: #9CA3AF; font-size: 13px; margin: 0 0 8px 0;"">{expiry}</p>
            <p style=""color: #9CA3AF; font-size: 13px; margin: 0;"">{ignore}</p>");
    }

    private string GetAccountLockedTemplate(string userName, DateTime lockedUntil, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var isEnglish = lang == "en";

        var title = isEnglish ? "Account Temporarily Locked" : "Compte temporairement verrouillé";
        var greeting = isEnglish ? $"Hi {userName}," : $"Bonjour {userName},";
        var notification = isEnglish
            ? "Your account has been temporarily locked due to multiple failed login attempts."
            : "Votre compte a été temporairement verrouillé en raison de plusieurs tentatives de connexion échouées.";
        var unlockInfo = isEnglish
            ? $"Your account will be automatically unlocked at:"
            : $"Votre compte sera automatiquement déverrouillé à :";
        var notYouTitle = isEnglish ? "Wasn't you?" : "Ce n'était pas vous ?";
        var notYouText = isEnglish
            ? "If you didn't attempt to log in, your password may be compromised. We recommend resetting it immediately."
            : "Si vous n'avez pas essayé de vous connecter, votre mot de passe pourrait être compromis. Nous vous recommandons de le réinitialiser.";
        var support = isEnglish
            ? "Need help? Contact us at support@sqordia.com"
            : "Besoin d'aide ? Contactez-nous à support@sqordia.com";

        return GetBaseEmailTemplate(lang, $@"
            <div style=""text-align: center; margin-bottom: 24px;"">
                <div style=""display: inline-block; width: 64px; height: 64px; background: #FEF2F2; border-radius: 50%; line-height: 64px;"">
                    <span style=""font-size: 32px;"">🔒</span>
                </div>
            </div>
            <h1 style=""color: #DC2626; font-size: 24px; font-weight: 600; margin: 0 0 24px 0; text-align: center;"">{title}</h1>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">{greeting}</p>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">{notification}</p>
            <div style=""background: #F3F4F6; padding: 20px; border-radius: 12px; margin: 0 0 24px 0; text-align: center;"">
                <p style=""color: #6B7280; font-size: 14px; margin: 0 0 8px 0;"">{unlockInfo}</p>
                <p style=""color: #1A2B47; font-size: 20px; font-weight: 600; margin: 0;"">{lockedUntil:yyyy-MM-dd HH:mm} UTC</p>
            </div>
            <div style=""background: #FFF7ED; border-left: 4px solid #F59E0B; padding: 16px 20px; border-radius: 0 8px 8px 0; margin: 0 0 24px 0;"">
                <p style=""color: #92400E; font-size: 14px; font-weight: 600; margin: 0 0 8px 0;"">{notYouTitle}</p>
                <p style=""color: #78716C; font-size: 14px; line-height: 1.5; margin: 0;"">{notYouText}</p>
            </div>
            <p style=""color: #9CA3AF; font-size: 13px; text-align: center; margin: 0;"">{support}</p>");
    }

    private string GetLoginAlertTemplate(string userName, string ipAddress, DateTime loginTime, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var isEnglish = lang == "en";

        var title = isEnglish ? "New Login Detected" : "Nouvelle connexion détectée";
        var greeting = isEnglish ? $"Hi {userName}," : $"Bonjour {userName},";
        var notification = isEnglish
            ? "We noticed a new login to your Sqordia account."
            : "Nous avons détecté une nouvelle connexion à votre compte Sqordia.";
        var detailsTitle = isEnglish ? "Login Details" : "Détails de connexion";
        var timeLabel = isEnglish ? "Time" : "Heure";
        var ipLabel = isEnglish ? "IP Address" : "Adresse IP";
        var wasYouTitle = isEnglish ? "Was this you?" : "C'était vous ?";
        var wasYouText = isEnglish
            ? "If yes, you can safely ignore this email."
            : "Si oui, vous pouvez ignorer cet email.";
        var notYouTitle = isEnglish ? "Wasn't you?" : "Ce n'était pas vous ?";
        var notYouText = isEnglish
            ? "Secure your account immediately by changing your password."
            : "Sécurisez votre compte immédiatement en changeant votre mot de passe.";

        return GetBaseEmailTemplate(lang, $@"
            <div style=""text-align: center; margin-bottom: 24px;"">
                <div style=""display: inline-block; width: 64px; height: 64px; background: #EFF6FF; border-radius: 50%; line-height: 64px;"">
                    <span style=""font-size: 32px;"">🔔</span>
                </div>
            </div>
            <h1 style=""color: #1A2B47; font-size: 24px; font-weight: 600; margin: 0 0 24px 0; text-align: center;"">{title}</h1>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">{greeting}</p>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">{notification}</p>
            <div style=""background: #F9FAFB; border: 1px solid #E5E7EB; padding: 20px; border-radius: 12px; margin: 0 0 24px 0;"">
                <p style=""color: #6B7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; margin: 0 0 12px 0;"">{detailsTitle}</p>
                <table style=""width: 100%; border-collapse: collapse;"">
                    <tr>
                        <td style=""color: #6B7280; font-size: 14px; padding: 8px 0;"">{timeLabel}</td>
                        <td style=""color: #1A2B47; font-size: 14px; font-weight: 500; padding: 8px 0; text-align: right;"">{loginTime:yyyy-MM-dd HH:mm} UTC</td>
                    </tr>
                    <tr>
                        <td style=""color: #6B7280; font-size: 14px; padding: 8px 0;"">{ipLabel}</td>
                        <td style=""color: #1A2B47; font-size: 14px; font-weight: 500; padding: 8px 0; text-align: right;"">{ipAddress}</td>
                    </tr>
                </table>
            </div>
            <table style=""width: 100%; border-collapse: collapse; margin: 0 0 24px 0;"">
                <tr>
                    <td style=""width: 50%; padding: 12px; background: #F0FDF4; border-radius: 8px 0 0 8px; vertical-align: top;"">
                        <p style=""color: #059669; font-size: 14px; font-weight: 600; margin: 0 0 4px 0;"">✓ {wasYouTitle}</p>
                        <p style=""color: #6B7280; font-size: 13px; margin: 0;"">{wasYouText}</p>
                    </td>
                    <td style=""width: 50%; padding: 12px; background: #FEF2F2; border-radius: 0 8px 8px 0; vertical-align: top;"">
                        <p style=""color: #DC2626; font-size: 14px; font-weight: 600; margin: 0 0 4px 0;"">✗ {notYouTitle}</p>
                        <p style=""color: #6B7280; font-size: 13px; margin: 0;"">{notYouText}</p>
                    </td>
                </tr>
            </table>");
    }

    private string GetWelcomeWithVerificationTemplate(string firstName, string lastName, string verificationToken, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var frontendUrl = GetFrontendBaseUrl();
        var verifyUrl = $"{frontendUrl}/verify-email?token={verificationToken}";

        var isEnglish = lang == "en";
        var welcome = isEnglish ? $"Welcome, {firstName}!" : $"Bienvenue, {firstName} !";
        var thankYou = isEnglish
            ? "Thank you for joining Sqordia! We're excited to help you build your business plan."
            : "Merci de rejoindre Sqordia ! Nous sommes ravis de vous aider à créer votre plan d'affaires.";
        var verifyTitle = isEnglish ? "Verify your email to get started" : "Vérifiez votre email pour commencer";
        var verifyText = isEnglish
            ? "Click the button below to verify your email address and activate your account."
            : "Cliquez sur le bouton ci-dessous pour vérifier votre adresse email et activer votre compte.";
        var buttonText = isEnglish ? "Verify Email" : "Vérifier mon email";
        var whatYouCanDo = isEnglish ? "What you can do with Sqordia" : "Ce que vous pouvez faire avec Sqordia";
        var feature1 = isEnglish ? "Create professional business plans with AI assistance" : "Créer des plans d'affaires professionnels avec l'IA";
        var feature2 = isEnglish ? "Track your business metrics and KPIs" : "Suivre vos métriques et indicateurs clés";
        var feature3 = isEnglish ? "Collaborate with your team in real-time" : "Collaborer avec votre équipe en temps réel";
        var feature4 = isEnglish ? "Export bank-ready documents" : "Exporter des documents prêts pour les banques";
        var expiry = isEnglish
            ? "This verification link will expire in 24 hours."
            : "Ce lien de vérification expirera dans 24 heures.";
        var support = isEnglish
            ? "Questions? We're here to help at support@sqordia.com"
            : "Des questions ? Nous sommes là pour vous aider à support@sqordia.com";

        return GetBaseEmailTemplate(lang, $@"
            <div style=""text-align: center; padding: 32px 24px; background: linear-gradient(135deg, #1A2B47 0%, #2D4263 100%); border-radius: 12px; margin: -32px -32px 32px -32px;"">
                <h1 style=""color: white; font-size: 28px; font-weight: 600; margin: 0;"">{welcome}</h1>
            </div>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">{thankYou}</p>

            <div style=""background: #FFF7ED; border: 2px solid #FF6B00; padding: 24px; border-radius: 12px; margin: 0 0 32px 0; text-align: center;"">
                <h2 style=""color: #1A2B47; font-size: 18px; font-weight: 600; margin: 0 0 12px 0;"">{verifyTitle}</h2>
                <p style=""color: #6B7280; font-size: 14px; margin: 0 0 20px 0;"">{verifyText}</p>
                <a href=""{verifyUrl}"" style=""display: inline-block; padding: 14px 36px; background: #FF6B00; color: white; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 16px;"">{buttonText}</a>
            </div>

            <h3 style=""color: #1A2B47; font-size: 16px; font-weight: 600; margin: 0 0 16px 0;"">{whatYouCanDo}</h3>
            <table style=""width: 100%; border-collapse: collapse; margin: 0 0 24px 0;"">
                <tr>
                    <td style=""padding: 12px 0; border-bottom: 1px solid #F3F4F6;"">
                        <table style=""border-collapse: collapse;""><tr>
                            <td style=""width: 40px; vertical-align: top;""><span style=""display: inline-block; width: 32px; height: 32px; background: #EFF6FF; border-radius: 8px; text-align: center; line-height: 32px; font-size: 16px;"">📊</span></td>
                            <td style=""color: #4B5563; font-size: 14px; padding-left: 12px;"">{feature1}</td>
                        </tr></table>
                    </td>
                </tr>
                <tr>
                    <td style=""padding: 12px 0; border-bottom: 1px solid #F3F4F6;"">
                        <table style=""border-collapse: collapse;""><tr>
                            <td style=""width: 40px; vertical-align: top;""><span style=""display: inline-block; width: 32px; height: 32px; background: #F0FDF4; border-radius: 8px; text-align: center; line-height: 32px; font-size: 16px;"">📈</span></td>
                            <td style=""color: #4B5563; font-size: 14px; padding-left: 12px;"">{feature2}</td>
                        </tr></table>
                    </td>
                </tr>
                <tr>
                    <td style=""padding: 12px 0; border-bottom: 1px solid #F3F4F6;"">
                        <table style=""border-collapse: collapse;""><tr>
                            <td style=""width: 40px; vertical-align: top;""><span style=""display: inline-block; width: 32px; height: 32px; background: #FFF7ED; border-radius: 8px; text-align: center; line-height: 32px; font-size: 16px;"">👥</span></td>
                            <td style=""color: #4B5563; font-size: 14px; padding-left: 12px;"">{feature3}</td>
                        </tr></table>
                    </td>
                </tr>
                <tr>
                    <td style=""padding: 12px 0;"">
                        <table style=""border-collapse: collapse;""><tr>
                            <td style=""width: 40px; vertical-align: top;""><span style=""display: inline-block; width: 32px; height: 32px; background: #FDF2F8; border-radius: 8px; text-align: center; line-height: 32px; font-size: 16px;"">📄</span></td>
                            <td style=""color: #4B5563; font-size: 14px; padding-left: 12px;"">{feature4}</td>
                        </tr></table>
                    </td>
                </tr>
            </table>
            <p style=""color: #9CA3AF; font-size: 13px; margin: 0 0 8px 0;"">{expiry}</p>
            <p style=""color: #9CA3AF; font-size: 13px; margin: 0;"">{support}</p>");
    }

    private string GetWelcomeEmailTemplate(string firstName, string lastName, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var frontendUrl = GetFrontendBaseUrl();

        var isEnglish = lang == "en";
        var welcome = isEnglish ? $"Welcome, {firstName}!" : $"Bienvenue, {firstName} !";
        var thankYou = isEnglish
            ? "Thank you for joining Sqordia! Your account is now active and you're ready to create your first business plan."
            : "Merci de rejoindre Sqordia ! Votre compte est maintenant actif et vous êtes prêt à créer votre premier plan d'affaires.";
        var buttonText = isEnglish ? "Go to Dashboard" : "Aller au tableau de bord";
        var whatYouCanDo = isEnglish ? "What you can do with Sqordia" : "Ce que vous pouvez faire avec Sqordia";
        var feature1 = isEnglish ? "Create professional business plans with AI assistance" : "Créer des plans d'affaires professionnels avec l'IA";
        var feature2 = isEnglish ? "Track your business metrics and KPIs" : "Suivre vos métriques et indicateurs clés";
        var feature3 = isEnglish ? "Collaborate with your team in real-time" : "Collaborer avec votre équipe en temps réel";
        var feature4 = isEnglish ? "Export bank-ready documents" : "Exporter des documents prêts pour les banques";
        var support = isEnglish
            ? "Questions? We're here to help at support@sqordia.com"
            : "Des questions ? Nous sommes là pour vous aider à support@sqordia.com";

        return GetBaseEmailTemplate(lang, $@"
            <div style=""text-align: center; padding: 32px 24px; background: linear-gradient(135deg, #059669 0%, #10B981 100%); border-radius: 12px; margin: -32px -32px 32px -32px;"">
                <h1 style=""color: white; font-size: 28px; font-weight: 600; margin: 0;"">{welcome}</h1>
            </div>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">{thankYou}</p>

            <div style=""text-align: center; margin: 0 0 32px 0;"">
                <a href=""{frontendUrl}/dashboard"" style=""display: inline-block; padding: 14px 36px; background: #FF6B00; color: white; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 16px;"">{buttonText}</a>
            </div>

            <h3 style=""color: #1A2B47; font-size: 16px; font-weight: 600; margin: 0 0 16px 0;"">{whatYouCanDo}</h3>
            <table style=""width: 100%; border-collapse: collapse; margin: 0 0 24px 0;"">
                <tr>
                    <td style=""padding: 12px 0; border-bottom: 1px solid #F3F4F6;"">
                        <table style=""border-collapse: collapse;""><tr>
                            <td style=""width: 40px; vertical-align: top;""><span style=""display: inline-block; width: 32px; height: 32px; background: #EFF6FF; border-radius: 8px; text-align: center; line-height: 32px; font-size: 16px;"">📊</span></td>
                            <td style=""color: #4B5563; font-size: 14px; padding-left: 12px;"">{feature1}</td>
                        </tr></table>
                    </td>
                </tr>
                <tr>
                    <td style=""padding: 12px 0; border-bottom: 1px solid #F3F4F6;"">
                        <table style=""border-collapse: collapse;""><tr>
                            <td style=""width: 40px; vertical-align: top;""><span style=""display: inline-block; width: 32px; height: 32px; background: #F0FDF4; border-radius: 8px; text-align: center; line-height: 32px; font-size: 16px;"">📈</span></td>
                            <td style=""color: #4B5563; font-size: 14px; padding-left: 12px;"">{feature2}</td>
                        </tr></table>
                    </td>
                </tr>
                <tr>
                    <td style=""padding: 12px 0; border-bottom: 1px solid #F3F4F6;"">
                        <table style=""border-collapse: collapse;""><tr>
                            <td style=""width: 40px; vertical-align: top;""><span style=""display: inline-block; width: 32px; height: 32px; background: #FFF7ED; border-radius: 8px; text-align: center; line-height: 32px; font-size: 16px;"">👥</span></td>
                            <td style=""color: #4B5563; font-size: 14px; padding-left: 12px;"">{feature3}</td>
                        </tr></table>
                    </td>
                </tr>
                <tr>
                    <td style=""padding: 12px 0;"">
                        <table style=""border-collapse: collapse;""><tr>
                            <td style=""width: 40px; vertical-align: top;""><span style=""display: inline-block; width: 32px; height: 32px; background: #FDF2F8; border-radius: 8px; text-align: center; line-height: 32px; font-size: 16px;"">📄</span></td>
                            <td style=""color: #4B5563; font-size: 14px; padding-left: 12px;"">{feature4}</td>
                        </tr></table>
                    </td>
                </tr>
            </table>
            <p style=""color: #9CA3AF; font-size: 13px; margin: 0;"">{support}</p>");
    }

    private string GetOrganizationInvitationTemplate(string email, string invitationToken, string? message, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var frontendUrl = GetFrontendBaseUrl();
        var acceptUrl = $"{frontendUrl}/accept-invitation?token={invitationToken}";

        var isEnglish = lang == "en";
        var title = isEnglish ? "You've Been Invited!" : "Vous êtes invité !";
        var greeting = isEnglish ? "Hi there," : "Bonjour,";
        var description = isEnglish
            ? "You've been invited to join an organization on Sqordia. Accept the invitation to start collaborating."
            : "Vous avez été invité à rejoindre une organisation sur Sqordia. Acceptez l'invitation pour commencer à collaborer.";
        var personalMessageLabel = isEnglish ? "Personal message:" : "Message personnel :";
        var buttonText = isEnglish ? "Accept Invitation" : "Accepter l'invitation";
        var altText = isEnglish
            ? "Or copy and paste this link into your browser:"
            : "Ou copiez et collez ce lien dans votre navigateur :";
        var expiry = isEnglish
            ? "This invitation will expire in 7 days."
            : "Cette invitation expirera dans 7 jours.";
        var ignore = isEnglish
            ? "If you don't recognize this invitation, you can safely ignore this email."
            : "Si vous ne reconnaissez pas cette invitation, vous pouvez ignorer cet email.";

        var messageSection = string.IsNullOrEmpty(message) ? "" : $@"
            <div style=""background: #F9FAFB; border-left: 4px solid #6366F1; padding: 16px 20px; border-radius: 0 8px 8px 0; margin: 0 0 24px 0;"">
                <p style=""color: #6B7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; margin: 0 0 8px 0;"">{personalMessageLabel}</p>
                <p style=""color: #4B5563; font-size: 14px; font-style: italic; margin: 0;"">""{message}""</p>
            </div>";

        return GetBaseEmailTemplate(lang, $@"
            <div style=""text-align: center; margin-bottom: 24px;"">
                <div style=""display: inline-block; width: 64px; height: 64px; background: #EEF2FF; border-radius: 50%; line-height: 64px;"">
                    <span style=""font-size: 32px;"">🤝</span>
                </div>
            </div>
            <h1 style=""color: #1A2B47; font-size: 24px; font-weight: 600; margin: 0 0 24px 0; text-align: center;"">{title}</h1>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">{greeting}</p>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">{description}</p>
            {messageSection}
            <div style=""text-align: center; margin: 32px 0;"">
                <a href=""{acceptUrl}"" style=""display: inline-block; padding: 14px 32px; background: #6366F1; color: white; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 16px;"">{buttonText}</a>
            </div>
            <p style=""color: #6B7280; font-size: 14px; margin: 24px 0 8px 0;"">{altText}</p>
            <p style=""word-break: break-all; color: #6366F1; font-size: 13px; background: #EEF2FF; padding: 12px 16px; border-radius: 8px; margin: 0 0 24px 0;"">{acceptUrl}</p>
            <p style=""color: #9CA3AF; font-size: 13px; margin: 0 0 8px 0;"">{expiry}</p>
            <p style=""color: #9CA3AF; font-size: 13px; margin: 0;"">{ignore}</p>");
    }

    private string GetAccountLockoutTemplate(string firstName, TimeSpan lockoutDuration, DateTime lockedAt, string? language = null)
    {
        var lang = GetEmailLanguage(language);
        var unlockTime = lockedAt.Add(lockoutDuration);
        var minutesRemaining = (int)lockoutDuration.TotalMinutes;

        var isEnglish = lang == "en";
        var title = isEnglish ? "Account Security Alert" : "Alerte de sécurité du compte";
        var greeting = isEnglish ? $"Hi {firstName}," : $"Bonjour {firstName},";
        var notification = isEnglish
            ? "Your account has been temporarily locked due to multiple failed login attempts."
            : "Votre compte a été temporairement verrouillé en raison de plusieurs tentatives de connexion échouées.";
        var detailsTitle = isEnglish ? "Lockout Details" : "Détails du verrouillage";
        var lockedAtLabel = isEnglish ? "Locked at" : "Verrouillé à";
        var durationLabel = isEnglish ? "Duration" : "Durée";
        var durationValue = isEnglish ? $"{minutesRemaining} minutes" : $"{minutesRemaining} minutes";
        var unlockAtLabel = isEnglish ? "Unlocks at" : "Déverrouillé à";
        var notYouTitle = isEnglish ? "Wasn't you?" : "Ce n'était pas vous ?";
        var notYouText = isEnglish
            ? "If you didn't attempt these logins, your password may be compromised. Reset it immediately after your account is unlocked."
            : "Si vous n'avez pas effectué ces tentatives, votre mot de passe pourrait être compromis. Réinitialisez-le dès que votre compte sera déverrouillé.";
        var support = isEnglish
            ? "Need immediate help? Contact support@sqordia.com"
            : "Besoin d'aide immédiate ? Contactez support@sqordia.com";

        return GetBaseEmailTemplate(lang, $@"
            <div style=""text-align: center; margin-bottom: 24px;"">
                <div style=""display: inline-block; width: 64px; height: 64px; background: #FEF2F2; border-radius: 50%; line-height: 64px;"">
                    <span style=""font-size: 32px;"">⚠️</span>
                </div>
            </div>
            <h1 style=""color: #DC2626; font-size: 24px; font-weight: 600; margin: 0 0 24px 0; text-align: center;"">{title}</h1>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">{greeting}</p>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">{notification}</p>

            <div style=""background: #F9FAFB; border: 1px solid #E5E7EB; padding: 20px; border-radius: 12px; margin: 0 0 24px 0;"">
                <p style=""color: #6B7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; margin: 0 0 12px 0;"">{detailsTitle}</p>
                <table style=""width: 100%; border-collapse: collapse;"">
                    <tr>
                        <td style=""color: #6B7280; font-size: 14px; padding: 8px 0;"">{lockedAtLabel}</td>
                        <td style=""color: #1A2B47; font-size: 14px; font-weight: 500; padding: 8px 0; text-align: right;"">{lockedAt:yyyy-MM-dd HH:mm} UTC</td>
                    </tr>
                    <tr>
                        <td style=""color: #6B7280; font-size: 14px; padding: 8px 0;"">{durationLabel}</td>
                        <td style=""color: #1A2B47; font-size: 14px; font-weight: 500; padding: 8px 0; text-align: right;"">{durationValue}</td>
                    </tr>
                    <tr>
                        <td style=""color: #6B7280; font-size: 14px; padding: 8px 0;"">{unlockAtLabel}</td>
                        <td style=""color: #059669; font-size: 14px; font-weight: 600; padding: 8px 0; text-align: right;"">{unlockTime:yyyy-MM-dd HH:mm} UTC</td>
                    </tr>
                </table>
            </div>

            <div style=""background: #FFF7ED; border-left: 4px solid #F59E0B; padding: 16px 20px; border-radius: 0 8px 8px 0; margin: 0 0 24px 0;"">
                <p style=""color: #92400E; font-size: 14px; font-weight: 600; margin: 0 0 8px 0;"">{notYouTitle}</p>
                <p style=""color: #78716C; font-size: 14px; line-height: 1.5; margin: 0;"">{notYouText}</p>
            </div>
            <p style=""color: #9CA3AF; font-size: 13px; text-align: center; margin: 0;"">{support}</p>");
    }

    private string GetBusinessPlanGeneratedTemplate(string userName, string businessPlanId, string businessPlanTitle)
    {
        var frontendUrl = GetFrontendBaseUrl();
        var planUrl = $"{frontendUrl}/plans/{businessPlanId}";

        return GetBaseEmailTemplate("en", $@"
            <div style=""text-align: center; padding: 32px 24px; background: linear-gradient(135deg, #059669 0%, #10B981 100%); border-radius: 12px; margin: -32px -32px 32px -32px;"">
                <div style=""font-size: 48px; margin-bottom: 12px;"">✨</div>
                <h1 style=""color: white; font-size: 24px; font-weight: 600; margin: 0;"">Your Business Plan is Ready!</h1>
            </div>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">Hi {userName},</p>
            <p style=""color: #4B5563; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">Great news! Your business plan has been successfully generated and is ready for review.</p>

            <div style=""background: #F0FDF4; border: 2px solid #059669; padding: 20px; border-radius: 12px; margin: 0 0 24px 0;"">
                <p style=""color: #059669; font-size: 18px; font-weight: 600; margin: 0 0 8px 0;"">{businessPlanTitle}</p>
                <p style=""color: #6B7280; font-size: 14px; margin: 0;"">Status: <span style=""color: #059669; font-weight: 600;"">✓ Completed</span></p>
            </div>

            <h3 style=""color: #1A2B47; font-size: 16px; font-weight: 600; margin: 0 0 16px 0;"">Your plan includes:</h3>
            <table style=""width: 100%; border-collapse: collapse; margin: 0 0 24px 0;"">
                <tr><td style=""padding: 8px 0; color: #4B5563; font-size: 14px;"">📊 Comprehensive market analysis</td></tr>
                <tr><td style=""padding: 8px 0; color: #4B5563; font-size: 14px;"">💼 Detailed business model</td></tr>
                <tr><td style=""padding: 8px 0; color: #4B5563; font-size: 14px;"">📈 Financial projections</td></tr>
                <tr><td style=""padding: 8px 0; color: #4B5563; font-size: 14px;"">🎯 Strategic roadmap</td></tr>
                <tr><td style=""padding: 8px 0; color: #4B5563; font-size: 14px;"">👥 Team & operations plan</td></tr>
            </table>

            <div style=""text-align: center; margin: 32px 0;"">
                <a href=""{planUrl}"" style=""display: inline-block; padding: 14px 36px; background: #059669; color: white; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 16px;"">View Your Business Plan</a>
            </div>
            <p style=""color: #9CA3AF; font-size: 13px; text-align: center; margin: 0;"">You can access your plan anytime from your Sqordia dashboard.</p>");
    }

    /// <summary>
    /// Modern base email template with Sqordia branding and brain icon
    /// </summary>
    private string GetBaseEmailTemplate(string language, string content)
    {
        var isEnglish = language == "en";
        var copyright = isEnglish
            ? $"© {DateTime.UtcNow.Year} Sqordia Inc. All rights reserved."
            : $"© {DateTime.UtcNow.Year} Sqordia Inc. Tous droits réservés.";
        var location = "Montréal, Québec, Canada";
        var unsubscribe = isEnglish ? "Unsubscribe" : "Se désabonner";
        var privacy = isEnglish ? "Privacy Policy" : "Politique de confidentialité";

        return $@"
<!DOCTYPE html>
<html lang=""{language}"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <title>Sqordia</title>
    <!--[if mso]>
    <noscript>
        <xml>
            <o:OfficeDocumentSettings>
                <o:PixelsPerInch>96</o:PixelsPerInch>
            </o:OfficeDocumentSettings>
        </xml>
    </noscript>
    <![endif]-->
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #F9FAFB; -webkit-font-smoothing: antialiased;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 20px;"">
                <table role=""presentation"" style=""width: 100%; max-width: 560px; border-collapse: collapse;"">
                    <!-- Header with Logo -->
                    <tr>
                        <td align=""center"" style=""padding: 0 0 32px 0;"">
                            <table role=""presentation"" style=""border-collapse: collapse;"">
                                <tr>
                                    <td style=""vertical-align: middle; padding-right: 12px;"">
                                        {BrainIconSvg}
                                    </td>
                                    <td style=""vertical-align: middle;"">
                                        <span style=""font-size: 28px; font-weight: 700; color: #1A2B47; letter-spacing: -0.5px;"">Sq<span style=""color: #FF6B00;"">o</span>rdia</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Main Content Card -->
                    <tr>
                        <td style=""background: white; border-radius: 16px; padding: 32px; box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1), 0 1px 2px rgba(0, 0, 0, 0.06);"">
                            {content}
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 32px 0 0 0;"">
                            <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
                                <tr>
                                    <td align=""center"" style=""padding: 0 0 16px 0;"">
                                        <table role=""presentation"" style=""border-collapse: collapse;"">
                                            <tr>
                                                <td style=""padding: 0 8px;"">
                                                    <a href=""https://sqordia.app/privacy"" style=""color: #6B7280; font-size: 12px; text-decoration: none;"">{privacy}</a>
                                                </td>
                                                <td style=""color: #D1D5DB;"">·</td>
                                                <td style=""padding: 0 8px;"">
                                                    <a href=""https://sqordia.app/unsubscribe"" style=""color: #6B7280; font-size: 12px; text-decoration: none;"">{unsubscribe}</a>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td align=""center"" style=""color: #9CA3AF; font-size: 12px; line-height: 1.5;"">
                                        <p style=""margin: 0 0 4px 0;"">{copyright}</p>
                                        <p style=""margin: 0;"">{location}</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    #endregion
}

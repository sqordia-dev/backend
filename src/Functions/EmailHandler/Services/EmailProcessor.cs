using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Sqordia.Functions.EmailHandler.Configuration;
using Sqordia.Functions.EmailHandler.Models;

namespace Sqordia.Functions.EmailHandler.Services;

/// <summary>
/// Service implementation for processing email jobs via SMTP (GCP compatible)
/// </summary>
public class EmailProcessor : IEmailProcessor
{
    private readonly ILogger<EmailProcessor> _logger;
    private readonly EmailConfiguration _config;

    public EmailProcessor(
        ILogger<EmailProcessor> logger,
        IOptions<EmailConfiguration> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    public async Task<bool> ProcessEmailJobAsync(EmailJobMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing email job {JobId} of type {EmailType} to {ToEmail}",
                message.JobId,
                message.EmailType,
                message.ToEmail);

            // Create email message
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_config.FromName, _config.FromEmail));
            emailMessage.To.Add(new MailboxAddress(message.ToName ?? message.ToEmail, message.ToEmail));
            emailMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder();
            if (!string.IsNullOrEmpty(message.HtmlBody))
            {
                bodyBuilder.HtmlBody = message.HtmlBody;
            }
            if (!string.IsNullOrEmpty(message.Body))
            {
                bodyBuilder.TextBody = message.Body;
            }
            emailMessage.Body = bodyBuilder.ToMessageBody();

            // Send email via SMTP
            using var client = new SmtpClient();
            await client.ConnectAsync(_config.SmtpHost, _config.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            
            if (!string.IsNullOrEmpty(_config.SmtpUsername) && !string.IsNullOrEmpty(_config.SmtpPassword))
            {
                await client.AuthenticateAsync(_config.SmtpUsername, _config.SmtpPassword, cancellationToken);
            }

            await client.SendAsync(emailMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "Successfully sent email job {JobId} to {ToEmail}",
                message.JobId,
                message.ToEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing email job {JobId} to {ToEmail}",
                message.JobId,
                message.ToEmail);
            throw;
        }
    }
}


using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqordia.Functions.EmailHandler.Configuration;
using Sqordia.Functions.EmailHandler.Models;

namespace Sqordia.Functions.EmailHandler.Services;

/// <summary>
/// Service implementation for processing email jobs via Azure Communication Services Email
/// </summary>
public class EmailProcessor : IEmailProcessor
{
    private readonly ILogger<EmailProcessor> _logger;
    private readonly EmailConfiguration _config;
    private readonly EmailClient _emailClient;

    public EmailProcessor(
        ILogger<EmailProcessor> logger,
        IOptions<EmailConfiguration> config,
        EmailClient emailClient)
    {
        _logger = logger;
        _config = config.Value;
        _emailClient = emailClient;
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

            // Prepare email content
            var emailContent = new EmailContent(message.Subject);
            
            // Set HTML body if provided
            if (!string.IsNullOrEmpty(message.HtmlBody))
            {
                emailContent.Html = message.HtmlBody;
            }
            
            // Set plain text body
            if (!string.IsNullOrEmpty(message.Body))
            {
                emailContent.PlainText = message.Body;
            }
            else if (!string.IsNullOrEmpty(message.HtmlBody))
            {
                // If only HTML is provided, use it as plain text fallback (Azure will strip HTML tags)
                emailContent.PlainText = message.HtmlBody;
            }

            // Prepare recipients
            var toRecipients = new List<EmailAddress>
            {
                new EmailAddress(message.ToEmail, displayName: message.ToName)
            };

            // Create email message
            // Based on API: EmailMessage(string senderAddress, EmailContent content, EmailRecipients recipients)
            // But constructor seems to expect different order - trying alternative approach
            var emailRecipients = new EmailRecipients(toRecipients);
            var emailMessage = new EmailMessage(_config.FromEmail, emailRecipients, emailContent);

            // Send email via Azure Communication Services
            var emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Started,
                emailMessage,
                cancellationToken);

            _logger.LogInformation(
                "Email job {JobId} queued for sending. Operation ID: {OperationId}, To: {ToEmail}",
                message.JobId,
                emailSendOperation.Id,
                message.ToEmail);

            // Wait for completion (optional - you can also check status later)
            await emailSendOperation.WaitForCompletionAsync(cancellationToken);

            if (emailSendOperation.HasCompleted && emailSendOperation.HasValue)
            {
                var result = emailSendOperation.Value;
                if (result.Status == EmailSendStatus.Succeeded)
                {
                    _logger.LogInformation(
                        "Successfully sent email job {JobId} to {ToEmail}",
                        message.JobId,
                        message.ToEmail);
                    return true;
                }
                else
                {
                    _logger.LogError(
                        "Email job {JobId} failed with status: {Status}",
                        message.JobId,
                        result.Status);
                    return false;
                }
            }

            _logger.LogWarning(
                "Email job {JobId} status unknown. Operation may still be in progress.",
                message.JobId);
            return true; // Assume success if operation started
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


using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sqordia.Functions.EmailHandler.Models;
using Sqordia.Functions.EmailHandler.Services;
using System.Text.Json;

namespace Sqordia.Functions.EmailHandler;

/// <summary>
/// Azure Function handler for processing email jobs from Service Bus topic
/// </summary>
public class Function
{
    private readonly ILogger<Function> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Function(ILogger<Function> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// This method is called for every Service Bus message. This method processes email jobs from the topic.
    /// </summary>
    [Function("ProcessEmailJob")]
    public async Task ProcessEmailJob(
        [ServiceBusTrigger("%AzureServiceBus__EmailTopic%", "%AzureServiceBus__EmailSubscription%", Connection = "AzureServiceBus__ConnectionString")]
        string messageBody,
        FunctionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailProcessor = scope.ServiceProvider.GetRequiredService<IEmailProcessor>();

        try
        {
            if (string.IsNullOrEmpty(messageBody))
            {
                _logger.LogWarning("Received empty message body");
                throw new ArgumentException("Empty message body");
            }

            // Deserialize the email job message
            var emailJobMessage = JsonSerializer.Deserialize<EmailJobMessage>(messageBody);
            if (emailJobMessage == null)
            {
                _logger.LogWarning("Failed to deserialize email job message");
                throw new ArgumentException("Failed to deserialize email job message");
            }

            _logger.LogInformation(
                "Processing email job from Service Bus. JobId: {JobId}, EmailType: {EmailType}, To: {ToEmail}",
                emailJobMessage.JobId,
                emailJobMessage.EmailType,
                emailJobMessage.ToEmail);

            // Process the email job
            var success = await emailProcessor.ProcessEmailJobAsync(emailJobMessage);
            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed email job. JobId: {JobId}, EmailType: {EmailType}, To: {ToEmail}",
                    emailJobMessage.JobId,
                    emailJobMessage.EmailType,
                    emailJobMessage.ToEmail);
            }
            else
            {
                _logger.LogError(
                    "Failed to process email job. JobId: {JobId}, EmailType: {EmailType}, To: {ToEmail}",
                    emailJobMessage.JobId,
                    emailJobMessage.EmailType,
                    emailJobMessage.ToEmail);
                throw new InvalidOperationException("Failed to process email job");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email job from Service Bus");
            throw;
        }
    }
}

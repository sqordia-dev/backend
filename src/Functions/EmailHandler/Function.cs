using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sqordia.Functions.EmailHandler.Models;
using Sqordia.Functions.EmailHandler.Services;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sqordia.Functions.EmailHandler;

/// <summary>
/// GCP Cloud Function handler for processing email jobs from Pub/Sub topic
/// </summary>
public class Function : IHttpFunction
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Function> _logger;

    /// <summary>
    /// Default constructor. This constructor is used by GCP Cloud Functions to construct the instance.
    /// </summary>
    public Function()
    {
        _serviceProvider = Startup.ConfigureServices();
        _logger = _serviceProvider.GetRequiredService<ILogger<Function>>();
    }

    /// <summary>
    /// This method is called for every Pub/Sub message. This method processes email jobs from the topic.
    /// </summary>
    public async Task HandleAsync(HttpContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailProcessor = scope.ServiceProvider.GetRequiredService<IEmailProcessor>();

        try
        {
            // Read the request body
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Received empty request body");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Empty request body");
                return;
            }

            // Parse Pub/Sub push message
            var pubsubMessage = JsonSerializer.Deserialize<PubSubPushMessage>(requestBody);
            if (pubsubMessage?.Message == null)
            {
                _logger.LogWarning("Failed to parse Pub/Sub message");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid Pub/Sub message format");
                return;
            }

            // Decode the message data
            string messageText;
            if (!string.IsNullOrEmpty(pubsubMessage.Message.Data))
            {
                var messageBytes = Convert.FromBase64String(pubsubMessage.Message.Data);
                messageText = Encoding.UTF8.GetString(messageBytes);
            }
            else
            {
                _logger.LogWarning("Message has no data. MessageId: {MessageId}", pubsubMessage.Message.MessageId);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Message has no data");
                return;
            }

            // Deserialize the email job message
            var emailJobMessage = JsonSerializer.Deserialize<EmailJobMessage>(messageText);
            if (emailJobMessage == null)
            {
                _logger.LogWarning("Failed to deserialize email job message. MessageId: {MessageId}", pubsubMessage.Message.MessageId);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Failed to deserialize email job message");
                return;
            }

            _logger.LogInformation(
                "Processing email job from Pub/Sub topic. MessageId: {MessageId}, PublishTime: {PublishTime}",
                pubsubMessage.Message.MessageId,
                pubsubMessage.Message.PublishTime);

            // Process the email job
            var success = await emailProcessor.ProcessEmailJobAsync(emailJobMessage);
            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed email job. JobId: {JobId}, EmailType: {EmailType}, To: {ToEmail}",
                    emailJobMessage.JobId,
                    emailJobMessage.EmailType,
                    emailJobMessage.ToEmail);
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            }
            else
            {
                _logger.LogError(
                    "Failed to process email job. JobId: {JobId}, EmailType: {EmailType}, To: {ToEmail}",
                    emailJobMessage.JobId,
                    emailJobMessage.EmailType,
                    emailJobMessage.ToEmail);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Failed to process email job");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email job from Pub/Sub");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }

    /// <summary>
    /// Pub/Sub push message structure
    /// </summary>
    private class PubSubPushMessage
    {
        [JsonPropertyName("message")]
        public PubSubMessage? Message { get; set; }

        [JsonPropertyName("subscription")]
        public string? Subscription { get; set; }
    }

    /// <summary>
    /// Pub/Sub message structure
    /// </summary>
    private class PubSubMessage
    {
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("messageId")]
        public string? MessageId { get; set; }

        [JsonPropertyName("publishTime")]
        public string? PublishTime { get; set; }

        [JsonPropertyName("attributes")]
        public Dictionary<string, string>? Attributes { get; set; }
    }
}

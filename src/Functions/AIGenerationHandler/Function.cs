using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sqordia.Functions.AIGenerationHandler.Models;
using Sqordia.Functions.AIGenerationHandler.Services;
using System.Text.Json;

namespace Sqordia.Functions.AIGenerationHandler;

/// <summary>
/// Azure Function handler for processing AI generation jobs from Service Bus topic
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
    /// This method is called for every Service Bus message. This method processes AI generation jobs from the topic.
    /// </summary>
    [Function("ProcessAIGenerationJob")]
    public async Task ProcessAIGenerationJob(
        [ServiceBusTrigger("%AzureServiceBus__AiGenerationTopic%", "%AzureServiceBus__AiGenerationSubscription%", Connection = "AzureServiceBus__ConnectionString")]
        string messageBody,
        FunctionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var aiProcessor = scope.ServiceProvider.GetRequiredService<IAIGenerationProcessor>();

        try
        {
            if (string.IsNullOrEmpty(messageBody))
            {
                _logger.LogWarning("Received empty message body");
                throw new ArgumentException("Empty message body");
            }

            // Deserialize the AI generation job message
            var aiJobMessage = JsonSerializer.Deserialize<AIGenerationJobMessage>(messageBody);
            if (aiJobMessage == null)
            {
                _logger.LogWarning("Failed to deserialize AI generation job message");
                throw new ArgumentException("Failed to deserialize AI generation job message");
            }

            _logger.LogInformation(
                "Processing AI generation job from Service Bus. JobId: {JobId}, BusinessPlanId: {BusinessPlanId}",
                aiJobMessage.JobId,
                aiJobMessage.BusinessPlanId);

            // Process the AI generation job
            var success = await aiProcessor.ProcessGenerationJobAsync(aiJobMessage);
            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed AI generation job. JobId: {JobId}, BusinessPlanId: {BusinessPlanId}",
                    aiJobMessage.JobId,
                    aiJobMessage.BusinessPlanId);
            }
            else
            {
                _logger.LogError(
                    "Failed to process AI generation job. JobId: {JobId}, BusinessPlanId: {BusinessPlanId}",
                    aiJobMessage.JobId,
                    aiJobMessage.BusinessPlanId);
                throw new InvalidOperationException("Failed to process AI generation job");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI generation job from Service Bus");
            throw;
        }
    }
}

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sqordia.Functions.ExportHandler.Models;
using Sqordia.Functions.ExportHandler.Services;
using System.Text.Json;

namespace Sqordia.Functions.ExportHandler;

/// <summary>
/// Azure Function handler for processing export jobs from Service Bus topic
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
    /// This method is called for every Service Bus message. This method processes export jobs from the topic.
    /// </summary>
    [Function("ProcessExportJob")]
    public async Task ProcessExportJob(
        [ServiceBusTrigger("%AzureServiceBus__ExportTopic%", "%AzureServiceBus__ExportSubscription%", Connection = "AzureServiceBus__ConnectionString")]
        string messageBody,
        FunctionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var exportProcessor = scope.ServiceProvider.GetRequiredService<IExportProcessor>();

        try
        {
            if (string.IsNullOrEmpty(messageBody))
            {
                _logger.LogWarning("Received empty message body");
                throw new ArgumentException("Empty message body");
            }

            // Deserialize the export job message
            var exportJobMessage = JsonSerializer.Deserialize<ExportJobMessage>(messageBody);
            if (exportJobMessage == null)
            {
                _logger.LogWarning("Failed to deserialize export job message");
                throw new ArgumentException("Failed to deserialize export job message");
            }

            _logger.LogInformation(
                "Processing export job from Service Bus. JobId: {JobId}, BusinessPlanId: {BusinessPlanId}",
                exportJobMessage.JobId,
                exportJobMessage.BusinessPlanId);

            // Process the export job
            var success = await exportProcessor.ProcessExportJobAsync(exportJobMessage);
            if (success)
            {
                _logger.LogInformation(
                    "Successfully processed export job. JobId: {JobId}, BusinessPlanId: {BusinessPlanId}",
                    exportJobMessage.JobId,
                    exportJobMessage.BusinessPlanId);
            }
            else
            {
                _logger.LogError(
                    "Failed to process export job. JobId: {JobId}, BusinessPlanId: {BusinessPlanId}",
                    exportJobMessage.JobId,
                    exportJobMessage.BusinessPlanId);
                throw new InvalidOperationException("Failed to process export job");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing export job from Service Bus");
            throw;
        }
    }
}

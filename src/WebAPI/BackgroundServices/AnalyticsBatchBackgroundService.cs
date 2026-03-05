using Sqordia.Application.Services;

namespace WebAPI.BackgroundServices;

/// <summary>
/// Background service that runs batch AI analytics daily at 2:00 AM UTC
/// </summary>
public class AnalyticsBatchBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AnalyticsBatchBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);

    public AnalyticsBatchBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AnalyticsBatchBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Analytics batch background service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var targetHour = 2; // 2:00 AM UTC

                // Calculate next run time
                var nextRun = now.Date.AddHours(targetHour);
                if (now.Hour >= targetHour)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                _logger.LogInformation("Next analytics batch run scheduled at {NextRun} UTC (in {Delay})", nextRun, delay);

                await Task.Delay(delay, stoppingToken);

                await RunBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in analytics batch background service");
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Analytics batch background service stopped");
    }

    private async Task RunBatchAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting scheduled batch analytics run");

        using var scope = _scopeFactory.CreateScope();
        var batchService = scope.ServiceProvider.GetRequiredService<IAnalyticsBatchService>();

        var result = await batchService.RunBatchAnalysisAsync(cancellationToken);

        if (result.IsSuccess)
            _logger.LogInformation("Batch analytics completed successfully");
        else
            _logger.LogWarning("Batch analytics completed with errors: {Error}", result.Error?.Message);
    }
}

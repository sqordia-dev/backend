using Sqordia.Application.Services;

namespace WebAPI.BackgroundServices;

/// <summary>
/// Background service that automatically disables maintenance mode
/// if the auto-disable timeout has been reached.
/// Prevents maintenance mode from staying enabled indefinitely if deployment fails.
/// </summary>
public class MaintenanceAutoDisableService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MaintenanceAutoDisableService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

    public MaintenanceAutoDisableService(
        IServiceProvider serviceProvider,
        ILogger<MaintenanceAutoDisableService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Maintenance auto-disable service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
                await CheckAndAutoDisableAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in maintenance auto-disable check");
                // Continue running even if there's an error
            }
        }

        _logger.LogInformation("Maintenance auto-disable service stopped");
    }

    private async Task CheckAndAutoDisableAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var maintenanceService = scope.ServiceProvider.GetRequiredService<IMaintenanceService>();

        var result = await maintenanceService.CheckAndAutoDisableAsync(stoppingToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Auto-disable check returned error: {Error}", result.Error?.Message);
        }
    }
}

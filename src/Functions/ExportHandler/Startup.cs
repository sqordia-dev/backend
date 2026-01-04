using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sqordia.Functions.Common;
using Sqordia.Functions.ExportHandler.Configuration;
using Sqordia.Functions.ExportHandler.Services;

namespace Sqordia.Functions.ExportHandler;

/// <summary>
/// Startup class for configuring dependency injection
/// </summary>
public static class Startup
{
    /// <summary>
    /// Configure services for dependency injection
    /// </summary>
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configuration
        var configuration = StartupBase.BuildConfiguration();
        var databaseConfig = StartupBase.GetDatabaseConfiguration(configuration);

        var gcpProjectId = configuration["GCP__ProjectId"] 
                        ?? configuration["GCP:ProjectId"] 
                        ?? Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT") 
                        ?? throw new InvalidOperationException("GCP ProjectId is not configured");

        var exportConfig = new ExportConfiguration
        {
            StorageBucketName = configuration["Storage__BucketName"] ?? configuration["Storage:BucketName"] ?? "sqordia-exports",
            GcpProjectId = gcpProjectId
        };

        services.AddSingleton(Options.Create(exportConfig));

        // GCP Services
        services.AddSingleton(StorageClient.Create());

        // Common services
        StartupBase.ConfigureCommonServices(services, configuration);

        // Application Services
        services.AddScoped<IExportProcessor, ExportProcessor>();

        return services.BuildServiceProvider();
    }
}


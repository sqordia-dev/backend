using Azure.Storage.Blobs;
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
    public static void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        var configuration = StartupBase.BuildConfiguration();
        var databaseConfig = StartupBase.GetDatabaseConfiguration(configuration);

        var storageConnectionString = configuration["AzureStorage__ConnectionString"] 
                                   ?? configuration["AzureStorage:ConnectionString"] 
                                   ?? throw new InvalidOperationException("Azure Storage connection string is not configured");

        var storageAccountName = configuration["AzureStorage__AccountName"] 
                              ?? configuration["AzureStorage:AccountName"] 
                              ?? throw new InvalidOperationException("Azure Storage account name is not configured");

        var exportConfig = new ExportConfiguration
        {
            StorageAccountName = storageAccountName,
            StorageConnectionString = storageConnectionString,
            ContainerName = configuration["AzureStorage__ContainerName"] ?? configuration["AzureStorage:ContainerName"] ?? "exports"
        };

        services.AddSingleton(Options.Create(exportConfig));

        // Azure Blob Storage Services
        services.AddSingleton(new BlobServiceClient(storageConnectionString));

        // Common services
        StartupBase.ConfigureCommonServices(services, configuration);

        // Application Services
        services.AddScoped<IExportProcessor, ExportProcessor>();
    }
}


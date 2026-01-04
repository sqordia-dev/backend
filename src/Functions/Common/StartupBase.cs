using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sqordia.Functions.Common;

/// <summary>
/// Base startup configuration for GCP Cloud Functions
/// </summary>
public static class StartupBase
{
    /// <summary>
    /// Builds configuration from environment variables
    /// </summary>
    public static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
    }

    /// <summary>
    /// Configures common services (logging, configuration)
    /// </summary>
    public static IServiceCollection ConfigureCommonServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }

    /// <summary>
    /// Gets database configuration from environment variables
    /// </summary>
    public static DatabaseConfiguration GetDatabaseConfiguration(IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings__SqordiaDb"] ?? configuration["ConnectionStrings__DefaultConnection"] ?? string.Empty;
        
        return new DatabaseConfiguration
        {
            ConnectionString = connectionString,
            DatabaseName = configuration["DatabaseName"] ?? "SqordiaDb",
            Environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production"
        };
    }
}

/// <summary>
/// Common database configuration for GCP Cloud Functions
/// </summary>
public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}


using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sqordia.Functions.Common;
using Sqordia.Functions.EmailHandler.Configuration;
using Sqordia.Functions.EmailHandler.Services;

namespace Sqordia.Functions.EmailHandler;

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

        // Azure Communication Services Email configuration
        var connectionString = configuration["AzureCommunicationServices__ConnectionString"] 
                            ?? configuration["AzureCommunicationServices:ConnectionString"]
                            ?? throw new InvalidOperationException("Azure Communication Services connection string is required. Set AzureCommunicationServices__ConnectionString or AzureCommunicationServices:ConnectionString");

        var emailConfig = new EmailConfiguration
        {
            ConnectionString = connectionString,
            FromEmail = configuration["Email__FromAddress"] ?? configuration["Email:FromAddress"] ?? throw new InvalidOperationException("Email sender address is required. Set Email__FromAddress or Email:FromAddress"),
            FromName = configuration["Email__FromName"] ?? configuration["Email:FromName"] ?? "Sqordia"
        };

        services.AddSingleton(Options.Create(emailConfig));

        // Register Azure Communication Services Email client
        services.AddSingleton(new EmailClient(emailConfig.ConnectionString));

        // Common services
        StartupBase.ConfigureCommonServices(services, configuration);

        // Application Services
        services.AddScoped<IEmailProcessor, EmailProcessor>();
    }
}


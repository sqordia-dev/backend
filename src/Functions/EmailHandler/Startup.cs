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

        var emailConfig = new EmailConfiguration
        {
            FromEmail = configuration["Email__FromAddress"] ?? configuration["Email:FromAddress"] ?? "noreply@sqordia.com",
            FromName = configuration["Email__FromName"] ?? configuration["Email:FromName"] ?? "Sqordia",
            SmtpHost = configuration["Email__SmtpHost"] ?? configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
            SmtpPort = int.Parse(configuration["Email__SmtpPort"] ?? configuration["Email:SmtpPort"] ?? "587"),
            SmtpUsername = configuration["Email__SmtpUsername"] ?? configuration["Email:SmtpUsername"],
            SmtpPassword = configuration["Email__SmtpPassword"] ?? configuration["Email:SmtpPassword"]
        };

        services.AddSingleton(Options.Create(emailConfig));

        // Common services
        StartupBase.ConfigureCommonServices(services, configuration);

        // Application Services
        services.AddScoped<IEmailProcessor, EmailProcessor>();
    }
}


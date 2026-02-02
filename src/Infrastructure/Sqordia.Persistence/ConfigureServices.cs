using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Persistence.Constants;
using Sqordia.Persistence.Contexts;
using Sqordia.Persistence.Interceptors;
using Sqordia.Persistence.Repositories;

namespace Sqordia.Persistence;

public static class ConfigureServices
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register interceptor first
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        // Configure database
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            // Get connection string from configuration
            var connectionString = GetConnectionString(configuration);

            // Use PostgreSQL only
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: DatabaseConstants.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(DatabaseConstants.MaxRetryDelaySeconds),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(DatabaseConstants.CommandTimeoutSeconds);
                });
            
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Register generic repository
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Register Prompt Repository
        services.AddScoped<IPromptRepository, PromptRepository>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        // Check for Railway's DATABASE_URL first (postgres:// format)
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            return ParseRailwayDatabaseUrl(databaseUrl);
        }

        // Get the connection string from configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // If no connection string, return a placeholder for design-time EF Core tooling.
        // At runtime the real connection string is always provided via DATABASE_URL,
        // appsettings.json, or Key Vault. In CI, dotnet ef --connection overrides this.
        if (string.IsNullOrEmpty(connectionString))
        {
            return "Host=localhost;Port=5432;Database=SqordiaDb;Username=postgres;Password=postgres";
        }

        return connectionString;
    }

    private static string ParseRailwayDatabaseUrl(string databaseUrl)
    {
        // Railway provides DATABASE_URL in format: postgresql://user:password@host:port/database
        try
        {
            var uri = new Uri(databaseUrl);
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            var username = uri.UserInfo.Split(':')[0];
            var password = uri.UserInfo.Split(':').Length > 1 ? uri.UserInfo.Split(':')[1] : string.Empty;

            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse DATABASE_URL: {databaseUrl}. Error: {ex.Message}", ex);
        }
    }

}

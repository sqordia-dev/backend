using Serilog;
using Serilog.Enrichers;
using Sqordia.Application;
using Sqordia.Infrastructure;
using Sqordia.Persistence;
using WebAPI.Configuration;
using WebAPI.Extensions;
using BCrypt.Net;

// Check for hash generation argument FIRST (before any initialization)
var argsList = args.ToList();
var generateHashIndex = argsList.IndexOf("--generate-hash");
if (generateHashIndex >= 0 && generateHashIndex + 1 < argsList.Count)
{
    var password = argsList[generateHashIndex + 1];
    var hash = BCrypt.Net.BCrypt.HashPassword(password);
    Console.WriteLine($"Password: {password}");
    Console.WriteLine($"Hash: {hash}");
    Console.WriteLine($"\nUse this hash in your seed script:");
    Console.WriteLine($"'{hash}'");
    Environment.Exit(0);
    return;
}

// Configure Serilog early for bootstrap logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{

    Log.Information("Starting Sqordia application");

    var builder = WebApplication.CreateBuilder(args);
    
    var customConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
    if (!string.IsNullOrEmpty(customConnectionString))
    {
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", customConnectionString }
        });
    }
    
    // Configure Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "Sqordia");
    });

    builder.Services.AddApplicationConfiguration(builder.Configuration);

    // Configure Kestrel server options for long-running requests (business plan generation)
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
        options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
    });

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddPersistenceServices(builder.Configuration);
    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddAuthenticationServices(builder.Configuration);
    builder.Services.AddCorsServices(builder.Configuration, builder.Environment);
    builder.Services.AddLocalizationServices();
    builder.Services.AddRateLimitingServices(builder.Configuration);
    builder.Services.AddHealthCheckServices();

    var app = builder.Build();

    await app.ApplyDatabaseMigrationsAsync();
    app.ConfigureMiddleware();

    Log.Information("Sqordia application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }

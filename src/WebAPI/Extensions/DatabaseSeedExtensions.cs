using Npgsql;

namespace WebAPI.Extensions;

public static class DatabaseSeedExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        try
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogWarning("No connection string configured. Skipping database seeding.");
                return;
            }

            // Check if we should run seeds (only in Development or if explicitly enabled)
            var environment = app.Environment.EnvironmentName;
            var runSeeds = environment == "Development" || 
                          configuration.GetValue<bool>("Database:RunSeedsOnStartup", false);

            if (!runSeeds)
            {
                logger.LogInformation("Database seeding is disabled for {Environment} environment.", environment);
                return;
            }

            // Try multiple paths to find the seed script
            var possiblePaths = new[]
            {
                Path.Combine("/app", "scripts", "seed-all.sql"), // Docker container path
                Path.Combine(AppContext.BaseDirectory, "scripts", "seed-all.sql"), // Published app directory
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "scripts", "seed-all.sql"), // Development path
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", "seed-all.sql"), // Current directory
            };

            string? seedScriptPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    seedScriptPath = path;
                    break;
                }
            }

            if (seedScriptPath != null && File.Exists(seedScriptPath))
            {
                logger.LogInformation("Running database seed script from {Path}...", seedScriptPath);

                var seedScript = await File.ReadAllTextAsync(seedScriptPath);

                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(seedScript, connection);
                command.CommandTimeout = 300; // 5 minutes timeout

                var rowsAffected = await command.ExecuteNonQueryAsync();

                logger.LogInformation("Database seed script completed. Rows affected: {RowsAffected}", rowsAffected);
            }
            else
            {
                logger.LogWarning("Seed script not found. Tried paths: {Paths}. Continuing with AI prompts seeding.", 
                    string.Join(", ", possiblePaths));
            }

            // Seed AI prompts from SQL script (after main SQL seed)
            await SeedAIPromptsFromSqlAsync(connectionString, logger);
        }
        catch (PostgresException pgEx)
        {
            logger.LogError(pgEx, "PostgreSQL error occurred while seeding database. Error Code: {SqlState}, Message: {Message}", 
                pgEx.SqlState, pgEx.Message);
            // Don't throw - allow application to continue
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding database. Application will continue.");
            // Don't throw - allow application to continue
        }
    }

    private static async Task SeedAIPromptsFromSqlAsync(string connectionString, ILogger logger)
    {
        try
        {
            logger.LogInformation("Starting AI prompts seeding from SQL script...");

            // Try multiple paths to find the AI prompts seed script
            var possiblePaths = new[]
            {
                Path.Combine("/app", "scripts", "seed-ai-prompts.sql"), // Docker container path
                Path.Combine(AppContext.BaseDirectory, "scripts", "seed-ai-prompts.sql"), // Published app directory
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "scripts", "seed-ai-prompts.sql"), // Development path
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", "seed-ai-prompts.sql"), // Current directory
            };

            string? seedScriptPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    seedScriptPath = path;
                    break;
                }
            }

            if (seedScriptPath == null || !File.Exists(seedScriptPath))
            {
                logger.LogWarning("AI prompts seed script not found. Tried paths: {Paths}. Skipping AI prompts seeding.", 
                    string.Join(", ", possiblePaths));
                return;
            }

            logger.LogInformation("Running AI prompts seed script from {Path}...", seedScriptPath);

            var seedScript = await File.ReadAllTextAsync(seedScriptPath);

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(seedScript, connection);
            command.CommandTimeout = 300; // 5 minutes timeout

            var rowsAffected = await command.ExecuteNonQueryAsync();

            logger.LogInformation("AI prompts seeding completed successfully. Rows affected: {RowsAffected}", rowsAffected);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding AI prompts. Application will continue.");
            // Don't throw - allow application to continue
        }
    }
}


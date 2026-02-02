using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Persistence;

/// <summary>
/// Factory for creating ApplicationDbContext at design time (used by dotnet ef commands).
/// This bypasses the full DI container and provides a placeholder connection string
/// so that EF Core tooling can discover the DbContext and generate/apply migrations.
/// The actual connection string is provided via --connection flag in CI/CD pipelines.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use DATABASE_URL if available (Railway), otherwise use a placeholder.
        // In CI, the real connection string is passed via `dotnet ef database update --connection "..."`.
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        var connectionString = !string.IsNullOrEmpty(databaseUrl)
            ? databaseUrl
            : Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
              ?? "Host=localhost;Port=5432;Database=SqordiaDb;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

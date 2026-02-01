using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sqordia.Persistence.Contexts;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/database-test")]
public class DatabaseTestController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public DatabaseTestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("connection")]
    public async Task<IActionResult> TestConnection(CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            return Ok(new
            {
                ConnectionString = connectionString,
                CanConnect = canConnect,
                EnvironmentVariables = new
                {
                    ConnectionString = connectionString
                }
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                Error = ex.Message,
                StackTrace = ex.StackTrace,
                ConnectionString = _context.Database.GetConnectionString(),
                EnvironmentVariables = new
                {
                    ConnectionString = _context.Database.GetConnectionString()
                }
            });
        }
    }

    [HttpGet("simple")]
    public async Task<IActionResult> SimpleTest(CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            return Ok(new
            {
                Success = true,
                ConnectionString = connectionString,
                CanConnect = canConnect,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                Success = false,
                Error = ex.Message,
                ConnectionString = _context.Database.GetConnectionString(),
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

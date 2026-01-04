using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sqordia.Persistence.Contexts;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseTestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DatabaseTestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            var canConnect = await _context.Database.CanConnectAsync();
            
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
    public async Task<IActionResult> SimpleTest()
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            var canConnect = await _context.Database.CanConnectAsync();
            
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

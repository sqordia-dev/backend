using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Attributes;

/// <summary>
/// Attribute that validates the deployment API key for CI/CD webhook endpoints.
/// Checks the X-Deployment-Key header against the configured key.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class DeploymentApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string HeaderName = "X-Deployment-Key";
    private const string ConfigKey = "DeploymentApiKey";
    private const string EnvVarKey = "DEPLOYMENT_API_KEY";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<DeploymentApiKeyAttribute>>();

        // Get the expected API key from environment variable or configuration
        var expectedKey = Environment.GetEnvironmentVariable(EnvVarKey)
                          ?? configuration[ConfigKey];

        if (string.IsNullOrEmpty(expectedKey))
        {
            logger.LogError("Deployment API key is not configured. Set {EnvVar} environment variable or {ConfigKey} in appsettings.",
                EnvVarKey, ConfigKey);
            context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            return;
        }

        // Check if the header is present
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey))
        {
            logger.LogWarning("Deployment API key header '{Header}' is missing", HeaderName);
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Missing deployment API key",
                message = $"The {HeaderName} header is required"
            });
            return;
        }

        // Validate the key using constant-time comparison to prevent timing attacks
        if (!CryptographicEquals(expectedKey, providedKey.ToString()))
        {
            logger.LogWarning("Invalid deployment API key provided");
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Invalid deployment API key",
                message = "The provided API key is not valid"
            });
            return;
        }

        await next();
    }

    /// <summary>
    /// Performs a constant-time comparison of two strings to prevent timing attacks.
    /// </summary>
    private static bool CryptographicEquals(string a, string b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        var aBytes = System.Text.Encoding.UTF8.GetBytes(a);
        var bBytes = System.Text.Encoding.UTF8.GetBytes(b);

        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}

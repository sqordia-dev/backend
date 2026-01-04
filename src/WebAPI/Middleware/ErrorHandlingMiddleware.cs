using Microsoft.AspNetCore.Mvc;
using Sqordia.Domain.Exceptions;
using System.Net;
using System.Text.Json;
using WebAPI.Constants;

namespace WebAPI.Middleware;

/// <summary>
/// Global error handling middleware
/// Catches and handles exceptions, returning appropriate HTTP responses
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case BusinessRuleValidationException validationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new
                {
                    errors = validationException.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
                break;
            case DomainException domainException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = domainException.Message });
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(new { error = WebAPI.Constants.ErrorMessages.UnauthorizedAccess });
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new { error = WebAPI.Constants.ErrorMessages.ResourceNotFound });
                break;
            default:
                _logger.LogError(exception, "An unhandled exception occurred");
                
                // Provide more specific error messages for common issues
                string errorMessage = exception switch
                {
                    InvalidOperationException when exception.Message.Contains("connection string", StringComparison.OrdinalIgnoreCase) => 
                        ErrorMessages.DatabaseConfigurationError,
                    InvalidOperationException when exception.Message.Contains("JWT", StringComparison.OrdinalIgnoreCase) => 
                        ErrorMessages.JwtConfigurationError,
                    InvalidOperationException when exception.Message.Contains("Email", StringComparison.OrdinalIgnoreCase) || 
                                                  exception.Message.Contains("PubSub", StringComparison.OrdinalIgnoreCase) => 
                        ErrorMessages.EmailServiceConfigurationError,
                    _ => ErrorMessages.UnexpectedError
                };
                
                result = JsonSerializer.Serialize(new { error = errorMessage });
                break;
        }

        // Only set response properties if response hasn't started
        if (!context.Response.HasStarted)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            await context.Response.WriteAsync(result);
        }
        else
        {
            // If response has started, just log the error
            _logger.LogError(exception, "Exception occurred after response started");
        }
    }
}

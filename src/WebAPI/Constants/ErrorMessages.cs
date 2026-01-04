namespace WebAPI.Constants;

/// <summary>
/// Centralized error messages
/// Improves maintainability and consistency
/// </summary>
public static class ErrorMessages
{
    // General errors
    public const string UnauthorizedAccess = "Unauthorized access";
    public const string ResourceNotFound = "Resource not found";
    public const string UnexpectedError = "An unexpected error occurred";
    
    // Configuration errors
    public const string DatabaseConfigurationError = "Database configuration error. Please check connection string configuration.";
    public const string JwtConfigurationError = "Authentication configuration error. Please check JWT settings.";
    public const string EmailServiceConfigurationError = "Email service configuration error. Please check EMAIL_QUEUE_URL environment variable.";
    
    // Validation errors
    public const string BusinessRuleValidationFailed = "Business rule validation failed";
}


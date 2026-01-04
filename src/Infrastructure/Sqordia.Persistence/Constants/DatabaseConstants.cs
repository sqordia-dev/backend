namespace Sqordia.Persistence.Constants;

/// <summary>
/// Database-related constants
/// Centralized to avoid magic numbers and improve maintainability
/// </summary>
public static class DatabaseConstants
{
    // Retry Configuration
    public const int MaxRetryCount = 3;
    public const int MaxRetryDelaySeconds = 30;
    
    // Command Timeout
    public const int CommandTimeoutSeconds = 60;
}


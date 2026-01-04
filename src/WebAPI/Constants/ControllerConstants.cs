namespace WebAPI.Constants;

/// <summary>
/// Constants for controllers
/// Centralized to avoid magic strings and improve maintainability
/// </summary>
public static class ControllerConstants
{
    // Role names
    public const string RoleAdmin = "Admin";
    public const string RoleOwner = "Owner";
    public const string RoleUser = "User";
    
    // Error messages
    public const string ErrorAiServiceUnavailable = "AI service is currently unavailable. Please try again later.";
    public const string ErrorInvalidGoogleToken = "Invalid Google token";
    public const string ErrorFailedToUnlinkGoogle = "Failed to unlink Google account";
    public const string ErrorAnErrorOccurred = "An error occurred";
}


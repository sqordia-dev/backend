namespace Sqordia.Functions.EmailHandler.Configuration;

/// <summary>
/// Email function configuration settings for Azure Communication Services Email
/// </summary>
public class EmailConfiguration
{
    /// <summary>
    /// Azure Communication Services connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Sender email address (must be verified in Azure Communication Services)
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Sender display name
    /// </summary>
    public string FromName { get; set; } = "Sqordia";
}


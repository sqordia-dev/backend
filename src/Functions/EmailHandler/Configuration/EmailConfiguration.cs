namespace Sqordia.Functions.EmailHandler.Configuration;

/// <summary>
/// Email function configuration settings
/// </summary>
public class EmailConfiguration
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Sqordia";
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
}


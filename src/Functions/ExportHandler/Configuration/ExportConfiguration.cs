namespace Sqordia.Functions.ExportHandler.Configuration;

/// <summary>
/// Export function configuration settings
/// </summary>
public class ExportConfiguration
{
    public string StorageAccountName { get; set; } = string.Empty;
    public string StorageConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "exports";
}


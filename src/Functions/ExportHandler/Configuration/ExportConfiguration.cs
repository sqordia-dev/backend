namespace Sqordia.Functions.ExportHandler.Configuration;

/// <summary>
/// Export function configuration settings
/// </summary>
public class ExportConfiguration
{
    public string StorageBucketName { get; set; } = string.Empty;
    public string GcpProjectId { get; set; } = string.Empty;
}


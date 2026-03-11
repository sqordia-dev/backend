namespace Sqordia.Contracts.Requests.AI;

public class SetActiveProviderRequest
{
    public string Provider { get; set; } = string.Empty;
}

public class SetProviderModelRequest
{
    public string Model { get; set; } = string.Empty;
}

public class SetFallbackProvidersRequest
{
    public List<string> Providers { get; set; } = new();
}

public class SetSectionOverrideRequest
{
    public string Provider { get; set; } = string.Empty;
    public string? Model { get; set; }
}

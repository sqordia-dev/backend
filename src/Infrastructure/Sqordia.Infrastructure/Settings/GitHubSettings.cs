namespace Sqordia.Infrastructure.Settings;

public class GitHubSettings
{
    public string PersonalAccessToken { get; set; } = string.Empty;
    public string FrontendRepoOwner { get; set; } = string.Empty;
    public string FrontendRepoName { get; set; } = "sqordia-frontend";
    public string BackendRepoOwner { get; set; } = string.Empty;
    public string BackendRepoName { get; set; } = "sqordia-backend";
}

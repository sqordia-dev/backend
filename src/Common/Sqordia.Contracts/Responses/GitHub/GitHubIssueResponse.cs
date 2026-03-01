namespace Sqordia.Contracts.Responses.GitHub;

public class GitHubIssueResponse
{
    public int IssueNumber { get; set; }
    public string IssueUrl { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GitHubIssueDetailResponse
{
    public int IssueNumber { get; set; }
    public string IssueUrl { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public List<GitHubLabelDto> Labels { get; set; } = new();
    public GitHubUserDto? Author { get; set; }
    public GitHubUserDto? Assignee { get; set; }
    public int CommentsCount { get; set; }
}

public class GitHubIssueListResponse
{
    public List<GitHubIssueListItem> Issues { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}

public class GitHubIssueListItem
{
    public int IssueNumber { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<GitHubLabelDto> Labels { get; set; } = new();
    public GitHubUserDto? Author { get; set; }
    public int CommentsCount { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
}

public class GitHubLabelDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class GitHubUserDto
{
    public string Login { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
}

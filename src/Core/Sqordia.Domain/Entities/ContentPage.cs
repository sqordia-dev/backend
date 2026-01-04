using Sqordia.Domain.Common;

namespace Sqordia.Domain.Entities;

/// <summary>
/// Content page for the landing page (CMS)
/// </summary>
public class ContentPage : BaseAuditableEntity
{
    public string PageKey { get; private set; } = null!; // e.g., "home", "about", "pricing"
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!; // HTML or Markdown content
    public string Language { get; private set; } = "fr"; // fr or en
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int Version { get; private set; }
    
    private ContentPage() { } // EF Core constructor
    
    public ContentPage(
        string pageKey,
        string title,
        string content,
        string language = "fr")
    {
        PageKey = pageKey ?? throw new ArgumentNullException(nameof(pageKey));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Language = language;
        IsPublished = false;
        Version = 1;
    }
    
    public void UpdateContent(string title, string content)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Version++;
    }
    
    public void Publish()
    {
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
    }
    
    public void Unpublish()
    {
        IsPublished = false;
        PublishedAt = null;
    }
}


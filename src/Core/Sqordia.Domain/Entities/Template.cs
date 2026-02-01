using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

public class Template : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public TemplateCategory Category { get; private set; }
    public TemplateType Type { get; private set; }
    public TemplateStatus Status { get; private set; }
    public string Industry { get; private set; } = string.Empty;
    public string TargetAudience { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public bool IsPublic { get; private set; }
    public bool IsDefault { get; private set; }
    public int UsageCount { get; private set; }
    public decimal Rating { get; private set; }
    public int RatingCount { get; private set; }
    public string Tags { get; private set; } = string.Empty;
    public string PreviewImage { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string AuthorEmail { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string Changelog { get; private set; } = string.Empty;
    public DateTime LastUsed { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public string UpdatedBy { get; private set; } = string.Empty;

    // Navigation properties
    public List<TemplateSection> Sections { get; private set; } = new();
    public List<TemplateCustomization> Customizations { get; private set; } = new();
    public List<TemplateRating> Ratings { get; private set; } = new();
    public List<TemplateUsage> Usages { get; private set; } = new();

    private Template() { } // EF Core constructor

    public static Template Create(
        string name,
        string description,
        string content,
        TemplateCategory category,
        TemplateType type,
        string author,
        string authorEmail,
        string createdBy,
        string? industry = null,
        string? targetAudience = null,
        string? language = null,
        string? country = null,
        bool isPublic = false,
        string? tags = null,
        string? previewImage = null,
        string? version = null,
        string? changelog = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required.", nameof(name));

        var template = new Template
        {
            Name = name,
            Description = description ?? string.Empty,
            Content = content ?? string.Empty,
            Category = category,
            Type = type,
            Status = TemplateStatus.Draft,
            Industry = industry ?? string.Empty,
            TargetAudience = targetAudience ?? string.Empty,
            Language = language ?? string.Empty,
            Country = country ?? string.Empty,
            IsPublic = isPublic,
            IsDefault = false,
            UsageCount = 0,
            Rating = 0,
            RatingCount = 0,
            Tags = tags ?? string.Empty,
            PreviewImage = previewImage ?? string.Empty,
            Author = author ?? string.Empty,
            AuthorEmail = authorEmail ?? string.Empty,
            Version = version ?? string.Empty,
            Changelog = changelog ?? string.Empty,
            LastUsed = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy ?? "System",
            UpdatedBy = createdBy ?? "System"
        };

        return template;
    }

    public void Update(
        string name,
        string description,
        string content,
        TemplateCategory category,
        TemplateType type,
        string? industry = null,
        string? targetAudience = null,
        string? language = null,
        string? country = null,
        bool? isPublic = null,
        string? tags = null,
        string? previewImage = null,
        string? author = null,
        string? version = null,
        string? changelog = null,
        string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required.", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        Content = content ?? string.Empty;
        Category = category;
        Type = type;
        Industry = industry ?? Industry;
        TargetAudience = targetAudience ?? TargetAudience;
        Language = language ?? Language;
        Country = country ?? Country;
        IsPublic = isPublic ?? IsPublic;
        Tags = tags ?? Tags;
        PreviewImage = previewImage ?? PreviewImage;
        Author = author ?? Author;
        Version = version ?? Version;
        Changelog = changelog ?? Changelog;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy ?? UpdatedBy;
    }

    public void Publish()
    {
        if (Status != TemplateStatus.Draft && Status != TemplateStatus.Review && Status != TemplateStatus.Approved)
            throw new InvalidOperationException($"Cannot publish template in '{Status}' status. Must be Draft, Review, or Approved.");

        Status = TemplateStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status == TemplateStatus.Archived)
            throw new InvalidOperationException("Template is already archived.");

        Status = TemplateStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRating(decimal ratingValue)
    {
        if (ratingValue < 0 || ratingValue > 5)
            throw new ArgumentOutOfRangeException(nameof(ratingValue), "Rating must be between 0 and 5.");

        // Recalculate weighted average
        var totalRating = Rating * RatingCount + ratingValue;
        RatingCount++;
        Rating = totalRating / RatingCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementUsage()
    {
        UsageCount++;
        LastUsed = DateTime.UtcNow;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }
}

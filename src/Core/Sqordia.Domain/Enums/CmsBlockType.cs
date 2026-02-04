namespace Sqordia.Domain.Enums;

/// <summary>
/// Represents the type of content stored in a CMS content block
/// </summary>
public enum CmsBlockType
{
    /// <summary>
    /// Plain text content
    /// </summary>
    Text = 0,

    /// <summary>
    /// Rich text / HTML content
    /// </summary>
    RichText = 1,

    /// <summary>
    /// Image reference or URL
    /// </summary>
    Image = 2,

    /// <summary>
    /// Hyperlink
    /// </summary>
    Link = 3,

    /// <summary>
    /// Structured JSON content
    /// </summary>
    Json = 4,

    /// <summary>
    /// Numeric value
    /// </summary>
    Number = 5,

    /// <summary>
    /// Boolean (true/false) value
    /// </summary>
    Boolean = 6
}

namespace Sqordia.Application.Services;

/// <summary>
/// Renders HTML to PDF bytes. Abstraction over headless browser rendering.
/// </summary>
public interface IHtmlToPdfRenderer
{
    /// <summary>
    /// Render an HTML document to PDF bytes.
    /// </summary>
    Task<byte[]> RenderAsync(string html, CancellationToken cancellationToken = default);
}

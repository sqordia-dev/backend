using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Sqordia.Application.Services;

namespace Sqordia.Infrastructure.Services.DocumentExport;

/// <summary>
/// Renders HTML to PDF using PuppeteerSharp (headless Chromium).
/// Produces PDF with selectable text, proper page breaks, and print background colors.
/// Registered as Singleton — reuses a single Chromium instance across requests.
/// </summary>
public class PuppeteerPdfRenderer : IHtmlToPdfRenderer, IDisposable
{
    private readonly ILogger _logger;
    private volatile IBrowser? _browser;
    private readonly SemaphoreSlim _browserLock = new(1, 1);

    public PuppeteerPdfRenderer(ILogger<PuppeteerPdfRenderer> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> RenderAsync(string html, CancellationToken cancellationToken = default)
    {
        var browser = await GetOrCreateBrowserAsync();
        await using var page = await browser.NewPageAsync();

        // Set viewport to Letter width (8.5in * 96dpi = 816px)
        await page.SetViewportAsync(new ViewPortOptions { Width = 816, Height = 1056 });

        await page.SetContentAsync(html, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
            Timeout = 30_000
        });

        // Wait for fonts/layout to settle
        await page.EvaluateExpressionAsync("document.fonts.ready");

        var pdfBytes = await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.Letter,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "0",
                Right = "0",
                Bottom = "0",
                Left = "0"
            },
            PreferCSSPageSize = true
        });

        _logger.LogInformation("Puppeteer PDF rendered: {SizeKB} KB", pdfBytes.Length / 1024);
        return pdfBytes;
    }

    /// <summary>
    /// Thread-safe lazy initialization with recovery if Chromium crashes.
    /// All reads/writes go through the semaphore — no unsynchronized access.
    /// </summary>
    private async Task<IBrowser> GetOrCreateBrowserAsync()
    {
        await _browserLock.WaitAsync();
        try
        {
            // Check if existing browser is still alive
            if (_browser is { IsClosed: false })
                return _browser;

            // Dispose crashed browser if needed
            if (_browser != null)
            {
                _logger.LogWarning("Chromium browser was closed unexpectedly, relaunching...");
                try { _browser.Dispose(); } catch { /* ignore cleanup errors */ }
                _browser = null;
            }

            // Use system Chromium if PUPPETEER_CHROMIUM_PATH is set (Docker),
            // otherwise download via BrowserFetcher (local dev)
            var systemChromium = Environment.GetEnvironmentVariable("PUPPETEER_CHROMIUM_PATH");
            string? executablePath = null;

            if (!string.IsNullOrEmpty(systemChromium) && File.Exists(systemChromium))
            {
                _logger.LogInformation("Using system Chromium at {Path}", systemChromium);
                executablePath = systemChromium;
            }
            else
            {
                _logger.LogInformation("Downloading/checking Chromium for Puppeteer...");
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
            }

            _logger.LogInformation("Launching headless Chromium...");
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = executablePath,
                Args = new[]
                {
                    "--no-sandbox",
                    "--disable-setuid-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu"
                }
            });

            return _browser;
        }
        finally
        {
            _browserLock.Release();
        }
    }

    public void Dispose()
    {
        _browser?.Dispose();
        _browserLock.Dispose();
    }
}

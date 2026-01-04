using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Sqordia.Application.Common.Interfaces;
using System.Globalization;

namespace Sqordia.Infrastructure.Localization;

public class LocalizationService : ILocalizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer _localizer;
    private readonly string[] _supportedLanguages = { "fr", "en" };
    private const string DefaultLanguage = "fr";

    public LocalizationService(
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizerFactory factory)
    {
        _httpContextAccessor = httpContextAccessor;
        
        // Load the Messages resource file from Sqordia.Application
        var type = typeof(Application.Resources.Messages);
        _localizer = factory.Create("Messages", "Sqordia.Application");
    }

    public string GetCurrentLanguage()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return DefaultLanguage;

        // Priority 1: Query parameter ?lang=en
        if (httpContext.Request.Query.TryGetValue("lang", out var langQuery))
        {
            var lang = NormalizeLanguageCode(langQuery.ToString());
            if (IsLanguageSupported(lang))
                return lang;
        }

        // Priority 2: Accept-Language header
        var acceptLanguage = httpContext.Request.Headers["Accept-Language"].ToString();
        if (!string.IsNullOrEmpty(acceptLanguage))
        {
            var languages = acceptLanguage.Split(',')
                .Select(l => l.Split(';')[0].Trim())
                .Select(NormalizeLanguageCode)
                .Where(IsLanguageSupported);

            var firstSupported = languages.FirstOrDefault();
            if (firstSupported != null)
                return firstSupported;
        }

        // Priority 3: User preference (from JWT claims - future enhancement)
        // Note: User language preference from claims can be added here when needed

        // Default: French
        return DefaultLanguage;
    }

    public string GetString(string key, params object[] args)
    {
        var currentLanguage = GetCurrentLanguage();
        
        // Set the current culture for the localizer
        CultureInfo.CurrentCulture = new CultureInfo(currentLanguage);
        CultureInfo.CurrentUICulture = new CultureInfo(currentLanguage);

        var localizedString = _localizer[key];
        
        if (localizedString.ResourceNotFound)
        {
            // Fallback to key if resource not found
            return key;
        }

        // Apply formatting if arguments provided
        if (args != null && args.Length > 0)
        {
            try
            {
                return string.Format(localizedString.Value, args);
            }
            catch
            {
                return localizedString.Value;
            }
        }

        return localizedString.Value;
    }

    public bool IsLanguageSupported(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return false;

        var normalized = NormalizeLanguageCode(language);
        return _supportedLanguages.Contains(normalized, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Normalizes language code to 2-letter ISO code
    /// </summary>
    /// <param name="language">Language code (e.g., "en-US", "fr-CA")</param>
    /// <returns>2-letter code (e.g., "en", "fr")</returns>
    private static string NormalizeLanguageCode(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return DefaultLanguage;

        // Extract the first two letters (e.g., "en-US" -> "en", "fr-CA" -> "fr")
        var normalized = language.Trim().ToLowerInvariant();
        
        if (normalized.Length >= 2)
        {
            return normalized.Substring(0, 2);
        }

        return normalized;
    }
}


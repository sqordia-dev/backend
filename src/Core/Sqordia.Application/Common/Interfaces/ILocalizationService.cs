namespace Sqordia.Application.Common.Interfaces;

/// <summary>
/// Service for handling localization and translations
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current language from the HTTP context
    /// </summary>
    /// <returns>Language code (e.g., 'fr', 'en')</returns>
    string GetCurrentLanguage();
    
    /// <summary>
    /// Gets a localized string by key
    /// </summary>
    /// <param name="key">Resource key (e.g., "Auth.Error.InvalidCredentials")</param>
    /// <param name="args">Optional format arguments</param>
    /// <returns>Localized string</returns>
    string GetString(string key, params object[] args);
    
    /// <summary>
    /// Checks if a language is supported
    /// </summary>
    /// <param name="language">Language code</param>
    /// <returns>True if supported, false otherwise</returns>
    bool IsLanguageSupported(string language);
}


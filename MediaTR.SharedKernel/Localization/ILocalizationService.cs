namespace MediaTR.SharedKernel.Localization;

/// <summary>
/// Service for retrieving localized strings based on current culture
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized string by key
    /// </summary>
    /// <param name="key">Resource key (e.g., "Validation.Required")</param>
    /// <returns>Localized string for current culture, or key if not found</returns>
    string GetString(string key);

    /// <summary>
    /// Gets a localized string by key with format arguments
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    string GetString(string key, params object[] args);

    /// <summary>
    /// Sets the current culture for the request
    /// </summary>
    /// <param name="culture">Culture code (e.g., "tr-TR", "en-US")</param>
    void SetCulture(string culture);

    /// <summary>
    /// Gets the current culture code
    /// </summary>
    string GetCurrentCulture();
}

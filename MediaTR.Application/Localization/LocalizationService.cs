using System.Globalization;
using System.Resources;
using MediaTR.SharedKernel.Localization;

namespace MediaTR.Application.Localization;

/// <summary>
/// Implementation of ILocalizationService using .NET ResourceManager
/// Supports tr-TR and en-US cultures
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;
    private CultureInfo _currentCulture;

    public LocalizationService()
    {
        // Point to ErrorMessages resource file in same assembly (Application)
        _resourceManager = new ResourceManager(
            "MediaTR.Application.Resources.ErrorMessages",
            typeof(LocalizationService).Assembly);

        _currentCulture = CultureInfo.CurrentCulture;
    }

    public string GetString(string key)
    {
        try
        {
            var value = _resourceManager.GetString(key, _currentCulture);
            return value ?? key; // Return key if not found
        }
        catch
        {
            return key; // Fallback to key on any error
        }
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);

        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format; // Return unformatted string if formatting fails
        }
    }

    public void SetCulture(string culture)
    {
        try
        {
            _currentCulture = new CultureInfo(culture);
            CultureInfo.CurrentCulture = _currentCulture;
            CultureInfo.CurrentUICulture = _currentCulture;
        }
        catch
        {
            // Invalid culture, keep current
        }
    }

    public string GetCurrentCulture()
    {
        return _currentCulture.Name;
    }
}

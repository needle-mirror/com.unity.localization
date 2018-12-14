namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This example shows how to create and setup localization support for several languages and make it active.
    /// </summary>
    public class CreateAndSetupLocalizationSettingsExample : MonoBehaviour
    {
        void Start()
        {
            // Create our localization settings
            var localizationSettings = LocalizationSettings.CreateDefault();

            // Add the locales we support
            var supportedLocales = localizationSettings.GetAvailableLocales();
            supportedLocales.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.Arabic)));
            supportedLocales.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.English)));
            supportedLocales.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.French)));
            supportedLocales.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.German)));
            supportedLocales.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.Japanese)));

            // Set English to be our default
            LocalizationSettings.Instance = localizationSettings;
        }
    }
}
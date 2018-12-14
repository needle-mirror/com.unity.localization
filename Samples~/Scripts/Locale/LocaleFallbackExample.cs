namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This example shows how the fallback locale can be configured.
    /// </summary>
    public class LocaleFallbackExample : MonoBehaviour
    {
        void Start()
        {
            // Create a locale to represent English.
            var localeId = new LocaleIdentifier(SystemLanguage.English);
            var locale = Locale.CreateLocale(localeId);
        
            // Create a regional locale to represent English UK.
            var regionalLocaleId = new LocaleIdentifier("en-GB");
            var regionalLocale = Locale.CreateLocale(regionalLocaleId);
        
            // Fallback from English(UK) to the non-regional English version.
            regionalLocale.FallbackLocale = locale;
        
            Debug.Log("English(en-GB) will fallback to " + regionalLocale.FallbackLocale);
        }
    }
}
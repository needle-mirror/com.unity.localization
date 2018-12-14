namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This shows how to create a locale for English and a regional locale for English(US).
    /// </summary>
    public class LocaleIdentifierIdExample2 : MonoBehaviour
    {
        void Start()
        {
            // Create a locale to represent English.
            var localeId = new LocaleIdentifier(SystemLanguage.English);
            var locale = Locale.CreateLocale(localeId);
            Debug.Log("English locale: " + locale);
        
            // Create a regional locale to represent English UK.
            var regionalLocaleId = new LocaleIdentifier("en-GB");
            var regionalLocale = Locale.CreateLocale(regionalLocaleId);
            Debug.Log("English(en-GB) locale: " + regionalLocale);
        }
    }
}
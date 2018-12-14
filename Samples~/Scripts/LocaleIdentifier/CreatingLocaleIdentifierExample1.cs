using System.Globalization;

namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This example shows the various ways to create a LocaleIdentifier.
    /// </summary>
    public class LocaleIdentifierIdExample1 : MonoBehaviour
    {
        void Start() 
        {
            // Create a locale identifier to represent English
            var localeEnglishSystemLanguage = new LocaleIdentifier(SystemLanguage.English);
            var localeEnglishCode = new LocaleIdentifier("en");
            var localeEnglishCi = new LocaleIdentifier(CultureInfo.GetCultureInfo("en"));
        
            Debug.Log(localeEnglishSystemLanguage);
            Debug.Log(localeEnglishCode);
            Debug.Log(localeEnglishCi);
        }
    }
}
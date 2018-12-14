namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This example sets the name to include both English and Japanese.
    /// </summary>
    public class LocaleNameExample : MonoBehaviour
    {
        void Start()
        {
            // Create a locale to represent Japanese.
            var localeId = new LocaleIdentifier(SystemLanguage.Japanese);
            var locale = Locale.CreateLocale(localeId);
        
            // Customize the name.
            locale.name = "Japanese(日本)";
        }
    }
}
namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This example shows how the cultureInfo can be retrieved after creating a LocaleIdentifier using a Code.
    /// </summary>
    public class LocaleIdentifierCultureInfoExample : MonoBehaviour
    {
        void Start()
        {
            var localeIdentifier = new LocaleIdentifier("en");
            Debug.Log("en maps to the CultureInfo: " + localeIdentifier.CultureInfo.EnglishName);
        }
    }
}
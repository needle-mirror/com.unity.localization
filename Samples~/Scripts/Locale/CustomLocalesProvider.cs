using System.Collections.Generic;

namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// By default the locale provider uses the Addressables system to load all supported locales.
    /// This example shows how to create a locale provider that includes the locales built in and requires no loading from Addressables.
    /// To use this, create a Custom Locale Provider asset through the asset menu and drag the asset into the Localization settings Locales Provider field.
    /// </summary>
    [CreateAssetMenu(menuName = "Localization/Examples/Custom Locale Provider")]
    public class CustomLocalesProvider : LocalesProvider
    {
        // We just declare a public list which can have Locales added and removed like a typical script would.
        [SerializeField] List<Locale> m_Locales = new List<Locale>();

        public override List<Locale> Locales
        {
            get { return m_Locales; }
            set { m_Locales = value; }
        }
    }
}
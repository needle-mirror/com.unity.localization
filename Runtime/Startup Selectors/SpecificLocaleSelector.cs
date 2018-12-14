using UnityEngine;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Returns the a specific locale if it exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Localization/Locale Selectors/Default Locale")]
    public class SpecificLocaleSelector : StartupLocaleSelector
    {
        [SerializeField]
        LocaleIdentifier m_LocaleId = new LocaleIdentifier(SystemLanguage.English);

        /// <summary>
        /// The Id of the Locale that should be used if possible when selecting.
        /// </summary>
        public LocaleIdentifier LocaleId
        {
            get { return m_LocaleId; }
            set { m_LocaleId = value; }
        }

        public override Locale GetStartupLocale(LocalesProvider availableLocales)
        {
            return availableLocales.GetLocale(LocaleId);
        }
    }
}
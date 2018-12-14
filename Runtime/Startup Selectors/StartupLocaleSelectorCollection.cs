using System.Collections.Generic;

namespace UnityEngine.Localization
{
    /// <summary>
    /// A collection of multiple startup selectors.
    /// The locale is selected by starting with item 0 in the list and trying each until one succeeds in return a locale that is not null.
    /// </summary>
    [CreateAssetMenu(menuName = "Localization/Locale Selectors/Startup Collection")]
    public class StartupLocaleSelectorCollection : StartupLocaleSelector
    {
        [SerializeField]
        List<StartupLocaleSelector> m_StartupSelectors;

        public List<StartupLocaleSelector> StartupSelectors
        {
            get { return m_StartupSelectors; }
            set { m_StartupSelectors = value; }
        }

        public override Locale GetStartupLocale(LocalesProvider availableLocales)
        {
            foreach (var startupLocaleSelector in m_StartupSelectors)
            {
                if (startupLocaleSelector != null)
                {
                    var locale = startupLocaleSelector.GetStartupLocale(availableLocales);
                    if(locale != null)
                        return locale;
                }
            }
            return null;
        }
    }
}
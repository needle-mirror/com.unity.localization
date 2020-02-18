using System;
using System.Globalization;

namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// Attempts to determine the Locale to use from the system that is running the application.
    /// </summary>
    [Serializable]
    public class SystemLocaleSelector : IStartupLocaleSelector
    {
        // There is a bug with SerializeReference that causes empty instances to not deserialize. This is a workaround while we wait for the fix (case 1183547)
        [SerializeField, HideInInspector]
        int dummyObject;

        /// <summary>
        /// Uses <see cref="Application.systemLanguage"/> to find a matching <see cref="Locale"/> from the available locales.
        /// </summary>
        /// <param name="availableLocales"></param>
        /// <returns></returns>
        public Locale GetStartupLocale(ILocalesProvider availableLocales)
        {
            Locale locale = null;
            if (Application.systemLanguage != SystemLanguage.Unknown)
            {
                locale = availableLocales.GetLocale(Application.systemLanguage);
            }

            if (locale == null)
            {
                var cultureInfo = CultureInfo.CurrentUICulture;
                var identifier = new LocaleIdentifier(cultureInfo);
                locale = availableLocales.GetLocale(identifier);
                if (locale == null)
                {
                    // Attempt to use CultureInfo fallbacks to find the closest locale
                    while (!Equals(cultureInfo, CultureInfo.InvariantCulture) && locale == null)
                    {
                        locale = availableLocales.GetLocale(identifier);
                        cultureInfo = cultureInfo.Parent;
                    }

                    if (locale != null)
                    {
                        Debug.Log($"Locale '{CultureInfo.CurrentUICulture}' is not supported, however the parent locale '{locale.Identifier.CultureInfo}' is.");
                    }
                }
            }
            return locale;
        }
    }
}

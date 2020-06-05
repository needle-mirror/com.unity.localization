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
            // We first check the CultureInfo as this is more accurate and contains regional information.
            var cultureInfo = GetSystemCulture();
            var locale = availableLocales.GetLocale(cultureInfo);
            if (locale == null)
            {
                // Attempt to use CultureInfo fallbacks to find the closest locale
                while (cultureInfo != CultureInfo.InvariantCulture && locale == null)
                {
                    locale = availableLocales.GetLocale(cultureInfo);
                    cultureInfo = cultureInfo.Parent;
                }

                if (locale != null)
                {
                    Debug.Log($"The Locale '{CultureInfo.CurrentUICulture}' is not available, however the parent locale '{locale.Identifier.CultureInfo}' is available.");
                }
            }

            // Fallback to Application.systemLanguage
            var systemLanguage = GetApplicationSystemLanguage();
            if (locale == null && systemLanguage != SystemLanguage.Unknown)
            {
                locale = availableLocales.GetLocale(systemLanguage);
            }

            return locale;
        }

        protected virtual CultureInfo GetSystemCulture() => CultureInfo.CurrentUICulture;

        protected virtual SystemLanguage GetApplicationSystemLanguage() => Application.systemLanguage;
    }
}

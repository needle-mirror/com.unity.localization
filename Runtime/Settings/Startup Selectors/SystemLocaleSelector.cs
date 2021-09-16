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
        /// <summary>
        /// Uses <see cref="CultureInfo.CurrentUICulture"/> and <see cref="Application.systemLanguage"/> to find a matching <see cref="Locale"/> from the available locales.
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
                cultureInfo = cultureInfo.Parent;
                while (cultureInfo != CultureInfo.InvariantCulture && locale == null)
                {
                    locale = availableLocales.GetLocale(cultureInfo);
                    cultureInfo = cultureInfo.Parent;
                }

                // Only show this message in the player so we don't spam the Editor.
                #if !UNITY_EDITOR
                if (locale != null)
                {
                    Debug.Log($"The Locale '{GetSystemCulture()}' is not available, however the parent locale '{locale.Identifier.CultureInfo}' is available.");
                }
                #endif
            }

            // Fallback to Application.systemLanguage
            var systemLanguage = GetApplicationSystemLanguage();
            if (locale == null && systemLanguage != SystemLanguage.Unknown)
            {
                locale = availableLocales.GetLocale(systemLanguage);
            }

            return locale;
        }

        /// <summary>
        /// Returns CultureInfo.CurrentUICulture.
        /// </summary>
        /// <returns></returns>
        protected virtual CultureInfo GetSystemCulture() => CultureInfo.CurrentUICulture;

        /// <summary>
        /// Returns Application.systemLanguage.
        /// </summary>
        /// <returns></returns>
        protected virtual SystemLanguage GetApplicationSystemLanguage() => Application.systemLanguage;
    }
}

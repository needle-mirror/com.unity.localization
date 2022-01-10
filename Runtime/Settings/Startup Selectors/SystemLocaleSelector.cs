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
            Locale locale = null;

            // Application preference on iOS
            #if UNITY_IOS && !UNITY_EDITOR
            locale = FindLocaleOrFallback(getPreferredLanguage(), availableLocales);
            if (locale != null)
                return locale;
            #endif

            // CultureInfo does is not reliable, on 2019.4 it does not seem to work at all. We will call directly into Android instead.
            #if UNITY_ANDROID && !UNITY_EDITOR
            locale = FindLocaleOrFallback(GetAndroidDeviceLanguage(), availableLocales);
            if (locale != null)
                return locale;
            #endif

            // We first check the CultureInfo as this is more accurate and contains regional information.
            locale = FindLocaleOrFallback(GetSystemCulture(), availableLocales);

            // Fallback to Application.systemLanguage
            var systemLanguage = GetApplicationSystemLanguage();
            if (locale == null && systemLanguage != SystemLanguage.Unknown)
            {
                locale = FindLocaleOrFallback(systemLanguage, availableLocales);
            }

            return locale;
        }

        static Locale FindLocaleOrFallback(LocaleIdentifier localeIdentifier, ILocalesProvider availableLocales)
        {
            var cultureInfo = localeIdentifier.CultureInfo;
            if (cultureInfo == null)
                return null;

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
                    Debug.Log($"The Locale '{localeIdentifier}' is not available, however the parent locale '{locale.Identifier}' is available.");
                }
                #endif
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

        #if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        extern static string getPreferredLanguage();
        #endif

        #if UNITY_ANDROID && !UNITY_EDITOR
        static string GetAndroidDeviceLanguage()
        {
            using (AndroidJavaClass cls = new AndroidJavaClass("java.util.Locale"))
            {
                if (cls != null)
                {
                    using (AndroidJavaObject locale = cls.CallStatic<AndroidJavaObject>("getDefault"))
                    {
                        if (locale != null)
                        {
                            // This API requires Android 21 which is the min version in 2021.2 and above. 
                            #if UNITY_2021_2_OR_NEWER
                            return locale.Call<string>("toLanguageTag");
                            #else
                            // We use the older method which supports all android API.
                            return $"{locale.Call<string>("getLanguage")}-{locale.Call<string>("getCountry")}";
                            #endif
                        }
                    }
                }
            }
            return null;
        }
        #endif
    }
}

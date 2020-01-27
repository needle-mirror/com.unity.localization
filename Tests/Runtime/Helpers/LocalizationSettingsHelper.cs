using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UnityEngine.Localization.Tests
{
    public static class LocalizationSettingsHelper
    {
        /// <summary>
        /// Create a Localization Settings which with no String/Asset database and a TestLocaleProvider.
        /// The idea here is to avoid loading anything Addressable so our tests can run in isolation and be fast.
        /// </summary>
        /// <returns></returns>
        public static LocalizationSettings CreateEmpty()
        {
            var settings = ScriptableObject.CreateInstance<LocalizationSettings>();
            settings.SetAssetDatabase(null);
            settings.SetStringDatabase(null);

            // Add the test locales
            var localeProvider = new TestLocaleProvider();
            settings.SetAvailableLocales(localeProvider);
            return settings;
        }
    }
}

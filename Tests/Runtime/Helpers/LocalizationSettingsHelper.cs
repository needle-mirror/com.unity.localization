using NUnit.Framework;
using UnityEngine.Localization.Settings;

namespace UnityEngine.Localization.Tests
{
    public static class LocalizationSettingsHelper
    {
        static LocalizationSettings s_SavedSettings;

        /// <summary>
        /// Save the current LocalizationSettings so that they can be restored after a test.
        /// </summary>
        public static void SaveCurrentSettings()
        {
            Assert.IsNull(s_SavedSettings, "Expected there to be no saved settings.");
            s_SavedSettings = LocalizationSettings.GetInstanceDontCreateDefault();
            LocalizationSettings.Instance = null;
        }

        /// <summary>
        /// Restores any previously saved LocalizationSettings
        /// </summary>
        public static void RestoreSettings(bool deleteOld = false)
        {
            if (deleteOld)
            {
                var instance = LocalizationSettings.GetInstanceDontCreateDefault();
                if (instance != null)
                    Object.DestroyImmediate(instance);
            }

            LocalizationSettings.Instance = s_SavedSettings;
            s_SavedSettings = null;
        }

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

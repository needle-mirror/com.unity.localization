using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Localization.Settings;

namespace UnityEngine.Localization.Tests
{
    public class PlayerPrefLocaleSelectorTests
    {
        readonly List<SystemLanguage> testLanguages = new List<SystemLanguage>
        {
            SystemLanguage.English,
            SystemLanguage.Spanish,
            SystemLanguage.Swedish,
            SystemLanguage.Russian
        };

        LocalizationSettings m_OriginalSettings;
        LocalizationSettings m_TestSettings;
        PlayerPrefLocaleSelector m_PlayerPrefLocaleSelector;

        const string k_PlayerPrefKey = "test-locale-selected";

        [SetUp]
        public void Init()
        {
            m_OriginalSettings = LocalizationSettings.Instance;
            m_TestSettings = LocalizationSettingsHelper.CreateEmpty();
            LocalizationSettings.Instance = m_TestSettings;

            // Add the test locales
            var localeProvider = m_TestSettings.GetAvailableLocales();
            testLanguages.ForEach(o => localeProvider.AddLocale(Locale.CreateLocale(o)));

            PlayerPrefs.DeleteKey(k_PlayerPrefKey);
            m_PlayerPrefLocaleSelector = new PlayerPrefLocaleSelector { PlayerPreferenceKey = k_PlayerPrefKey };
            m_TestSettings.GetStartupLocaleSelectors().Add(m_PlayerPrefLocaleSelector);
        }

        [TearDown]
        public void Teardown()
        {
            LocalizationSettings.Instance = m_OriginalSettings;
            Object.DestroyImmediate(m_TestSettings);
        }

        [Test]
        public void PlayerPrefLocaleSelector_WhenLocaleIsChanged_RecordsLocaleToPlayerPrefs()
        {
            var spanish = m_TestSettings.GetAvailableLocales().GetLocale(SystemLanguage.Spanish);
            Assert.NotNull(spanish);
            Assert.AreNotEqual(m_TestSettings.GetSelectedLocale(), spanish);

            m_TestSettings.SetSelectedLocale(spanish);

            Assert.IsTrue(PlayerPrefs.HasKey(k_PlayerPrefKey), "Expected Player Pref Key to be set when the selected locale was changed.");

            var pref = PlayerPrefs.GetString(k_PlayerPrefKey);
            Assert.AreEqual(spanish.Identifier.Code, pref, "Expected the player prefs to contain the selected locale");
        }

        [Test]
        public void PlayerPrefLocaleSelector_ReturnsLocaleFromPlayerPref()
        {
            var russian = m_TestSettings.GetAvailableLocales().GetLocale(SystemLanguage.Russian);
            Assert.NotNull(russian);
            Assert.AreNotEqual(m_TestSettings.GetSelectedLocale(), russian);

            m_TestSettings.SetSelectedLocale(russian);

            Assert.AreEqual(russian, m_PlayerPrefLocaleSelector.GetStartupLocale(m_TestSettings.GetAvailableLocales()), "Expected Selected Locale to be returned by the player pref selector.");
        }

        [Test]
        public void PlayerPrefLocaleSelector_ReturnsNullIfPlayerPrefIsNotSet()
        {
            Assert.IsNull(m_PlayerPrefLocaleSelector.GetStartupLocale(m_TestSettings.GetAvailableLocales()), "Expected null to be returned when no player pref key is set.");
        }

        [Test]
        public void PlayerPrefLocaleSelector_ReturnsNullIfPlayerPrefIsEmpty()
        {
            PlayerPrefs.SetString(k_PlayerPrefKey, "");
            Assert.IsNull(m_PlayerPrefLocaleSelector.GetStartupLocale(m_TestSettings.GetAvailableLocales()), "Expected null to be returned when no player pref key is empty.");
        }
    }
}

using NUnit.Framework;
using UnityEngine.Localization.Settings;

namespace UnityEngine.Localization.Tests.Settings
{
    public class LocalizationSettingsLocaleChangedTests
    {
        LocalizationSettings m_Settings;

        [SetUp]
        public void CreateTestLocalizationSettings()
        {
            m_Settings = LocalizationSettingsHelper.CreateEmpty();
            var localeProvider = m_Settings.GetAvailableLocales();
            localeProvider.AddLocale(Locale.CreateLocale(SystemLanguage.Arabic));
            localeProvider.AddLocale(Locale.CreateLocale(SystemLanguage.English));
            localeProvider.AddLocale(Locale.CreateLocale(SystemLanguage.French));
            localeProvider.AddLocale(Locale.CreateLocale(SystemLanguage.German));
            localeProvider.AddLocale(Locale.CreateLocale(SystemLanguage.Japanese));
        }

        [TearDown]
        public void Teardown()
        {
            foreach (var locale in m_Settings.GetAvailableLocales().Locales)
            {
                Object.DestroyImmediate(locale);
            }
            Object.DestroyImmediate(m_Settings);
        }

        [Test]
        public void ChangingSelectedLocale_SendsLocaleChangedEvent()
        {
            Locale selectedLocale = null;
            m_Settings.OnSelectedLocaleChanged += (loc) => selectedLocale = loc;

            // Change the locale resulting in the event being sent.
            var japaneseLocale = m_Settings.GetAvailableLocales().GetLocale(SystemLanguage.Japanese);
            Assert.IsNotNull(japaneseLocale, "Expected Japanese locale to be returned but it was not.");
            m_Settings.SetSelectedLocale(japaneseLocale);

            Assert.IsNotNull(selectedLocale, "Current language is null, the LocaleChanged event was not sent.");
            Assert.AreEqual(japaneseLocale, selectedLocale, "Expected current language to be Japanese.");
        }

        [Test]
        public void ChangingSelectedLocaleToTheSame_DoesNotSendLocaleChangedEvent()
        {
            Locale selectedLocale = null;
            m_Settings.OnSelectedLocaleChanged += (loc) => selectedLocale = loc;

            // Change the locale resulting in the event being sent.
            var japaneseLocale = m_Settings.GetAvailableLocales().GetLocale(SystemLanguage.Japanese);
            Assert.IsNotNull(japaneseLocale, "Expected Japanese locale to be returned but it was not.");
            m_Settings.SetSelectedLocale(japaneseLocale);

            // Reset and assign the same locale again. No event should be sent this time.
            selectedLocale = null;
            m_Settings.SetSelectedLocale(japaneseLocale);
            Assert.IsNull(selectedLocale, "Expected the LocaleChanged event to not be sent when the locale was the same as previously.");
        }
    }
}

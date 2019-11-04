using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Localization.Settings;
/*
namespace UnityEngine.Localization.Tests
{
    public class LocalizationSettingsTests
    {
        LocalizationSettings m_Settings;

        public class SimpleLocalesProvider : LocalesProvider
        {
            public override List<Locale> Locales { get; set; }

            public SimpleLocalesProvider()
            {
                Locales = new List<Locale>();
            }
        }

        Locale m_OnSelectedLocaleChangedLocale;
        void OnSelectedLocaleChanged(Locale locale)
        {
            Assert.IsNotNull(locale, "Locale should not be null, it should be Japanese.");
            m_OnSelectedLocaleChangedLocale = locale;
        }

        [SetUp]
        public void CreateTestLocalizationSettings()
        {
            m_Settings = LocalizationSettings.CreateDefault();
            LocalizationSettings.Instance = m_Settings;

            var simpleLocalesProvider = ScriptableObject.CreateInstance<SimpleLocalesProvider>();
            m_Settings.SetAvailableLocales(simpleLocalesProvider);
            var sl = LocalizationSettings.AvailableLocales;
            sl.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.Arabic)));
            sl.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.English)));
            sl.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.French)));
            sl.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.German)));
            sl.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.Japanese)));
        }
        [SetUp]
        public void Setup()
        {
            m_OnSelectedLocaleChangedLocale = null;
        }

        [TearDown]
        public void Teardown()
        {
            LocalizationSettings.Instance = null;
            Object.DestroyImmediate(m_Settings);
        }

        [Test]
        public void ChangingSelectedLocale_SendsLocaleChangedEvent()
        {
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

            // Change the locale resulting in the event being sent.
            Assert.IsNull(m_OnSelectedLocaleChangedLocale);
            var japaneseLocale = LocalizationSettings.AvailableLocales.GetLocale(SystemLanguage.Japanese);
            Assert.IsNotNull(japaneseLocale);
            LocalizationSettings.SelectedLocale = japaneseLocale;

            Assert.IsNotNull(m_OnSelectedLocaleChangedLocale, "Current language is null, the selectedLocaleChanged event was not sent.");
            Assert.AreEqual(LocalizationSettings.AvailableLocales.GetLocale(SystemLanguage.Japanese), m_OnSelectedLocaleChangedLocale, "Expected current language to be Japanese.");
        }

        [Test]
        public void ChangingSelectedLocaleToTheSame_DoesNotSendLocaleChangedEvent()
        {
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

            // Change the locale resulting in the event being sent.
            Assert.IsNull(m_OnSelectedLocaleChangedLocale);
            var japaneseLocale = LocalizationSettings.AvailableLocales.GetLocale(SystemLanguage.Japanese);
            Assert.IsNotNull(japaneseLocale);
            LocalizationSettings.SelectedLocale = japaneseLocale;

            // Reset and assign the same locale again. No event should be sent this time
            m_OnSelectedLocaleChangedLocale = null;
            LocalizationSettings.SelectedLocale = japaneseLocale;

            Assert.IsNull(m_OnSelectedLocaleChangedLocale, "The selectedLocaleChanged event was sent even though the language did not change.");
        }
    }
}
*/
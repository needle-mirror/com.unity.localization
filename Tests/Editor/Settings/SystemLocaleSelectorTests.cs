using System.Globalization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tests;

namespace UnityEditor.Localization.Tests
{
    public class SystemLocaleSelectorTests
    {
        class SystemLocaleSelectorFixture : SystemLocaleSelector
        {
            public SystemLanguage ApplicationSystemLanguage { get; set; }
            public CultureInfo SystemCulture { get; set; }

            protected override SystemLanguage GetApplicationSystemLanguage() => ApplicationSystemLanguage;
            protected override CultureInfo GetSystemCulture() => SystemCulture;
        }

        TestLocaleProvider m_LocaleProvider;
        Locale m_English, m_EnglishGB, m_Japanese;

        [OneTimeSetUp]
        public void Setup()
        {
            m_LocaleProvider = new TestLocaleProvider();
            m_LocaleProvider.AddLocale(m_English = Locale.CreateLocale("en"));
            m_LocaleProvider.AddLocale(m_EnglishGB = Locale.CreateLocale("en-GB"));
            m_LocaleProvider.AddLocale(m_Japanese = Locale.CreateLocale("ja"));
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_English);
            Object.DestroyImmediate(m_EnglishGB);
            Object.DestroyImmediate(m_Japanese);
        }

        [Test]
        public void CultureInfoIsUsedBeforeApplicationSystemLanguage()
        {
            var selector = new SystemLocaleSelectorFixture { ApplicationSystemLanguage = SystemLanguage.English, SystemCulture = new CultureInfo("en-GB")};
            var selectedLocale = selector.GetStartupLocale(m_LocaleProvider);
            Assert.AreEqual(m_EnglishGB, selectedLocale, "Expected 'en-GB' to be selected but it was not.");
        }

        [Test]
        public void ApplicationSystemLanguageIsUsedWhenCulturInfoIsNotAvailable()
        {
            var selector = new SystemLocaleSelectorFixture { ApplicationSystemLanguage = SystemLanguage.Japanese, SystemCulture = new CultureInfo("fr")};
            var selectedLocale = selector.GetStartupLocale(m_LocaleProvider);
            Assert.AreEqual(m_Japanese, selectedLocale, "Expected 'ja' to be selected but it was not.");
        }

        [Test]
        public void NoLocaleIsSelectedIfCulrtureInfoAndApplicationSystemLangaugeAreNotAvailable()
        {
            var selector = new SystemLocaleSelectorFixture { ApplicationSystemLanguage = SystemLanguage.Swedish, SystemCulture = new CultureInfo("fr")};
            var selectedLocale = selector.GetStartupLocale(m_LocaleProvider);
            Assert.IsNull(selectedLocale, "Expected no locale to be returned when no suitable locales are available.");
        }
    }
}

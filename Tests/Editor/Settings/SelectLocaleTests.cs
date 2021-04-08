using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.Localization.Settings;
using UnityEngine.TestTools;

namespace UnityEngine.Localization.Tests
{
    public class SelectLocaleTests
    {
        LocalizationSettings m_Settings;
        ILocalesProvider m_LocalesProvider;

        [SetUp]
        public void CreateTestLocalizationSettings()
        {
            m_Settings = LocalizationSettingsHelper.CreateEmpty();
            m_Settings.IsPlayingOverride = true;
            m_LocalesProvider = m_Settings.GetAvailableLocales();
            m_LocalesProvider.AddLocale(Locale.CreateLocale(SystemLanguage.Arabic));
            m_LocalesProvider.AddLocale(Locale.CreateLocale(SystemLanguage.English));
            m_LocalesProvider.AddLocale(Locale.CreateLocale(SystemLanguage.French));
            m_LocalesProvider.AddLocale(Locale.CreateLocale(SystemLanguage.German));
            m_LocalesProvider.AddLocale(Locale.CreateLocale(SystemLanguage.Japanese));
        }

        [TearDown]
        public void Teardown()
        {
            CleanupLocales();
            Object.DestroyImmediate(m_Settings);
        }

        void CleanupLocales()
        {
            foreach (var locale in m_LocalesProvider.Locales)
            {
                Object.DestroyImmediate(locale);
            }
            m_LocalesProvider.Locales.Clear();
        }

        [Test]
        public void NullAvailableLocalesProducesError()
        {
            m_Settings.SetAvailableLocales(null);
            Assert.IsNull(m_Settings.GetSelectedLocale(), "Expected no locale to be returned.");
            LogAssert.Expect(LogType.Error, "AvailableLocales is null, can not pick a Locale.");
        }

        [Test]
        public void NullLocalesListProducesError()
        {
            m_Settings.SetAvailableLocales(new TestLocaleProvider { Locales = null });
            Assert.IsNull(m_Settings.GetSelectedLocale(), "Expected no locale to be returned.");
            LogAssert.Expect(LogType.Error, "AvailableLocales.Locales is null, can not pick a Locale.");
        }

        [Test]
        public void EmptyLocalesListProducesError()
        {
            CleanupLocales();
            Assert.IsNull(m_Settings.GetSelectedLocale(), "Expected no locale to be returned.");
            LogAssert.Expect(LogType.Error, new Regex("No Locales were available. Did you build the Addressables?"));
        }

        [Test]
        public void EmptyStartupSelectorsProducesError()
        {
            m_Settings.GetStartupLocaleSelectors().Clear();
            Assert.IsNull(m_Settings.GetSelectedLocale(), "Expected no locale to be returned.");
            LogAssert.Expect(LogType.Error, new Regex("No Locale could be selected"));
        }

        [Test]
        public void NoMatchingStartupSelectorLocaleProducesError()
        {
            m_Settings.GetStartupLocaleSelectors().Clear();
            m_Settings.GetStartupLocaleSelectors().Add(new CommandLineLocaleSelector());
            m_Settings.GetStartupLocaleSelectors().Add(new SpecificLocaleSelector { LocaleId = "bg" }); // Bulgarian

            Assert.IsNull(m_Settings.GetSelectedLocale(), "Expected no locale to be returned.");
            LogAssert.Expect(LogType.Error, new Regex("No Locale could be selected"));
        }

        [Test]
        public void SelectorPriorityIsRespected()
        {
            // Items at the start of the list have higher priority.
            m_Settings.GetStartupLocaleSelectors().Clear();
            m_Settings.GetStartupLocaleSelectors().Add(new SpecificLocaleSelector { LocaleId = "ja" }); // Japanese
            m_Settings.GetStartupLocaleSelectors().Add(new SpecificLocaleSelector { LocaleId = "en" }); // English

            var selectedLocale = m_Settings.GetSelectedLocale();
            Assert.NotNull(selectedLocale, "Expected a Locale to be selected.");
            Assert.AreEqual("ja", m_Settings.GetSelectedLocale().Identifier.Code, "Expected Japanese locale to be selected.");
        }
    }
}

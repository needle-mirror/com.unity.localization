using NUnit.Framework;

namespace UnityEngine.Localization.Tests
{
    public class LocaleSystemLanguage
    {
        [TestCase(SystemLanguage.Afrikaans, "af")]
        [TestCase(SystemLanguage.Arabic, "ar")]
        [TestCase(SystemLanguage.Basque, "eu")]
        [TestCase(SystemLanguage.Belarusian, "be")]
        [TestCase(SystemLanguage.Bulgarian, "bg")]
        [TestCase(SystemLanguage.Catalan, "ca")]
        [TestCase(SystemLanguage.Chinese, "zh-CN")]
        [TestCase(SystemLanguage.ChineseSimplified, "zh-hans")]
        [TestCase(SystemLanguage.ChineseTraditional, "zh-hant")]
        [TestCase(SystemLanguage.SerboCroatian, "hr")]
        [TestCase(SystemLanguage.Czech, "cs")]
        [TestCase(SystemLanguage.Danish, "da")]
        [TestCase(SystemLanguage.Dutch, "nl")]
        [TestCase(SystemLanguage.English, "en")]
        [TestCase(SystemLanguage.Estonian, "et")]
        [TestCase(SystemLanguage.Faroese, "fo")]
        [TestCase(SystemLanguage.Finnish, "fi")]
        [TestCase(SystemLanguage.French, "fr")]
        [TestCase(SystemLanguage.German, "de")]
        [TestCase(SystemLanguage.Greek, "el")]
        [TestCase(SystemLanguage.Hebrew, "he")]
        [TestCase(SystemLanguage.Hungarian, "hu")]
        [TestCase(SystemLanguage.Icelandic, "is")]
        [TestCase(SystemLanguage.Indonesian, "id")]
        [TestCase(SystemLanguage.Italian, "it")]
        [TestCase(SystemLanguage.Japanese, "ja")]
        [TestCase(SystemLanguage.Korean, "ko")]
        [TestCase(SystemLanguage.Latvian, "lv")]
        [TestCase(SystemLanguage.Lithuanian, "lt")]
        [TestCase(SystemLanguage.Norwegian, "no")]
        [TestCase(SystemLanguage.Polish, "pl")]
        [TestCase(SystemLanguage.Portuguese, "pt")]
        [TestCase(SystemLanguage.Romanian, "ro")]
        [TestCase(SystemLanguage.Russian, "ru")]
        [TestCase(SystemLanguage.Slovak, "sk")]
        [TestCase(SystemLanguage.Slovenian, "sl")]
        [TestCase(SystemLanguage.Spanish, "es")]
        [TestCase(SystemLanguage.Swedish, "sv")]
        [TestCase(SystemLanguage.Thai, "th")]
        [TestCase(SystemLanguage.Turkish, "tr")]
        [TestCase(SystemLanguage.Ukrainian, "uk")]
        [TestCase(SystemLanguage.Vietnamese, "vi")]
        public static void SystemLanguageMapsToLocaleIdentifier(SystemLanguage lang, string expectedCode)
        {
            var localeId = new LocaleIdentifier(lang);
            Assert.IsNotNull(localeId.CultureInfo, "Expected the SystemLanguage to be mapped to a cultureInfo but it was not." + localeId);
            Assert.AreEqual(expectedCode, localeId.Code);
        }
    }
}

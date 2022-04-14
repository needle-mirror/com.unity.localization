#if UNITY_ANDROID

using NUnit.Framework;
using UnityEditor.Localization.Platform.Android;

namespace UnityEngine.Localization.Tests
{
    public class LocaleAndroidMappingTests
    {
        [TestCase("zh-hans", "zh+Hans")]
        [TestCase("zh-hant", "zh+Hant")]
        [TestCase("zh-Hans", "zh+Hans")]
        [TestCase("zh-Hant", "zh+Hant")]
        public void AddingLocalizationToAndroid_MapsCorrectly(string code, string expectedCode)
        {
            var locale = Locale.CreateLocale(code);
            Assert.AreEqual(expectedCode, Player.GenerateAndroidLanguageCode(locale.Identifier));
            Object.DestroyImmediate(locale);
        }
    }
}

#endif

using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    public class LocalizationEditorSettingsTests
    {
        protected static List<Locale> GenerateSampleLocales()
        {
            return new List<Locale>()
            {
                Locale.CreateLocale(SystemLanguage.English),
                Locale.CreateLocale(SystemLanguage.French),
                Locale.CreateLocale(SystemLanguage.Arabic),
                Locale.CreateLocale(SystemLanguage.Japanese),
                Locale.CreateLocale(SystemLanguage.Chinese)
            };
        }

        [Test]
        public void AddLocale_WithNonPersistentLocale_GeneratesError()
        {
            var locale = ScriptableObject.CreateInstance<Locale>();
            Assert.Throws<AssetNotPersistentException>(() => LocalizationEditorSettings.AddLocale(locale));
            Object.DestroyImmediate(locale);
        }

        [TestCase("Locale-en", "en")]
        [TestCase("Locale-en-GB", "en-GB")]
        [TestCase("Locale-ja", "ja")]
        [TestCase("Locale-zh-CHS", "zh-CHS")]
        [TestCase("Locale-ar-AE", "ar-AE")]
        public void LocaleLabelToId_CorrectlyConvertsLabel(string label, string expectedCode)
        {
            var localeId = AddressHelper.LocaleLabelToId(label);
            Assert.AreEqual(expectedCode, localeId.Code, "Failed to convert the Addressables Locale label to the correct LocaleIdentifier");
        }

        [TestCaseSource(nameof(GenerateSampleLocales))]
        public void LocaleLabelToId_WorksWithLabelsGeneratedUsing_FormatAssetLabel(Locale locale)
        {
            var label = AddressHelper.FormatAssetLabel(locale.Identifier);
            LocaleLabelToId_CorrectlyConvertsLabel(label, locale.Identifier.Code);
        }

        [TestCaseSource(nameof(GenerateSampleLocales))]
        public void IsLocaleLabel_WorksWithLabelsGeneratedUsing_FormatAssetLabel(Locale locale)
        {
            var label = AddressHelper.FormatAssetLabel(locale.Identifier);
            Assert.IsTrue(AddressHelper.IsLocaleLabel(label), "Expected the Addressables Locale label to be recognized by IsLocaleLabel.");
        }

        [Test]
        public void CreateCollectionThrowsExceptionIfTypeIsNotDerivedFromLocalizedTableCollection()
        {
            Assert.Throws<ArgumentException>(() => LocalizationEditorSettings.Instance.CreateCollection(typeof(StringTable), null, null, null));
        }

        [Test]
        public void CreateCollectionThrowsExceptionIfTableCollectionNameIsNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() => LocalizationEditorSettings.Instance.CreateCollection(typeof(StringTableCollection), null, null, null));
            Assert.Throws<ArgumentException>(() => LocalizationEditorSettings.Instance.CreateCollection(typeof(StringTableCollection), "", null, null));
        }
    }
}

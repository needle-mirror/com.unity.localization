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
        public void CreateCollectionThrowsExceptionIfTypeIsNotDerivedFromLocalizationTableCollection()
        {
            Assert.Throws<ArgumentException>(() => LocalizationEditorSettings.Instance.CreateCollection(typeof(StringTable), null, null, null));
        }

        [Test]
        public void CreateCollectionThrowsExceptionIfTableCollectionNameIsNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() => LocalizationEditorSettings.Instance.CreateCollection(typeof(StringTableCollection), null, null, null));
            Assert.Throws<ArgumentException>(() => LocalizationEditorSettings.Instance.CreateCollection(typeof(StringTableCollection), "", null, null));
        }

        [Test]
        public void SettingShowLocaleMenuInGameViewUpdatesEditorPrefs()
        {
            LocalizationEditorSettings.ShowLocaleMenuInGameView = false;
            Assert.IsFalse(EditorPrefs.GetBool(LocalizationEditorSettings.k_GameViewPref));

            LocalizationEditorSettings.ShowLocaleMenuInGameView = true;
            Assert.IsTrue(EditorPrefs.GetBool(LocalizationEditorSettings.k_GameViewPref));
        }

        [Test]
        public void ShowLocaleMenuInGameViewUsesEditorPrefsValue()
        {
            EditorPrefs.SetBool(LocalizationEditorSettings.k_GameViewPref, false);
            Assert.IsFalse(EditorPrefs.GetBool(LocalizationEditorSettings.k_GameViewPref));

            EditorPrefs.SetBool(LocalizationEditorSettings.k_GameViewPref, true);
            Assert.IsTrue(EditorPrefs.GetBool(LocalizationEditorSettings.k_GameViewPref));
        }

        [Test]
        public void ProjectLocalesIsUpdatedWhenRemoveLocaleIsUndone()
        {
            const string localeAssetPath = "Assets/HebrewRemove.asset";
            var locale = Locale.CreateLocale(SystemLanguage.Hebrew);

            AssetDatabase.CreateAsset(locale, localeAssetPath);
            LocalizationEditorSettings.AddLocale(locale, false);
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Contain(locale), "Expected new locale asset to be added to Project Locales.");

            LocalizationEditorSettings.RemoveLocale(locale, true);
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Not.Contains(locale), "Expected locale to not be in project locales after calling RemoveLocale.");

            Undo.PerformUndo();
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Contain(locale), "Expected locale asset to be in project locale after calling Undo.");

            AssetDatabase.DeleteAsset(localeAssetPath);
        }

        [Test]
        public void ProjectLocalesIsUpdatedWhenAddLocaleIsUndone()
        {
            const string localeAssetPath = "Assets/HebrewAdd.asset";
            var locale = Locale.CreateLocale(SystemLanguage.Hebrew);

            AssetDatabase.CreateAsset(locale, localeAssetPath);
            LocalizationEditorSettings.AddLocale(locale, false);
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Contain(locale), "Expected new locale asset to be added to Project Locales.");

            LocalizationEditorSettings.RemoveLocale(locale, false);
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Not.Contains(locale), "Expected locale to not be in project locales after calling RemoveLocale.");

            LocalizationEditorSettings.AddLocale(locale, true);
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Contain(locale), "Expected locale asset to be in project locale after calling AddLocale.");

            Undo.PerformUndo();
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Not.Contains(locale), "Expected locale to not be in project locales after calling Undo.");

            AssetDatabase.DeleteAsset(localeAssetPath);
        }
    }
}

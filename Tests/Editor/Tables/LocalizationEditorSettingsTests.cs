using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Tests;
using Object = UnityEngine.Object;
using UnityEngine.Localization.Pseudo;

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

        [SetUp]
        public void Setup()
        {
            LocalizationSettingsHelper.SaveCurrentSettings();
        }

        [TearDown]
        public void Teardown()
        {
            LocalizationSettingsHelper.RestoreSettings();
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
        public void CreateCollection_InvalidArguments_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => LocalizationEditorSettings.Instance.CreateCollection(typeof(StringTable), "test", "Assets/", null));
            Assert.Throws<ArgumentException>(() => LocalizationEditorSettings.Instance.CreateCollection(typeof(StringTableCollection), "test", null, null));
        }

        [TestCase("<invalid>", "Table collection name cannot contain invalid filename characters.*")]
        [TestCase("b%d name/", "Table collection name cannot contain invalid filename characters.*")]
        [TestCase("     ", "Table collection name cannot be blank or whitespace")]
        [TestCase("", "Table collection name cannot be blank or whitespace")]
        [TestCase(null, "Table collection name cannot be blank or whitespace")]
        [TestCase("[no good]", @"Table collection name cannot contain both '\[' and '\]'")]
        [TestCase(" leading whitespace", "Table collection name cannot contain leading or trailing whitespace")]
        [TestCase("trailing whitespace ", "Table collection name cannot contain leading or trailing whitespace")]
        [TestCase(" leading trailing whitespace ", "Table collection name cannot contain leading or trailing whitespace")]
        public void CreateCollection_InvalidTableName_ThrowsException(string tableName, string exceptionRegex)
        {
            var exception = Assert.Throws<ArgumentException>(() => LocalizationEditorSettings.Instance.CreateCollection(typeof(StringTableCollection), tableName, "Assets/Temp", null));
            StringAssert.IsMatch(exceptionRegex, exception.Message);
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
            Undo.ClearAll();

            const string localeAssetPath = "Assets/HebrewRemove.asset";
            var locale = Locale.CreateLocale(SystemLanguage.Hebrew);

            AssetDatabase.CreateAsset(locale, localeAssetPath);
            LocalizationEditorSettings.AddLocale(locale, false);
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Contain(locale), "Expected new locale asset to be added to Project Locales.");

            LocalizationEditorSettings.RemoveLocale(locale, true);
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Not.Contains(locale), "Expected locale to not be in project locales after calling RemoveLocale.");

            Undo.PerformUndo();
            Assert.That(LocalizationEditorSettings.GetLocales(), Does.Contain(locale), "Expected locale asset to be in project locale after calling Undo.");

            Assert.True(AssetDatabase.DeleteAsset(localeAssetPath), "Failed to delete asset");
            Undo.ClearAll();
        }

        [Test]
        public void ProjectLocalesIsUpdatedWhenAddLocaleIsUndone()
        {
            Undo.ClearAll();
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

            Assert.True(AssetDatabase.DeleteAsset(localeAssetPath), "Failed to delete asset");
            Undo.ClearAll();
        }

        [Test]
        public void AddingAndRemovingPseudoLocale_UpdatesAvailablePseudoLocales()
        {
            var pseudoLocaleAssetName = "Pseudo-Locale_en";
            var path = Application.dataPath + "/" + pseudoLocaleAssetName + ".asset";
            var relativePath = PathHelper.MakePathRelative(path);
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(relativePath);

            var pseudoLocale = PseudoLocale.CreatePseudoLocale();
            pseudoLocale.name = pseudoLocaleAssetName;
            AssetDatabase.CreateAsset(pseudoLocale, assetPath);
            Assert.That(LocalizationEditorSettings.GetPseudoLocales(), Does.Contain(pseudoLocale), "Expected pseudo locale asset to be in project locales.");

            AssetDatabase.DeleteAsset(assetPath);
            Assert.That(LocalizationEditorSettings.GetPseudoLocales(), !Does.Contain(pseudoLocale), "Expected pseudo locale asset to be in project locales.");
        }
    }
}

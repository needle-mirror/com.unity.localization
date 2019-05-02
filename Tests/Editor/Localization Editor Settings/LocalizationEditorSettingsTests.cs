using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    public class LocalizationEditorSettingsTests
    {
        protected FakedLocalizationEditorSettings Settings { get; set; }

        protected KeyDatabase KeyDb { get; set; }

        [SetUp]
        public void Init()
        {
            Settings = new FakedLocalizationEditorSettings();
            LocalizationEditorSettings.Instance = Settings;
            KeyDb = ScriptableObject.CreateInstance<KeyDatabase>();
        }

        [TearDown]
        public void Teardown()
        {
            LocalizationEditorSettings.Instance = null;
            Object.DestroyImmediate(KeyDb);
        }

        public static List<Type> AllTableTypes()
        {
            var tableTypes = new List<Type>();
            AssemblyScanner.FindSubclasses<LocalizedTable>(tableTypes);
            return tableTypes;
        }

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

        [TestCaseSource("AllTableTypes")]
        public void CreateTables_WithNullKeyDatabaseArgument_CreatesAndAssignsNewKeyDatabase(Type tableType)
        {
            Assert.IsEmpty(Settings.CreatedKeyDatabases);
            var createdTables = LocalizationEditorSettings.CreateAssetTables(GenerateSampleLocales(), null, "Table 123", tableType, "");
            Assert.AreEqual(1, Settings.CreatedKeyDatabases.Count, "Expected a new Key Database to be created for the tables.");

            var keyDb = Settings.CreatedKeyDatabases[0];
            foreach (var table in createdTables)
            {
                Assert.AreSame(keyDb, table.Keys, "Expected the table to be using the newly created Key Database");
            }
        }

        [TestCaseSource("AllTableTypes")]
        public void CreateTables_WithKeyDatabaseProvided_DoesNotCreateANewKeyDatabase(Type tableType)
        {
            var createdTables = LocalizationEditorSettings.CreateAssetTables(GenerateSampleLocales(), KeyDb, "Table 123", tableType, "");
            Assert.IsEmpty(Settings.CreatedKeyDatabases, "Expected a no new Key Databases to be created for the tables.");

            foreach (var table in createdTables)
            {
                Assert.AreSame(KeyDb, table.Keys, "Expected the table to be using the provided Key Database");
            }
        }

        [TestCaseSource("AllTableTypes")]
        public void CreateTables_AssignsTableNameToAllNewTables(Type tableType)
        {
            Assert.IsEmpty(Settings.CreatedKeyDatabases);
            const string tableName = "CreateTables_AssignsTableNameToAllNewTables";
            var createdTables = LocalizationEditorSettings.CreateAssetTables(GenerateSampleLocales(), KeyDb, tableName, tableType, "");

            foreach (var table in createdTables)
            {
                Assert.AreEqual(tableName, table.TableName, "Expected the table name to be :" + tableName);
            }
        }

        [TestCaseSource("GenerateSampleLocales")]
        public void AddLocale_WithNonPersistentLocale_GeneratesError(Locale locale)
        {
            LocalizationEditorSettings.AddLocale(locale);
            LogAssert.Expect(LogType.Error, "Only persistent assets can be addressable. The asset needs to be saved on disk.");
        }

        [TestCase("Locale-en", "en")]
        [TestCase("Locale-en-GB", "en-GB")]
        [TestCase("Locale-ja", "ja")]
        [TestCase("Locale-zh-CHS", "zh-CHS")]
        [TestCase("Locale-ar-AE", "ar-AE")]
        public void LocaleLabelToId_CorrectlyConvertsLabel(string label, string expectedCode)
        {
            var localeId = LocalizationEditorSettings.LocaleLabelToId(label);
            Assert.AreEqual(expectedCode, localeId.Code, "Failed to convert the Addressables Locale label to the correct LocaleIdentifier");
        }

        [TestCaseSource("GenerateSampleLocales")]
        public void LocaleLabelToId_WorksWithLabelsGeneratedUsing_FormatAssetLabel(Locale locale)
        {
            var label = LocalizationEditorSettings.FormatAssetLabel(locale.Identifier);
            LocaleLabelToId_CorrectlyConvertsLabel(label, locale.Identifier.Code);
        }

        [TestCaseSource("GenerateSampleLocales")]
        public void IsLocaleLabel_WorksWithLabelsGeneratedUsing_FormatAssetLabel(Locale locale)
        {
            var label = LocalizationEditorSettings.FormatAssetLabel(locale.Identifier);
            Assert.IsTrue(LocalizationEditorSettings.IsLocaleLabel(label), "Expected the Addressables Locale label to be recognized by IsLocaleLabel.");
        }
    }
}

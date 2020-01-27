using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.TestTools;

namespace UnityEditor.Localization.Tests
{
    public class LocalizationEditorSettingsTests
    {
        protected FakedLocalizationEditorSettings Settings { get; set; }

        [SetUp]
        public void Init()
        {
            Settings = new FakedLocalizationEditorSettings();
            LocalizationEditorSettings.Instance = Settings;
        }

        [TearDown]
        public void Teardown()
        {
            LocalizationEditorSettings.Instance = null;
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

        [TestCase(typeof(AssetTable))]
        [TestCase(typeof(StringTable))]
        public void CreateTables_AssignsTableNameToAllNewTables(Type tableType)
        {
            Assert.IsEmpty(Settings.CreatedSharedTableDatas);
            const string tableName = nameof(CreateTables_AssignsTableNameToAllNewTables);
            var createdTables = LocalizationEditorSettings.CreateAssetTableCollection(GenerateSampleLocales(), tableName, tableType, "");

            foreach (var table in createdTables)
            {
                Assert.AreEqual(tableName, table.TableName, "Expected the table name to be :" + tableName);
            }
        }

        [Test]
        public void AddLocale_WithNonPersistentLocale_GeneratesError()
        {
            var locale = new Locale();
            LocalizationEditorSettings.AddLocale(locale, false);
            LogAssert.Expect(LogType.Error, new Regex("Only persistent assets can be addressable."));
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
    }
}

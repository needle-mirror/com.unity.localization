using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests
{
    public class AddressableTests : AddressableAssetTestBase
    {
        public KeyDatabase KeyDb { get; set; }

        public List<Texture2DAssetTable> Texture2DAssetTables { get; set; }

        protected override void OnInit()
        {
            KeyDb = ScriptableObject.CreateInstance<KeyDatabase>();
            CreateAsset(KeyDb, "General Use Key Db");
            var createdTables = LocalizationEditorSettings.CreateAssetTables(GenerateSampleLocales(), KeyDb, "General Use Textures", typeof(Texture2DAssetTable), k_TestConfigFolder);
            Texture2DAssetTables = createdTables.ConvertAll(tbl => tbl as Texture2DAssetTable);
        }

        List<Locale> GenerateSampleLocales()
        {
            var locales = new List<Locale>();
            var languages = GenerateSampleLanguages();
            languages.ForEach(lang => locales.Add(Locale.CreateLocale(lang)));
            return locales;
        }

        static bool CollectionContainsTables(Type tableType, string tableName, List<LocalizedTable> tables, List<AssetTableCollection> tableCollection)
        {
            Assert.IsNotEmpty(tableCollection);
            Assert.IsNotEmpty(tables);

            int foundMatches = 0;
            foreach (var tc in tableCollection)
            {
                if (tc.TableType == tableType && tc.TableName == tableName)
                {
                    foreach (var table in tables)
                    {
                        if (tc.Tables.Contains(table))
                            foundMatches++;
                    }
                }
            }
            return foundMatches == tables.Count;
        }

        static bool CollectionContainsEntry(Type tableType, string tableName, List<AssetTableCollection> tableCollection)
        {
            Assert.IsNotEmpty(tableCollection);
            foreach (var tc in tableCollection)
            {
                if (tc.TableType == tableType && tc.TableName == tableName)
                {
                    return true;
                }
            }
            return false;
        }

        [TestCaseSource("AllTableTypes")]
        public void CreatedTables_AreAddedToAddressables(Type tableType)
        {
            var createdTables = LocalizationEditorSettings.CreateAssetTables(GenerateSampleLocales(), null, "CreatedTables_AreAddedToAddressables", tableType, k_TestConfigFolder);

            foreach (var table in createdTables)
            {
                VerifyAssetIsInAddressables(table);
            }
        }

        [TestCaseSource("AllTableTypes")]
        public void CreatedTable_IsAddedToAddressables(Type tableType)
        {
            var assetPath = Path.Combine(k_TestConfigFolder, "CreatedTable_IsAddedToAddressables_" + tableType + ".asset");
            var createdTable = LocalizationEditorSettings.CreateAssetTable(Locale.CreateLocale(SystemLanguage.English), KeyDb, "CreatedTable_IsAddedToAddressables", tableType, assetPath);
            VerifyAssetIsInAddressables(createdTable);
        }

        [TestCaseSource("AllTableTypes")]
        public void ImportedTable_IsAddedToAddressables(Type tableType)
        {
            const string tableName = "ImportedTable_IsAddedToAddressables";
            var assetPath = Path.Combine(k_TestConfigFolder, "ImportedTable_IsAddedToAddressables" + tableType + ".asset");

            var relativePath = LocalizationEditorSettings.MakePathRelative(assetPath);
            var createdTable = (LocalizedTable)ScriptableObject.CreateInstance(tableType);
            createdTable.TableName = tableName;
            createdTable.Keys = KeyDb;
            createdTable.LocaleIdentifier = Locale.CreateLocale(SystemLanguage.Catalan).Identifier;
            createdTable.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(createdTable, relativePath);

            VerifyAssetIsInAddressables(createdTable, "Expected a table that was imported or created externally to be added to Addressables but it was not.");
        }

        [TestCaseSource("AllTableTypes")]
        public void GetAssetTablesCollectionInternal_UpdatesWhenTableAssetsAreAddedAndRemoved(Type tableType)
        {
            const string tableName = "GetAssetTablesCollectionInternal_UpdatesWhenTableAssetsAreAddedAndRemoved";

            // Check the table is not already in the collection.
            var tableCollection = LocalizationEditorSettings.GetAssetTablesCollection<LocalizedTable>();
            Assert.IsFalse(CollectionContainsEntry(tableType, tableName, tableCollection), "Expected no table entry to exist in the AssetTablesCollection.");

            // Create the new tables and check they are returned.
            var createdTables = LocalizationEditorSettings.CreateAssetTables(GenerateSampleLocales(), KeyDb, tableName, tableType, k_TestConfigFolder);
            tableCollection = LocalizationEditorSettings.GetAssetTablesCollection<LocalizedTable>();
            Assert.IsTrue(CollectionContainsEntry(tableType, tableName, tableCollection), "Expected the new tables to have an entry in the AssetTablesCollection.");
            Assert.IsTrue(CollectionContainsTables(tableType, tableName, createdTables, tableCollection), "Expected the new tables to have been contained in the AssetTablesCollection.");

            // Now delete the table assets and check they are no longer returned.
            //AssetDatabase.StartAssetEditing();
            foreach (var table in createdTables)
            {
                DeleteAsset(table);
            }
            //AssetDatabase.StopAssetEditing();

            tableCollection = LocalizationEditorSettings.GetAssetTablesCollection<LocalizedTable>();
            Assert.IsFalse(CollectionContainsEntry(tableType, tableName, tableCollection), "Expected the deleted tables to not have an entry in the AssetTablesCollection.");
        }

        [TestCaseSource("AllTableTypes")]
        public void GetAssetTablesCollectionInternal_TableEntriesAllHaveTheSameTypeAndName(Type tableType)
        {
            const string tableName = "GetAssetTablesCollectionInternal_AssetsAreCollatedByTypeAndName";

            // Create the new tables and check the entries do all contain the same type and name
            LocalizationEditorSettings.CreateAssetTables(GenerateSampleLocales(), KeyDb, tableName, tableType, k_TestConfigFolder);
            var tableCollection = LocalizationEditorSettings.GetAssetTablesCollection<LocalizedTable>();
            Assert.IsNotEmpty(tableCollection);

            foreach (var tc in tableCollection)
            {
                Assert.IsTrue(tc.Tables.TrueForAll(tbl => tbl.TableName == tc.TableName), "All tables should have the same Table Name however they do not. Problem found in: " + tc.TableName);
                Assert.IsTrue(tc.Tables.TrueForAll(tbl => tbl.GetType() == tc.TableType), "All tables should be the same type however they are not. Problem found with: " + tc.TableType);
            }
        }

        [TestCaseSource("GenerateSampleLanguages")]
        public void AddLocale_IsAddedToAddressables(SystemLanguage lang)
        {
            var locale = Locale.CreateLocale(lang);
            CreateAsset(locale, "AddLocale_IsAddedToAddressables-" + locale);
            LocalizationEditorSettings.AddLocale(locale);
            VerifyAssetIsInAddressables(locale);
        }

        [TestCaseSource("GenerateSampleLanguages")]
        public void RemoveLocale_IsRemovedFromAddressables(SystemLanguage lang)
        {
            var locale = Locale.CreateLocale(lang);
            CreateAsset(locale, "RemoveLocale_IsRemovedFromAddressables-" + locale);
            LocalizationEditorSettings.AddLocale(locale);
            VerifyAssetIsInAddressables(locale);

            LocalizationEditorSettings.RemoveLocale(locale);
            VerifyAssetIsNotInAddressables(locale);
        }

        [Test]
        public void AddAssetToTable_Texture2DAssetTable_AssetIsAddedToAddressables()
        {
            var keyEntry = KeyDb.AddKey("AddAssetToTable_Texture2DAssetTable_AssetIsAddedToAddressables");
            var textureAsset = CreateTestTexture("AddAssetToTable_Texture2DAssetTable_AssetIsAddedToAddressables");
            LocalizationEditorSettings.AddAssetToTable(Texture2DAssetTables[0], keyEntry.Id, textureAsset);
            VerifyAssetIsInAddressables(textureAsset);
        }

        [Test]
        public void AddAssetToTable_Texture2DAssetTable_SharedAssetHasCorrectLabels()
        {
            var keyEntry = KeyDb.AddKey("AddAssetToTable_Texture2DAssetTable_SharedAssetHasCorrectLabels");
            var textureAsset = CreateTestTexture("AddAssetToTable_Texture2DAssetTable_SharedAssetHasCorrectLabels_Texture");
            Assert.Greater(Texture2DAssetTables.Count, 0);

            foreach (var table in Texture2DAssetTables)
            {
                LocalizationEditorSettings.AddAssetToTable(table, keyEntry.Id, textureAsset);
            }

            var addressableEntry = FindAddressableAssetEntry(textureAsset);

            foreach (var table in Texture2DAssetTables)
            {
                var label = LocalizationEditorSettings.FormatAssetLabel(table.LocaleIdentifier);
                Assert.IsTrue(addressableEntry.labels.Contains(label), "Expected the asset to have the locale '" + table.LocaleIdentifier.Code + "' added as a label.");
            }
        }

        [Test]
        public void RemoveAssetFromTable_Texture2DAssetTable_IsRemovedFromAddressables()
        {
            var keyEntry = KeyDb.AddKey("RemoveAssetFromTable_Texture2DAssetTable_IsRemovedFromAddressables");
            var textureAsset = CreateTestTexture("RemoveAssetFromTable_Texture2DAssetTable_IsRemovedFromAddressables_Texture");

            LocalizationEditorSettings.AddAssetToTable(Texture2DAssetTables[0], keyEntry.Id, textureAsset);
            LocalizationEditorSettings.AddAssetToTable(Texture2DAssetTables[1], keyEntry.Id, textureAsset);
            VerifyAssetIsInAddressables(textureAsset);

            LocalizationEditorSettings.RemoveAssetFromTable(Texture2DAssetTables[0], keyEntry.Id, textureAsset);
            VerifyAssetIsInAddressables(textureAsset, "Removing an asset from a table when the asset is still being used by another table should result in the asset staying in Addressables.");

            LocalizationEditorSettings.RemoveAssetFromTable(Texture2DAssetTables[1], keyEntry.Id, textureAsset);
            VerifyAssetIsNotInAddressables(textureAsset, "The asset should be removed from the table when no other tables are using it.");
        }

        [Test]
        public void RemoveAssetFromTable_Texture2DAssetTable_IsNotRemovedFromAddressables_WhenUsedByAnotherKey()
        {
            var keyEntry1 = KeyDb.AddKey("1-RemoveAssetFromTable_Texture2DAssetTable_IsNotRemovedFromAddressables_WhenUsedByAnotherKey");
            var keyEntry2 = KeyDb.AddKey("2-RemoveAssetFromTable_Texture2DAssetTable_IsNotRemovedFromAddressables_WhenUsedByAnotherKey");
            var textureAsset = CreateTestTexture("RemoveAssetFromTable_Texture2DAssetTable_IsNotRemovedFromAddressables_WhenUsedByAnotherKey_Texture");

            LocalizationEditorSettings.AddAssetToTable(Texture2DAssetTables[0], keyEntry1.Id, textureAsset);
            LocalizationEditorSettings.AddAssetToTable(Texture2DAssetTables[0], keyEntry2.Id, textureAsset);
            VerifyAssetIsInAddressables(textureAsset);

            LocalizationEditorSettings.RemoveAssetFromTable(Texture2DAssetTables[0], keyEntry1.Id, textureAsset);
            VerifyAssetIsInAddressables(textureAsset, "Removing an asset from a table when the asset is still being used by the same table should result in the asset staying in Addressables.");

            LocalizationEditorSettings.RemoveAssetFromTable(Texture2DAssetTables[0], keyEntry2.Id, textureAsset);
            VerifyAssetIsNotInAddressables(textureAsset, "The asset should be removed from the table when no other tables are using it.");
        }

        [Test]
        public void AssetTableDeleted_Texture2DAssetTable_UnusedAssetsAreRemovedFromAddressables()
        {
            var keyEntry = KeyDb.AddKey("AssetTableDeleted_Texture2DAssetTable_UnusedAssetsAreRemovedFromAddressables");
            var assetPath = Path.Combine(k_TestConfigFolder, "AssetTableDeleted_Texture2DAssetTable_UnusedAssetsAreRemovedFromAddressables_Table.asset");
            var createdTable = LocalizationEditorSettings.CreateAssetTable(Locale.CreateLocale(SystemLanguage.English), KeyDb, "CreatedTable_IsAddedToAddressables", typeof(Texture2DAssetTable), assetPath) as Texture2DAssetTable;
            var textureAsset = CreateTestTexture("AssetTableDeleted_Texture2DAssetTable_UnusedAssetsAreRemovedFromAddressables_Texture");

            LocalizationEditorSettings.AddAssetToTable(createdTable, keyEntry.Id, textureAsset);
            VerifyAssetIsInAddressables(textureAsset);

            // Delete the table, the asset should now be removed as it is no longer used.
            DeleteAsset(createdTable);

            VerifyAssetIsNotInAddressables(textureAsset, "Asset should be removed when the table is deleted and no other table references it.");
        }
    }
}

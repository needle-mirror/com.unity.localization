using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.TestTools;

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

        [TestCaseSource(nameof(AllTableTypes))]
        public void CreatedTables_AreAddedToAddressables(Type tableType)
        {
            var createdTables = LocalizationEditorSettings.CreateAssetTables(GenerateSampleLocales(), null, nameof(CreatedTables_AreAddedToAddressables), tableType, k_TestConfigFolder);

            foreach (var table in createdTables)
            {
                VerifyAssetIsInAddressables(table);
            }
        }

        [TestCaseSource(nameof(AllTableTypes))]
        public void CreatedTable_IsAddedToAddressables(Type tableType)
        {
            var assetPath = Path.Combine(k_TestConfigFolder,$"{nameof(CreatedTable_IsAddedToAddressables)}_{tableType}.asset");
            var createdTable = LocalizationEditorSettings.CreateAssetTable(Locale.CreateLocale(SystemLanguage.English), KeyDb, nameof(CreatedTable_IsAddedToAddressables), tableType, assetPath);
            VerifyAssetIsInAddressables(createdTable);
        }

        [TestCaseSource(nameof(AllTableTypes))]
        public void ImportedTable_IsAddedToAddressables(Type tableType)
        {
            const string tableName = nameof(ImportedTable_IsAddedToAddressables);
            var assetPath = Path.Combine(k_TestConfigFolder, $"{nameof(ImportedTable_IsAddedToAddressables)}_{tableType}.asset");

            var relativePath = LocalizationEditorSettings.MakePathRelative(assetPath);
            var createdTable = (LocalizedTable)ScriptableObject.CreateInstance(tableType);
            createdTable.TableName = tableName;
            createdTable.Keys = KeyDb;
            createdTable.LocaleIdentifier = Locale.CreateLocale(SystemLanguage.Catalan).Identifier;
            createdTable.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(createdTable, relativePath);

            VerifyAssetIsInAddressables(createdTable, "Expected a table that was imported or created externally to be added to Addressables but it was not.");
        }

        [TestCaseSource(nameof(AllTableTypes))]
        public void GetAssetTablesCollectionInternal_UpdatesWhenTableAssetsAreAddedAndRemoved(Type tableType)
        {
            const string tableName = nameof(GetAssetTablesCollectionInternal_UpdatesWhenTableAssetsAreAddedAndRemoved);

            // Check the table is not already in the collection.
            var tableCollection = LocalizationEditorSettings.GetAssetTablesCollection<LocalizedTable>();
            Assert.IsFalse(CollectionContainsEntry(tableType, tableName, tableCollection), "Expected no table entry to exist in the AssetTablesCollection.");

            // Create the new tables and check they are returned.
            var createdTables = LocalizationEditorSettings.CreateAssetTables(GenerateSampleLocales(), KeyDb, tableName, tableType, k_TestConfigFolder);
            tableCollection = LocalizationEditorSettings.GetAssetTablesCollection<LocalizedTable>();
            Assert.IsTrue(CollectionContainsEntry(tableType, tableName, tableCollection), "Expected the new tables to have an entry in the AssetTablesCollection.");
            Assert.IsTrue(CollectionContainsTables(tableType, tableName, createdTables, tableCollection), "Expected the new tables to have been contained in the AssetTablesCollection.");

            // Now delete the table assets and check they are no longer returned.
            foreach (var table in createdTables)
            {
                DeleteAsset(table);
            }

            tableCollection = LocalizationEditorSettings.GetAssetTablesCollection<LocalizedTable>();
            Assert.IsFalse(CollectionContainsEntry(tableType, tableName, tableCollection), "Expected the deleted tables to not have an entry in the AssetTablesCollection.");
        }

        [TestCaseSource(nameof(AllTableTypes))]
        public void GetAssetTablesCollectionInternal_TableEntriesAllHaveTheSameTypeAndName(Type tableType)
        {
            const string tableName = nameof(GetAssetTablesCollectionInternal_TableEntriesAllHaveTheSameTypeAndName);

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

        [TestCaseSource(nameof(GenerateSampleLanguages))]
        public void AddLocale_IsAddedToAddressables(SystemLanguage lang)
        {
            var locale = Locale.CreateLocale(lang);
            CreateAsset(locale, $"{nameof(AddLocale_IsAddedToAddressables)}-{locale}");
            LocalizationEditorSettings.AddLocale(locale);
            VerifyAssetIsInAddressables(locale);
        }

        [TestCaseSource(nameof(GenerateSampleLanguages))]
        public void RemoveLocale_IsRemovedFromAddressables(SystemLanguage lang)
        {
            var locale = Locale.CreateLocale(lang);
            CreateAsset(locale, $"{nameof(RemoveLocale_IsRemovedFromAddressables)}-{locale}");
            LocalizationEditorSettings.AddLocale(locale);
            VerifyAssetIsInAddressables(locale);

            LocalizationEditorSettings.RemoveLocale(locale);
            VerifyAssetIsNotInAddressables(locale);
        }

        [Test]
        public void AddAssetToTable_Texture2DAssetTable_NonPersistentAssetIsRejected()
        {
            var keyEntry = KeyDb.AddKey(nameof(AddAssetToTable_Texture2DAssetTable_NonPersistentAssetIsRejected));

            var nonPersistentTexture = new Texture2D(1, 1) { name = "nonPersistentTexture" };
            LocalizationEditorSettings.AddAssetToTable(Texture2DAssetTables[0], keyEntry.Id, nonPersistentTexture);
            LogAssert.Expect(LogType.Error, new Regex("Only persistent assets can be addressable"));
        }

        [Test]
        public void AddAssetToTable_Texture2DAssetTable_AssetIsAddedToAddressables()
        {
            var keyEntry = KeyDb.AddKey(nameof(AddAssetToTable_Texture2DAssetTable_AssetIsAddedToAddressables));
            var textureAsset = CreateTestTexture(nameof(AddAssetToTable_Texture2DAssetTable_AssetIsAddedToAddressables));
            LocalizationEditorSettings.AddAssetToTable(Texture2DAssetTables[0], keyEntry.Id, textureAsset);
            VerifyAssetIsInAddressables(textureAsset);
        }

        [Test]
        public void AddAssetToTable_Texture2DAssetTable_TableIsDirtyAfterAddingAsset()
        {
            var keyEntry = KeyDb.AddKey(nameof(AddAssetToTable_Texture2DAssetTable_TableIsDirtyAfterAddingAsset));
            var textureAsset = CreateTestTexture(nameof(AddAssetToTable_Texture2DAssetTable_TableIsDirtyAfterAddingAsset));

            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(textureAsset, out string guid, out long localid), "Failed to get texture guid");
            Assert.IsFalse(string.IsNullOrEmpty(guid), "Expected a valid guid for the texture asset.");

            int dirtyCount = EditorUtility.GetDirtyCount(Texture2DAssetTables[0]);
            LocalizationEditorSettings.AddAssetToTable(Texture2DAssetTables[0], keyEntry.Id, textureAsset);
            int newDirtyCount = EditorUtility.GetDirtyCount(Texture2DAssetTables[0]);
            Assert.Greater(newDirtyCount, dirtyCount, "Expected texture table be dirty after adding an asset.");
        }

        [Test]
        public void AddAssetToTable_Texture2DAssetTable_SharedAssetHasCorrectLabels()
        {
            var keyEntry = KeyDb.AddKey(nameof(AddAssetToTable_Texture2DAssetTable_SharedAssetHasCorrectLabels));
            var textureAsset = CreateTestTexture($"{nameof(AddAssetToTable_Texture2DAssetTable_SharedAssetHasCorrectLabels)}_Texture");
            Assert.Greater(Texture2DAssetTables.Count, 0);

            foreach (var table in Texture2DAssetTables)
            {
                LocalizationEditorSettings.AddAssetToTable(table, keyEntry.Id, textureAsset);
            }

            var addressableEntry = FindAddressableAssetEntry(textureAsset);

            foreach (var table in Texture2DAssetTables)
            {
                var label = LocalizationEditorSettings.FormatAssetLabel(table.LocaleIdentifier);
                Assert.IsTrue(addressableEntry.labels.Contains(label), $"Expected the asset to have the locale '{table.LocaleIdentifier.Code}' added as a label.");
            }
        }

        [Test]
        public void RemoveAssetFromTable_Texture2DAssetTable_IsRemovedFromAddressables()
        {
            var keyEntry = KeyDb.AddKey(nameof(RemoveAssetFromTable_Texture2DAssetTable_IsRemovedFromAddressables));
            var textureAsset = CreateTestTexture($"{nameof(RemoveAssetFromTable_Texture2DAssetTable_IsRemovedFromAddressables)}_Texture");

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
            var keyEntry1 = KeyDb.AddKey($"1-{nameof(RemoveAssetFromTable_Texture2DAssetTable_IsNotRemovedFromAddressables_WhenUsedByAnotherKey)}");
            var keyEntry2 = KeyDb.AddKey($"2-{nameof(RemoveAssetFromTable_Texture2DAssetTable_IsNotRemovedFromAddressables_WhenUsedByAnotherKey)}");
            var textureAsset = CreateTestTexture($"{nameof(RemoveAssetFromTable_Texture2DAssetTable_IsNotRemovedFromAddressables_WhenUsedByAnotherKey)}_Texture");

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
            var keyEntry = KeyDb.AddKey(nameof(AssetTableDeleted_Texture2DAssetTable_UnusedAssetsAreRemovedFromAddressables));
            var assetPath = Path.Combine(k_TestConfigFolder, $"{nameof(AssetTableDeleted_Texture2DAssetTable_UnusedAssetsAreRemovedFromAddressables)}_Table.asset");
            var createdTable = LocalizationEditorSettings.CreateAssetTable(Locale.CreateLocale(SystemLanguage.English), KeyDb, nameof(AssetTableDeleted_Texture2DAssetTable_UnusedAssetsAreRemovedFromAddressables), typeof(Texture2DAssetTable), assetPath) as Texture2DAssetTable;
            var textureAsset = CreateTestTexture($"{nameof(AssetTableDeleted_Texture2DAssetTable_UnusedAssetsAreRemovedFromAddressables)}_Texture");

            LocalizationEditorSettings.AddAssetToTable(createdTable, keyEntry.Id, textureAsset);
            VerifyAssetIsInAddressables(textureAsset);

            // Delete the table, the asset should now be removed as it is no longer used.
            DeleteAsset(createdTable);

            VerifyAssetIsNotInAddressables(textureAsset, "Asset should be removed when the table is deleted and no other table references it.");
        }
    }
}

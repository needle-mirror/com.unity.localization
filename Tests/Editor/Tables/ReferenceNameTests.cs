using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Tests;
using UnityEngine.ResourceManagement;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    public class ReferenceNameTests
    {
        const string kStringTableCollectionName = "My String Table Collection";
        readonly Guid kStringTableNameGuid = new Guid("b3eefa4bca2d6464a9ac5a5dd9938a99");

        const string kAssetTableCollectionName = "My Asset Table Collection";
        readonly Guid kAssetTableNameGuid = new Guid("a3eefa4bca2d6464a9ac5a5dd9938a99");

        const long kStringKeyId = 123;
        const string kStringKeyName = "My Entry";

        LocalizationSettings m_Settings;
        SharedTableData m_SharedStringTableData;
        SharedTableData m_SharedAssetTableData;

        [SetUp]
        public void Setup()
        {
            LocalizationSettingsHelper.SaveCurrentSettings();

            m_SharedStringTableData = ScriptableObject.CreateInstance<SharedTableData>();
            m_SharedStringTableData.TableCollectionName = kStringTableCollectionName;
            m_SharedStringTableData.TableCollectionNameGuid = kStringTableNameGuid;
            m_SharedStringTableData.AddKey(kStringKeyName, kStringKeyId);

            m_SharedAssetTableData = ScriptableObject.CreateInstance<SharedTableData>();
            m_SharedAssetTableData.TableCollectionName = kAssetTableCollectionName;
            m_SharedAssetTableData.TableCollectionNameGuid = kAssetTableNameGuid;

            m_Settings = LocalizationSettingsHelper.CreateEmpty();
            var stringDb = new LocalizedStringDatabase();
            m_Settings.SetStringDatabase(stringDb);
            var assetDb = new LocalizedAssetDatabase();
            m_Settings.SetAssetDatabase(assetDb);

            var rm = new ResourceManager();
            stringDb.SharedTableDataOperations[kStringTableNameGuid] = rm.CreateCompletedOperation(m_SharedStringTableData, null);
            assetDb.SharedTableDataOperations[kAssetTableNameGuid] = rm.CreateCompletedOperation(m_SharedAssetTableData, null);

            LocalizationSettings.Instance = m_Settings;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Settings);
            Object.DestroyImmediate(m_SharedStringTableData);
            Object.DestroyImmediate(m_SharedAssetTableData);
            LocalizationSettingsHelper.RestoreSettings();
        }

        [Test]
        public void ResolvesName_WhenIsGuid_AndSharedTableDataIsAvailableIn_StringDatabase()
        {
            TableReference tableReference = kStringTableNameGuid;
            Assert.AreEqual(TableReference.Type.Guid, tableReference.ReferenceType);
            Assert.AreEqual(kStringTableCollectionName, tableReference.TableCollectionName, "Expected to resolve table name using Guid");
        }

        [Test]
        public void ToString_ResolvesName_WhenIsGuid_AndSharedTableDataIsAvailableIn_StringDatabase()
        {
            string expectedToString = $"TableReference({kStringTableNameGuid} - {kStringTableCollectionName})";

            TableReference tableReference = kStringTableNameGuid;
            Assert.AreEqual(TableReference.Type.Guid, tableReference.ReferenceType);
            Assert.AreEqual(expectedToString, tableReference.ToString(), "Expected to resolve table name using Guid");
        }

        [Test]
        public void ResolvesName_WhenIsGuid_AndSharedTableDataIsAvailableIn_AssetDatabase()
        {
            TableReference tableReference = kAssetTableNameGuid;
            Assert.AreEqual(TableReference.Type.Guid, tableReference.ReferenceType);
            Assert.AreEqual(kAssetTableCollectionName, tableReference.TableCollectionName, "Expected to resolve table name using Guid");
        }

        [Test]
        public void TableCollectionName_ReturnsNull_WhenIsGuidAndSharedTableDataIsNotAvailable()
        {
            var tableNameGuid = new Guid("c3eefa4bca2d6464a9ac5a5dd9938a11");

            TableReference tableReference = tableNameGuid;
            Assert.AreEqual(TableReference.Type.Guid, tableReference.ReferenceType);
            Assert.Null(tableReference.TableCollectionName);
        }

        [Test]
        public void EntryNameIsResolvedFromId()
        {
            TableReference tableReference = kStringTableNameGuid;
            TableEntryReference tableEntryReference = kStringKeyId;

            Assert.AreEqual("TableEntryReference(123 - My Entry)", tableEntryReference.ToString(tableReference));
        }

        [Test]
        public void EntryIdIsResolvedFromName()
        {
            TableReference tableReference = kStringTableNameGuid;
            TableEntryReference tableEntryReference = kStringKeyName;

            Assert.AreEqual("TableEntryReference(123 - My Entry)", tableEntryReference.ToString(tableReference));
        }
    }
}
